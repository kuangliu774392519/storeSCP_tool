using System.Text;
using FellowOakDicom;
using FellowOakDicom.Network;
using Microsoft.Extensions.Logging;

namespace StorescpTool.Infrastructure.Dicom;

public sealed class DicomStoreScp : DicomService, IDicomServiceProvider, IDicomCStoreProvider, IDicomCEchoProvider
{
    private string? _sessionId;
    private bool _sessionClosed;

    public DicomStoreScp(
        INetworkStream stream,
        Encoding fallbackEncoding,
        ILogger logger,
        DicomServiceDependencies dependencies)
        : base(stream, fallbackEncoding, logger, dependencies)
    {
    }

    public void OnReceiveAbort(DicomAbortSource source, DicomAbortReason reason)
    {
        State?.LogService.Warning("StoreSCP", $"Association aborted: {source} / {reason}", "Association");
        CompleteSession("Aborted", $"{source} / {reason}");
    }

    public void OnConnectionClosed(Exception exception)
    {
        if (exception is null)
        {
            State?.LogService.Info("StoreSCP", "Connection closed", "Association");
            CompleteSession("Closed");
        }
        else
        {
            State?.LogService.Error("StoreSCP", "Connection closed with exception", exception, "Association");
            CompleteSession("Faulted", exception.Message);
        }
    }

    public Task OnReceiveAssociationRequestAsync(DicomAssociation association)
    {
        _sessionId = Guid.NewGuid().ToString("N");
        _sessionClosed = false;
        State?.ReceiveSessionService.StartSession(
            _sessionId,
            association.CallingAE,
            association.CalledAE,
            association.RemoteHost);

        State?.LogService.Info(
            "StoreSCP",
            $"Association request from {association.CallingAE} ({association.RemoteHost}) to {association.CalledAE}",
            "Association");

        if (State?.Config.ValidateCalledAe == true &&
            !string.Equals(association.CalledAE, State.Config.LocalAeTitle, StringComparison.OrdinalIgnoreCase))
        {
            State.LogService.Warning("StoreSCP", $"Rejected association due to Called AE mismatch: {association.CalledAE}", "Association");
            CompleteSession("Rejected", $"Called AE mismatch: {association.CalledAE}");
            return SendAssociationRejectAsync(
                DicomRejectResult.Permanent,
                DicomRejectSource.ServiceUser,
                DicomRejectReason.CalledAENotRecognized);
        }

        foreach (var presentationContext in association.PresentationContexts)
        {
            presentationContext.AcceptTransferSyntaxes(
                DicomTransferSyntax.ImplicitVRLittleEndian,
                DicomTransferSyntax.ExplicitVRLittleEndian,
                DicomTransferSyntax.ExplicitVRBigEndian);
        }

        return SendAssociationAcceptAsync(association);
    }

    public Task OnReceiveAssociationReleaseRequestAsync()
    {
        State?.LogService.Info("StoreSCP", "Association release requested", "Association");
        CompleteSession("Completed");
        return SendAssociationReleaseResponseAsync();
    }

    public async Task<DicomCStoreResponse> OnCStoreRequestAsync(DicomCStoreRequest request)
    {
        try
        {
            var state = State ?? throw new InvalidOperationException("Store SCP state not initialized.");
            var dataset = request.File.Dataset;
            var studyUid = dataset.GetSingleValueOrDefault(DicomTag.StudyInstanceUID, "UNKNOWN_STUDY");
            var sopUid = dataset.GetSingleValueOrDefault(DicomTag.SOPInstanceUID, Guid.NewGuid().ToString("N"));
            var patientName = dataset.GetSingleValueOrDefault(DicomTag.PatientName, string.Empty);
            var targetDirectory = Path.Combine(
                state.Config.ReceiveDirectory,
                DateTime.Now.ToString("yyyy-MM-dd"),
                Sanitize(studyUid));

            Directory.CreateDirectory(targetDirectory);
            var targetPath = Path.Combine(targetDirectory, $"{Sanitize(sopUid)}.dcm");

            await request.File.SaveAsync(targetPath);
            state.ReceiveRecordService.Add(new StorescpTool.Core.Models.ReceiveRecord
            {
                ReceivedAt = DateTime.Now,
                CallingAe = Association?.CallingAE ?? string.Empty,
                CalledAe = Association?.CalledAE ?? string.Empty,
                RemoteIp = Association?.RemoteHost ?? string.Empty,
                PatientName = patientName,
                StudyInstanceUid = studyUid,
                SopInstanceUid = sopUid,
                FilePath = targetPath
            });
            if (!string.IsNullOrWhiteSpace(_sessionId))
            {
                state.ReceiveSessionService.MarkFileReceived(_sessionId, targetPath);
            }
            state.LogService.Info("StoreSCP", $"Received DICOM instance: {targetPath}", "C-STORE SCP");
            return new DicomCStoreResponse(request, DicomStatus.Success);
        }
        catch (Exception ex)
        {
            State?.LogService.Error("StoreSCP", "Failed to save incoming DICOM file", ex, "C-STORE SCP");
            return new DicomCStoreResponse(request, DicomStatus.ProcessingFailure);
        }
    }

    public Task OnCStoreRequestExceptionAsync(string tempFileName, Exception e)
    {
        State?.LogService.Error("StoreSCP", $"C-STORE parsing failed. Temp file: {tempFileName}", e, "C-STORE SCP");
        return Task.CompletedTask;
    }

    public Task<DicomCEchoResponse> OnCEchoRequestAsync(DicomCEchoRequest request)
    {
        State?.LogService.Info("StoreSCP", "Received C-ECHO request", "C-ECHO SCP");
        return Task.FromResult(new DicomCEchoResponse(request, DicomStatus.Success));
    }

    private StoreScpUserState? State => UserState as StoreScpUserState;

    private void CompleteSession(string status, string? errorMessage = null)
    {
        if (_sessionClosed || string.IsNullOrWhiteSpace(_sessionId) || State is null)
        {
            return;
        }

        _sessionClosed = true;
        State.ReceiveSessionService.CompleteSession(_sessionId, status, errorMessage);
    }

    private static string Sanitize(string value)
    {
        foreach (var invalid in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(invalid, '_');
        }

        return value;
    }
}

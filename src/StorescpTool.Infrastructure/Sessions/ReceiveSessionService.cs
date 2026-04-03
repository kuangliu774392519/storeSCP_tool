using System.Text.Json;
using StorescpTool.Application.Contracts;
using StorescpTool.Core.Models;

namespace StorescpTool.Infrastructure.Sessions;

public sealed class ReceiveSessionService : IReceiveSessionService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly List<ReceiveSessionSummary> _sessions = [];
    private readonly object _sync = new();
    private readonly string _storagePath;

    public event EventHandler<ReceiveSessionSummary>? SessionChanged;

    public ReceiveSessionService(string baseDirectory)
    {
        _storagePath = Path.Combine(baseDirectory, "data", "state", "receive_sessions.json");
        Load();
    }

    public IReadOnlyList<ReceiveSessionSummary> GetSessions()
    {
        lock (_sync)
        {
            return _sessions.ToList();
        }
    }

    public void StartSession(string sessionId, string callingAe, string calledAe, string remoteIp)
    {
        var session = new ReceiveSessionSummary
        {
            SessionId = sessionId,
            StartTime = DateTime.Now,
            CallingAe = callingAe,
            CalledAe = calledAe,
            RemoteIp = remoteIp,
            Status = "Started"
        };

        Upsert(session);
    }

    public void MarkFileReceived(string sessionId, string filePath)
    {
        lock (_sync)
        {
            var index = _sessions.FindIndex(s => s.SessionId == sessionId);
            if (index < 0)
            {
                return;
            }

            var current = _sessions[index];
            var updated = new ReceiveSessionSummary
            {
                SessionId = current.SessionId,
                StartTime = current.StartTime,
                EndTime = current.EndTime,
                CallingAe = current.CallingAe,
                CalledAe = current.CalledAe,
                RemoteIp = current.RemoteIp,
                ReceivedCount = current.ReceivedCount + 1,
                Status = "Receiving",
                LastFilePath = filePath,
                ErrorMessage = current.ErrorMessage
            };

            _sessions[index] = updated;
            Save();
            SessionChanged?.Invoke(this, updated);
        }
    }

    public void CompleteSession(string sessionId, string status, string? errorMessage = null)
    {
        lock (_sync)
        {
            var index = _sessions.FindIndex(s => s.SessionId == sessionId);
            if (index < 0)
            {
                return;
            }

            var current = _sessions[index];
            var updated = new ReceiveSessionSummary
            {
                SessionId = current.SessionId,
                StartTime = current.StartTime,
                EndTime = DateTime.Now,
                CallingAe = current.CallingAe,
                CalledAe = current.CalledAe,
                RemoteIp = current.RemoteIp,
                ReceivedCount = current.ReceivedCount,
                Status = status,
                LastFilePath = current.LastFilePath,
                ErrorMessage = errorMessage ?? current.ErrorMessage
            };

            _sessions[index] = updated;
            Save();
            SessionChanged?.Invoke(this, updated);
        }
    }

    private void Upsert(ReceiveSessionSummary session)
    {
        lock (_sync)
        {
            var index = _sessions.FindIndex(s => s.SessionId == session.SessionId);
            if (index >= 0)
            {
                _sessions[index] = session;
            }
            else
            {
                _sessions.Insert(0, session);
                if (_sessions.Count > 200)
                {
                    _sessions.RemoveAt(_sessions.Count - 1);
                }
            }

            Save();
            SessionChanged?.Invoke(this, session);
        }
    }

    private void Load()
    {
        var directory = Path.GetDirectoryName(_storagePath)!;
        Directory.CreateDirectory(directory);

        if (!File.Exists(_storagePath))
        {
            return;
        }

        try
        {
            var json = File.ReadAllText(_storagePath);
            var sessions = JsonSerializer.Deserialize<List<ReceiveSessionSummary>>(json, JsonOptions);
            if (sessions is not null)
            {
                _sessions.AddRange(sessions);
            }
        }
        catch
        {
            // Best effort load only.
        }
    }

    private void Save()
    {
        var directory = Path.GetDirectoryName(_storagePath)!;
        Directory.CreateDirectory(directory);
        var json = JsonSerializer.Serialize(_sessions, JsonOptions);
        File.WriteAllText(_storagePath, json);
    }
}

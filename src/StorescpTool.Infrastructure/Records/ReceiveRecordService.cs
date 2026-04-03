using System.Text.Json;
using StorescpTool.Application.Contracts;
using StorescpTool.Core.Models;

namespace StorescpTool.Infrastructure.Records;

public sealed class ReceiveRecordService : IReceiveRecordService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly List<ReceiveRecord> _records = [];
    private readonly object _sync = new();
    private readonly string _storagePath;

    public event EventHandler<ReceiveRecord>? RecordAdded;

    public ReceiveRecordService(string baseDirectory)
    {
        _storagePath = Path.Combine(baseDirectory, "data", "state", "receive_records.json");
        Load();
    }

    public IReadOnlyList<ReceiveRecord> GetRecords()
    {
        lock (_sync)
        {
            return _records.ToList();
        }
    }

    public void Add(ReceiveRecord record)
    {
        lock (_sync)
        {
            _records.Insert(0, record);
            if (_records.Count > 500)
            {
                _records.RemoveAt(_records.Count - 1);
            }

            Save();
        }

        RecordAdded?.Invoke(this, record);
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
            var records = JsonSerializer.Deserialize<List<ReceiveRecord>>(json, JsonOptions);
            if (records is not null)
            {
                _records.AddRange(records);
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
        var json = JsonSerializer.Serialize(_records, JsonOptions);
        File.WriteAllText(_storagePath, json);
    }
}

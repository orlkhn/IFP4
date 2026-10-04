using System.Text.Json;

public sealed class JsonStore : IAttendanceStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private readonly string _path;
    public JsonStore(string path) => _path = path;

    public void Append(IEnumerable<AttendanceEntry> entries)
    {
        var newEntries = entries.ToList();          // перечисляем ровно один раз
        var all = Load().ToList();                  // 1) прочитать то, что есть
        all.AddRange(newEntries);                   // 2) слить

        var dir = Path.GetDirectoryName(Path.GetFullPath(_path));
        if (dir != null) Directory.CreateDirectory(dir);

        // 3) записать всё обратно через временный файл, чтобы сбой
        //    посреди записи не испортил уже сохранённые данные
        string tmp = _path + ".tmp";
        using (var stream = File.Create(tmp))
        {
            JsonSerializer.Serialize(stream, all, Options);
        }
        File.Move(tmp, _path, overwrite: true);
    }

    public IEnumerable<AttendanceEntry> Load()
    {
        if (!File.Exists(_path) || new FileInfo(_path).Length == 0)
            return Array.Empty<AttendanceEntry>();

        using var stream = File.OpenRead(_path);
        return JsonSerializer.Deserialize<List<AttendanceEntry>>(stream)
               ?? new List<AttendanceEntry>();
    }
}
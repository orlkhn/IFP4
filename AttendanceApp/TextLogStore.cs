using System.Text;

public sealed class TextLogStore : IAttendanceStore
{
    private readonly string _path;
    public TextLogStore(string path) => _path = path;

    public void Append(IEnumerable<AttendanceEntry> entries)
    {
        // Сначала превращаем всё в строки: если формат сломается,
        // файл вообще не будет открыт и не получит половину данных.
        var lines = entries.Select(EntryFormat.ToLine).ToList();

        var dir = Path.GetDirectoryName(Path.GetFullPath(_path));
        if (dir != null) Directory.CreateDirectory(dir);

        using var writer = new StreamWriter(_path, append: true, Encoding.UTF8);
        foreach (var line in lines)
            writer.WriteLine(line);
        // using вызывает Dispose даже при исключении: дескриптор не останется заблокированным
    }

    public IEnumerable<AttendanceEntry> Load()
    {
        if (!File.Exists(_path)) return Array.Empty<AttendanceEntry>();
        return File.ReadAllLines(_path, Encoding.UTF8)
                   .Where(l => !string.IsNullOrWhiteSpace(l))
                   .Select(EntryFormat.FromLine)
                   .ToList();   // ToList: читаем сразу, файл закрыт к моменту возврата
    }
}
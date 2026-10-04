using System.Globalization;

public static class EntryFormat
{
    
    public static string ToLine(AttendanceEntry e)
    {
        if (e.Name.Contains('\n') || e.Name.Contains('\r'))
            throw new ArgumentException("Name must not contain line breaks.");
        return e.CheckInTime.ToString("O", CultureInfo.InvariantCulture) + "," + e.Name;
    }

    public static AttendanceEntry FromLine(string line)
    {
        int i = line.IndexOf(',');
        if (i < 0) throw new FormatException("Invalid log line: " + line);
        var time = DateTime.Parse(line[..i], CultureInfo.InvariantCulture,
                                  DateTimeStyles.RoundtripKind);
        return new AttendanceEntry(line[(i + 1)..], time);
    }
}
public static class AttendanceLogStarter
{
    public static void Record(string path, string name)
    {
        string line = name + "," + DateTime.Now;
        File.WriteAllText(path, line);
    }

    public static string ReadAll(string path)
    {
        return File.ReadAllText(path);
    }
}
Task1();
Console.WriteLine();
Task2();
Console.WriteLine();
Task3();
Console.WriteLine();
Task4();

static void Task1()
{
    string p = "task1.txt";
    if (File.Exists(p)) File.Delete(p);


    try { AttendanceLogStarter.ReadAll(p); }
    catch (Exception ex) { Console.WriteLine("Bug2: " + ex.GetType().Name); }

   
    AttendanceLogStarter.Record(p, "Alice");
    AttendanceLogStarter.Record(p, "Bob");
    Console.WriteLine("Bug1 lines: " + File.ReadAllLines(p).Length);
    Console.WriteLine(AttendanceLogStarter.ReadAll(p));

    
    var t = new DateTime(2026, 10, 4, 15, 30, 0);
    string ru = t.ToString(new System.Globalization.CultureInfo("ru-RU"));
    string us = t.ToString(new System.Globalization.CultureInfo("en-US"));
    Console.WriteLine($"Bug3: ru={ru} | us={us}");

    
    AttendanceLogStarter.Record(p, "Smith, John");
    Console.WriteLine("Bug4 split parts: " + AttendanceLogStarter.ReadAll(p).Split(',').Length);
}
static List<AttendanceEntry> Demo(IAttendanceStore store, string title)
{
    var t0 = new DateTime(2026, 10, 4, 9, 0, 0, DateTimeKind.Utc);
    Console.WriteLine($"--- {title} ---");
    Console.WriteLine("Before: " + store.Load().Count());      // 0

    store.Append(new[]
    {
        new AttendanceEntry("Alice", t0),
        new AttendanceEntry("Bob, Jr.", t0.AddMinutes(5))
    });

    var loaded = store.Load().ToList();
    foreach (var e in loaded) Console.WriteLine($"{e.Name} @ {e.CheckInTime:O}");
    Console.WriteLine("Count: " + loaded.Count);
    return loaded;
}

static void Task2()
{
    Console.WriteLine("===== TASK 2 (in-memory check) =====");
    var e = new AttendanceEntry("Smith, John", new DateTime(2026, 10, 4, 9, 0, 0, DateTimeKind.Utc));
    string line = EntryFormat.ToLine(e);
    Console.WriteLine(line);
    Console.WriteLine("Round trip equal: " + (EntryFormat.FromLine(line) == e));
}

static void Task3()
{
    foreach (var f in new[] { "t3.log", "t3.json" }) if (File.Exists(f)) File.Delete(f);

    var a = Demo(new TextLogStore("t3.log"), "TextLogStore");
    var b = Demo(new JsonStore("t3.json"), "JsonStore");
    Console.WriteLine("Identical: " + a.SequenceEqual(b));      // True
}
static void Pipeline(IAttendanceStore store, string label, int run,
                     IEnumerable<AttendanceEntry> newEntries, List<AttendanceEntry> expectedSoFar)
{
    store.Append(newEntries);
    var loaded = store.Load().ToList();
    Console.WriteLine($"[Run {run}] {label}: {loaded.Count} entries");
    foreach (var e in loaded) Console.WriteLine($"   {e.Name} @ {e.CheckInTime:O}");

    bool ok = loaded.SequenceEqual(expectedSoFar);              // ничего не потеряно и не лишнее
    bool noDup = loaded.Distinct().Count() == loaded.Count;     // без дублей
    Console.WriteLine($"   preserved+exact={ok}, no duplicates={noDup}");
}

static void Task4()
{
    foreach (var f in new[] { "t4.log", "t4.json" }) if (File.Exists(f)) File.Delete(f);
    var text = new TextLogStore("t4.log");
    var json = new JsonStore("t4.json");
    var t0 = new DateTime(2026, 10, 5, 10, 0, 0, DateTimeKind.Utc);

    var run1 = new[] { new AttendanceEntry("Alice", t0), new AttendanceEntry("Bob", t0.AddMinutes(1)) };
    var run2 = new[] { new AttendanceEntry("Carol", t0.AddHours(1)), new AttendanceEntry("Dan", t0.AddHours(1).AddMinutes(2)) };

    var expected = new List<AttendanceEntry>();

    expected.AddRange(run1);
    Pipeline(text, "TextLogStore", 1, run1, expected);
    Pipeline(json, "JsonStore",    1, run1, expected);

    expected.AddRange(run2);
    Pipeline(text, "TextLogStore", 2, run2, expected);
    Pipeline(json, "JsonStore",    2, run2, expected);

    Console.WriteLine("Stores equal: " + text.Load().SequenceEqual(json.Load()));
}
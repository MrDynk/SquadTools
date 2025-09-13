using System;
using System.IO;

public class RaidContext
{
    public string LogFilePath { get; set; }
    public DateTime RaidStart { get; set; }
    public DateTime RaidEnd { get; set; }
}

public static class RaidContextInitializer
{
    public static RaidContext? Initialize(string[] args)
    {
        // Example: parse args for log file path, start, and end times
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: <logFilePath> <raidStart:yyyy-MM-ddTHH:mm> <raidEnd:yyyy-MM-ddTHH:mm>");
            return null;
        }

        string logFilePath = args[0];
        if (!File.Exists(logFilePath))
        {
            Console.WriteLine($"Log file not found: {logFilePath}");
            return null;
        }

        if (!DateTime.TryParse(args[1], out DateTime raidStart))
        {
            Console.WriteLine($"Invalid raid start time: {args[1]}");
            return null;
        }
        if (!DateTime.TryParse(args[2], out DateTime raidEnd))
        {
            Console.WriteLine($"Invalid raid end time: {args[2]}");
            return null;
        }

        return new RaidContext
        {
            LogFilePath = logFilePath,
            RaidStart = raidStart,
            RaidEnd = raidEnd
        };
    }
}

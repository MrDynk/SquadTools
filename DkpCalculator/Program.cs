// See https://aka.ms/new-console-template for more information
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Logs;
using SquadSheets;



//grab log location and dkp sheet location from args
if (args.Length < 1)
{
    Console.WriteLine("Usage: DkpCalculator <logfile>, <dkpsheet>");
    return;
}


string logFilePath = args[0];
if (!File.Exists(logFilePath))
{
    Console.WriteLine($"Log file not found: {logFilePath}");
    return;
}

// Parse file name from logFilePath
string logFileName = Path.GetFileName(logFilePath);
Console.WriteLine($"Parsed log file name: {logFileName}");



GoogleDriveRepository googleDriveRepository = new GoogleDriveRepository();

string squadSheetPath = args[1];
googleDriveRepository.DownloadFile(ApplicationOptions.DKPFileName, squadSheetPath);

if (!File.Exists(squadSheetPath))
{
    Console.WriteLine($"SquadSheet not found: {squadSheetPath}");
    return;
}

//prompt user for start and end time
Console.WriteLine("Enter raid start time (yyyy-MM-dd HH:mm):");

DateTime raidStart;
while (!DateTime.TryParse(Console.ReadLine(), out raidStart))
{
    //Console.WriteLine("Invalid format. Please enter the start time in the format yyyy-MM-dd HH:mm:");
    raidStart = DateTime.Parse("2025-09-03 21:00");
    Console.WriteLine("Using Debugging Value 2025-09-03 21:00");
    break;
}

Console.WriteLine("Enter raid end time (yyyy-MM-dd HH:mm):");
DateTime raidEnd;
while (!DateTime.TryParse(Console.ReadLine(), out raidEnd) || raidEnd <= raidStart)
{
    //Console.WriteLine("Invalid format or end time is before start time. Please enter the end time in the format yyyy-MM-dd HH:mm:");
    raidEnd = DateTime.Parse("2025-09-03 23:31");
    Console.WriteLine("Using Debugging Value 2025-09-03 23:31");
    break;
}

// Initialize contexts
var squadSheetContext = new SquadSheetContext
{
    RaidStart = raidStart,
    RaidEnd = raidEnd,
    ZoneInfo = new List<Tuple<DateTime, string>>(),
    CombatantInfo = new List<Tuple<DateTime, string>>(),
    Deaths = new List<Tuple<DateTime, string>>(),
    Loot = new List<Loot>(),
    SquadPlayers = new List<Player>()
};

// Initialize repositories and calculator
ILogRepository logRepository = new TwowLogRepository(logFilePath);
//ISquadSheetRepository squadSheetRepository = new SquadSheetRepositoryZaretto(squadSheetPath, squadSheetContext);
ISquadSheetRepository squadSheetRepository = new SquadSheetRepositoryOds(squadSheetPath);
IDkpCalculator dkpCalculator = new DkpCalculator();
var PlayerHydrater = new PlayerHydrater();


logRepository.GetPriliminaryDataPoints(squadSheetContext);
squadSheetRepository.GetRosterDetails(squadSheetContext);
logRepository.GetPlayerActivity(squadSheetContext);
PlayerHydrater.PopulateSquadPlayerDetailsForRaid(squadSheetContext);

if(squadSheetContext.RaidEnd > squadSheetContext.BossesDefeated.Last().KillTime)
{
    Console.WriteLine(" ");
    Console.WriteLine(" ");
    Console.WriteLine($"Warning: Raid end time {squadSheetContext.RaidEnd} is after {squadSheetContext.BossesDefeated.Last().Name} kill time {squadSheetContext.BossesDefeated.Last().KillTime}. Adjusting raid end time to last boss kill time.");
    Console.WriteLine(" "); 
    Console.WriteLine(" ");
    squadSheetContext.RaidEnd = squadSheetContext.BossesDefeated.Last().KillTime;
}

dkpCalculator.CalculateDkp(squadSheetContext);

ConsoleReporter reporter = new ConsoleReporter();
reporter.Report(squadSheetContext);

//todo: update DKP in squadsheet
squadSheetRepository.UpdateDkp(squadSheetContext);

//googleDriveRepository.UploadFileSharedWithMe(ApplicationOptions.DKPFileName, squadSheetPath);

googleDriveRepository.UploadFile(ApplicationOptions.DKPFileName, squadSheetPath);



//stitch alts to mains

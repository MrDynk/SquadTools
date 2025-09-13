// See https://aka.ms/new-console-template for more information

using System.Text.RegularExpressions;
using Logs;
using SquadSheets;


/*
var raidContext = RaidContextInitializer.Initialize(args);
if (raidContext == null)
{
    return;
}
*/


// Iterate over all .txt files in LogsAwaitingParse
string awaitingDir = Path.Combine(Directory.GetCurrentDirectory(), "LogsAwaitingParse");
string finishedDir = Path.Combine(Directory.GetCurrentDirectory(), "LogsFinishedParse");
if (!Directory.Exists(awaitingDir)) Directory.CreateDirectory(awaitingDir);
if (!Directory.Exists(finishedDir)) Directory.CreateDirectory(finishedDir);

var logFiles = Directory.GetFiles(awaitingDir, "*.txt");
foreach (var logFilePath in logFiles)
{

    // Example filename: 9-10_2044_2309_Naxx.txt
    string fileName = Path.GetFileNameWithoutExtension(logFilePath);
    Console.WriteLine($"Processing file: {fileName}");
    // Regex: M-d_HHmm_HHmm_ZONE
    var match = Regex.Match(fileName, @"^(\d{1,2})-(\d{1,2})_(\d{4})_(\d{4})_(\w+)$");
    if (!match.Success)
    {
        Console.WriteLine($"Filename format invalid: {fileName}");
        continue;
    }


    // Prompt user for DKP for whole raid
    Console.Write("Add DKP for whole raid? (Y/N): ");
    string addDkpWholeRaid = Console.ReadLine()?.Trim().ToUpperInvariant();
    int dkpWholeRaidAmount = 0;
    if (addDkpWholeRaid == "Y")
    {
        Console.Write("Amount?: ");
        if (int.TryParse(Console.ReadLine(), out int amt))
            dkpWholeRaidAmount = amt;
    }

    // Prompt user for DKP for specific players
    Console.Write("Add DKP For Players? (Y/N): ");
    string addDkpForPlayers = Console.ReadLine()?.Trim().ToUpperInvariant();
    Dictionary<string, int> playerDkp = new Dictionary<string, int>();
    if (addDkpForPlayers == "Y")
    {
        Console.WriteLine("Players (format: {Name:Amount},{Name:Amount}): ");
        string input = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(input))
        {
            var entries = input.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var entry in entries)
            {
                var trimmed = entry.Trim().Trim('{', '}');
                var parts = trimmed.Split(':', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2 && int.TryParse(parts[1], out int val))
                {
                    playerDkp[parts[0].ToLower()] = val;
                }
            }
        }
    }
    int month = int.Parse(match.Groups[1].Value);
    int day = int.Parse(match.Groups[2].Value);
    int startHour = int.Parse(match.Groups[3].Value.Substring(0, 2));
    int startMin = int.Parse(match.Groups[3].Value.Substring(2, 2));
    int endHour = int.Parse(match.Groups[4].Value.Substring(0, 2));
    int endMin = int.Parse(match.Groups[4].Value.Substring(2, 2));
    string zoneAbbreviation = match.Groups[5].Value;
    // Use current year
    int year = DateTime.Now.Year;
    DateTime raidStart = new DateTime(year, month, day, startHour, startMin, 0);
    DateTime raidEnd = new DateTime(year, month, day, endHour, endMin, 0);

    var squadSheetContext = new SquadSheetContext
    {
        RaidStart = raidStart,
        RaidEnd = raidEnd,
        ZoneInfo = new List<Tuple<DateTime, string>>(),
        CombatantInfo = new List<Tuple<DateTime, string>>(),
        Deaths = new List<Tuple<DateTime, string>>(),
        Loot = new List<Loot>(),
        SquadPlayers = new List<Player>(),
        AliasTimeStamps = new Dictionary<string, List<DateTime>>(),
        BossesDefeated = new List<Boss>(),
        RaidDkpAwardedByLeadership = dkpWholeRaidAmount,
        PlayerDkpAwardedByLeadership = playerDkp
    };



    GoogleSheetRepository googleSheetRepository = new GoogleSheetRepository();
    var sheetTokens = new List<string> { "Program", raidStart.ToString("MMM"), raidStart.ToString("yy") };
    var squadsheet = googleSheetRepository.DownloadGoogleSheet(sheetTokens, squadSheetContext);
    ILogRepository logRepository = new TwowLogRepository(logFilePath);
    ISquadSheetRepository squadSheetRepository = new SquadSheetRepositoryGoogleSheet(squadsheet);
    IDkpCalculator dkpCalculator = new DkpCalculator();
    var PlayerHydrater = new PlayerHydrater();

    logRepository.GetPriliminaryDataPoints(squadSheetContext);
    squadSheetRepository.GetRosterDetails(squadSheetContext);
    logRepository.GetPlayerActivity(squadSheetContext);
    PlayerHydrater.PopulateSquadPlayerDetailsForRaid(squadSheetContext);

    if (squadSheetContext.RaidEnd > squadSheetContext.BossesDefeated.Last().KillTime)
    {
        Console.WriteLine(" ");
        Console.WriteLine($"Raid end time {squadSheetContext.RaidEnd} is after {squadSheetContext.BossesDefeated.Last().Name} kill time {squadSheetContext.BossesDefeated.Last().KillTime}. Adjusting raid end time to last boss kill time.");
        Console.WriteLine(" ");
        squadSheetContext.RaidEnd = squadSheetContext.BossesDefeated.Last().KillTime;
    }


    dkpCalculator.CalculateDkp(squadSheetContext);
    squadSheetRepository.PopulateRaidDetails(squadSheetContext);
    squadSheetRepository.UpdateDkp(squadSheetContext);
    googleSheetRepository.UpdateGoogleSheet(sheetTokens, squadSheetContext, squadsheet);

    sheetTokens.Add("Audit");
    var auditSquadsheet = googleSheetRepository.DownloadGoogleSheet(sheetTokens, squadSheetContext);
    AuditSheetRepository auditSheetRepository = new AuditSheetRepository(auditSquadsheet);
    auditSheetRepository.Update(squadSheetContext);
    googleSheetRepository.UpdateGoogleSheet(sheetTokens, squadSheetContext, auditSquadsheet);

    // Move processed file to LogsFinishedParse
    string destPath = Path.Combine(finishedDir, Path.GetFileName(logFilePath));
    File.Move(logFilePath, destPath, overwrite: true);
    //Console.WriteLine($"Processed and moved: {logFilePath} -> {destPath}");
}






//stitch alts to mains

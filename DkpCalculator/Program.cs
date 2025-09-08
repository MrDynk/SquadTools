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
        
        
        string squadSheetPath = args[1];
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
            Console.WriteLine("Invalid format. Please enter the start time in the format yyyy-MM-dd HH:mm:");
        }

        Console.WriteLine("Enter raid end time (yyyy-MM-dd HH:mm):");
        DateTime raidEnd;
        while (!DateTime.TryParse(Console.ReadLine(), out raidEnd) || raidEnd <= raidStart)
        {
            Console.WriteLine("Invalid format or end time is before start time. Please enter the end time in the format yyyy-MM-dd HH:mm:");
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
        ISquadSheetRepository squadSheetRepository = new SquadSheetRepositoryODS(squadSheetPath, squadSheetContext);
        IDkpCalculator dkpCalculator = new DkpCalculator();

        // Data retrieval
        logRepository.GetPriliminaryDataPoints(squadSheetContext);
        squadSheetRepository.GetRosterDetails(squadSheetContext);




//Build Valid Squad Players List and associate timestamps
foreach (var logCombatant in squadSheetContext.CombatantInfo)
{
    var combatantName = logCombatant.Item2;
    var player = squadSheetContext.SquadPlayers
        .FirstOrDefault(p => p.PlayerAliases.Contains(combatantName));

    if (player == null)
    {
        Console.WriteLine($"Detected non squad player in Log {combatantName}");
        continue;
    }

    player.AliasTimeStamps[combatantName].Add(logCombatant.Item1);
    player.PresentInRaid = true;
}

//remove players without timestamps
squadSheetContext.SquadPlayers = squadSheetContext.SquadPlayers
    .Where(p => p.PresentInRaid).ToList();

//associate loot
foreach (var item in squadSheetContext.Loot)
{
    //Console.WriteLine($"Time Looted: {item.TimeStamp} Player: {item.PlayerName} Item: {item.Item} Cost: Could Be Read from discord bot");
    var player = squadSheetContext.SquadPlayers
        .FirstOrDefault(p => p.PlayerAliases.Contains(item.PlayerName));

        if( player == null)
        {
            Console.WriteLine($"Looted item for non squad player {item.PlayerName} Item: {item.Item} Time: {item.TimeStamp}");
            continue;
        }
        player.FatLoot.Add(item);
}

//output players detected in raid, their timestamps and loot

/* EXAMPLE:
===================================
Alias: Sharkblood, Timestamps: 22:03:49, 22:05:15, 22:09:37, 22:14:53, 22:28:26, 22:29:13
Alias: Sharkdog, Timestamps: 20:37:13, 20:39:38, 20:47:34, 20:50:31, 20:54:48, 21:08:09, 21:11:18, 21:14:16, 21:18:53, 21:46:11, 21:47:20
Total DKP: 124
Loot:
  Time: 9/5/2025 9:17:38 PM, Item: Qiraji Lord's Insignia
  Time: 9/5/2025 9:20:37 PM, Item: Heavy Silithid Husk
  Time: 9/5/2025 9:22:03 PM, Item: Ancient Qiraji Artifact
  Time: 9/5/2025 9:27:36 PM, Item: Qiraji Lord's Insignia
  Time: 9/5/2025 9:28:36 PM, Item: Ancient Qiraji Artifact
  Time: 9/5/2025 9:28:38 PM, Item: Ancient Qiraji Artifact
  Time: 9/5/2025 9:29:47 PM, Item: Ancient Qiraji Artifact
  Time: 9/5/2025 9:30:43 PM, Item: Ancient Qiraji Artifact
  Time: 9/5/2025 9:33:26 PM, Item: Ancient Qiraji Artifact
  Time: 9/5/2025 9:37:12 PM, Item: Qiraji Lord's Insignia
  Time: 9/5/2025 9:43:40 PM, Item: Qiraji Lord's Insignia
  Time: 9/5/2025 9:46:09 PM, Item: Barb of the Sand Reaver
  Time: 9/5/2025 9:51:40 PM, Item: Qiraji Lord's Insignia
  Time: 9/5/2025 9:56:01 PM, Item: Bronze Scarab
  Time: 9/5/2025 9:57:38 PM, Item: Heavy Silithid Husk
  Time: 9/5/2025 10:01:48 PM, Item: Qiraji Lord's Insignia
  Time: 9/5/2025 10:39:16 PM, Item: Qiraji Lord's Insignia
  Time: 9/5/2025 10:39:34 PM, Item: Qiraji Lord's Insignia
  Time: 9/5/2025 10:42:59 PM, Item: Vek'lor's Gloves of Devastation
  Time: 9/5/2025 10:57:32 PM, Item: Qiraji Lord's Insignia

===================================
*/
foreach (var player in squadSheetContext.SquadPlayers)
{
    Console.WriteLine("===================================");
    foreach (var alias in player.PlayerAliases)
    {
        player.AliasTimeStamps[alias] = player.AliasTimeStamps[alias].OrderBy(t => t).ToList();
        var timestamps = string.Join(", ", player.AliasTimeStamps[alias].Select(t => t.ToString("HH:mm:ss")));
        Console.WriteLine($"Alias: {alias}, Timestamps: {timestamps}");
    }
    //Console.WriteLine($"earned DKP{player.EarnedDkp}");
    Console.WriteLine($"Total DKP: {player.TotalDkp}");
    Console.WriteLine("Loot:");
    foreach (var loot in player.FatLoot)
    {
        Console.WriteLine($"  Time: {loot.TimeStamp}, Item: {loot.Item}");
    }
    Console.WriteLine();
}

//todo: implement dkp calculation
dkpCalculator.CalculateDkp(squadSheetContext);

//todo: update DKP in squadsheet
squadSheetRepository.UpdateDkp(squadSheetContext);



//stitch alts to mains

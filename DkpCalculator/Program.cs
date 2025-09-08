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
    /*  var matchingSquadEntry = squadSheetContext.SquadSheetPlayerRoster
    .FirstOrDefault(s => s.Item2.Contains(logCombatant.Item2, StringComparison.OrdinalIgnoreCase));
   

    if (matchingSquadEntry == null)
    {
        Console.WriteLine($"Player in log not found in squad sheet: {combatantName}");
        continue;
    }
     */
    var combatantName = logCombatant.Item2;
    var player = squadSheetContext.SquadPlayers
        .FirstOrDefault(p => p.PlayerAliases.Contains(combatantName));

    if (player == null)
    {
        Console.WriteLine($"Detected non squad player in raid {combatantName}");
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
foreach (var player in squadSheetContext.SquadPlayers)
{
    Console.WriteLine("===================================");
    foreach (var alias in player.PlayerAliases)
    {
        player.AliasTimeStamps[alias] = player.AliasTimeStamps[alias].OrderBy(t => t).ToList();
        var timestamps = string.Join(", ", player.AliasTimeStamps[alias].Select(t => t.ToString("HH:mm:ss")));
        Console.WriteLine($"Alias: {alias}, Timestamps: {timestamps}");
    }

    Console.WriteLine("Loot:");
    foreach (var loot in player.FatLoot)
    {
        Console.WriteLine($"  Time: {loot.TimeStamp}, Item: {loot.Item}");
    }
    Console.WriteLine();
}



/*
DATA RETRIEVAL
*/
//LogRepository(log loc)
//.GetPriliminaryDataPoints: read zone_info, combatant_info, dies, loot from log file keep timestamps

//SquadSheetRepository(file loc, start time, end time)
//.GetPlayerList:  read playerlist from dkp sheet

//report any found players not present in dkp sheet and remove them from context

//LogRepository.GetPlayerActivity log timestamps for squad players

//DKPCalculator
// CalculateDKP(context)


//SquadsheetRepository.UpdateDKP(context)



//stitch alts to mains

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
            Loot = new List<Tuple<DateTime, string>>(),
            SquadSheetPlayerRoster = new List<Tuple<int, string>>()
        };

        // Initialize repositories and calculator
        ILogRepository logRepository = new TwowLogRepository(logFilePath);
        ISquadSheetRepository squadSheetRepository = new SquadSheetRepository(squadSheetPath);
        IDkpCalculator dkpCalculator = new DkpCalculator();

        // Data retrieval
        logRepository.GetPriliminaryDataPoints(squadSheetContext);
        squadSheetRepository.GetRosterDetails(squadSheetContext);


squadSheetContext.SquadPlayers = new List<Player>();



/*
//Build Valid Squad Players List
foreach (var logCombatant in squadSheetContext.CombatantInfo)
{
    var matchingSquadEntry = squadSheetContext.SquadSheetPlayerRoster
    .FirstOrDefault(s => s.Item2.Contains(logCombatant.Item2, StringComparison.OrdinalIgnoreCase));

    if (matchingSquadEntry == null)
    {
        Console.WriteLine($"Player in log not found in squad sheet: {logCombatant.Item2}");
        continue;
    }

    var player = squadSheetContext.SquadPlayers
        .FirstOrDefault(p => string.Equals(p.NamesAndAliases, matchingSquadEntry.Item2, StringComparison.OrdinalIgnoreCase));

    if (player != null && !player.CombatantTimeStamps.Contains(logCombatant.Item1))
    {
        Console.WriteLine($"Detected Alt in raid activity for Player Family: {player.NamesAndAliases}");
        player.CombatantTimeStamps.Add(logCombatant.Item1);
        continue;
    }
    player = new Player
    {
        SquadSheetLocation = matchingSquadEntry.Item1,
        NamesAndAliases = matchingSquadEntry.Item2,
        TotalDkp = 0,
        EarnedDkp = 0,
        FatLoot = new List<string>(),
        CombatantTimeStamps = new List<DateTime>()
    };

    player.CombatantTimeStamps.Add(logCombatant.Item1);
    squadSheetContext.SquadPlayers.Add(player);
}
*/
//associate loot        




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

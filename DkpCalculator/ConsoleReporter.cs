public class ConsoleReporter
{
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
    public void Report(SquadSheetContext context)
    {

        foreach (var player in context.SquadPlayers)
        {
            Console.WriteLine("===================================");
            
            Console.WriteLine("Raider:");
            Console.WriteLine($"    {string.Join(", ", player.PlayerAliases)}");
            Console.WriteLine("DKP Stats:");
            Console.WriteLine($"    Available DKP: {player.AvailableDkp}");
            Console.WriteLine($"    DKP Earned in current Month: {player.MonthlyEarnedDkp}");
            Console.WriteLine($"    DKP Spent in current Month: {player.MonthlySpentDkp}");
            Console.WriteLine($"    DKP Earned in raid: {player.RaidEarnedDkp}");

            if (player.FatLoot.Count > 0) Console.WriteLine("Loot:");
            foreach (var loot in player.FatLoot)
            {
                Console.WriteLine($"    Time: {loot.TimeStamp.ToString("HH:mm:ss")}, Item: {loot.Item}");
            }
            if(player.Deaths.Count > 0) Console.WriteLine("Deaths:");
            foreach (var death in player.Deaths)
            {
                Console.WriteLine($"    {death.ToString("HH:mm:ss")}");
            }
            if(player.DkpDeductions.Count > 0) Console.WriteLine("DKP Deductions:");
            foreach (var deduction in player.DkpDeductions)
            {
                Console.WriteLine($"    Reason: {deduction.Item1}, Amount: {deduction.Item2}");
            }
        }
        Console.WriteLine("===================================");
        Console.WriteLine("Boss Kills:");
        foreach (var boss in context.BossesDefeated)
        {
            Console.WriteLine($"    {boss.Name} at {boss.KillTime.ToString("HH:mm:ss")}");
        }
    }
}
public class DkpCalculator: IDkpCalculator
{
    // Calculates DKP based on the provided contexts
    public void CalculateDkp(SquadSheetContext squadSheetContext)
    {
        var raidDuration = (squadSheetContext.RaidEnd - squadSheetContext.RaidStart).TotalHours;
        // unecissary complexity, just dock people if caught jackin it... var dkpPerHour = 1; // Example rate, can be adjusted

        foreach (var player in squadSheetContext.SquadPlayers)
        {
            if (player.PlayerAliases.FirstOrDefault(x => x.Equals("Lucii", StringComparison.OrdinalIgnoreCase)) != null)
            {
                Console.WriteLine("Debug Break");
            }
        
            var sortedMergedTimestamps = player.AliasTimeStamps.Values.SelectMany(t => t).OrderBy(ts => ts).ToList();
            DateTime firstTimestamp = sortedMergedTimestamps.FirstOrDefault();
            DateTime lastTimestamp = sortedMergedTimestamps.LastOrDefault();
            if (firstTimestamp > squadSheetContext.RaidStart)
            {
                player.DkpDeductions.Add(Tuple.Create($"Late Join {{ {firstTimestamp} }}", 1));
                player.RaidEarnedDkp -= 1;
                //Console.WriteLine($"Player {string.Join("/", player.PlayerAliases)} first joined after raid start at {firstTimestamp} Deduct 1 dkp");

            }
            if (squadSheetContext.RaidEnd - lastTimestamp > TimeSpan.FromMinutes(ApplicationOptions.DeathOnFinalBossBuffer))
            {
                player.DkpDeductions.Add(Tuple.Create($"Early Leave {{ {lastTimestamp} }}", 1));
                player.RaidEarnedDkp -= 1;
                //Console.WriteLine($"Player {string.Join("/", player.PlayerAliases)} last left before raid end at {lastTimestamp} Deduct 1 dkp");
            }
            if(player.ActivityGaps.Count > 0)
            {
                foreach (var gap in player.ActivityGaps)
                {
                    player.DkpDeductions.Add(Tuple.Create($"Inactivity:{{ {gap.GapStart} to {gap.GapEnd} }}", 1));
                    player.RaidEarnedDkp -= 1;
                    //Console.WriteLine($"Player {string.Join("/", player.PlayerAliases)} was jackin it from {gap.GapStart} to {gap.GapEnd}. for {gap.GapDuration} Deduct 1 dkp");
                }
            }

        }
    }
}
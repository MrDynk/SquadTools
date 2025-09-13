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
                DkpDeduction dkpDeduction = new DkpDeduction
                {
                    Reason = DeductionReasonEnum.LateJoin,
                    Amount = 1,
                    RelatedTimeStamps = new List<DateTime> { firstTimestamp }
                };
                player.DkpDeductions.Add(dkpDeduction);
                player.RaidEarnedDkp -= 1;

            }
            if (squadSheetContext.RaidEnd - lastTimestamp > TimeSpan.FromMinutes(ApplicationOptions.DeathOnFinalBossBuffer))
            {
                 DkpDeduction dkpDeduction = new DkpDeduction
                {
                    Reason = DeductionReasonEnum.EarlyLeave,
                    Amount = 1,
                    RelatedTimeStamps = new List<DateTime> { lastTimestamp }
                };
                player.DkpDeductions.Add(dkpDeduction);
                player.RaidEarnedDkp -= 1;
                
            }
            if(player.ActivityGaps.Count > 0)
            {
                foreach (var gap in player.ActivityGaps)
                {
                    DkpDeduction dkpDeduction = new DkpDeduction
                    {
                        Reason = DeductionReasonEnum.Inactivity,
                        Amount = 1,
                        RelatedTimeStamps = new List<DateTime> { gap.GapStart, gap.GapEnd }
                    };
                    player.DkpDeductions.Add(dkpDeduction);
                    player.RaidEarnedDkp -= 1;
                    
                }
            }

        }
    }
}
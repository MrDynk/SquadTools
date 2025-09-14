public class DkpCalculator: IDkpCalculator
{
    private readonly ApplicationOptions _options;
    public DkpCalculator(ApplicationOptions options)
    {
        _options = options;
    }
    // Calculates DKP based on the provided contexts
    public void CalculateDkp(SquadSheetContext squadSheetContext)
    {
        foreach (var player in squadSheetContext.SquadPlayers)
        {
            ProcessAwards(squadSheetContext, player);
            ProcessDeductions(squadSheetContext, player);
        }
    }

    private void ProcessAwards(SquadSheetContext squadSheetContext, Player player)
    {
        player.RaidEarnedDkp = squadSheetContext.PotentialDkpEarnedForRaid;
        foreach (var award in player.LeadershipAwardedDkpList)
        {
            player.RaidEarnedDkp += award.Amount;
        }
        player.MonthlyEarnedDkp += player.RaidEarnedDkp;
        player.AvailableDkp += player.RaidEarnedDkp;
    }

    private void ProcessDeductions(SquadSheetContext squadSheetContext, Player player)
    {
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
        if (squadSheetContext.RaidEnd - lastTimestamp > TimeSpan.FromMinutes(_options.DeathOnFinalBossBuffer))
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
        if (player.ActivityGaps.Count > 0)
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
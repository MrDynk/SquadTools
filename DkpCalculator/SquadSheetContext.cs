using System.Reflection.Metadata;

public class SquadSheetContext
{
    public required List<Boss> BossesDefeated { get; set; }
    public int RaidColumnIdx = -1;
    public DateTime RaidStart { get; set; }
    public DateTime RaidEnd { get; set; }

    public string RaidZone
    {
        get
        {
            if (BossesDefeated != null && BossesDefeated.Count > 0)
            {
                return BossesDefeated[0].Zone;
            }
            return string.Empty;
        }
    }
    public int PotentialDkpEarnedForRaid { get; set; }
    public int RaidDkpAwardedByLeadership { get; set; }
    public required Dictionary<string, int> PlayerDkpAwardedByLeadership { get; set; }
    //log data
    public required List<Tuple<DateTime, string>> ZoneInfo { get; set; }
    public required List<Tuple<DateTime, string>> CombatantInfo { get; set; }
    public required List<Tuple<DateTime, string>> Deaths { get; set; }

    public required Dictionary<string, List<DateTime>> AliasTimeStamps { get; set; }
    public required List<Loot> Loot { get; set; }

    //squad sheet data
    //public List<Tuple<int, string>> SquadSheetPlayerRoster { get; set; }

    public required List<Player> SquadPlayers { get; set; }
}
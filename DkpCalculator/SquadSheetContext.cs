using System.Reflection.Metadata;

public class SquadSheetContext
{

    public List<Boss> BossesDefeated { get; set; }
    public int RaidColumn = -1;
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
    //log data
    public List<Tuple<DateTime, string>> ZoneInfo { get; set; }
    public List<Tuple<DateTime, string>> CombatantInfo { get; set; }
    public List<Tuple<DateTime, string>> Deaths { get; set; }

    public Dictionary<string, List<DateTime>> AliasTimeStamps { get; set; }
    public List<Loot> Loot { get; set; }

    //squad sheet data
    //public List<Tuple<int, string>> SquadSheetPlayerRoster { get; set; }

    public List<Player> SquadPlayers { get; set; }


  
}
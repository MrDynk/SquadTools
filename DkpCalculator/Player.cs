public class Player
{
    public bool PresentInRaid = false;
    public int SquadSheetLocation { get; set; }
    public int AvailableDkp { get; set; }
    public int MonthlyEarnedDkp { get; set; }
    public int MonthlySpentDkp { get; set; }
    public int RaidEarnedDkp { get; set; }
    public List<Loot> FatLoot { get; set; }

    public List<Tuple<string, int>> DkpDeductions { get; set; }
    public Dictionary<string, List<DateTime>> AliasTimeStamps { get; set; }

    public List<PlayerActivityGap> ActivityGaps { get; set; }
    public List<DateTime> Deaths = new List<DateTime>();

    public List<string> PlayerAliases {
        get
        {
            return AliasTimeStamps.Keys.ToList();
        }
    }
}
public class Player
{
    public bool PresentInRaid = false;
    public int SquadSheetLocation { get; set; }
    public int AvailableDkp { get; set; }
    public int MonthlyEarnedDkp { get; set; }
    public int MonthlySpentDkp { get; set; }
    public int RaidEarnedDkp { get; set; }
    public required List<Loot> FatLoot { get; set; }

    public required List<DkpDeduction> DkpDeductions { get; set; }
    public required Dictionary<string, List<DateTime>> AliasTimeStamps { get; set; }

    public required List<PlayerActivityGap> ActivityGaps { get; set; }
    public List<DateTime> Deaths = new List<DateTime>();

    public List<string> PlayerAliases {
        get
        {
            return AliasTimeStamps.Keys.ToList();
        }
    }
}
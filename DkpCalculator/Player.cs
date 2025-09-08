public class Player
{
    public bool PresentInRaid = false;
    public int SquadSheetLocation { get; set; }
    public int TotalDkp { get; set; }
    public int EarnedDkp { get; set; }
    public List<Loot> FatLoot { get; set; }
    public Dictionary<string, List<DateTime>> AliasTimeStamps { get; set; }

    public List<string> PlayerAliases {
        get
        {
            return AliasTimeStamps.Keys.ToList();
        }
    }
}
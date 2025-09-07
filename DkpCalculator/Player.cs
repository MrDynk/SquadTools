public class Player
{
    public int SquadSheetLocation { get; set; }
    public string NamesAndAliases { get; set; }
    public int TotalDkp { get; set; }
    public int EarnedDkp { get; set; }
    public List<string> FatLoot { get; set; }
    public Tuple<string, DateTime> LastTimeActiveBeforeBossDefeat { get; set; }

    public List<DateTime> CombatantTimeStamps { get; set; }
}
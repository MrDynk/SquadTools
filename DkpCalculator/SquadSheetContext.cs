public class SquadSheetContext
{
    public const int RaidIdRow = 1;
    public const int PlayerRosterColumn = 0;
    public const int CurrentDkpColumn = 6;
    public const int DkpSpentColumn = 4;

    public int RaidColumn { get; set; }
    public DateTime RaidStart { get; set; }
    public DateTime RaidEnd { get; set; }

    //log data
    public List<Tuple<DateTime, string>> ZoneInfo { get; set; }
    public List<Tuple<DateTime, string>> CombatantInfo { get; set; }
    public List<Tuple<DateTime, string>> Deaths { get; set; }
    public List<Loot> Loot { get; set; }

    //squad sheet data
    //public List<Tuple<int, string>> SquadSheetPlayerRoster { get; set; }
    
    public List<Player> SquadPlayers { get; set; }


}
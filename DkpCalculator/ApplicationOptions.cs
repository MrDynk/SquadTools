public class ApplicationOptions
{
    public const int RowsRemovedFromAuditSheet = 7;
    public const int raidDateRowIndex = 0;
    public const int RaidIdRowIndex = 1;
    public const int PlayerRosterColumnIndex = 0;
    public const int FirstPlayerRowIndex = 2;
    public const int MonthlyEarnedDkpColumnIndex = 2;
    public const int MonthlySpentDkpColumnIndex = 4;
    public const int AvailableDkpColumnIndex = 6;
    public const int InactivityThresholdMinutes = 5;

    public const int DeathOnFinalBossBuffer = 10;

    public const string DKPSpreadSheetName = "SQUAD DKP test";
    //public const string DKPFileName = "DKPOnMyDrive";
    
    public static readonly Dictionary<string, string> ZoneToAbbrevLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "naxxramas", "Naxx" },
        {"ahn'qiraj","TAQ"},
        {"tower of karazhan","uKara" }

    };

        public static readonly Dictionary<string, int> DkpPotential = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {
        { "naxxramas", 6 },
        {"ahn'qiraj", 6 },
        {"tower of karazhan", 6 }

    };


    public static readonly Dictionary<string, List<string>> bossNames = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
        {
            { "naxxramas", new List<string> { "Patchwerk", "Noth the Plaguebringer", "Heigan the Unclean", "Loatheb", "Anub'Rekhan", "Grand Widow Faerlina", "Maexxna", "Instructor Razuvious", "Gothik the Harvester","Gluth", "Grobbulus", "Thaddius", "The Four Horsemen", "Thane Korth'azz", "Baron Rivendare", "Lady Blaumeux", "Sir Zeliek" } },
            { "the upper necropolis", new List<string> { "Sapphiron", "Kel'Thuzad"  } },
            { "ahn'qiraj", new List<string> {
                "The Prophet Skeram",
                "Bug Trio",
                "Vem",
                "Yauj",
                "Kri",
                "Battleguard Sartura",
                "Fankriss the Unyielding",
                "Viscidus",
                "Princess Huhuran",
                "Twin Emperors",
                "Emperor Vek'lor",
                "Emperor Vek'nilash",
                "Ouro",
                "C'Thun"
            }},
            {"tower of karazhan", new List<string> {
                "Keeper Gnarlmoon",
                "Ley-Watcher Incantagos",
                "Anomalus",
                "Arcane Warden",
                "Mana Confluence",
                "Echo of Medivh",
                "Shadow Councilor Yath'amon",
                "The Curator (Upper)",
                "Viz'aduum the Watcher"
            } }
        };
}
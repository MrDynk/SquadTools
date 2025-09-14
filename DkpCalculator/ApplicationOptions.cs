using System.Collections.Generic;
public class ApplicationOptions
{
    public int ColumnsRemovedFromAuditSheet { get; set; }
    public int auditSheetAdditionalDkpRow { get; set; }
    public int auditRowPlayerIndexOffset { get; set; }
    public int raidDateRowIndex { get; set; }
    public int RaidIdRowIndex { get; set; }
    public int PlayerRosterColumnIndex { get; set; }
    public int FirstPlayerRowIndex { get; set; }
    public int MonthlyEarnedDkpColumnIndex { get; set; }
    public int MonthlySpentDkpColumnIndex { get; set; }
    public int AvailableDkpColumnIndex { get; set; }
    public int InactivityThresholdMinutes { get; set; }
    public int DeathOnFinalBossBuffer { get; set; }
    public string? DKPSpreadSheetName { get; set; }
    public Dictionary<string, string>? ZoneToAbbrevLookup { get; set; }
    public Dictionary<string, int>? DkpPotential { get; set; }
    public Dictionary<string, List<string>>? bossNames { get; set; }
}
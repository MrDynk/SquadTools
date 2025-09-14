using System.Text;
using Google.Apis.Sheets.v4.Data;

public class AuditSheetRepository
{
    private readonly ApplicationOptions _options;
    private readonly ValueRange _auditSheet;

    public AuditSheetRepository(ValueRange auditSheet, ApplicationOptions options)
    {
        _auditSheet = auditSheet;
        _options = options;
    }
    public void Update(SquadSheetContext context)
    {
        var sb = new StringBuilder();
    var raidColumnInAuditSheet = context.RaidColumnIdx - _options.ColumnsRemovedFromAuditSheet;
        if (context.RaidDkpAwardedByLeadership > 0)
        {
            var auditSheetAdditionalDkpRow = _options.auditSheetAdditionalDkpRow;
            var row = _auditSheet.Values[auditSheetAdditionalDkpRow];

            if (row.Count < raidColumnInAuditSheet)
            {
                // If the row doesn't have enough columns, add empty cells
                for (int j = row.Count; j <= raidColumnInAuditSheet; j++)
                {
                    row.Add(string.Empty);
                }
            }
            row[raidColumnInAuditSheet] = context.RaidDkpAwardedByLeadership;
        }

        foreach (var player in context.SquadPlayers)
        {
            /*if (player.FatLoot.Count > 0) sb.AppendLine("Loot:");
             foreach (var loot in player.FatLoot)
             {
                 sb.AppendLine($"    {loot.Item}");
             }
             */
            if (player.DkpDeductions.Count > 0) sb.AppendLine("Deduct:");
            foreach (var deduction in player.DkpDeductions)
            {
                //sb.AppendLine($"Reason:");
                sb.AppendLine($"==========");
                sb.AppendLine($"{deduction.Reason.ToString()}");
                //sb.AppendLine($"TimeStamps:");
                sb.AppendLine($"{string.Join(", ", deduction.RelatedTimeStamps.Select(ts => ts.ToString("HH:mm")))}");
                //sb.AppendLine($"Amount:");
                sb.AppendLine($"{deduction.Amount}");
                sb.AppendLine($"==========");
            }
            if (player.LeadershipAwardedDkpList.FirstOrDefault(x => x.Reason == AwardEnum.PlayerAddition) != null) sb.AppendLine("Awarded:");
            foreach (var award in player.LeadershipAwardedDkpList)
            {
                if (award.Reason != AwardEnum.RaidAddition)
                {
                    //sb.AppendLine($"Reason:");
                    sb.AppendLine($"==========");
                    sb.AppendLine($"{award.Reason.ToString()}");
                    sb.AppendLine($"{award.Amount}");
                    sb.AppendLine($"==========");
                }

            }
            var row = _auditSheet.Values[player.SquadSheetLocation + _options.auditRowPlayerIndexOffset];
            if (row == null) continue;

            
            if (row.Count < raidColumnInAuditSheet)
            {
                // If the row doesn't have enough columns, add empty cells
                for (int j = row.Count; j <= raidColumnInAuditSheet; j++)
                {
                    row.Add(string.Empty);
                }
            }

            row[raidColumnInAuditSheet] = sb.ToString();
            sb.Clear();
        }

    }
}
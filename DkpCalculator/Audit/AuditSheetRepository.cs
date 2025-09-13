using System.Text;
using Google.Apis.Sheets.v4.Data;

public class AuditSheetRepository
{
    private readonly ValueRange _auditSheet;

    public AuditSheetRepository(ValueRange auditSheet)
    {
        _auditSheet = auditSheet;
    }
    public void Update(SquadSheetContext context)
    {
        var sb = new StringBuilder();

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
            var row = _auditSheet.Values[player.SquadSheetLocation];
            if (row == null) continue;

            var raidColumnInAuditSheet = context.RaidColumnIdx - ApplicationOptions.RowsRemovedFromAuditSheet;
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
using System.Text;
using System.IO;

public class AuditLogRepository
{
    public void Write(SquadSheetContext context)
    {
    string AuditFileName = $"AuditLog_{context.RaidStart:yyyyMMdd_HHmmss}_{context.BossesDefeated[0].Zone}.txt";
        var sb = new StringBuilder();

        foreach (var player in context.SquadPlayers)
        {
            sb.AppendLine("===================================");
            sb.AppendLine("Raider:");
            sb.AppendLine($"    {string.Join(", ", player.PlayerAliases)}");
            sb.AppendLine("DKP Stats:");
            sb.AppendLine($"    Available DKP: {player.AvailableDkp}");
            sb.AppendLine($"    DKP Earned in current Month: {player.MonthlyEarnedDkp}");
            sb.AppendLine($"    DKP Spent in current Month: {player.MonthlySpentDkp}");
            sb.AppendLine($"    DKP Earned in raid: {player.RaidEarnedDkp}");

            if (player.FatLoot.Count > 0) sb.AppendLine("Loot:");
            foreach (var loot in player.FatLoot)
            {
                sb.AppendLine($"    Time: {loot.TimeStamp:HH:mm:ss}, Item: {loot.Item}");
            }
            if (player.Deaths.Count > 0) sb.AppendLine("Deaths:");
            foreach (var death in player.Deaths)
            {
                sb.AppendLine($"    {death:HH:mm:ss}");
            }
           if (player.DkpDeductions.Count > 0) sb.AppendLine("DKP Deductions:");
            foreach (var deduction in player.DkpDeductions)
            {
                sb.AppendLine($"Reason:");
                sb.AppendLine($"{deduction.Reason.ToString()}");
                sb.AppendLine($"Amount:");
                sb.AppendLine($"{deduction.Amount}");
                sb.AppendLine($"TimeStamps:");
                sb.AppendLine($"{string.Join(", ", deduction.RelatedTimeStamps.Select(ts => ts.ToString("HH:mm")))}");
            }
        }
        sb.AppendLine("===================================");
        sb.AppendLine("Boss Kills:");
        foreach (var boss in context.BossesDefeated)
        {
            sb.AppendLine($"    {boss.Name} at {boss.KillTime:HH:mm:ss}");
        }

        File.WriteAllText($"Testfiles\\{AuditFileName}", sb.ToString());
    }
}
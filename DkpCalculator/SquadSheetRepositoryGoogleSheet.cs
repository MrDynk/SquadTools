using System;
using System.Collections.Generic;
using System.Linq;
using Google.Apis.Sheets.v4.Data;

namespace SquadSheets
{
    public class SquadSheetRepositoryGoogleSheet: ISquadSheetRepository
    {

        private readonly Spreadsheet _squadSheet;

        public SquadSheetRepositoryGoogleSheet(Spreadsheet squadSheet)
        {
            _squadSheet = squadSheet;
        }


        public void GetRosterDetails(SquadSheetContext context)
        {
            var monthAbv = context.RaidStart.ToString("MMM");
            string year = context.RaidStart.ToString("yy");
            // Find the sheet whose title contains the month and year
            var sheet = _squadSheet.Sheets.FirstOrDefault(s =>
                s.Properties.Title.Contains(monthAbv, StringComparison.OrdinalIgnoreCase) &&
                s.Properties.Title.Contains(year));
            if (sheet == null)
                throw new Exception($"No sheet found with name containing '{monthAbv}' and '{year}'");

            var values = sheet.Data?.FirstOrDefault()?.RowData;
            if (values == null)
                throw new Exception("No data found in the sheet.");

            for (int i = 1; i < values.Count; i++) // skip header
            {
                var row = values[i]?.Values;
                if (row == null || row.Count <= ApplicationOptions.PlayerRosterColumnIndex)
                    break;
                string playerName = row[ApplicationOptions.PlayerRosterColumnIndex]?.FormattedValue;
                if (string.IsNullOrEmpty(playerName))
                    break;
                var monthlySpentDkp = row.ElementAtOrDefault(ApplicationOptions.MonthlySpentDkpColumnIndex)?.FormattedValue;
                var availableDkp = row.ElementAtOrDefault(ApplicationOptions.AvailableDkpColumnIndex)?.FormattedValue;
                var monthlyEarnedDkp = row.ElementAtOrDefault(ApplicationOptions.MonthlyEarnedDkpColumnIndex)?.FormattedValue;

                Player player = new Player
                {
                    SquadSheetLocation = i,
                    AvailableDkp = string.IsNullOrEmpty(availableDkp) ? 0 : int.Parse(availableDkp),
                    MonthlySpentDkp = string.IsNullOrEmpty(monthlySpentDkp) ? 0 : int.Parse(monthlySpentDkp),
                    MonthlyEarnedDkp = string.IsNullOrEmpty(monthlyEarnedDkp) ? 0 : int.Parse(monthlyEarnedDkp),
                    FatLoot = new List<Loot>(),
                    AliasTimeStamps = new Dictionary<string, List<DateTime>>(),
                    ActivityGaps = new List<PlayerActivityGap>(),
                    DkpDeductions = new List<Tuple<string, int>>(),
                    RaidEarnedDkp = context.PotentialDkpEarnedForRaid
                };
                List<string> playerAliases = playerName.Split('/').Select(a => a.Trim()).ToList();
                foreach (var alias in playerAliases)
                    player.AliasTimeStamps.Add(alias, new List<DateTime>());

                context.SquadPlayers.Add(player);
            }
        }

        public void UpdateDkp(SquadSheetContext context)
        {
            var monthAbv = context.RaidStart.ToString("MMM");
            string year = context.RaidStart.ToString("yy");
            // Find the sheet whose title contains the month and year
            var sheet = _squadSheet.Sheets.FirstOrDefault(s =>
                s.Properties.Title.Contains(monthAbv, StringComparison.OrdinalIgnoreCase) &&
                s.Properties.Title.Contains(year));
            if (sheet == null)
                throw new Exception($"No sheet found with name containing '{monthAbv}' and '{year}'");

            var values = sheet.Data?.FirstOrDefault()?.RowData;
            if (values == null)
                throw new Exception("No data found in the sheet.");

            for (int i = 0; i < context.SquadPlayers.Count; i++)
            {
                var player = context.SquadPlayers[i];
                int rowIndex = player.SquadSheetLocation;
                var row = values.ElementAtOrDefault(rowIndex)?.Values;
                if (row == null) continue;

                SetCellValue(row, ApplicationOptions.AvailableDkpColumnIndex, player.AvailableDkp.ToString());
                SetCellValue(row, ApplicationOptions.MonthlyEarnedDkpColumnIndex, player.MonthlyEarnedDkp.ToString());
                SetCellValue(row, ApplicationOptions.MonthlySpentDkpColumnIndex, player.MonthlySpentDkp.ToString());
                if (context.RaidColumn != -1)
                    SetCellValue(row, context.RaidColumn, player.RaidEarnedDkp.ToString());
            }
        }


        // Helper to set a cell value in a row (colIndex is 0-based)
        private void SetCellValue(IList<CellData> row, int colIndex, string value)
        {
            if (row.Count <= colIndex)
            {
                // Expand the row if needed
                while (row.Count <= colIndex)
                    row.Add(new CellData());
            }
            row[colIndex].UserEnteredValue = new ExtendedValue { StringValue = value };
        }

    }
}
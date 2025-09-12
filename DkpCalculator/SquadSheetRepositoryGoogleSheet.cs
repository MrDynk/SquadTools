using System;
using System.Collections.Generic;
using System.Linq;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace SquadSheets
{
    public class SquadSheetRepositoryGoogleSheet : ISquadSheetRepository
    {

        private readonly ValueRange _squadSheet;

        public SquadSheetRepositoryGoogleSheet(ValueRange squadSheet)
        {
            _squadSheet = squadSheet;
        }


        public void PopulateRaidDetails(SquadSheetContext context)
        {
            if (_squadSheet.Values == null || _squadSheet.Values.Count == 0)
                throw new Exception("No data found in the sheet.");

            IList<object> headerRow =   _squadSheet.Values[0];
            context.RaidColumn = FindRaidColumn(context, headerRow);
        }

        public void GetRosterDetails(SquadSheetContext context)
        {
            if (_squadSheet.Values == null)
                throw new Exception("No data found in the sheet.");

            for (int i = 1; i < _squadSheet.Values.Count; i++) // skip header
            {
                var row = _squadSheet.Values[i];
                if (row == null || row.Count <= ApplicationOptions.PlayerRosterColumnIndex)
                    break;
                string playerName = row[ApplicationOptions.PlayerRosterColumnIndex].ToString();
                if (string.IsNullOrEmpty(playerName))
                    break;
                var monthlySpentDkp = row.ElementAtOrDefault(ApplicationOptions.MonthlySpentDkpColumnIndex)?.ToString();
                var availableDkp = row.ElementAtOrDefault(ApplicationOptions.AvailableDkpColumnIndex)?.ToString();
                var monthlyEarnedDkp = row.ElementAtOrDefault(ApplicationOptions.MonthlyEarnedDkpColumnIndex)?.ToString();

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

            for (int i = 0; i < context.SquadPlayers.Count; i++)
            {
                var player = context.SquadPlayers[i];
                var row = _squadSheet.Values[player.SquadSheetLocation];
                if (row == null) continue;

                row[ApplicationOptions.AvailableDkpColumnIndex] = player.AvailableDkp.ToString();
                row[ApplicationOptions.MonthlyEarnedDkpColumnIndex] = player.MonthlyEarnedDkp.ToString();
                row[ApplicationOptions.MonthlySpentDkpColumnIndex] = player.MonthlySpentDkp.ToString();
                if (row.Count < context.RaidColumn)
                {
                    // If the row doesn't have enough columns, add empty cells
                    for (int j = row.Count; j <= context.RaidColumn; j++)
                    {
                        row.Add(string.Empty);
                    }
                }

                row[context.RaidColumn] = player.RaidEarnedDkp.ToString();
            }
        }

    private int FindRaidColumn(SquadSheetContext context, IList<object> headerRow)
        {
            var zonesInLog = context.ZoneInfo.Select(z => z.Item2).Distinct().ToList();
            var monthDay = context.RaidStart.ToString("M/d");
            for (int col = 0; col < headerRow.Count; col++)
            {
                foreach (var zone in zonesInLog)
                {
                    if (!ApplicationOptions.ZoneToAbbrevLookup.TryGetValue(zone, out var abbrev))
                    {
                        continue;
                    }
                    var columnTitle = headerRow[col]?.ToString();
                    if (columnTitle.Contains(abbrev, StringComparison.OrdinalIgnoreCase) && columnTitle.Contains(monthDay, StringComparison.OrdinalIgnoreCase))
                    {
                        return col;
                    }
                }

            }
            return -1; // Not found
        }
    }
}
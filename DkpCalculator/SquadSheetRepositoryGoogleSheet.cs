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

            IList<object> dateHeaderRow =   _squadSheet.Values[0];
            var dateColumnValue = context.RaidStart.ToString("M/d/yyyy");
            List<int> matchingDateColumns = dateHeaderRow.Where(cell => cell.ToString().Contains(dateColumnValue)).Select(cell => dateHeaderRow.IndexOf(cell)).ToList();
            IList<object> RaidNameHeaderRow =   _squadSheet.Values[1];

            context.RaidColumnIdx = FindRaidColumn(context,  RaidNameHeaderRow, matchingDateColumns);
        }

        public void GetRosterDetails(SquadSheetContext context)
        {
            if (_squadSheet.Values == null)
                throw new Exception("No data found in the sheet.");

            for (int i = ApplicationOptions.FirstPlayerRowIndex; i < _squadSheet.Values.Count; i++) 
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
                    DkpDeductions = new List<DkpDeduction>(),
                    RaidEarnedDkp = 0,
                    LeadershipAwardedDkpList = new List<LeadershipAwardedDkp>(),
                };

                if (context.RaidDkpAwardedByLeadership > 0)
                { 
                    player.LeadershipAwardedDkpList.Add(new LeadershipAwardedDkp
                    {
                        Reason = AwardEnum.RaidAddition,
                        Amount = context.RaidDkpAwardedByLeadership
                    });
                }

                List<string> playerAliases = playerName.Split('/').Select(a => a.ToLower().Trim()).ToList();
                foreach (var alias in playerAliases)
                {
                    player.AliasTimeStamps.Add(alias, new List<DateTime>());
                    if(context.PlayerDkpAwardedByLeadership != null && context.PlayerDkpAwardedByLeadership.TryGetValue(alias, out var dkpFromLeadership))
                    {
                        player.LeadershipAwardedDkpList.Add(new LeadershipAwardedDkp
                        {
                            Reason = AwardEnum.PlayerAddition,
                            Amount = dkpFromLeadership
                        });
                    }
                }

                context.SquadPlayers.Add(player);
            }
        }

        public void UpdateDkp(SquadSheetContext context)
        {

            foreach (var player in context.SquadPlayers)
            {
                var row = _squadSheet.Values[player.SquadSheetLocation];
                if (row == null) continue;

                row[ApplicationOptions.AvailableDkpColumnIndex] = player.AvailableDkp.ToString();
                row[ApplicationOptions.MonthlyEarnedDkpColumnIndex] = player.MonthlyEarnedDkp.ToString();
                row[ApplicationOptions.MonthlySpentDkpColumnIndex] = player.MonthlySpentDkp.ToString();
                //raidColumnIdx is 1-based
                 int totalRows = row.Count;
                int lastRowIdx = totalRows - 1;
                if (lastRowIdx < context.RaidColumnIdx)
                {
                    // If the row doesn't have enough columns, add empty cells, raidColumnIdx is 1-based
                    
                    // if (lastRowIdx < context.RaidColumnIdx)
                    // {
                    //     Console.WriteLine($"Extending row for player {player.PlayerAliases[0]} from {totalRows} to {context.RaidColumnIdx} columns.");
                    // }

                    for (int j = lastRowIdx; j < context.RaidColumnIdx; j++)
                    {
                        row.Add(string.Empty);
                    }
                }

                row[context.RaidColumnIdx] = player.RaidEarnedDkp.ToString();
            }
        }

    private int FindRaidColumn(SquadSheetContext context, IList<object> headerRow, List<int> matchingDateColumns)
        {
            var zonesInLog = context.ZoneInfo.Select(z => z.Item2).Distinct().ToList();
            
            foreach (var colIndex in matchingDateColumns)
            {
                foreach (var zone in zonesInLog)
                {
                    if (!ApplicationOptions.ZoneToAbbrevLookup.TryGetValue(zone, out var abbrev))
                    {
                        continue;
                    }
                    var columnTitle = headerRow[colIndex]?.ToString();
                    if (columnTitle.Contains(abbrev, StringComparison.OrdinalIgnoreCase))
                    {
                        return colIndex;
                    }
                }
            }   
            return -1; // Not found
        }
    }
}
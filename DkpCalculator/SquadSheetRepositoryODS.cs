using System.Data;
using Zaretto.ODS;
namespace SquadSheets
{
    public class SquadSheetRepositoryODS : ISquadSheetRepository
    {
        private string squadSheetPath;
        private int dkpTableIndex = 0;
        private DataTable dkpTable;

        public SquadSheetRepositoryODS(string squadSheetPath, SquadSheetContext context)
        {
            this.squadSheetPath = squadSheetPath;
            var odsReaderWriter = new ODSReaderWriter();
            var monthAbv = context.RaidStart.ToString("MMM");
            try
            {
                var spreadsheetData = odsReaderWriter.ReadOdsFile(squadSheetPath);
                while (!spreadsheetData.Tables[dkpTableIndex].TableName.Contains(monthAbv) && dkpTableIndex < spreadsheetData.Tables.Count - 1)
                {
                    dkpTableIndex++;
                }

                //spreadsheetData.Tables.
                dkpTable = spreadsheetData.Tables[dkpTableIndex];
                Console.WriteLine($"Sheet Found: {dkpTable.TableName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading ODS file: {0}", ex.Message);
            }

        }

        // Retrieves squad sheet data and updates the context
        public void GetRosterDetails(SquadSheetContext context)
        {

            context.SquadPlayers = new List<Player>();
            for (int i = 1; i < dkpTable.Rows.Count; i++)
            {
                var row = dkpTable.Rows[i];
                var playerName = row[SquadSheetContext.PlayerRosterColumn]?.ToString();
                if (string.IsNullOrEmpty(playerName))
                {
                    break;                    
                }
                    //context.SquadSheetPlayerRoster.Add(new Tuple<int, string>(i, playerName));

                var dkpSpent = row[SquadSheetContext.DkpSpentColumn]?.ToString();
                var currentDkp = row[SquadSheetContext.CurrentDkpColumn]?.ToString();

                Player player = new Player
                {
                    SquadSheetLocation = i,
                    TotalDkp = string.IsNullOrEmpty(currentDkp) ? 0 : int.Parse(currentDkp),
                    EarnedDkp = 0,
                    FatLoot = new List<Loot>(),
                    AliasTimeStamps = new Dictionary<string, List<DateTime>>()

                };
                List<string> playerAliases = playerName.Split('/').Select(a => a.Trim()).ToList();
                foreach (var alias in playerAliases)
                    player.AliasTimeStamps.Add(alias, new List<DateTime>());

                context.SquadPlayers.Add(player);
            }  
        }
        

        public void UpdateDkp(SquadSheetContext context)
        {

        }
    }
}
using System.Data;
using Zaretto.ODS;
using System.Text; 
namespace SquadSheets
{
    public class SquadSheetRepositoryZaretto : ISquadSheetRepository
    {
        private string squadSheetPath;
        private int dkpTableIndex = 0;
        private DataTable dkpTable;

        //private ODSReaderWriter _odsReaderWriter;
        //DataSet _spreadsheetData;

        public SquadSheetRepositoryZaretto(string squadSheetPath, SquadSheetContext context)
        {

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            this.squadSheetPath = squadSheetPath;
            var _odsReaderWriter = new ODSReaderWriter();
            var monthAbv = context.RaidStart.ToString("MMM");
            try
            {
                var _spreadsheetData = _odsReaderWriter.ReadOdsFile(squadSheetPath);
                while (!_spreadsheetData.Tables[dkpTableIndex].TableName.Contains(monthAbv) && dkpTableIndex < _spreadsheetData.Tables.Count - 1)
                {
                    dkpTableIndex++;
                }

                //spreadsheetData.Tables.
                dkpTable = _spreadsheetData.Tables[dkpTableIndex];
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
                var playerName = row[ApplicationOptions.PlayerRosterColumnIndex]?.ToString();
                if (string.IsNullOrEmpty(playerName))
                {
                    break;                    
                }
                    //context.SquadSheetPlayerRoster.Add(new Tuple<int, string>(i, playerName));

                var dkpSpent = row[ApplicationOptions.MonthlySpentDkpColumnIndex]?.ToString();
                var currentDkp = row[ApplicationOptions.AvailableDkpColumnIndex]?.ToString();

                Player player = new Player
                {
                    SquadSheetLocation = i,
                    AvailableDkp = string.IsNullOrEmpty(currentDkp) ? 0 : int.Parse(currentDkp),
                    MonthlyEarnedDkp = 0,
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
            var _odsReaderWriter = new ODSReaderWriter();
            var monthAbv = context.RaidStart.ToString("MMM");
            foreach (var player in context.SquadPlayers)
            {
                var row = dkpTable.Rows[player.SquadSheetLocation];
                row[ApplicationOptions.AvailableDkpColumnIndex] = -1;
            }
            //squad file is a fucking bemoth of trash we can't write the whole thing unless people have 60gb of ram jesus christ guys
            //_odsReaderWriter.WriteOdsFile(_spreadsheetData, squadSheetPath);


            //lets try just the sheet
            
            DataRowCollection rows = dkpTable.Rows;
            IEnumerable<DataRow> collection = rows.Cast<DataRow>();
            var dictionary = new Dictionary<string, IEnumerable<DataRow>> {
                { dkpTable.TableName, collection },
            };
            _odsReaderWriter.WriteOdsFile(dictionary, "test-d.ods");
            
        }
    }
}
using System;
using System.IO;
using System.Diagnostics;
using System.Data;
using ExcelDataReader;
using Microsoft.Win32;
using ClosedXML.Excel;

namespace SquadSheets
{
    public class SquadSheetRepositoryXLSX : ISquadSheetRepository
    {

        private readonly string _xlsxFilePath;

        public SquadSheetRepositoryXLSX(string xlsxFilePath)
        {
            _xlsxFilePath = xlsxFilePath;
        }


        public void GetRosterDetails(SquadSheetContext context)
        {

            if (!File.Exists(_xlsxFilePath))
                throw new FileNotFoundException($"XLSX file not found: {_xlsxFilePath}");

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            var monthAbv = context.RaidStart.ToString("MMM");
            string year = context.RaidStart.ToString("yy");

            using (var stream = File.Open(_xlsxFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                DataSet result = reader.AsDataSet();
                DataTable table = null;
                foreach (DataTable t in result.Tables)
                {
                    if (t.TableName != null && t.TableName.Contains(monthAbv, StringComparison.OrdinalIgnoreCase) && t.TableName.Contains(year))
                    {
                        table = t;
                        break;
                    }
                }
                if (table == null)
                    throw new Exception($"No table found with name containing '{monthAbv}'");


                for (int i = 1; i < table.Rows.Count; i++) // skip header
                {
                    var row = table.Rows[i];
                    string playerName = row[ApplicationOptions.PlayerRosterColumnIndex]?.ToString();
                    if (string.IsNullOrEmpty(playerName))
                    {
                        break;
                    }
                    var monthlySpentDkp = row[ApplicationOptions.MonthlySpentDkpColumnIndex]?.ToString();
                    var availableDkp = row[ApplicationOptions.AvailableDkpColumnIndex]?.ToString();
                    var monthlyEarnedDkp = row[ApplicationOptions.MonthlyEarnedDkpColumnIndex]?.ToString();

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
        }

        public void UpdateDkp(SquadSheetContext context)
        {
            if (!File.Exists(_xlsxFilePath))
                throw new FileNotFoundException($"XLSX file not found: {_xlsxFilePath}");


            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var monthAbv = context.RaidStart.ToString("MMM");
            string year = context.RaidStart.ToString("yy");

            // Read the XLSX file
            DataSet result;
            using (var stream = File.Open(_xlsxFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                result = reader.AsDataSet();
            }

            //find table with 3rd party library since openXML is fucked.
            string targetTableName = null;
            DataTable dkpTable = null;
            foreach (DataTable t in result.Tables)
            {
                if (t.TableName != null && t.TableName.Contains(monthAbv, StringComparison.OrdinalIgnoreCase) && t.TableName.Contains(year))
                {
                    targetTableName = t.TableName;
                    dkpTable = t;
                    break;
                }
            }

            //find the index using the 3rd party library because openXML is fucked.
            context.RaidColumn = FindRaidColumn(context, dkpTable);


            if (targetTableName == null)
                throw new Exception($"No table found with name containing '{monthAbv}' and '{year}'");

            using (var document = DocumentFormat.OpenXml.Packaging.SpreadsheetDocument.Open(_xlsxFilePath, true))
            {
                var workbookPart = document.WorkbookPart;
                var sheet = workbookPart.Workbook.Sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>()
                    .FirstOrDefault(s => s.Name == targetTableName);
                if (sheet == null)
                    throw new Exception($"Worksheet '{targetTableName}' not found in XLSX.");


                var worksheetPart = (DocumentFormat.OpenXml.Packaging.WorksheetPart)workbookPart.GetPartById(sheet.Id);
                var sheetData = worksheetPart.Worksheet.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.SheetData>();

                //find row column
                int RaidIdRowIndexOXml = ApplicationOptions.RaidIdRowIndex;
                var RaidIdRow = sheetData.Elements<DocumentFormat.OpenXml.Spreadsheet.Row>().FirstOrDefault(r => r.RowIndex == (uint)RaidIdRowIndexOXml);

                for (int i = 0; i < context.SquadPlayers.Count; i++)
                {
                    var player = context.SquadPlayers[i];
                    int rowIndex = player.SquadSheetLocation + 1; // DataTable is 0-based, OpenXML is 1-based
                    var row = sheetData.Elements<DocumentFormat.OpenXml.Spreadsheet.Row>().FirstOrDefault(r => r.RowIndex == (uint)rowIndex);
                    if (row == null) continue;

                    // Update columns as needed, e.g. AvailableDkp, MonthlyEarnedDkp, MonthlySpentDkp, RaidEarnedDkp
                    // You must know the correct column indices (A=1, B=2, etc.)
                    UpdateCell(row, ApplicationOptions.AvailableDkpColumnIndex + 1, player.AvailableDkp.ToString());
                    UpdateCell(row, ApplicationOptions.MonthlyEarnedDkpColumnIndex + 1, player.MonthlyEarnedDkp.ToString());
                    UpdateCell(row, ApplicationOptions.MonthlySpentDkpColumnIndex + 1, player.MonthlySpentDkp.ToString());
                    if (context.RaidColumn != -1)
                        UpdateCell(row, context.RaidColumn + 1, player.RaidEarnedDkp.ToString());
                }
                worksheetPart.Worksheet.Save();
            }
        }


            // Helper to update a cell in a row (colIndex is 1-based)
            private void UpdateCell(DocumentFormat.OpenXml.Spreadsheet.Row row, int colIndex, string value)
        {
            string cellRef = GetExcelColumnName(colIndex) + row.RowIndex;
            var cell = row.Elements<DocumentFormat.OpenXml.Spreadsheet.Cell>().FirstOrDefault(c => c.CellReference == cellRef);
            if (cell == null)
            {
                cell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = cellRef };
                row.Append(cell);
            }
            cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
            cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(value);
        }

        // Helper to get Excel column name from index (1-based)
        private string GetExcelColumnName(int columnNumber)
        {
            string columnName = "";
            while (columnNumber > 0)
            {
                int modulo = (columnNumber - 1) % 26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                columnNumber = (columnNumber - modulo) / 26;
            }
            return columnName;
        }

        private int FindRaidColumn(SquadSheetContext context, DataTable table)
        {
            var zonesInLog = context.ZoneInfo.Select(z => z.Item2).Distinct().ToList();
            var monthDay = context.RaidStart.ToString("M/d");
            for (int col = 0; col < table.Columns.Count; col++)
            {
                foreach (var zone in zonesInLog)
                {
                    if (!ApplicationOptions.ZoneToAbbrevLookup.TryGetValue(zone, out var abbrev))
                    {
                        continue;
                    }
                    var columnTitle = table.Rows[ApplicationOptions.RaidIdRowIndex][col]?.ToString();
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
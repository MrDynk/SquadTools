using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.IO;
using System.Linq;
using System.Threading;
using System.CodeDom.Compiler;

public class GoogleSheetRepository
{
	private readonly string[] Scopes = { DriveService.Scope.DriveFile, DriveService.Scope.DriveReadonly,DriveService.Scope.DriveAppdata,SheetsService.Scope.Spreadsheets };
	private readonly string ApplicationName = "SquadToolsDkp";
	private readonly Google.Apis.Drive.v3.Data.File? _file;
	private SheetsService _sheetsService;

	//private string _sheetTitle;
	//private string _spreadsheetId;
    private List<string> _fileNames;
    private readonly bool _useServiceAccount = true; // Set to false to use OAuth
	private DriveService GetDriveService()
	{
		if (_useServiceAccount)
		{
			GoogleCredential credential;
			using (var stream = new FileStream("credentials/service-account.json", FileMode.Open, FileAccess.Read))
			{
				credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
			}
			return new DriveService(new BaseClientService.Initializer()
			{
				HttpClientInitializer = credential,
				ApplicationName = ApplicationName,
			});
		}
		else
		{
			UserCredential credential;
			using (var stream = new FileStream("credentials/credentials.json", FileMode.Open, FileAccess.Read))
			{
				string credPath = "token.json";
				credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
					GoogleClientSecrets.FromStream(stream).Secrets,
					Scopes,
					"user",
					CancellationToken.None,
					new FileDataStore(credPath, true)).Result;
			}
			return new DriveService(new BaseClientService.Initializer()
			{
				HttpClientInitializer = credential,
				ApplicationName = ApplicationName,
			});
		}
	}

    public GoogleSheetRepository()
    {
        var driveService = GetDriveService();
        var request = driveService.Files.List();
        request.Q = $"trashed=false";
        request.Fields = "files(id, name)";
        var result = request.Execute();
        // get the list of files these credentials have access to
        _fileNames = result.Files.Select(f => f.Name).ToList();
       /* foreach (var f in result.Files)
        {
            Console.WriteLine($"- {f.Name}");
        }
		*/

        // Use Sheets API to get the spreadsheet object
        _sheetsService = new SheetsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = driveService.HttpClientInitializer,
            ApplicationName = ApplicationName,
        });
        
               	
        request = driveService.Files.List();
		request.Q = $"name='{ApplicationOptions.DKPSpreadSheetName}' and mimeType='application/vnd.google-apps.spreadsheet' and trashed=false";
		request.Fields = "files(id, name)";
		 result = request.Execute();

		_file = result.Files?.FirstOrDefault();
		if (_file == null)
			throw new FileNotFoundException($"Google Sheet '{ApplicationOptions.DKPSpreadSheetName}' not found in Google Drive.");          
	}

	public ValueRange DownloadGoogleSheet(List<string> sheetTokens, SquadSheetContext context)
	{

		var getRequest = _sheetsService.Spreadsheets.Get(_file.Id);
		Spreadsheet spreadsheet = getRequest.Execute();


		// Find the sheet whose title contains all of the sheetTokens
		Sheet? sheet = spreadsheet.Sheets.FirstOrDefault(s =>
			sheetTokens.All(token => s.Properties.Title.Contains(token, StringComparison.OrdinalIgnoreCase)));

		if (sheet == null)
			throw new Exception($"No sheet found containing all tokens: {string.Join(", ", sheetTokens)}");
		///find the sheet whose title contains all of the sheetTokens        
        
		if (sheet == null)
			throw new Exception($"No sheet found containing all tokens: {string.Join(", ", sheetTokens)}");
		//Console.WriteLine($"Sheet Title: {sheet.Properties.Title}, Sheet ID: {sheet.Properties.SheetId}");

		var sheetTitle = sheet.Properties.Title;
		var spreadsheetId = spreadsheet.SpreadsheetId;
		//goofy shit "R1C1:R328C61
		ValueRange data = _sheetsService.Spreadsheets.Values.Get(spreadsheetId, sheetTitle).Execute();


		// foreach (var row in data.Values)
		// {
		// 	for (int i = 0; i < row.Count; i++)
		// 	{
		// 		Console.Write($"'{row[i]}' ||");
		// 	}
		// }
		return data;
	}

//spreadsheet is the whole file sheet is the specific tab
    public void UpdateGoogleSheet(List<string> sheetTokens, SquadSheetContext context, ValueRange updatedSheet)
    {
        var getRequest = _sheetsService.Spreadsheets.Get(_file.Id);
        Spreadsheet spreadsheet = getRequest.Execute();


        // Find the sheet whose title contains all of the sheetTokens
        Sheet? sheet = spreadsheet.Sheets.FirstOrDefault(s =>
            sheetTokens.All(token => s.Properties.Title.Contains(token, StringComparison.OrdinalIgnoreCase)));

        // Calculate correct range based on data shape
        int rowCount = updatedSheet.Values?.Count ?? 0;
        int colCount = updatedSheet.Values?.Max(r => r.Count) ?? 0;
        if (rowCount == 0 || colCount == 0)
        {
            Console.WriteLine("No data to update.");
            return;
        }
        string endCol = GetExcelColumnName(colCount);
        string sheetRange = $"{sheet.Properties.Title}!A1:{endCol}{rowCount}";
        updatedSheet.Range = sheetRange;

        var updateRequest = _sheetsService.Spreadsheets.Values.Update(updatedSheet, spreadsheet.SpreadsheetId, sheetRange);
        updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
        var updateResponse = updateRequest.Execute();
        Console.WriteLine($"Updated {updateResponse.UpdatedCells} cells in range {sheetRange} of spreadsheet {spreadsheet.SpreadsheetId}.");
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


}

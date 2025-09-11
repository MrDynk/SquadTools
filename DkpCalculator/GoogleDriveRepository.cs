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

public class GoogleDriveRepository
{
	private readonly string[] Scopes = { DriveService.Scope.DriveFile, DriveService.Scope.DriveReadonly };
	private readonly string ApplicationName = "SquadToolsDkp";

	private DriveService GetDriveService()
	{
		UserCredential credential;
		using (var stream = new FileStream("credentials\\credentials.json", FileMode.Open, FileAccess.Read))
		{
			string credPath = "token.json";
			credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
				GoogleClientSecrets.Load(stream).Secrets,
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

	public void UploadFileSharedWithMe(string fileName, string filePath)
	{
		var service = GetDriveService();
		// Search for the file in 'sharedWithMe' space
		var listRequest = service.Files.List();
		listRequest.Q = $"name='{fileName}' and trashed=false";
		listRequest.Spaces = "drive";
		listRequest.Fields = "files(id, name)";
		listRequest.Corpora = "user";
		listRequest.IncludeItemsFromAllDrives = true;
		listRequest.SupportsAllDrives = true;
		var result = listRequest.Execute();
		var sharedFile = result.Files?.FirstOrDefault();
		if (sharedFile == null)
		{
			Console.WriteLine($"File '{fileName}' not found in 'Shared with me'.");
			return;
		}

		// Update the content of the shared file (if you have edit permissions)
		using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
		{
			try
			{
			var updateRequest = service.Files.Update(new Google.Apis.Drive.v3.Data.File(), sharedFile.Id, fileStream, "application/octet-stream");
			updateRequest.Fields = "id";
			updateRequest.SupportsAllDrives = true;
			Console.WriteLine($"Updating content of shared file '{fileName}' (ID: {sharedFile.Id})...");
			var uploadResult = updateRequest.Upload();
				if (uploadResult.Status == Google.Apis.Upload.UploadStatus.Completed)
				{
					Console.WriteLine($"Update successful. File ID: {updateRequest.ResponseBody?.Id}");
				}
				else
				{
					Console.WriteLine($"Update failed: {uploadResult.Exception?.Message}");

				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error updating file: {ex.Message}");
			}
		}
	}

	public void UploadFile(string fileName, string filePath)
	{
	// string testName = "testFile.ods";
		string fileNameExt = fileName;
		var service = GetDriveService();
		var fileMetadata = new Google.Apis.Drive.v3.Data.File()
		{
			Name = fileNameExt
		};
		using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
		{
			var request = service.Files.Create(fileMetadata, fileStream, "application/octet-stream");
			request.Fields = "id";
			Console.WriteLine($"Uploading file '{filePath}' to Google Drive as '{fileMetadata.Name}'");
			var uploadResult = request.Upload();
			if (uploadResult.Status == Google.Apis.Upload.UploadStatus.Completed)
			{
				Console.WriteLine($"Upload successful. File ID: {request.ResponseBody?.Id}");
			}
			else
			{
				Console.WriteLine($"Upload failed: {uploadResult.Exception?.Message}");
			}
		}
	}

	public Spreadsheet DownloadGoogleSheet(string sheetName)
	{
		var driveService = GetDriveService();
		// Find the file by name and type
		var request = driveService.Files.List();
		request.Q = $"name='{sheetName}' and mimeType='application/vnd.google-apps.spreadsheet' and trashed=false";
		request.Fields = "files(id, name)";
		var result = request.Execute();
		var file = result.Files?.FirstOrDefault();
		if (file == null)
			throw new FileNotFoundException($"Google Sheet '{sheetName}' not found in Google Drive.");

		// Use Sheets API to get the spreadsheet object
		var sheetsService = new SheetsService(new BaseClientService.Initializer
		{
			HttpClientInitializer = driveService.HttpClientInitializer,
			ApplicationName = ApplicationName,
		});

		var getRequest = sheetsService.Spreadsheets.Get(file.Id);
		var spreadsheet = getRequest.Execute();
		return spreadsheet;
	}

	public void UpdateGoogleSheet(Spreadsheet spreadsheet)
	{
		var driveService = GetDriveService();
		var sheetsService = new SheetsService(new BaseClientService.Initializer
		{
			HttpClientInitializer = driveService.HttpClientInitializer,
			ApplicationName = ApplicationName,
		});

		foreach (var sheet in spreadsheet.Sheets)
		{
			var sheetTitle = sheet.Properties.Title;
			var rowData = sheet.Data?.FirstOrDefault()?.RowData;
			if (rowData == null) continue;

			// Convert RowData to IList<IList<object>>
			var values = new List<IList<object>>();
			foreach (var row in rowData)
			{
				var rowValues = new List<object>();
				if (row.Values != null)
				{
					foreach (var cell in row.Values)
					{
						rowValues.Add(cell.FormattedValue ?? "");
					}
				}
				values.Add(rowValues);
			}

			// Calculate range (A1 notation)
			int rowCount = values.Count;
			int colCount = values.Max(r => r.Count);
			string endCol = GetExcelColumnName(colCount);
			string range = $"{sheetTitle}!A1:{endCol}{rowCount}";

			var valueRange = new ValueRange
			{
				Range = range,
				Values = values
			};

			var updateRequest = sheetsService.Spreadsheets.Values.Update(valueRange, spreadsheet.SpreadsheetId, range);
			updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
			var updateResponse = updateRequest.Execute();
			Console.WriteLine($"Updated {updateResponse.UpdatedCells} cells in range {range} of spreadsheet {spreadsheet.SpreadsheetId}.");
		}
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

	public void DownloadFile(string fileName, string localPath)
	{
		var service = GetDriveService();
		var request = service.Files.List();
		request.Q = "trashed=false";
		request.Fields = "files(id, name)";
		var result = request.Execute();
		if (result.Files == null || result.Files.Count == 0)
		{
			Console.WriteLine("No files found in Google Drive.");
		}
		else
		{
			var debug = true;
			if (debug)
			{
				Console.WriteLine("Files found in Google Drive:");
				foreach (var f in result.Files)
				{
					Console.WriteLine($"- {f.Name}");
				}
			}
		}

		var file = result.Files.FirstOrDefault(f => f.Name == fileName);
		if (file == null)
			throw new FileNotFoundException($"File '{fileName}' not found in Google Drive.");

		// If the file is a Google Sheet, use Export; otherwise, use Get
		//if (file.Name.EndsWith(".gsheet", StringComparison.OrdinalIgnoreCase) || file.Name.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) || file.Name.EndsWith(".ods", StringComparison.OrdinalIgnoreCase))
		//{
		// Try export as ODS

		Console.WriteLine($"Exporting Google Sheet '{file.Name}' as xlsx to '{localPath}'");
		var xslxMine = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
		var odsMime = "application/vnd.oasis.opendocument.spreadsheet";
		var exportRequest = service.Files.Export(file.Id, xslxMine);
		using (var stream = new FileStream(localPath, FileMode.Create, FileAccess.Write))
		{
			exportRequest.Download(stream);
		}
		/*}
		else
		{
			var getRequest = service.Files.Get(file.Id);
			using (var stream = new FileStream(localPath, FileMode.Create, FileAccess.Write))
			{
				getRequest.Download(stream);
			}
		}*/
	}
}

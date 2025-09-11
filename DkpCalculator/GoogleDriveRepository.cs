
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

	public void UploadFile(string filePath)
	{
		var service = GetDriveService();
		var fileMetadata = new Google.Apis.Drive.v3.Data.File()
		{
			Name = Path.GetFileName(filePath)
		};
		using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
		{
			var request = service.Files.Create(fileMetadata, fileStream, "application/octet-stream");
			request.Fields = "id";
			request.Upload();
		}
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
            var debug = false;
            if(debug){
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
			var exportRequest = service.Files.Export(file.Id, "application/vnd.oasis.opendocument.spreadsheet");
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

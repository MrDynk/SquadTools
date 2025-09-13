using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Threading;

public class GoogleDocRepository : IFileStorageRepository
{
    private readonly string[] Scopes = { DriveService.Scope.Drive };
    private readonly string ApplicationName = "SquadApp";


    public void SaveFile(string filePath)
    {
        // Uploads a file to Google Drive by reading it from disk
        UserCredential credential;
        using (var stream = new FileStream("credentials\\credentials.json", FileMode.Open, FileAccess.Read))
        {
            string credPath = "token.json";
            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new Google.Apis.Util.Store.FileDataStore(credPath, true)).Result;
        }

        var service = new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });

        var fileMetadata = new Google.Apis.Drive.v3.Data.File()
        {
            Name = Path.GetFileName(filePath)
        };
        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            var request = service.Files.Create(fileMetadata, fileStream, "application/octet-stream");
            request.Fields = "id";
            var file = request.Upload();
        }
    }

    public byte[] GetFile(string fileName)
    {
        // Downloads a file by name from Google Drive (first match)
        UserCredential credential;
        using (var stream = new FileStream("credentials\\credentials.json", FileMode.Open, FileAccess.Read))
        {
            string credPath = "token.json";
            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true)).Result;
        }

        // Create Drive API service.
        var service = new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });

        // Find the file by name
        var request = service.Files.List();
        request.Q = $"name='{fileName}' and trashed=false";
        request.Fields = "files(id, name)";
        var result = request.Execute();
        var file = result.Files.FirstOrDefault();
        if (file == null)
            throw new FileNotFoundException($"File '{fileName}' not found in Google Drive.");

        // Download the file
        var getRequest = service.Files.Get(file.Id);
        var streamOut = new MemoryStream();
        getRequest.Download(streamOut);
        return streamOut.ToArray();
    }

    public void DeleteFile(string filePath)
    {
        // Implementation for deleting a file from Google Docs (not implemented here)
        throw new NotImplementedException();
    }

    // Example: Download DKP.ods to local disk
    public void DownloadDkpOds(string localPath)
    {
        var data = GetFile("DKP.ods");
        System.IO.File.WriteAllBytes(localPath, data);
    }
}

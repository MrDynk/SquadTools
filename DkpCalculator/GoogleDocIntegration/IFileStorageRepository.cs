public interface IFileStorageRepository
{
    // Method to save a file to storage
    void SaveFile(string filePath);

    // Method to retrieve a file from storage
    byte[] GetFile(string filePath);

    // Method to delete a file from storage
    void DeleteFile(string filePath);
}
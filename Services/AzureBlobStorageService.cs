// File: Services/AzureBlobStorageService.cs
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Medlemsnavet.Services;
namespace Medlemsnavet.Api.Services;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobStorageService(IConfiguration configuration)
    {
        // Assumes you have "BlobStorageConnectionString" in your User Secrets
        _blobServiceClient = new BlobServiceClient(configuration["BlobStorageConnectionString"]);
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var uniqueFileName = $"{Guid.NewGuid()}-{fileName}";
        var blobClient = containerClient.GetBlobClient(uniqueFileName);

        await blobClient.UploadAsync(fileStream, true);

        return blobClient.Uri.ToString();
    }
}
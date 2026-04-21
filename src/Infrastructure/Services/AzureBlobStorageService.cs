using System;
using System.IO;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using GoodHamburger.Domain.Exceptions;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Infrastructure.Services;

/// <summary>
/// Azure Blob Storage implementation for <see cref="IStorageService"/>
/// </summary>
public class AzureBlobStorageService : IStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<AzureBlobStorageService> _logger;

    public AzureBlobStorageService(BlobServiceClient blobServiceClient, ILogger<AzureBlobStorageService> logger)
    {
        _blobServiceClient = blobServiceClient;
        _logger = logger;
    }

    public async Task<string> UploadAsync(string bucketName, string objectName, string contentType, Stream stream)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(bucketName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

            var blobClient = containerClient.GetBlobClient(objectName);
            await blobClient.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
            });

            var publicUrl = GetPublicUrl(bucketName, objectName);
            _logger.LogInformation("Successfully uploaded blob {ObjectName} to container {Container}", objectName, bucketName);

            return publicUrl;
        }
        catch (RequestFailedException ex) when (ex.Status == 403)
        {
            _logger.LogError(ex, "Access denied uploading blob {ObjectName} to container {Container}", objectName, bucketName);
            throw new StorageException("Access denied to Azure Blob Storage", bucketName, objectName, "Upload", ex);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Azure error uploading blob {ObjectName} to container {Container}", objectName, bucketName);
            throw new StorageException($"Failed to upload to Azure Blob Storage: {ex.Message}", bucketName, objectName, "Upload", ex);
        }
    }

    public async Task DownloadAsync(string bucketName, string objectName, Stream destination)
    {
        try
        {
            var blobClient = _blobServiceClient
                .GetBlobContainerClient(bucketName)
                .GetBlobClient(objectName);

            await blobClient.DownloadToAsync(destination);
            _logger.LogInformation("Downloaded blob {ObjectName} from container {Container}", objectName, bucketName);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogError(ex, "Blob {ObjectName} not found in container {Container}", objectName, bucketName);
            throw new StorageException($"Object '{objectName}' not found in container '{bucketName}'", bucketName, objectName, "Download", ex);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Azure error downloading blob {ObjectName} from container {Container}", objectName, bucketName);
            throw new StorageException($"Failed to download from Azure Blob Storage: {ex.Message}", bucketName, objectName, "Download", ex);
        }
    }

    public async Task DeleteAsync(string bucketName, string objectName)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(bucketName);
            var blobClient = containerClient.GetBlobClient(objectName);
            var response = await blobClient.DeleteIfExistsAsync();

            if (!response.Value)
            {
                _logger.LogWarning("Blob {ObjectName} not found in container {Container} during delete", objectName, bucketName);
            }
        }
        catch (RequestFailedException ex) when (ex.Status == 403)
        {
            _logger.LogError(ex, "Access denied deleting blob {ObjectName} from container {Container}", objectName, bucketName);
            throw new StorageException($"Access denied to delete object '{objectName}'", bucketName, objectName, "Delete", ex);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Azure error deleting blob {ObjectName} from container {Container}", objectName, bucketName);
            throw new StorageException($"Failed to delete from Azure Blob Storage: {ex.Message}", bucketName, objectName, "Delete", ex);
        }
    }

    public string GetPublicUrl(string bucketName, string objectName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(bucketName);
        return containerClient.GetBlobClient(objectName).Uri.ToString();
    }
}

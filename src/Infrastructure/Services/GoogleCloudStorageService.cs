using System;
using System.IO;
using System.Threading.Tasks;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;
using GoodHamburger.Domain.Exceptions;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Infrastructure.Services;

/// <summary>
/// Google Cloud Storage implementation for <see cref="IStorageService"/>
/// </summary>
public class GoogleCloudStorageService : IStorageService
{
    private readonly StorageClient _storageClient;
    private readonly ILogger<GoogleCloudStorageService> _logger;

    public GoogleCloudStorageService(StorageClient storageClient, ILogger<GoogleCloudStorageService> logger)
    {
        _storageClient = storageClient;
        _logger = logger;
    }

    public async Task<string> UploadAsync(string bucketName, string objectName, string contentType, Stream stream)
    {
        try
        {
            _logger.LogInformation("Uploading object {ObjectName} to bucket {BucketName}", objectName, bucketName);

            var obj = await _storageClient.UploadObjectAsync(
                bucket: bucketName,
                objectName: objectName,
                contentType: contentType,
                source: stream);

            var publicUrl = GetPublicUrl(bucketName, objectName);
            _logger.LogInformation("Successfully uploaded object {ObjectName}. URL: {Url}", objectName, publicUrl);

            return publicUrl;
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogError(ex, "Bucket {BucketName} not found", bucketName);
            throw new StorageException($"Bucket '{bucketName}' not found", bucketName, objectName, "Upload", ex);
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            _logger.LogError(ex, "Access denied to bucket {BucketName}", bucketName);
            throw new StorageException($"Access denied to bucket '{bucketName}'", bucketName, objectName, "Upload", ex);
        }
        catch (Google.GoogleApiException ex)
        {
            _logger.LogError(ex, "Google Cloud error uploading {ObjectName} to {BucketName}", objectName, bucketName);
            throw new StorageException($"Failed to upload to Google Cloud Storage: {ex.Message}", bucketName, objectName, "Upload", ex);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "IO error reading stream for {ObjectName}", objectName);
            throw new StorageException($"Failed to read file stream: {ex.Message}", bucketName, objectName, "Upload", ex);
        }
    }

    public async Task DownloadAsync(string bucketName, string objectName, Stream destination)
    {
        try
        {
            _logger.LogInformation("Downloading object {ObjectName} from bucket {BucketName}", objectName, bucketName);

            await _storageClient.DownloadObjectAsync(
                bucket: bucketName,
                objectName: objectName,
                destination: destination);

            _logger.LogInformation("Successfully downloaded object {ObjectName}", objectName);
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogError(ex, "Object {ObjectName} not found in bucket {BucketName}", objectName, bucketName);
            throw new StorageException($"Object '{objectName}' not found in bucket '{bucketName}'", bucketName, objectName, "Download", ex);
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            _logger.LogError(ex, "Access denied to object {ObjectName} in bucket {BucketName}", objectName, bucketName);
            throw new StorageException($"Access denied to object '{objectName}'", bucketName, objectName, "Download", ex);
        }
        catch (Google.GoogleApiException ex)
        {
            _logger.LogError(ex, "Google Cloud error downloading {ObjectName} from {BucketName}", objectName, bucketName);
            throw new StorageException($"Failed to download from Google Cloud Storage: {ex.Message}", bucketName, objectName, "Download", ex);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "IO error writing to destination stream for {ObjectName}", objectName);
            throw new StorageException($"Failed to write to destination stream: {ex.Message}", bucketName, objectName, "Download", ex);
        }
    }

    public async Task DeleteAsync(string bucketName, string objectName)
    {
        try
        {
            _logger.LogInformation("Deleting object {ObjectName} from bucket {BucketName}", objectName, bucketName);

            await _storageClient.DeleteObjectAsync(bucket: bucketName, objectName: objectName);

            _logger.LogInformation("Successfully deleted object {ObjectName}", objectName);
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning(ex, "Object {ObjectName} not found in bucket {BucketName} (already deleted?)", objectName, bucketName);
            // Delete is idempotent
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            _logger.LogError(ex, "Access denied to delete object {ObjectName} in bucket {BucketName}", objectName, bucketName);
            throw new StorageException($"Access denied to delete object '{objectName}'", bucketName, objectName, "Delete", ex);
        }
        catch (Google.GoogleApiException ex)
        {
            _logger.LogError(ex, "Google Cloud error deleting {ObjectName} from {BucketName}", objectName, bucketName);
            throw new StorageException($"Failed to delete from Google Cloud Storage: {ex.Message}", bucketName, objectName, "Delete", ex);
        }
    }

    public string GetPublicUrl(string bucketName, string objectName)
    {
        return $"https://storage.googleapis.com/{bucketName}/{objectName}";
    }
}

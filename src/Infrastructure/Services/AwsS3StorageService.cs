using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using GoodHamburger.Domain.Exceptions;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Infrastructure.Services;

/// <summary>
/// AWS S3 implementation for <see cref="IStorageService"/>
/// </summary>
public class AwsS3StorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<AwsS3StorageService> _logger;

    public AwsS3StorageService(IAmazonS3 s3Client, ILogger<AwsS3StorageService> logger)
    {
        _s3Client = s3Client;
        _logger = logger;
    }

    public async Task<string> UploadAsync(string bucketName, string objectName, string contentType, Stream stream)
    {
        try
        {
            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = objectName,
                InputStream = stream,
                ContentType = contentType
            };

            await _s3Client.PutObjectAsync(request);
            var publicUrl = GetPublicUrl(bucketName, objectName);
            _logger.LogInformation("Uploaded object {ObjectName} to bucket {Bucket}", objectName, bucketName);
            return publicUrl;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogError(ex, "Bucket {Bucket} not found", bucketName);
            throw new StorageException($"Bucket '{bucketName}' not found", bucketName, objectName, "Upload", ex);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            _logger.LogError(ex, "Access denied to bucket {Bucket}", bucketName);
            throw new StorageException($"Access denied to bucket '{bucketName}'", bucketName, objectName, "Upload", ex);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "AWS error uploading object {Object} to bucket {Bucket}", objectName, bucketName);
            throw new StorageException($"Failed to upload to AWS S3: {ex.Message}", bucketName, objectName, "Upload", ex);
        }
    }

    public async Task DownloadAsync(string bucketName, string objectName, Stream destination)
    {
        try
        {
            using var response = await _s3Client.GetObjectAsync(bucketName, objectName);
            await response.ResponseStream.CopyToAsync(destination);
            _logger.LogInformation("Downloaded object {ObjectName} from bucket {Bucket}", objectName, bucketName);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogError(ex, "Object {ObjectName} not found in bucket {Bucket}", objectName, bucketName);
            throw new StorageException($"Object '{objectName}' not found in bucket '{bucketName}'", bucketName, objectName, "Download", ex);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "AWS error downloading object {ObjectName} from bucket {Bucket}", objectName, bucketName);
            throw new StorageException($"Failed to download from AWS S3: {ex.Message}", bucketName, objectName, "Download", ex);
        }
    }

    public async Task DeleteAsync(string bucketName, string objectName)
    {
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = objectName
            };

            await _s3Client.DeleteObjectAsync(request);
            _logger.LogInformation("Deleted object {ObjectName} from bucket {Bucket}", objectName, bucketName);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning(ex, "Object {ObjectName} not found in bucket {Bucket} during delete", objectName, bucketName);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            _logger.LogError(ex, "Access denied deleting object {ObjectName} from bucket {Bucket}", objectName, bucketName);
            throw new StorageException($"Access denied to delete object '{objectName}'", bucketName, objectName, "Delete", ex);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "AWS error deleting object {ObjectName} from bucket {Bucket}", objectName, bucketName);
            throw new StorageException($"Failed to delete from AWS S3: {ex.Message}", bucketName, objectName, "Delete", ex);
        }
    }

    public string GetPublicUrl(string bucketName, string objectName)
    {
        return $"https://{bucketName}.s3.amazonaws.com/{objectName}";
    }
}

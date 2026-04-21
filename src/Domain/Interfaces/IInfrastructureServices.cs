using System.Threading.Tasks;

namespace GoodHamburger.Domain.Interfaces;

/// <summary>
/// Interface for message queue service
/// Provides async messaging capabilities through RabbitMQ
/// </summary>
public interface IQueueService
{
    /// <summary>
    /// Publishes a message to the specified queue
    /// </summary>
    /// <param name="queueName">Name of the queue</param>
    /// <param name="message">Message content as string</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishAsync(string queueName, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes an object as JSON to the specified queue
    /// </summary>
    /// <param name="queueName">Name of the queue</param>
    /// <param name="payload">Object to serialize and publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishAsync(string queueName, object payload, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for cloud storage service
/// Provides blob storage operations independent of vendor implementation
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Uploads a file to cloud storage
    /// </summary>
    /// <param name="bucketName">Bucket name</param>
    /// <param name="objectName">Object/file name</param>
    /// <param name="contentType">MIME content type</param>
    /// <param name="stream">File content stream</param>
    Task<string> UploadAsync(string bucketName, string objectName, string contentType, System.IO.Stream stream);

    /// <summary>
    /// Downloads a file from cloud storage
    /// </summary>
    /// <param name="bucketName">Bucket name</param>
    /// <param name="objectName">Object/file name</param>
    /// <param name="destination">Destination stream</param>
    Task DownloadAsync(string bucketName, string objectName, System.IO.Stream destination);

    /// <summary>
    /// Deletes a file from cloud storage
    /// </summary>
    /// <param name="bucketName">Bucket name</param>
    /// <param name="objectName">Object/file name</param>
    Task DeleteAsync(string bucketName, string objectName);

    /// <summary>
    /// Gets the public URL of an object
    /// </summary>
    /// <param name="bucketName">Bucket name</param>
    /// <param name="objectName">Object/file name</param>
    string GetPublicUrl(string bucketName, string objectName);
}

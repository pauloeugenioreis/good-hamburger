using System;

namespace GoodHamburger.Domain.Exceptions;

/// <summary>
/// Exception thrown when storage operations (upload, download, delete) fail
/// Represents failures in cloud storage services (Google Cloud Storage, Azure Blob, S3, etc.)
/// </summary>
public class StorageException : Exception
{
    public string BucketName { get; }
    public string ObjectName { get; }
    public string Operation { get; }

    public StorageException(string message, string bucketName, string objectName, string operation, Exception? innerException = null)
        : base(message, innerException)
    {
        BucketName = bucketName;
        ObjectName = objectName;
        Operation = operation;
    }

    public StorageException() : base()
    {
        BucketName = string.Empty;
        ObjectName = string.Empty;
        Operation = string.Empty;
    }
}

/// <summary>
/// Exception thrown when JWT token validation or generation fails
/// Represents authentication token-related errors
/// </summary>
public class TokenValidationException : Exception
{
    public string? TokenType { get; }

    public TokenValidationException(string message, string? tokenType = null, Exception? innerException = null)
        : base(message, innerException)
    {
        TokenType = tokenType;
    }
}

/// <summary>
/// Exception thrown when event store operations fail
/// Represents failures in event sourcing operations (Marten, EventStore, etc.)
/// </summary>
public class EventStoreException : Exception
{
    public string? AggregateId { get; }
    public string Operation { get; }

    public EventStoreException(string message, string operation, string? aggregateId = null, Exception? innerException = null)
        : base(message, innerException)
    {
        AggregateId = aggregateId;
        Operation = operation;
    }
}

using System;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Azure.Identity;
using Azure.Storage.Blobs;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using GoodHamburger.Domain;
using GoodHamburger.Domain.Interfaces;
using GoodHamburger.Infrastructure.Services;

namespace GoodHamburger.Infrastructure.Extensions;

/// <summary>
/// Extension methods for configuring multi-cloud storage providers
/// </summary>
public static class StorageExtension
{
    /// <summary>
    /// Registers the appropriate cloud storage provider based on configuration
    /// </summary>
    public static IServiceCollection AddStorage<TProgram>(this IServiceCollection services)
    {
        services.AddSingleton<IStorageService>(sp =>
        {
            var appSettings = sp.GetRequiredService<IOptions<AppSettings>>().Value;
            var storageSettings = appSettings.Infrastructure?.Storage
                ?? throw new InvalidOperationException("Storage settings are missing from configuration.");

            var provider = storageSettings.Provider?.Trim().ToLowerInvariant() ?? "google";

            return provider switch
            {
                "google" => new GoogleCloudStorageService(
                    CreateGoogleStorageClient<TProgram>(sp, storageSettings),
                    sp.GetRequiredService<ILogger<GoogleCloudStorageService>>()),

                "azure" => new AzureBlobStorageService(
                    CreateAzureBlobServiceClient<TProgram>(sp, storageSettings),
                    sp.GetRequiredService<ILogger<AzureBlobStorageService>>()),

                "aws" => new AwsS3StorageService(
                    CreateAmazonS3Client<TProgram>(sp, storageSettings),
                    sp.GetRequiredService<ILogger<AwsS3StorageService>>()),

                _ => throw new NotSupportedException($"Storage provider '{storageSettings.Provider}' is not supported.")
            };
        });

        return services;
    }

    private static StorageClient CreateGoogleStorageClient<TProgram>(IServiceProvider serviceProvider, StorageSettings storageSettings)
    {
        var appSettings = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value;
        var logger = serviceProvider.GetRequiredService<ILogger<TProgram>>();
        var isProduction = appSettings.Infrastructure?.Environment == "Production";

        var googleSettings = storageSettings.Google;

        if (!string.IsNullOrEmpty(googleSettings?.ServiceAccount))
        {
            try
            {
                var credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromJson(googleSettings.ServiceAccount);
                logger.LogInformation("StorageClient created using explicit Google Service Account credentials.");
                return StorageClient.Create(credential);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to parse Google Service Account JSON. Using default credentials as fallback.");
            }
        }

        try
        {
            logger.LogInformation("Creating StorageClient with default Google environment credentials.");
            return StorageClient.Create();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to create Google StorageClient. No valid credentials found.");

            if (isProduction)
            {
                throw;
            }

            logger.LogWarning("Returning unauthenticated StorageClient for non-production environment.");
            return StorageClient.CreateUnauthenticated();
        }
    }

    private static BlobServiceClient CreateAzureBlobServiceClient<TProgram>(IServiceProvider serviceProvider, StorageSettings storageSettings)
    {
        var appSettings = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value;
        var logger = serviceProvider.GetRequiredService<ILogger<TProgram>>();
        var isProduction = appSettings.Infrastructure?.Environment == "Production";
        var azureSettings = storageSettings.Azure;

        if (!string.IsNullOrWhiteSpace(azureSettings?.ConnectionString))
        {
            logger.LogInformation("Creating BlobServiceClient using connection string.");
            return new BlobServiceClient(azureSettings.ConnectionString);
        }

        if (!string.IsNullOrWhiteSpace(azureSettings?.BlobServiceUri))
        {
            var credentialOptions = new DefaultAzureCredentialOptions();

            if (!string.IsNullOrWhiteSpace(azureSettings.ManagedIdentityClientId))
            {
                credentialOptions.ManagedIdentityClientId = azureSettings.ManagedIdentityClientId;
            }

            logger.LogInformation("Creating BlobServiceClient using DefaultAzureCredential.");
            var credential = new DefaultAzureCredential(credentialOptions);
            return new BlobServiceClient(new Uri(azureSettings.BlobServiceUri), credential);
        }

        if (!isProduction)
        {
            logger.LogWarning("Azure Storage configuration missing. Falling back to Azurite development storage.");
            return new BlobServiceClient("UseDevelopmentStorage=true");
        }

        throw new InvalidOperationException("Azure Storage requires either ConnectionString or BlobServiceUri configuration.");
    }

    private static IAmazonS3 CreateAmazonS3Client<TProgram>(IServiceProvider serviceProvider, StorageSettings storageSettings)
    {
        var appSettings = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value;
        var logger = serviceProvider.GetRequiredService<ILogger<TProgram>>();
        var isProduction = appSettings.Infrastructure?.Environment == "Production";
        var awsSettings = storageSettings.Aws ?? new AwsStorageSettings();

        var regionEndpoint = RegionEndpoint.GetBySystemName(awsSettings.Region ?? "us-east-1");
        var config = new AmazonS3Config
        {
            RegionEndpoint = regionEndpoint
        };

        if (!string.IsNullOrWhiteSpace(awsSettings.ServiceUrl))
        {
            config.ServiceURL = awsSettings.ServiceUrl;
            config.ForcePathStyle = true;
        }

        if (!string.IsNullOrWhiteSpace(awsSettings.AccessKeyId) && !string.IsNullOrWhiteSpace(awsSettings.SecretAccessKey))
        {
            logger.LogInformation("Creating AmazonS3Client using explicit access keys.");
            var credentials = new BasicAWSCredentials(awsSettings.AccessKeyId, awsSettings.SecretAccessKey);
            return new AmazonS3Client(credentials, config);
        }

        if (!string.IsNullOrWhiteSpace(awsSettings.Profile))
        {
            logger.LogInformation("Creating AmazonS3Client using stored profile {Profile}", awsSettings.Profile);
            var chain = new CredentialProfileStoreChain();
            if (chain.TryGetAWSCredentials(awsSettings.Profile, out var profileCredentials))
            {
                return new AmazonS3Client(profileCredentials, config);
            }

            logger.LogWarning("AWS profile {Profile} not found. Falling back to default credential chain.", awsSettings.Profile);
        }

        try
        {
            logger.LogInformation("Creating AmazonS3Client using default credential chain.");
            return new AmazonS3Client(config);
        }
        catch (AmazonClientException ex)
        {
            logger.LogCritical(ex, "Failed to create AmazonS3Client with default credentials.");

            if (isProduction)
            {
                throw;
            }

            logger.LogWarning("Falling back to anonymous AmazonS3Client for non-production environment (LocalStack).");
            var fallbackConfig = new AmazonS3Config
            {
                ForcePathStyle = true,
                RegionEndpoint = regionEndpoint
            };

            if (!string.IsNullOrWhiteSpace(awsSettings.ServiceUrl))
            {
                fallbackConfig.ServiceURL = awsSettings.ServiceUrl;
                fallbackConfig.ForcePathStyle = true;
            }

            return new AmazonS3Client(fallbackConfig);
        }
    }
}

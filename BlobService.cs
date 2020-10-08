using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace WebUpload
{
    public class BlobService : IBlobService
    {
        private readonly BlobServiceClient _blobClient;
        private readonly IConfiguration _configuration;

        public BlobService(BlobServiceClient blobServiceClient, IConfiguration configuration)
        {
            _blobClient = blobServiceClient;
            _configuration = configuration;
        }

        public async Task<bool> UploadFileBlobAsync(string blobContainerName, Stream fileStream, string fileName)
        {

            var containerClient = GetContainerClient(blobContainerName);

            var blobClient = containerClient.GetBlobClient(fileName);

            // To overwrite blob true param has been passed. By default it's false.
            await blobClient.UploadAsync(fileStream, true);

            return true;

        }

        public string DownloadFileBlobAsync(string blobContainerName, string fileName)
        {
            var containerClient = GetContainerClient(blobContainerName);
            var blobClient = containerClient.GetBlobClient(fileName);


            if (containerClient.Exists())
            {
                // Set the expiration time and permissions for the container.
                // In this case, the start time is specified as a few 
                // minutes in the past, to mitigate clock skew.
                // The shared access signature will be valid immediately.
                BlobSasBuilder sas = new BlobSasBuilder
                {
                    Resource = "b",
                    BlobName = fileName,
                    BlobContainerName = blobContainerName,
                    StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                    ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(15)
                };

                sas.SetPermissions(BlobSasPermissions.Read);

                // Create StorageSharedKeyCredentials object by reading
                // the values from the configuration (appsettings.json)
                StorageSharedKeyCredential storageCredential =
                    new StorageSharedKeyCredential(containerClient.AccountName, _configuration.GetValue<string>("AzureStorageAccountKey"));

                // Create a SAS URI to the storage account
                UriBuilder sasUri = new UriBuilder(containerClient.Uri);
                sasUri.Query = sas.ToSasQueryParameters(storageCredential).ToString();



                // Create the URI using the SAS query token.
                string sasBlobUri = containerClient.Uri + "/" +
                                    fileName + sasUri.Query;


                return sasBlobUri;
            }

            return "";
        }

        private BlobContainerClient GetContainerClient(string blobContainerName)
        {
            var containerClient = _blobClient.GetBlobContainerClient(blobContainerName);
            containerClient.CreateIfNotExists(PublicAccessType.None);
            return containerClient;
        }
    }
}

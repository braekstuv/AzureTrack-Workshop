using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;
using System;

namespace RMotownFestival.Api.Common
{
    public class BlobUtility
    {
        public BlobUtility(StorageSharedKeyCredential credential, BlobServiceClient client, IOptions<BlobSettingsOptions> options)
        {
            Credential = credential;
            Client = client;
            Options = options.Value;
        }

        public StorageSharedKeyCredential Credential { get; }
        public BlobServiceClient Client { get; }
        public BlobSettingsOptions Options { get; }

        public BlobContainerClient GetPicturesContainer()
            => Client.GetBlobContainerClient(Options.PicturesContainer);

        public BlobContainerClient GetThumbsContainer()
            => Client.GetBlobContainerClient(Options.ThumbsContainer);

        public string GetSasUri(BlobContainerClient container, string blobName)
        {
            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = container.Name,
                BlobName = blobName,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow.AddMinutes(-1),
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(2),
            };

            sasBuilder.SetPermissions(BlobAccountSasPermissions.Read);

            // Use the key to get the SAS token.
            string sasToken = sasBuilder.ToSasQueryParameters(Credential).ToString();

            return $"{container.Uri.AbsoluteUri}/{blobName}?{sasToken}";
        }
    }
}

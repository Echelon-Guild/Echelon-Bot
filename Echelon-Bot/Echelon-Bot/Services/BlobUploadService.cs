using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Discord;

namespace EchelonBot.Services
{
    public class BlobUploadService
    {
        private BlobServiceClient _blobServiceClient;

        public BlobUploadService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        public async Task<string> UploadBlobAsync(IAttachment attachment, string container)
        {
            if (attachment == null)
            {
                throw new FileNotFoundException();
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient(container);

            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(attachment.Filename);

            using var httpClient = new HttpClient();
            using var stream = await httpClient.GetStreamAsync(attachment.Url);
            await blobClient.UploadAsync(stream, overwrite: true);

            string blobUrl = blobClient.Uri.ToString();

            return blobUrl;
        }
    }
}

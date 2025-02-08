using Azure.Storage.Blobs;
using Discord;

namespace EchelonBot.Services
{
    public class BlobUploadService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly HttpClient _httpClient;

        public BlobUploadService(BlobServiceClient blobServiceClient, HttpClient httpClient)
        {
            _blobServiceClient = blobServiceClient;
            _httpClient = httpClient;
        }

        public async Task<Uri> UploadBlobAsync(IAttachment attachment, string container)
        {
            if (attachment == null)
            {
                throw new FileNotFoundException();
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient(container);

            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(attachment.Filename);

            using var stream = await _httpClient.GetStreamAsync(attachment.Url);
            await blobClient.UploadAsync(stream, overwrite: true);

            return blobClient.Uri;
        }
    }
}

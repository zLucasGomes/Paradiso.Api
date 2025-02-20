using Azure.Storage.Sas;

namespace Paradiso.API.Service.Services;

public class BlobService : IBlobService
{
    private BlobServiceClient _client { get; set; }

    public BlobService(BlobServiceClient client)
    {
        _client = client;
    }

    public BlobContainerClient GetBlobContainerClient(EContainer container) => _client.GetBlobContainerClient(container.GetName());

    private string GetUrlFromBlob(EContainer containerName, string blobName) =>
        GetBlobContainerClient(containerName).GetBlobClient(blobName)
        .GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddYears(50)).AbsoluteUri.ToString();

    public async Task<string> UploadBlobFileAsync(EContainer containerName, IFormFile file, Guid id, string hash)
    {
        try
        {
            var container = GetBlobContainerClient(containerName);

            using var stream = file.OpenReadStream();

            var blobName = $"{id}.{hash}{Path.GetExtension(file.FileName)}";
            var blobClient = container.GetBlobClient(blobName);

            if (await blobClient.ExistsAsync())
                throw new ExceptionDto() { Message = EException.FileExist.DisplayName() };

            await blobClient.UploadAsync(stream, true);

            return GetUrlFromBlob(containerName, blobName);
        }
        catch (Exception ex)
        {
            throw new ExceptionDto() { Message = ex.Message };
        }
    }

    public async Task<string> UpdateBlobFileAsync(EContainer containerName, IFormFile file, Guid id, string hash)
    {
        try
        {
            if (file.Length == 0)
                throw new ExceptionDto() { Message = EException.FileNotSelected.DisplayName() };

            using var stream = file.OpenReadStream();

            var container = GetBlobContainerClient(containerName);

            var blobName = $"{id}.{hash}{Path.GetExtension(file.FileName)}";
            var blobClient = container.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync())
                throw new ExceptionDto() { Message = EException.FileNotFound.DisplayName() };

            await blobClient.DeleteAsync();
            await blobClient.UploadAsync(stream, true);

            return GetUrlFromBlob(containerName, blobName);

        }
        catch (Exception ex)
        {
            throw new ExceptionDto() { Message = ex.Message };
        }
    }

    public async Task DeleteBlobFileAsync(EContainer containerName, IEnumerable<string> blobNamesToDelete)
    {
        try
        {
            var container = GetBlobContainerClient(containerName);

            foreach (var item in blobNamesToDelete)
            {
                await container.DeleteBlobAsync(item);
            }
        }
        catch (Exception ex)
        {
            throw new ExceptionDto() { Message = ex.Message };
        }
    }
}

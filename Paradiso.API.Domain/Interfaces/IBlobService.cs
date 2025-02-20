using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Paradiso.API.Domain.Enums;

namespace Paradiso.API.Domain.Interfaces;

public interface IBlobService
{
    public BlobContainerClient GetBlobContainerClient(EContainer container);
    public Task DeleteBlobFileAsync(EContainer containerName, IEnumerable<string> blobNamesToDelete);
    public Task<string> UpdateBlobFileAsync(EContainer containerName, IFormFile file, Guid id, string hash);
    public Task<string> UploadBlobFileAsync(EContainer containerName, IFormFile file, Guid id, string hash);
}

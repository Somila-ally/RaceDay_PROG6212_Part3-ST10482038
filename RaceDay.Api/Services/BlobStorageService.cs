using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Api.Services;

public sealed class BlobStorageService(IConfiguration configuration)
{
    public async Task<string> UploadAsync(IFormFile? file, string folder, CancellationToken cancellationToken)
    {
        var connectionString = configuration["AzureBlobStorage:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString)) throw new InvalidOperationException("Azure Blob Storage is not configured. Set AzureBlobStorage__ConnectionString.");
        if (file is null || file.Length == 0 || file.Length > 5 * 1024 * 1024) throw new ArgumentException("Choose an image up to 5 MB.");
        var contentType = file.ContentType.ToLowerInvariant();
        if (contentType is not ("image/jpeg" or "image/png" or "image/webp")) throw new ArgumentException("Only JPEG, PNG and WebP images are supported.");
        var containerName = configuration["AzureBlobStorage:ContainerName"] ?? "raceday-images";
        var container = new BlobContainerClient(connectionString, containerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);
        var extension = contentType switch { "image/jpeg" => ".jpg", "image/png" => ".png", _ => ".webp" };
        var blob = container.GetBlobClient($"{folder}/{Guid.NewGuid():N}{extension}");
        await using var stream = file.OpenReadStream();
        await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = contentType }, cancellationToken: cancellationToken);
        return blob.Uri.ToString();
    }
}

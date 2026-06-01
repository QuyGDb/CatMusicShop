using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;


namespace MusicShop.Infrastructure.Storage;

public sealed class S3ImageService(
    IAmazonS3 s3Client,
    IOptions<S3Settings> settings) : IImageService
{
    public async Task<string> UploadImageAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string folder = "general",
        CancellationToken ct = default)
    {
        string uniqueFileName = $"{Guid.NewGuid()}-{fileName}";
        string key = string.IsNullOrWhiteSpace(folder)
            ? uniqueFileName
            : $"{folder.TrimEnd('/')}/{uniqueFileName}";

        PutObjectRequest request = new PutObjectRequest
        {
            BucketName = settings.Value.BucketName,
            Key = key,
            InputStream = fileStream,
            ContentType = contentType,
        };

        await s3Client.PutObjectAsync(request, ct);

        return $"{settings.Value.CdnBaseUrl.TrimEnd('/')}/{key}";
    }

    public async Task DeleteImageAsync(string imageUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return;
        }

        string cdnUrl = settings.Value.CdnBaseUrl.TrimEnd('/');

        if (!imageUrl.StartsWith(cdnUrl, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        string key = imageUrl[cdnUrl.Length..].TrimStart('/');

        DeleteObjectRequest request = new()
        {
            BucketName = settings.Value.BucketName,
            Key = key
        };

        await s3Client.DeleteObjectAsync(request, ct);
    }
}


using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Shop.Business.DTOs;
using Shop.Business.Interfaces;

namespace Shop.Business.Services;

// ═══════════════════════════════════════════════════════════════
// Implementation MinIO Object Storage — Bài 9
// ═══════════════════════════════════════════════════════════════
// Đây là lớp cụ thể (concrete class) implement interface IObjectStorageService
// Sử dụng MinIO .NET SDK (NuGet: Minio)
//
// ĐIỂM QUAN TRỌNG: SDK này tương thích chuẩn S3 API
// → Đổi sang AWS S3 hoặc Cloudflare R2 chỉ cần đổi:
//   - Endpoint (vd: s3.amazonaws.com, xxx.r2.cloudflarestorage.com)
//   - Access Key / Secret Key
//   - Region (nếu cần)
// → Code business logic KHÔNG thay đổi gì (nhờ DIP - SOLID)

public class MinioStorageService : IObjectStorageService
{
    private readonly IMinioClient _minioClient;

    public MinioStorageService(IMinioClient minioClient)
    {
        _minioClient = minioClient;
    }

    // ── Upload file lên MinIO ──────────────────────────────────
    public async Task<UploadResultDto> UploadAsync(
        string bucketName, string objectKey, Stream stream,
        string contentType, string originalFileName)
    {
        // Đảm bảo bucket tồn tại trước khi upload
        await EnsureBucketExistsAsync(bucketName);

        var size = stream.Length;

        // PutObjectArgs: cấu hình cho thao tác upload
        var putArgs = new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectKey)
            .WithStreamData(stream)
            .WithObjectSize(size)
            .WithContentType(contentType);

        await _minioClient.PutObjectAsync(putArgs);

        return new UploadResultDto(bucketName, objectKey, originalFileName, size, contentType);
    }

    // ── Download file từ MinIO ─────────────────────────────────
    public async Task<Stream> DownloadAsync(string bucketName, string objectKey)
    {
        // Tạo MemoryStream để chứa dữ liệu download
        var memoryStream = new MemoryStream();

        var getArgs = new GetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectKey)
            .WithCallbackStream(stream => stream.CopyTo(memoryStream));

        await _minioClient.GetObjectAsync(getArgs);

        memoryStream.Position = 0; // Reset về đầu stream để đọc
        return memoryStream;
    }

    // ── Move/Rename object ─────────────────────────────────────
    // MinIO/S3 không có lệnh "move" trực tiếp
    // Cách làm: Copy sang vị trí mới → Xóa bản gốc
    public async Task<MoveResultDto> MoveAsync(
        string sourceBucket, string sourceKey,
        string destBucket, string destKey)
    {
        await EnsureBucketExistsAsync(destBucket);

        // Bước 1: Copy object sang vị trí mới
        var copySource = new CopySourceObjectArgs()
            .WithBucket(sourceBucket)
            .WithObject(sourceKey);

        var copyArgs = new CopyObjectArgs()
            .WithBucket(destBucket)
            .WithObject(destKey)
            .WithCopyObjectSource(copySource);

        await _minioClient.CopyObjectAsync(copyArgs);

        // Bước 2: Xóa bản gốc
        await DeleteAsync(sourceBucket, sourceKey);

        return new MoveResultDto(sourceBucket, sourceKey, destBucket, destKey);
    }

    // ── Delete object ──────────────────────────────────────────
    public async Task DeleteAsync(string bucketName, string objectKey)
    {
        var removeArgs = new RemoveObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectKey);

        await _minioClient.RemoveObjectAsync(removeArgs);
    }

    // ── Lấy thông tin object (metadata) ────────────────────────
    public async Task<ObjectInfoDto> GetInfoAsync(string bucketName, string objectKey)
    {
        var statArgs = new StatObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectKey);

        var stat = await _minioClient.StatObjectAsync(statArgs);

        return new ObjectInfoDto(
            bucketName,
            stat.ObjectName,
            stat.Size,
            stat.ContentType,
            stat.LastModified
        );
    }

    // ── Liệt kê objects trong bucket ───────────────────────────
    public async Task<List<ObjectInfoDto>> ListObjectsAsync(string bucketName, string? prefix = null)
    {
        var objects = new List<ObjectInfoDto>();

        var listArgs = new ListObjectsArgs()
            .WithBucket(bucketName)
            .WithRecursive(true);

        if (!string.IsNullOrEmpty(prefix))
            listArgs = listArgs.WithPrefix(prefix);

        // MinIO SDK trả về IObservable, cần subscribe để lấy kết quả
        var tcs = new TaskCompletionSource<bool>();
        var observable = _minioClient.ListObjectsAsync(listArgs);

        observable.Subscribe(
            item => objects.Add(new ObjectInfoDto(
                bucketName,
                item.Key,
                (long)item.Size,
                item.ContentType ?? "application/octet-stream",
                item.LastModifiedDateTime ?? DateTime.UtcNow
            )),
            ex => tcs.SetException(ex),
            () => tcs.SetResult(true)
        );

        await tcs.Task;
        return objects;
    }

    // ── Tạo Presigned URL (link tạm thời) ─────────────────────
    // Link này cho phép truy cập PRIVATE object mà không cần xác thực
    // Hết hạn sau expirySeconds giây → an toàn hơn public access
    public async Task<PresignedUrlDto> GetPresignedDownloadUrlAsync(
        string bucketName, string objectKey, int expirySeconds = 3600)
    {
        var presignedArgs = new PresignedGetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectKey)
            .WithExpiry(expirySeconds);

        var url = await _minioClient.PresignedGetObjectAsync(presignedArgs);
        var expiresAt = DateTime.UtcNow.AddSeconds(expirySeconds);

        return new PresignedUrlDto(url, expirySeconds, expiresAt);
    }

    // ── Đảm bảo bucket tồn tại ────────────────────────────────
    public async Task EnsureBucketExistsAsync(string bucketName)
    {
        var existsArgs = new BucketExistsArgs().WithBucket(bucketName);
        bool exists = await _minioClient.BucketExistsAsync(existsArgs);

        if (!exists)
        {
            var makeArgs = new MakeBucketArgs().WithBucket(bucketName);
            await _minioClient.MakeBucketAsync(makeArgs);
        }
    }
}

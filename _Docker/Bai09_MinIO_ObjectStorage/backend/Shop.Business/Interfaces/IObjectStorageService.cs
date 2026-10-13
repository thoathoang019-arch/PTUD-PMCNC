using Shop.Business.DTOs;

namespace Shop.Business.Interfaces;

// ═══════════════════════════════════════════════════════════════
// Interface cho Object Storage — Bài 9
// ═══════════════════════════════════════════════════════════════
// Tuân thủ nguyên lý DIP (Dependency Inversion Principle — SOLID):
//   - Business layer chỉ phụ thuộc vào interface (trừu tượng)
//   - Không phụ thuộc trực tiếp vào MinIO SDK (chi tiết cụ thể)
//   - Có thể đổi sang AWS S3, Cloudflare R2 mà KHÔNG sửa Business layer
//
// Tuân thủ nguyên lý ISP (Interface Segregation Principle — SOLID):
//   - Interface tập trung vào các thao tác Object Storage
//   - Không pha trộn các trách nhiệm khác

public interface IObjectStorageService
{
    /// <summary>
    /// Upload file từ stream lên Object Storage
    /// </summary>
    /// <param name="bucketName">Tên bucket</param>
    /// <param name="objectKey">Object key (đường dẫn trong bucket, vd: images/avatar.jpg)</param>
    /// <param name="stream">Nội dung file</param>
    /// <param name="contentType">MIME type</param>
    /// <param name="originalFileName">Tên file gốc</param>
    Task<UploadResultDto> UploadAsync(string bucketName, string objectKey, Stream stream, string contentType, string originalFileName);

    /// <summary>
    /// Download file từ Object Storage về stream
    /// </summary>
    Task<Stream> DownloadAsync(string bucketName, string objectKey);

    /// <summary>
    /// Move/Rename object (copy rồi xóa bản gốc)
    /// </summary>
    Task<MoveResultDto> MoveAsync(string sourceBucket, string sourceKey, string destBucket, string destKey);

    /// <summary>
    /// Xóa 1 object
    /// </summary>
    Task DeleteAsync(string bucketName, string objectKey);

    /// <summary>
    /// Lấy thông tin 1 object (metadata)
    /// </summary>
    Task<ObjectInfoDto> GetInfoAsync(string bucketName, string objectKey);

    /// <summary>
    /// Liệt kê tất cả object trong bucket (hoặc theo prefix)
    /// </summary>
    Task<List<ObjectInfoDto>> ListObjectsAsync(string bucketName, string? prefix = null);

    /// <summary>
    /// Tạo presigned URL (link tạm thời) để download — PRIVATE access
    /// Ai có link mới truy cập được, link hết hạn sau expirySeconds giây
    /// </summary>
    Task<PresignedUrlDto> GetPresignedDownloadUrlAsync(string bucketName, string objectKey, int expirySeconds = 3600);

    /// <summary>
    /// Đảm bảo bucket tồn tại (tạo nếu chưa có)
    /// </summary>
    Task EnsureBucketExistsAsync(string bucketName);
}

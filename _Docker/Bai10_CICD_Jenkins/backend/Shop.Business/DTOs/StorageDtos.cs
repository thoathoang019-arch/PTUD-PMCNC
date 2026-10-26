namespace Shop.Business.DTOs;

// ═══════════════════════════════════════════════════════════════
// DTOs cho MinIO / Object Storage — Bài 9
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Kết quả sau khi upload file lên Object Storage
/// </summary>
public record UploadResultDto(
    string BucketName,      // Tên bucket chứa file
    string ObjectKey,       // Object key (đường dẫn trong bucket, vd: images/avatar.jpg)
    string FileName,        // Tên file gốc người dùng upload
    long Size,              // Kích thước file (bytes)
    string ContentType      // MIME type (vd: image/jpeg)
);

/// <summary>
/// Thông tin một object trong bucket
/// </summary>
public record ObjectInfoDto(
    string BucketName,
    string ObjectKey,
    long Size,
    string ContentType,
    DateTime LastModified
);

/// <summary>
/// Kết quả tạo presigned URL (link tạm thời)
/// </summary>
public record PresignedUrlDto(
    string Url,             // URL có thể truy cập trực tiếp
    int ExpirySeconds,      // Thời gian hết hạn (giây)
    DateTime ExpiresAt      // Thời điểm hết hạn
);

/// <summary>
/// Kết quả move/rename object
/// </summary>
public record MoveResultDto(
    string SourceBucket,
    string SourceKey,
    string DestBucket,
    string DestKey
);

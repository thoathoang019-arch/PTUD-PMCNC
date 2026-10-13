using Microsoft.AspNetCore.Mvc;
using Shop.Business.Interfaces;

namespace Shop.Presentation.Controllers;

// ═══════════════════════════════════════════════════════════════
// StorageController — API thao tác với MinIO Object Storage (Bài 9)
// ═══════════════════════════════════════════════════════════════
// Cung cấp các endpoint cho: Upload, Download, Move, Delete, List, Presigned URL
// Minh họa cách ứng dụng backend tích hợp Object Storage
//
// Lưu ý: Controller chỉ phụ thuộc vào IObjectStorageService (interface)
//   → Tuân thủ DIP (Dependency Inversion Principle — SOLID)
//   → Không cần sửa controller khi đổi từ MinIO sang AWS S3

[ApiController]
[Route("api/[controller]")]
public class StorageController : ControllerBase
{
    private readonly IObjectStorageService _storage;
    private const string DefaultBucket = "onlineshop-files";

    public StorageController(IObjectStorageService storage)
    {
        _storage = storage;
    }

    // ── 1. UPLOAD file ─────────────────────────────────────────
    // POST /api/storage/upload
    // Form-data: file (IFormFile), folder (optional string)
    [HttpPost("upload")]
    [RequestSizeLimit(50 * 1024 * 1024)] // Giới hạn 50MB
    public async Task<IActionResult> Upload(IFormFile file, [FromForm] string? folder = null)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "Vui lòng chọn file để upload" });

        // Tạo object key với thư mục ảo (virtual folder)
        // Ví dụ: images/avatar_20260923_abc123.jpg
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var safeFileName = Path.GetFileNameWithoutExtension(file.FileName)
            .Replace(" ", "_");
        var extension = Path.GetExtension(file.FileName);
        var objectKey = string.IsNullOrEmpty(folder)
            ? $"{safeFileName}_{timestamp}{extension}"
            : $"{folder.TrimEnd('/')}/{safeFileName}_{timestamp}{extension}";

        using var stream = file.OpenReadStream();
        var result = await _storage.UploadAsync(
            DefaultBucket, objectKey, stream, file.ContentType, file.FileName);

        return Ok(new
        {
            message = "Upload thành công!",
            data = result
        });
    }

    // ── 2. DOWNLOAD file ───────────────────────────────────────
    // GET /api/storage/download?key=images/avatar.jpg
    [HttpGet("download")]
    public async Task<IActionResult> Download([FromQuery] string key)
    {
        if (string.IsNullOrEmpty(key))
            return BadRequest(new { error = "Thiếu parameter 'key'" });

        try
        {
            var info = await _storage.GetInfoAsync(DefaultBucket, key);
            var stream = await _storage.DownloadAsync(DefaultBucket, key);
            var fileName = Path.GetFileName(key);
            return File(stream, info.ContentType, fileName);
        }
        catch (Exception ex)
        {
            return NotFound(new { error = $"Không tìm thấy object: {key}", detail = ex.Message });
        }
    }

    // ── 3. LIỆT KÊ objects ────────────────────────────────────
    // GET /api/storage/list?prefix=images/
    [HttpGet("list")]
    public async Task<IActionResult> ListObjects([FromQuery] string? prefix = null)
    {
        try
        {
            await _storage.EnsureBucketExistsAsync(DefaultBucket);
            var objects = await _storage.ListObjectsAsync(DefaultBucket, prefix);
            return Ok(new
            {
                bucket = DefaultBucket,
                prefix = prefix ?? "(tất cả)",
                count = objects.Count,
                objects
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // ── 4. LẤY THÔNG TIN object ───────────────────────────────
    // GET /api/storage/info?key=images/avatar.jpg
    [HttpGet("info")]
    public async Task<IActionResult> GetInfo([FromQuery] string key)
    {
        if (string.IsNullOrEmpty(key))
            return BadRequest(new { error = "Thiếu parameter 'key'" });

        try
        {
            var info = await _storage.GetInfoAsync(DefaultBucket, key);
            return Ok(info);
        }
        catch (Exception ex)
        {
            return NotFound(new { error = $"Không tìm thấy object: {key}", detail = ex.Message });
        }
    }

    // ── 5. TẠO PRESIGNED URL (link tạm thời / private) ────────
    // GET /api/storage/presigned-url?key=images/avatar.jpg&expiry=3600
    // Link này ai có cũng truy cập được, nhưng hết hạn sau expiry giây
    [HttpGet("presigned-url")]
    public async Task<IActionResult> GetPresignedUrl(
        [FromQuery] string key, [FromQuery] int expiry = 3600)
    {
        if (string.IsNullOrEmpty(key))
            return BadRequest(new { error = "Thiếu parameter 'key'" });

        try
        {
            var result = await _storage.GetPresignedDownloadUrlAsync(DefaultBucket, key, expiry);

            // Thay thế endpoint nội bộ (minio:9000) bằng localhost để trình duyệt truy cập được
            var publicUrl = result.Url.Replace("minio:9000", "localhost:9000");

            return Ok(new
            {
                message = $"Link tạm thời, hết hạn sau {expiry} giây",
                url = publicUrl,
                expirySeconds = result.ExpirySeconds,
                expiresAt = result.ExpiresAt
            });
        }
        catch (Exception ex)
        {
            return NotFound(new { error = $"Không tìm thấy object: {key}", detail = ex.Message });
        }
    }

    // ── 6. MOVE / RENAME object ────────────────────────────────
    // POST /api/storage/move  { "sourceKey": "old.jpg", "destKey": "new.jpg" }
    [HttpPost("move")]
    public async Task<IActionResult> MoveObject([FromBody] MoveRequest request)
    {
        if (string.IsNullOrEmpty(request.SourceKey) || string.IsNullOrEmpty(request.DestKey))
            return BadRequest(new { error = "Cần cả sourceKey và destKey" });

        try
        {
            var result = await _storage.MoveAsync(
                DefaultBucket, request.SourceKey,
                DefaultBucket, request.DestKey);

            return Ok(new
            {
                message = "Move/Rename thành công!",
                data = result
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Move thất bại: {ex.Message}" });
        }
    }

    // ── 7. DELETE object ───────────────────────────────────────
    // DELETE /api/storage/delete?key=images/avatar.jpg
    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteObject([FromQuery] string key)
    {
        if (string.IsNullOrEmpty(key))
            return BadRequest(new { error = "Thiếu parameter 'key'" });

        try
        {
            await _storage.DeleteAsync(DefaultBucket, key);
            return Ok(new { message = $"Đã xóa object: {key}" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Xóa thất bại: {ex.Message}" });
        }
    }

    // ── 8. UPLOAD ẢNH ĐẠI DIỆN (minh họa use case thực tế) ───
    // POST /api/storage/avatar/{userId}
    [HttpPost("avatar/{userId}")]
    [RequestSizeLimit(10 * 1024 * 1024)] // Giới hạn 10MB cho avatar
    public async Task<IActionResult> UploadAvatar(string userId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "Vui lòng chọn ảnh" });

        // Validate chỉ cho phép ảnh
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType))
            return BadRequest(new { error = "Chỉ chấp nhận file ảnh (JPEG, PNG, GIF, WebP)" });

        // Object key cố định theo userId → upload lại sẽ ghi đè (đúng logic avatar)
        var extension = Path.GetExtension(file.FileName);
        var objectKey = $"avatars/{userId}{extension}";

        using var stream = file.OpenReadStream();
        var result = await _storage.UploadAsync(
            DefaultBucket, objectKey, stream, file.ContentType, file.FileName);

        // Tạo presigned URL để hiển thị avatar
        var presigned = await _storage.GetPresignedDownloadUrlAsync(DefaultBucket, objectKey, 86400);
        var publicUrl = presigned.Url.Replace("minio:9000", "localhost:9000");

        return Ok(new
        {
            message = $"Đã cập nhật avatar cho user {userId}",
            avatarUrl = publicUrl,
            data = result
        });
    }
}

// Request model cho Move
public record MoveRequest(string SourceKey, string DestKey);

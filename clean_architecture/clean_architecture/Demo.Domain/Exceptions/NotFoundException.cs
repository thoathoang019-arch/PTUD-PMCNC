namespace Demo.Domain.Exceptions;

/// <summary>
/// Custom exception khi không tìm thấy entity.
/// Được định nghĩa ở Domain layer để các layer khác có thể sử dụng.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"Entity \"{entityName}\" with key ({key}) was not found.")
    {
    }
}

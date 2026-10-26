namespace ECommerce.Shared.Messaging;

// ============================================================
// Các khái niệm nền của giao tiếp BẤT ĐỒNG BỘ (event-driven).
// Dùng chung cho cả bus in-process lẫn RabbitMQ — module chỉ biết các interface này.
// ============================================================

/// <summary>Event tích hợp giữa các module (Integration Event).</summary>
public abstract record IntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>Tên event — dùng để định tuyến (và đặt tên queue logic).</summary>
    public abstract string EventName { get; }
}

/// <summary>Bên XỬ LÝ một loại event (Consumer). Mỗi module tự cài handler của mình.</summary>
public interface IIntegrationEventHandler<in TEvent> where TEvent : IntegrationEvent
{
    Task HandleAsync(TEvent @event, CancellationToken ct = default);
}

/// <summary>Bên PHÁT event (Producer). Order chỉ cần biết interface này để publish.</summary>
public interface IEventBus
{
    Task PublishAsync(IntegrationEvent @event, CancellationToken ct = default);
}

/// <summary>
/// Điều khiển consumer để DEMO tính bất đồng bộ (tắt/bật 1 consumer).
/// Chỉ bus in-process cài; bus RabbitMQ không cài (dùng management UI để pause).
/// </summary>
public interface IConsumerControl
{
    void Pause(string consumerName);
    void Resume(string consumerName);
    IReadOnlyCollection<ConsumerStatus> GetStatus();
}

public sealed record ConsumerStatus(string ConsumerName, string EventName, bool Paused, int Pending, long Processed);

/// <summary>Một đăng ký subscribe: consumer tên gì, nghe event nào, do handler nào xử lý.</summary>
public sealed record EventSubscription(string ConsumerName, Type EventType, Type HandlerType)
{
    public string EventName { get; init; } = EventType.Name;
}

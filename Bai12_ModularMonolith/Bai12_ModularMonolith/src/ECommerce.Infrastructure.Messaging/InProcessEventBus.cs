using System.Threading.Channels;
using ECommerce.Shared.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Messaging;

// ============================================================
// Event bus IN-PROCESS (mặc định) — chạy được NGAY, không cần RabbitMQ broker.
// Mô phỏng đúng ý tưởng của message queue:
//   - fanout: publish 1 event -> đẩy vào queue RIÊNG của từng consumer.
//   - mỗi consumer có 1 Channel (queue) + 1 worker chạy nền, xử lý độc lập.
//   - Pause(consumer): worker ngừng đọc -> message VẪN NẰM CHỜ trong channel (queue),
//     Resume() thì xử lý tiếp — minh hoạ "tắt consumer, message không mất".
// (Bản RabbitMQ thật: xem RabbitMqEventBus.cs)
// ============================================================
public sealed class InProcessEventBus : IEventBus, IConsumerControl, IHostedService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<InProcessEventBus> _log;
    private readonly List<Worker> _workers = new();
    private readonly ILookup<string, Worker> _byEvent;
    private CancellationTokenSource? _cts;

    public InProcessEventBus(IServiceProvider sp, IEnumerable<EventSubscription> subs, ILogger<InProcessEventBus> log)
    {
        _sp = sp;
        _log = log;
        foreach (var s in subs) _workers.Add(new Worker(s));
        _byEvent = _workers.ToLookup(w => w.Sub.EventName);
    }

    public Task PublishAsync(IntegrationEvent @event, CancellationToken ct = default)
    {
        var targets = _byEvent[@event.EventName].ToList();
        _log.LogInformation("[bus] publish {Event} -> {Count} consumer(s)", @event.EventName, targets.Count);
        foreach (var w in targets)
            w.Queue.Writer.TryWrite(@event);   // fanout: mỗi consumer một bản vào queue riêng
        return Task.CompletedTask;
    }

    public void Pause(string consumerName) => Find(consumerName).Pause();
    public void Resume(string consumerName) => Find(consumerName).Resume();

    public IReadOnlyCollection<ConsumerStatus> GetStatus() =>
        _workers.Select(w => new ConsumerStatus(
            w.Sub.ConsumerName, w.Sub.EventName, w.Paused, w.Pending, Interlocked.Read(ref w.Processed))).ToList();

    private Worker Find(string name) =>
        _workers.FirstOrDefault(w => string.Equals(w.Sub.ConsumerName, name, StringComparison.OrdinalIgnoreCase))
        ?? throw new KeyNotFoundException($"Không có consumer tên '{name}'.");

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = new CancellationTokenSource();
        foreach (var w in _workers) _ = RunAsync(w, _cts.Token);
        _log.LogInformation("[bus] in-process bus started with {Count} consumer(s)", _workers.Count);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cts?.Cancel();
        return Task.CompletedTask;
    }

    private async Task RunAsync(Worker w, CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                await w.WaitWhilePausedAsync();                 // paused -> dừng ở đây, message nằm chờ trong queue
                var evt = await w.Queue.Reader.ReadAsync(ct);
                try
                {
                    using var scope = _sp.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService(w.Sub.HandlerType);
                    var method = w.Sub.HandlerType.GetMethod("HandleAsync")!;
                    await (Task)method.Invoke(handler, new object[] { evt, ct })!;
                    Interlocked.Increment(ref w.Processed);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _log.LogError(ex, "[bus] consumer {Name} xử lý lỗi", w.Sub.ConsumerName);
                }
            }
        }
        catch (OperationCanceledException) { /* shutdown bình thường */ }
    }

    // Mỗi consumer = 1 queue (Channel) + trạng thái pause riêng.
    private sealed class Worker
    {
        public EventSubscription Sub { get; }
        public Channel<IntegrationEvent> Queue { get; } = Channel.CreateUnbounded<IntegrationEvent>();
        public long Processed;
        public bool Paused { get; private set; }
        public int Pending => Queue.Reader.Count;

        private volatile TaskCompletionSource _gate = Done();

        public Worker(EventSubscription sub) => Sub = sub;

        public Task WaitWhilePausedAsync() => _gate.Task;

        public void Pause()
        {
            if (Paused) return;
            Paused = true;
            _gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public void Resume()
        {
            if (!Paused) return;
            Paused = false;
            _gate.TrySetResult();
        }

        private static TaskCompletionSource Done()
        {
            var t = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            t.SetResult();
            return t;
        }
    }
}

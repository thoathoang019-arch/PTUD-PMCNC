using System.Text;
using System.Text.Json;
using ECommerce.Shared.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ECommerce.Infrastructure.Messaging;

// ============================================================
// Event bus RabbitMQ THẬT (bật khi Messaging:Mode = "RabbitMq").
//   - Khai báo 1 fanout exchange "ecommerce.events".
//   - Mỗi consumer một QUEUE DURABLE riêng, bind vào exchange.
//   - Consumer tắt -> queue durable vẫn giữ message -> bật lại xử lý tiếp (không mất).
// Chạy kèm RabbitMQ broker (xem docker-compose.yml).
// ============================================================
public sealed class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string Exchange { get; set; } = "ecommerce.events";
}

public sealed class RabbitMqEventBus : IEventBus, IHostedService, IDisposable
{
    private readonly IServiceProvider _sp;
    private readonly IReadOnlyList<EventSubscription> _subs;
    private readonly RabbitMqOptions _opt;
    private readonly ILogger<RabbitMqEventBus> _log;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMqEventBus(IServiceProvider sp, IEnumerable<EventSubscription> subs,
        IOptions<RabbitMqOptions> opt, ILogger<RabbitMqEventBus> log)
    {
        _sp = sp;
        _subs = subs.ToList();
        _opt = opt.Value;
        _log = log;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _opt.HostName,
            Port = _opt.Port,
            UserName = _opt.UserName,
            Password = _opt.Password,
            DispatchConsumersAsync = true
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Fanout: message đẩy tới MỌI queue được bind, bất kể routing key.
        _channel.ExchangeDeclare(_opt.Exchange, ExchangeType.Fanout, durable: true);

        foreach (var sub in _subs)
        {
            var queue = $"{sub.ConsumerName}.{sub.EventName}";
            _channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(queue, _opt.Exchange, routingKey: "");

            var captured = sub;
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (_, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var evt = (IntegrationEvent)JsonSerializer.Deserialize(json, captured.EventType)!;

                    using var scope = _sp.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService(captured.HandlerType);
                    var method = captured.HandlerType.GetMethod("HandleAsync")!;
                    await (Task)method.Invoke(handler, new object[] { evt, CancellationToken.None })!;

                    _channel!.BasicAck(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "[rabbit] consumer {Name} lỗi, requeue", captured.ConsumerName);
                    _channel!.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
                }
            };
            _channel.BasicConsume(queue, autoAck: false, consumer);
            _log.LogInformation("[rabbit] bind + consume queue {Queue}", queue);
        }
        return Task.CompletedTask;
    }

    public Task PublishAsync(IntegrationEvent @event, CancellationToken ct = default)
    {
        if (_channel is null) throw new InvalidOperationException("RabbitMQ channel chưa sẵn sàng.");

        var json = JsonSerializer.Serialize(@event, @event.GetType());
        var body = Encoding.UTF8.GetBytes(json);

        var props = _channel.CreateBasicProperties();
        props.Persistent = true;                 // message lưu xuống đĩa -> không mất khi broker restart
        props.ContentType = "application/json";

        _channel.BasicPublish(_opt.Exchange, routingKey: "", basicProperties: props, body: body);
        _log.LogInformation("[rabbit] published {Event}", @event.EventName);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Dispose();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        try { _channel?.Close(); _connection?.Close(); } catch { /* ignore khi shutdown */ }
        _channel?.Dispose();
        _connection?.Dispose();
    }
}

using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace VideoProcessorWorker.Infra.Messaging
{
    public class KafkaProducer : IKafkaProducer, IAsyncDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<KafkaProducer> _logger;

        public KafkaProducer(IOptions<KafkaSettings> options, ILogger<KafkaProducer> logger)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = options.Value.BootstrapServers,
                Acks = Acks.All,
                EnableIdempotence = true,
                MessageSendMaxRetries = 3
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
            _logger = logger;
        }

        public async Task ProduceAsync<T>(string topic, string key, T payload, CancellationToken cancellationToken = default)
        {
            var envelope = new EventEnvelope<T> { Data = payload };
            var json = JsonSerializer.Serialize(envelope);

            var message = new Message<string, string>
            {
                Key = key,
                Value = json
            };

            try
            {
                var result = await _producer.ProduceAsync(topic, message, cancellationToken);
                _logger.LogInformation("Produced to {Topic} [{Partition}] @ {Offset}",
                    result.Topic, result.Partition.Value, result.Offset.Value);
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex, "Failed to produce to {Topic}", topic);
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            _producer.Flush(TimeSpan.FromSeconds(10));
            _producer.Dispose();
            await Task.CompletedTask;
        }
    }
}

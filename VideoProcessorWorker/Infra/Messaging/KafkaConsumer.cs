using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace VideoProcessorWorker.Infra.Messaging
{
    public class KafkaConsumer<TKey, TValue> : IKafkaConsumer<TKey, TValue>, IDisposable
    {
        private readonly IConsumer<TKey, TValue> _consumer;
        private readonly ILogger<KafkaConsumer<TKey, TValue>> _logger;

        public KafkaConsumer(
            IOptions<KafkaSettings> options, 
            ILogger<KafkaConsumer<TKey, TValue>> logger,
            IDeserializer<TKey>? keyDeserializer = null,
            IDeserializer<TValue>? valueDeserializer = null)
        {
            _logger = logger;

            var config = new ConsumerConfig
            {
                BootstrapServers = options.Value.BootstrapServers,
                GroupId = options.Value.ConsumerGroupId ?? "video-processor-worker-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                EnableAutoOffsetStore = false
            };

            var builder = new ConsumerBuilder<TKey, TValue>(config);

            if (keyDeserializer != null)
            {
                builder.SetKeyDeserializer(keyDeserializer);
            }

            if (valueDeserializer != null)
            {
                builder.SetValueDeserializer(valueDeserializer);
            }

            _consumer = builder.Build();
        }

        public void Subscribe(IEnumerable<string> topics)
        {
            _consumer.Subscribe(topics);
            _logger.LogInformation("Subscribed to topics: {Topics}", string.Join(", ", topics));
        }

        public void Subscribe(string topic)
        {
            _consumer.Subscribe(topic);
            _logger.LogInformation("Subscribed to topic: {Topic}", topic);
        }

        public ConsumeResult<TKey, TValue>? Consume(CancellationToken cancellationToken = default)
        {
            try
            {
                return _consumer.Consume(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Error consuming message: {Reason}", ex.Error.Reason);
                throw;
            }
        }

        public ConsumeResult<TKey, TValue>? Consume(TimeSpan timeout)
        {
            try
            {
                return _consumer.Consume(timeout);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Error consuming message: {Reason}", ex.Error.Reason);
                throw;
            }
        }

        public void Commit(ConsumeResult<TKey, TValue> result)
        {
            _consumer.Commit(result);
        }

        public void Close()
        {
            _consumer.Close();
        }

        public void Dispose()
        {
            _consumer.Dispose();
        }
    }
}

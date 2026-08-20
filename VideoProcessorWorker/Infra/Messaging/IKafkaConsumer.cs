using Confluent.Kafka;

namespace VideoProcessorWorker.Infra.Messaging
{
    public interface IKafkaConsumer<TKey, TValue>
    {
        void Subscribe(IEnumerable<string> topics);
        void Subscribe(string topic);
        ConsumeResult<TKey, TValue>? Consume(CancellationToken cancellationToken = default);
        ConsumeResult<TKey, TValue>? Consume(TimeSpan timeout);
        void Commit(ConsumeResult<TKey, TValue> result);
        void Close();
    }
}

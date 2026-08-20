namespace VideoProcessorWorker.Infra.Messaging
{
    public interface IKafkaProducer
    {
        Task ProduceAsync<T>(string topic, string key, T payload, CancellationToken cancellationToken = default);
    }
}

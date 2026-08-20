namespace VideoProcessorWorker.Infra.Messaging
{
    public class EventEnvelope<T>
    {
        public string EventType { get; set; } = typeof(T).Name;
        public string EventId { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
        public T Data { get; set; } = default!;
    }
}

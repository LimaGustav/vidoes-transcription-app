namespace VideoProcessorWorker.Application.GenerateSubtitles
{
    public interface IGenerateSubtitlesUseCase
    {
        Task<bool> ExecuteAsync(MinioNotificationEvent notification, CancellationToken cancellationToken = default);
    }
}

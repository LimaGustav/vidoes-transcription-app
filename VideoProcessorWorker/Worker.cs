using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using VideoProcessorWorker.Application.GenerateSubtitles;
using VideoProcessorWorker.Infra.Messaging;

namespace VideoProcessorWorker
{
    public class Worker : BackgroundService
    {
        private readonly IKafkaConsumer<string, string> _kafkaConsumer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly KafkaSettings _kafkaSettings;
        private readonly ILogger<Worker> _logger;

        public Worker(
            IKafkaConsumer<string, string> kafkaConsumer,
            IServiceScopeFactory scopeFactory,
            IOptions<KafkaSettings> kafkaOptions,
            ILogger<Worker> logger)
        {
            _kafkaConsumer = kafkaConsumer;
            _scopeFactory = scopeFactory;
            _kafkaSettings = kafkaOptions.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var topic = _kafkaSettings.MinioVideoUploadedTopic ?? "minio.video.uploaded";
            _logger.LogInformation("Iniciando Worker de Processamento de Vídeo. Subscribing no tópico Kafka: {Topic}", topic);
            
            _kafkaConsumer.Subscribe(topic);

            // Roda o loop de consumo do Kafka em uma thread separada/background Task para não bloquear
            await Task.Yield();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _kafkaConsumer.Consume(stoppingToken);
                    if (consumeResult == null || consumeResult.Message == null)
                    {
                        continue;
                    }

                    var messageValue = consumeResult.Message.Value;
                    _logger.LogInformation("Mensagem Kafka recebida da partição {Partition} no offset {Offset}",
                        consumeResult.Partition.Value, consumeResult.Offset.Value);

                    if (string.IsNullOrWhiteSpace(messageValue))
                    {
                        _kafkaConsumer.Commit(consumeResult);
                        continue;
                    }

                    MinioNotificationEvent? notificationEvent = null;
                    try
                    {
                        notificationEvent = JsonSerializer.Deserialize<MinioNotificationEvent>(messageValue, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Falha ao desserializar mensagem JSON do MinIO: {RawMessage}", messageValue);
                        // Comita offset de mensagem corrompida para evitar loop infinito
                        _kafkaConsumer.Commit(consumeResult);
                        continue;
                    }

                    if (notificationEvent == null)
                    {
                        _logger.LogWarning("Mensagem do MinIO vazia ou inválida.");
                        _kafkaConsumer.Commit(consumeResult);
                        continue;
                    }

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var useCase = scope.ServiceProvider.GetRequiredService<IGenerateSubtitlesUseCase>();
                        var processed = await useCase.ExecuteAsync(notificationEvent, stoppingToken);

                        if (processed)
                        {
                            _kafkaConsumer.Commit(consumeResult);
                            _logger.LogInformation("Offset {Offset} comitado com sucesso.", consumeResult.Offset.Value);
                        }
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Consumo do Kafka interrompido por cancelamento.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro inesperado ao processar mensagem do Kafka.");
                    // Pequeno atraso para evitar flood de logs em caso de erro transiente
                    await Task.Delay(2000, stoppingToken);
                }
            }

            _logger.LogInformation("Fechando conexão do Kafka Consumer...");
            try
            {
                _kafkaConsumer.Close();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao fechar Kafka Consumer.");
            }
        }
    }
}

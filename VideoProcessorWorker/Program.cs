using Microsoft.EntityFrameworkCore;
using VideoProcessorWorker;
using VideoProcessorWorker.Application.GenerateSubtitles;
using VideoProcessorWorker.Infra.Messaging;
using VideoProcessorWorker.Infra.Persistence.Contexts;
using VideoProcessorWorker.Infra.Storage;
using VideoProcessorWorker.Services.Audio;
using VideoProcessorWorker.Services.Transcription;


var builder = Host.CreateApplicationBuilder(args);

// Configurations
builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("Kafka"));
builder.Services.Configure<MinioSettings>(builder.Configuration.GetSection("Minio"));
builder.Services.Configure<WhisperSettings>(builder.Configuration.GetSection("Whisper"));
builder.Services.Configure<FfmpegSettings>(builder.Configuration.GetSection("Ffmpeg"));

// Database Context
builder.Services.AddDbContext<VideoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("VideoServiceDb")));

// Kafka Services
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();
builder.Services.AddTransient(typeof(IKafkaConsumer<,>), typeof(KafkaConsumer<,>));

// MinIO / Storage Service
builder.Services.AddSingleton<IObjectStorageService, MinioObjectStorageService>();

// Audio & Transcription Services
builder.Services.AddTransient<IAudioExtractionService, FfmpegAudioExtractionService>();
builder.Services.AddSingleton<ITranscriptionService, WhisperTranscriptionService>();

// Application Use Cases
builder.Services.AddScoped<IGenerateSubtitlesUseCase, GenerateSubtitlesUseCase>();

// Worker Background Service
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();


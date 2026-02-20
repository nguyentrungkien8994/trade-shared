using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.AppTest;

var builder = Host.CreateApplicationBuilder(args);
string kafka_bootstrap = builder.Configuration["KAFKA_BOOTSTRAP"];
string kafka_group = builder.Configuration["KAFKA_GROUP_ID"];
string kafka_topic = builder.Configuration["KAFKA_TOPIC"];
string mongoDbUri = builder.Configuration["MONGODB_URI"];
string mongoDbDatabase = builder.Configuration["MONGODB_DATABASE"];
// Add services to the container.

// Kafka options
builder.Services.AddConfluentKafka(kafka_bootstrap, kafka_group, kafka_topic);

// Background worker
builder.Services.AddHostedService<KafkaConsumerWorker>();
var host = builder.Build();
await host.RunAsync();

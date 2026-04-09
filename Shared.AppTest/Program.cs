using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.AppTest;
using Shared.Database.MongoDb;
using Shared.Logger.Serilog;
using Shared.Telegram;

var builder = Host.CreateApplicationBuilder(args);
string kafka_bootstrap = builder.Configuration["KAFKA_BOOTSTRAP"];
string kafka_group = builder.Configuration["KAFKA_GROUP_ID"];
string kafka_topic = builder.Configuration["KAFKA_TOPIC"];
string mongoDbUri = builder.Configuration["MONGODB_URI"];
string mongoDbDatabase = builder.Configuration["MONGODB_DATABASE"];
string neo4jDbUri = builder.Configuration["NEO4JDB_URI"];
string neo4jUsername = builder.Configuration["NEO4JDB_USERNAME"];
string neo4jPassword = builder.Configuration["NEO4JDB_PASSWORD"];
string telegramToken = builder.Configuration["TELEGRAM_TOKEN"];
string strTeleChatId = builder.Configuration["TELEGRAM_CHATID"];
long.TryParse(strTeleChatId, out long teleChatId);
// Add services to the container.

// Kafka options
//builder.Services.AddConfluentKafka(kafka_bootstrap, kafka_group, kafka_topic);
builder.Services.AddOpenAI();
// Background worker
builder.Services.AddHostedService<KafkaConsumerWorker>();
builder.Services.AddSerilog(builder.Configuration);
builder.Services.UseDatabaseMongoDb(mongoDbUri, mongoDbDatabase);
builder.Services.UseDatabaseNeo4j(neo4jDbUri, neo4jUsername, neo4jPassword);
builder.Services.AddOracle("Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP) (HOST=172.20.91.100)(PORT=1521))) (CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=SHAREDATA))); User Id=SHARE_GRAPH;Password=V1sualiz@t!0n;");
builder.Services.AddSingleton(new TelegramConfig
{
    BotToken = telegramToken, // "8247270418:AAGyelnBHXnYX1TGl80O7B-3Ta7PqB7wS2Q",
    ChatId = teleChatId// -1003629499475
});
builder.Services.AddRedis(option =>
{
    option.ConnectionString = "localhost:6379";
    option.InstanceName = "tradeapp:";
});
builder.Services.AddSingleton<ITelegramTopicService, TelegramTopicService>();
var host = builder.Build();
await host.RunAsync();

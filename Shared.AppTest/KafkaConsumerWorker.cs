using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared.Kafka;
using Shared.OpenAI;
using System.Reflection;

namespace Shared.AppTest
{
    public sealed class KafkaConsumerWorker : BackgroundService
    {
        private readonly ILogger<KafkaConsumerWorker> _logger;
        //private readonly IKafkaConsumer _kafkaConsumer;
        //private readonly IKafkaProducer _kafkaProducer;
        //private readonly IOptions<KafkaOptions> _options;
        //private readonly IServiceProvider _provider;
        //private readonly ITradeCommandParser _tradeCommandParser;
        

        //private readonly ITradeCommandParser _tradeCommandParser;

        public KafkaConsumerWorker(ILogger<KafkaConsumerWorker> logger
            //IKafkaConsumer kafkaConsumer,
            //ITradeCommandParser tradeCommandParser,
            //IOptions<KafkaOptions> options,
            //IServiceProvider provider,
            //IKafkaProducer kafkaProducer
            )
        {
            //_kafkaConsumer = kafkaConsumer;
            //_options = options;
            //_provider = provider;
            //_kafkaProducer = kafkaProducer;
            //_tradeCommandParser = tradeCommandParser;
            _logger = logger;
        }
        private string ImageToBase64(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Image not found", filePath);

            byte[] imageBytes = File.ReadAllBytes(filePath);
            return Convert.ToBase64String(imageBytes);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("info");
                _logger.LogWarning("warning");
                _logger.LogError("error");
                //string imgBase64 = ImageToBase64(Path.Combine(Environment.CurrentDirectory,"imgs","image.png"));
                //var rs = await _tradeCommandParser.ParseImageAsync(imgBase64);
                //var rs = await _tradeCommandParser.ParseAsync("eth spot 1947 5% stop");
                //string msg = "limit short Chillguy: 73960 SL: 75k (2% risk) :Short: LIMIT Chillguy | Entry: 73960 | SL: 75000 (≤ 1.41%) | Risk: 2.0% Status: ⏳ Valid limit order • Today at 8:16 PM :peepo_wg: Position Overview , Use the buttons below to set your balance and risk. Image 💰 Set My Balance... , 🎯 Override Risk (%)... , , Only you can see this • Dismiss message , 8:46 PM";
                //msg = msg.Replace("WG Bot replying to WG Bot","");
                //if (msg.Contains("Status"))
                //{
                //    msg = msg.Split("Status")[0];
                //}
                //var rs = await _tradeCommandParser.ParseAsync(msg);
                //await _kafkaProducer.ProduceAsync("test", "test", "test");
                //await _kafkaConsumer.ConsumeAsync(_options.Value.Topic, HandleMessage, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        
        private async Task HandleMessage(string? key, string value)
        {
            // TODO: xử lý nghiệp vụ tại đây
            try
            {
                _logger.LogInformation($"Process message id {key ?? Guid.Empty.ToString()}");
                if (string.IsNullOrWhiteSpace(value)) return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
}

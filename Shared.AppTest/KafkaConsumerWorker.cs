using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared.Kafka;
using System.Reflection;

namespace Shared.AppTest
{
    public sealed class KafkaConsumerWorker : BackgroundService
    {
        private readonly ILogger<KafkaConsumerWorker> _logger;
        private readonly IKafkaConsumer _kafkaConsumer;
        private readonly IKafkaProducer _kafkaProducer;
        private readonly IOptions<KafkaOptions> _options;
        private readonly IServiceProvider _provider;

        public KafkaConsumerWorker(IKafkaConsumer kafkaConsumer,
            ILogger<KafkaConsumerWorker> logger,
            IOptions<KafkaOptions> options,
            IServiceProvider provider,
            IKafkaProducer kafkaProducer)
        {
            _kafkaConsumer = kafkaConsumer;
            _options = options;
            _logger = logger;
            _provider = provider;
          
            _kafkaProducer = kafkaProducer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await _kafkaProducer.ProduceAsync("test", "test", "test");
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

using Core.Database;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Newtonsoft.Json;
using Shared.AppTest.Entities;
using Shared.Database.Neo4j;
using Shared.Database.Neo4j.Requests;
using Shared.Database.Neo4j.Service;
using Shared.Telegram;

namespace Shared.AppTest
{
    public sealed class KafkaConsumerWorker : BackgroundService
    {
        private readonly IServiceBase<Customer, ObjectId, IRepositoryBase<Customer, ObjectId>> _serviceBase;
        private readonly IServiceBaseNeo4j<CustomerNeo4j, string> _serviceBaseNeo4j;
        private readonly ILogger<KafkaConsumerWorker> _logger;
        private readonly ITelegramTopicService _topicService;
        //private readonly IKafkaConsumer _kafkaConsumer;
        //private readonly IKafkaProducer _kafkaProducer;
        //private readonly IOptions<KafkaOptions> _options;
        //private readonly IServiceProvider _provider;
        //private readonly ITradeCommandParser _tradeCommandParser;


        //private readonly ITradeCommandParser _tradeCommandParser;

        public KafkaConsumerWorker(ILogger<KafkaConsumerWorker> logger,
            ITelegramTopicService topicService,
            IServiceBase<Customer, ObjectId, IRepositoryBase<Customer, ObjectId>> serviceBase,
            IServiceBaseNeo4j<CustomerNeo4j, string> serviceBaseNeo4j
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
            _topicService = topicService;
            _logger = logger;
            _serviceBase = serviceBase;
            _serviceBaseNeo4j = serviceBaseNeo4j;
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
                //_logger.LogInformation("info");
                //_logger.LogWarning("warning");
                //_logger.LogError("error");
                //int effects = await _serviceBase1.InsertAsync(new Customer()
                //{
                //    code = "CODE001",
                //    name = "Nguyen Van C",
                //    created_at = DateTime.UtcNow.Ticks,
                //    updated_at = DateTime.UtcNow.Ticks,
                //    created_by = "admin",
                //    updated_by = "admin"
                //});
                //var a = await _serviceBase.GetAllAsync();

               await  _topicService.SendMessageAsync(243, "<b>Test</b> html");
                //await _serviceBaseNeo4j.InsertAsync(new CustomerNeo4j()
                //{
                //    id = Guid.NewGuid().ToString(),
                //    code = "CODE001",
                //    name = "Nguyen Van A",
                //    created_at = DateTime.UtcNow.Ticks,
                //    updated_at = DateTime.UtcNow.Ticks,
                //    created_by = "admin",
                //    updated_by = "admin"
                //});
                //await _serviceBaseNeo4j.GetAllAsync();

                //await _serviceBaseNeo4j.DeleteAsync("acc43449-8b28-4609-9f7e-04be2cb2bbc8");
                //string json = "{\"node\":\"Company\",\"filter\":{\"id\":{\"$gt\":2},\"Status\":\"ACTIVE\",\"$or\":[{\"Balance\":{\"$gte\":1000}},{\"Vip\":true}]}}";
                //string json = "{\"node\":\"Company\",\"filter\":{\"$or\":[{\"id\":{\"$gte\":3}},{\"code\":\"Vinpearl\"}]}}";
                //SearchParam searchParam = JsonConvert.DeserializeObject<SearchParam>(json);
                //Utils utils = new();
                //var b = utils.Parse(json);
                //string query = "MATCH(n:Company)-[r]-(c) RETURN n,r,c";
                //var c = await _serviceBaseNeo4j.SearchNode(new Database.Neo4j.Responses.CypherQuery() { Query = query, Params = null });

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

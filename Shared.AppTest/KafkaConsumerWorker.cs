using KLib.Core.Database;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Newtonsoft.Json;
using Shared.AppTest.Entities;
using Shared.AppTest.Entities.Oracle;
using Shared.Database.Neo4j.Requests;
using Shared.Database.Neo4j.Service;
using Shared.Database.Oracle.Service;
using Shared.OpenAI;
using Shared.Redis;
using Shared.Telegram;

namespace Shared.AppTest
{
    public sealed class KafkaConsumerWorker : BackgroundService
    {
        private readonly IServiceBaseOracle _oracleService;
        private readonly IServiceBaseOracle<Company, int> _oracleServiceCompany;
        //private readonly IServiceBase<Customer, ObjectId, IRepositoryBase<Customer, ObjectId>> _serviceBase;
        private readonly IServiceBaseNeo4j _serviceBaseNeo4j;
        private readonly ILogger<KafkaConsumerWorker> _logger;
        private readonly ITelegramTopicService _topicService;
        //private readonly IKafkaConsumer _kafkaConsumer;
        //private readonly IKafkaProducer _kafkaProducer;
        //private readonly IOptions<KafkaOptions> _options;
        //private readonly IServiceProvider _provider;
        private readonly ITradeCommandParser _tradeCommandParser;
        private readonly IRedisStreamService _redisStreamService;


        //private readonly ITradeCommandParser _tradeCommandParser;

        public KafkaConsumerWorker(ILogger<KafkaConsumerWorker> logger,
            ITelegramTopicService topicService,
            //IServiceBase<Customer, ObjectId, IRepositoryBase<Customer, ObjectId>> serviceBase,
            IServiceBaseNeo4j serviceBaseNeo4j,
            IServiceBaseOracle oracleService,
        //IKafkaConsumer kafkaConsumer,
        ITradeCommandParser tradeCommandParser,
            IRedisStreamService redisStreamService
            //IOptions<KafkaOptions> options,
            //IServiceProvider provider,
            //IKafkaProducer kafkaProducer
            )
        {
            //_kafkaConsumer = kafkaConsumer;
            //_options = options;
            //_provider = provider;
            //_kafkaProducer = kafkaProducer;
            _tradeCommandParser = tradeCommandParser;
            _topicService = topicService;
            _logger = logger;
            _serviceBaseNeo4j = serviceBaseNeo4j;
            _oracleService = oracleService;
            _redisStreamService = redisStreamService;

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

                //mongodb
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

                //Telegram
                // StringBuilder sb = new();
                // sb.AppendLine("🟢 LONG BTC");
                // sb.AppendLine("📈 Entry: 65,000");
                // sb.AppendLine("🚀 Target: 70,000");
                //await  _topicService.SendMessageAsync(243, sb.ToString());


                //neo4j
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
                //string json = "{\"node\":\"TaxPayer\",\"filter\":{\"taxCode\":{\"$eq\":\"A\"}},\"relations\":[{\"type\":\"HAS_BUCKET\",\"direction\":\"out\",\"target\":\"InvoiceBucket\",\"depth\":{\"min\":1,\"max\":3}},{\"type\":\"TO\",\"direction\":\"in\",\"target\":\"Invoice\"}]}";
                //string json = "{\"node\":\"TaxPayer\",\"filter\":{\"taxCode\":{\"$eq\":\"A\"}}}";
                //string json = "{\"node\":\"TaxPayer\",\"target\":{\"node\":\"InvoiceBucket\"}}";
                //string json = "{\"node\":\"Company\",\"filter\":{\"$or\":[{\"id\":{\"$gt\":3}},{\"code\":{\"$eq\":\"Vinhomes\"}}]}}";
                //string json = "{\"node\":\"Company\",\"filter\":{\"$and\":[{\"id\":{\"$gt\":3}},{\"code\":{\"$eq\":\"Vinhomes\"}}]}}";
                //string json = "{\"node\":\"Company\",\"filter\":{\"$and\":[{\"id\":{\"$gte\":2}},{\"code\":{\"$neq\":\"Vinhomes\"}}]}}";
                string json = "{\"node\":\"Company\",\"filter\":{\"$or\":[{\"$and\":[{\"id\":{\"$gte\":2}},{\"code\":{\"$neq\":\"Vinhomes\"}}]},{\"code\":{\"$eq\":\"Vinhomes\"}}]}}";
                SearchParam searchParam = JsonConvert.DeserializeObject<SearchParam>(json);
                var c = await _serviceBaseNeo4j.SearchNode(searchParam);
                var a = 1;
                //Utils utils = new();
                //var b = utils.Parse(json);
                //var c = await _serviceBaseNeo4j.SearchNode(new Database.Neo4j.Responses.CypherQuery() { Query = b.Query, Params = b.Params });
                //var a = await _serviceNeo4j.GetAllObjectAsync("TaxPayer");

                //Oracle
                //var filters = new Dictionary<string, object>
                //{
                //    { "$and", new object[]
                //        {
                //            new Dictionary<string, object>
                //            {
                //                { "SYNC_STATUS", new Dictionary<string, object> { { "$eq", 0 } } }
                //            },
                //            new Dictionary<string, object>
                //            {
                //                { "TID", new Dictionary<string, object> { { "$eq", "ATB" } } }
                //            }
                //        }
                //    }
                //};
                //var filters = new Dictionary<string, object>
                //            {
                //                {"$or", new object[]{
                //                        new Dictionary<string, object>
                //                        {
                //                            { "id", new Dictionary<string, object> { { "$gt", 4891160 } } }
                //                        },
                //                        new Dictionary<string, object>
                //                        {
                //                            { "id", new Dictionary<string, object> { { "$eq", 1 } } }
                //                        }

                //                } }
                //            };
                //var pagingObject = await _oracleService.PagingObjectAsync("SYS_PERSON",1,50);
                //var c = await _serviceBaseNeo4j.UpSertNodeAsync(pagingObject.Data, "SYS_PERSON", "ID");
                //var rels = pagingObject.Data.Select(x => new Shared.Database.Neo4j.Requests.Relationship() { 
                //    FromId = x.FID,
                //    ToId = x.TID,
                //    FromNode = x.FNODE,
                //    ToNode = x.TNODE,
                //    RelationName = x.RELATION_NAME
                //});
                //await _serviceBaseNeo4j.UpSertRelationshipAsync(rels,fromKey:"ID", toKey:"TICKER");
                //await _serviceBaseNeo4j.UpSertNodeAsync(pagingObject.Data,"TICKER");
                //var b = 1;



                //OpenAI
                //var rs = await _tradeCommandParser.ParseAsync("LONG LIMIT TAO Entry: 312.8 SL: 304.6 (≤ 2.62%) Risk: 2.0% ");
                //trade parser
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

                //Kafka
                //await _kafkaProducer.ProduceAsync("test", "test", "test");
                //await _kafkaConsumer.ConsumeAsync(_options.Value.Topic, HandleMessage, stoppingToken);

                //Redis
                //const string stream = "trade.test";
                //const string group = "order-group";
                //var consumer = Environment.MachineName;
                ////await _redisStreamService.AddAsync<object>(stream, "test message");

                //await _redisStreamService.CreateConsumerGroupAsync(stream, group);
                //while (true)
                //{
                //    var messages = await _redisStreamService.ReadGroupAsync<object>(
                //    stream,
                //    group,
                //    consumer,
                //    count: 10);
                //    if (messages.Count > 0)
                //    {
                //        foreach ( var message in messages)
                //        {
                //            Console.WriteLine(message.Data.ToString());
                //        }
                //    }   
                //    else Console.WriteLine("Nothing");
                //    Thread.Sleep(3000);
                //}
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

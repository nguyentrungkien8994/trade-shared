using KLib.Core.Database;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Newtonsoft.Json;
using Shared.AppTest.Entities;
using Shared.AppTest.Entities.Oracle;
using Shared.Database.Neo4j.Requests;
using Shared.Database.Neo4j.Responses;
using Shared.Database.Neo4j.Service;
using Shared.Database.Oracle.Service;
using Shared.OpenAI;
using Shared.Redis;
using Shared.Telegram;
using System.Text.Json;

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

        private const int PersonCount = 1_000_000;
        private const int TransactionCount = 2_000_000;
        private const int BatchSize = 5000;
        // =========================
        // 1. PERSON
        // =========================
        private async Task CreatePersons()
        {
            try
            {
                for (int i = 0; i < PersonCount; i += BatchSize)
                {
                    var batch = Enumerable.Range(i, BatchSize)
                        .Select(x => new Dictionary<string, object>()
                        {
                        {"Mst",GenerateMst(x) },
                        { "Name", GenerateName(x) }
                        });

                    int effects = await _serviceBaseNeo4j.UpSertNodeAsync(batch, "Person", "Mst");

                    Console.WriteLine($"Inserted Persons: {effects}");
                }
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        // =========================
        // 2. TRANSACTION 
        // =========================
        private async Task CreateTransactions()
        {
            var rand = new Random();

            for (int i = 0; i < TransactionCount; i += BatchSize)
            {
                var batch = Enumerable.Range(i, BatchSize)
                         .Select(x => new Dictionary<string, object>()
                         {
                            {"Id", $"TX-{Guid.NewGuid().ToString("N")}" },
                            {"Amount", rand.Next(1000, 1_000_000) },
                            {"TransactionDate", DateTime.UtcNow.AddDays(- rand.Next(0, 1000)).AddHours(- rand.Next(0,24)).AddMinutes(-rand.Next(0,60))}
                         });

                int effects = await _serviceBaseNeo4j.UpSertNodeAsync(batch, "Transaction", "Id");
                Console.WriteLine($"Inserted Persons: {effects}");
            }
        }

        public IEnumerable<Shared.Database.Neo4j.Requests.Relationship> GenerateRelationships(
    List<string> personMsts,
    List<string> transactionIds)
        {
            if (transactionIds.Count < personMsts.Count)
                throw new Exception("Transaction phải >= Person để đảm bảo mỗi person có ít nhất 1 transaction");

            var rand = new Random();

            // =========================
            // Phase 1: đảm bảo mỗi person có >= 1 transaction
            // =========================
            for (int i = 0; i < personMsts.Count; i++)
            {
                var from = personMsts[i];
                var to = personMsts[rand.Next(personMsts.Count)];
                var tx = transactionIds[i];

                foreach (var rel in BuildTriplet(from, to, tx))
                    yield return rel;
            }

            // =========================
            // Phase 2: phần còn lại
            // =========================
            for (int i = personMsts.Count; i < transactionIds.Count; i++)
            {
                var from = personMsts[rand.Next(personMsts.Count)];
                var to = personMsts[rand.Next(personMsts.Count)];
                var tx = transactionIds[i];

                foreach (var rel in BuildTriplet(from, to, tx))
                    yield return rel;
            }
        }

        private IEnumerable<Shared.Database.Neo4j.Requests.Relationship> BuildTriplet(
    string fromMst,
    string toMst,
    string txId)
        {
            // 1. HAS
            yield return new Shared.Database.Neo4j.Requests.Relationship
            {
                FromNode = "Person",
                FromId = fromMst,
                ToNode = "Transaction",
                ToId = txId,
                RelationName = "HAS"
            };

            // 2. TO
            yield return new Shared.Database.Neo4j.Requests.Relationship
            {
                FromNode = "Transaction",
                FromId = txId,
                ToNode = "Person",
                ToId = toMst,
                RelationName = "TO"
            };

            // 3. SELL (derived)
            yield return new Shared.Database.Neo4j.Requests.Relationship
            {
                FromNode = "Person",
                FromId = fromMst,
                ToNode = "Person",
                ToId = toMst,
                RelationName = "SELL"
            };
        }

        private int Hash(int x)
        {
            unchecked
            {
                x ^= (x << 13);
                x ^= (x >> 17);
                x ^= (x << 5);
                return x;
            }
        }
        private string GenerateMst(int seed)
        {
            // không tăng dần → hash
            return $"MST-{Math.Abs(Hash(seed)) % 1_000_000_000:D9}";
        }
        readonly string[] LastNames = new[]
{
    "Nguyễn","Trần","Lê","Phạm","Hoàng","Huỳnh","Phan",
    "Vũ","Võ","Đặng","Bùi","Đỗ","Hồ","Ngô","Dương","Lý"
};
        readonly string[] MiddleNames = new[]
{
    "Văn","Thị","Hữu","Đức","Ngọc","Quang","Thanh",
    "Minh","Xuân","Hoàng","Gia","Anh","Bảo","Phúc"
};
        readonly string[] FirstNames = new[]
{
    "An","Bình","Cường","Dũng","Hạnh","Khánh","Long",
    "Nam","Phúc","Quân","Sơn","Trang","Tuấn","Vy",
    "Yến","Linh","Hải","Hiếu","Tâm","Thảo"
};
        private string GenerateName(int seed)
        {
            var rand = new Random(Hash(seed));

            string lastName = WeightedLastName(rand);
            string middleName = MiddleNames[rand.Next(MiddleNames.Length)];
            string firstName = FirstNames[rand.Next(FirstNames.Length)];

            // thêm 1 tên đệm phụ (tăng diversity)
            if (rand.NextDouble() < 0.3)
            {
                string extra = MiddleNames[rand.Next(MiddleNames.Length)];
                return $"{lastName} {middleName} {extra} {firstName}";
            }

            return $"{lastName} {middleName} {firstName}";
        }
        private string WeightedLastName(Random rand)
        {
            int r = rand.Next(100);

            if (r < 30) return "Nguyễn";   // ~30%
            if (r < 42) return "Trần";     // ~12%
            if (r < 52) return "Lê";       // ~10%
            if (r < 60) return "Phạm";
            if (r < 68) return "Hoàng";
            if (r < 75) return "Phan";
            if (r < 82) return "Vũ";
            if (r < 88) return "Đặng";
            if (r < 94) return "Bùi";

            return LastNames[rand.Next(LastNames.Length)];
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {

                //await CreatePersons();
                //await CreateTransactions();
                var persons2 = await _serviceBaseNeo4j.SearchNodeByCypherRawAsync<object>("MATCH (a:Person {Mst: \"MST-658790018\"}),\r\n      (b:Person)\r\nMATCH p = shortestPath((a)-[:SELL*1..100]->(b))\r\nRETURN p LIMIT 100");
                var persons = await _serviceBaseNeo4j.SearchNodeByCypherRawAsync<string>("Match(n:Person) return n.Mst");
                var trans = await _serviceBaseNeo4j.SearchNodeByCypherRawAsync<string>("Match(n:Transaction) return n.Id");

                List<string> listPerson = new();
                List<string> listTrans = new();
                //List<string> listPerson = persons.ToList();
                //List<string> listTrans = trans.ToList();

                var rels = GenerateRelationships(listPerson, listTrans);
                var grouped = rels.GroupBy(x => new { x.FromNode, x.ToNode, x.RelationName }).ToList();
                foreach (var group in grouped)
                {
                    int max = group.Count();
                    string fKey = "";
                    string tKey = "";
                    var obj = group.FirstOrDefault();
                    if (obj != null)
                    {
                        if (obj.FromNode.Equals("Person", StringComparison.OrdinalIgnoreCase))
                            fKey = "Mst";
                        else if (obj.FromNode.Equals("Transaction", StringComparison.OrdinalIgnoreCase))
                            fKey = "Id";

                        if (obj.ToNode.Equals("Person", StringComparison.OrdinalIgnoreCase))
                            tKey = "Mst";
                        else if (obj.ToNode.Equals("Transaction", StringComparison.OrdinalIgnoreCase))
                            tKey = "Id";
                    }
                    for (int i = 0; i < max; i += BatchSize)
                    {
                        var chunks = group.Skip(i).Take(BatchSize);

                        var effects = await _serviceBaseNeo4j.UpSertRelationshipAsync(chunks, fKey, tKey);
                        Console.WriteLine($"Inserted Relationship: {effects}");
                    }
                }

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
                //string json = "{\"node\":\"SYS_PERSON\",\"relations\":[{\"type\":\"HOLD\",\"direction\":\"out\",\"depth\":{\"min\":1,\"max\":3}}],\"target\":{\"Node\":\"SYS_COMPANY\"}}";
                //string json = "{\"node\":\"SYS_PERSON\"}";
                //string json = "{\"node\":\"TaxPayer\",\"target\":{\"node\":\"InvoiceBucket\"}}";
                //string json = "{\"node\":\"Company\",\"filter\":{\"$or\":[{\"id\":{\"$gt\":3}},{\"code\":{\"$eq\":\"Vinhomes\"}}]}}";
                //string json = "{\"node\":\"Company\",\"filter\":{\"$and\":[{\"id\":{\"$gt\":3}},{\"code\":{\"$eq\":\"Vinhomes\"}}]}}";
                //string json = "{\"node\":\"Company\",\"filter\":{\"$and\":[{\"id\":{\"$gte\":2}},{\"code\":{\"$neq\":\"Vinhomes\"}}]}}";
                //string json = "{\"node\":\"SYS_PERSON\",\"filter\":{\"$or\":[{\"$and\":[{\"id\":{\"$gte\":2}},{\"SYNC_STATUS\":{\"$eq\":\"Vinhomes\"}}]},{\"code\":{\"$eq\":\"Vinhomes\"}}]}}";
                //string json = "{\"node\":\"SYS_PERSON\",\"filter\":{\"$and\":[{\"SYNC_STATUS\":{\"$eq\":0}}]}}";
                //string json = "{\"node\":\"TaxPayer\",\"filter\":{\"taxCode\":{\"$eq\":\"C\"}},\"relations\":[{\"type\":\"HAS_BUCKET\",\"direction\":\"out\",\"depth\":{\"min\":1,\"max\":3}}]}";
                //string json = "{\"node\":\"SYS_PERSON\",\"filter\":{\"@elementId\":{\"$eq\":\"4:956c80ef-2014-41f2-b04c-07ae8ef32f12:89\"}},\"relations\":[{\"type\":\"\",\"direction\":\"out\",\"depth\":{\"min\":1,\"max\":3}}]}";
                //string json = "{\"node\":\"SYS_PERSON\",\"filter\":{\"@elementId\":{\"$eq\":\"4:956c80ef-2014-41f2-b04c-07ae8ef32f12:1516\"}},\"relations\":[{\"type\":\"HOLD\",\"depth\":{\"min\":1,\"max\":3}}],\"target\":{\"node\":\"SYS_PERSON\",\"filter\":{\"@elementId\":{\"$eq\":\"4:956c80ef-2014-41f2-b04c-07ae8ef32f12:89\"}}}}";
                //SearchParam searchParam = JsonConvert.DeserializeObject<SearchParam>(json);
                //var c = await _serviceBaseNeo4j.SearchNode(searchParam);
                //var a = 1;
                //Utils utils = new();
                //var b = utils.Parse(json);
                //var c = await _serviceBaseNeo4j.SearchNode(new Database.Neo4j.Responses.CypherQuery() { Query = b.Query, Params = b.Params });
                //var a = await _serviceNeo4j.GetAllObjectAsync("TaxPayer");
                //var cypher = new CypherQuery()
                //{
                //    Query = "MATCH(n:SYS_PERSON) WHERE n.ID=\"242\" SET n.FULLNAME=\"Lê Quốc Bình 1\" RETURN n"
                //};
                //var a = await _serviceBaseNeo4j.SearchNodeByRawCypherAsync(cypher);
                //var b = 1;

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
                //var rs = await _tradeCommandParser.ParseAsync("LONG BASED Entry: 0.1923 SL: 0.1805 (≤ 6.14%) Risk: 0.5% TPs: ✓ 0.2144 (50%) R/R: 4.17R");
                //var a = 1;
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

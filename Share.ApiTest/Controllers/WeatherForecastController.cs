using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Shared.Database.Neo4j;
using Shared.Database.Neo4j.Requests;
using Shared.Database.Neo4j.Service;
using Shared.Redis;

namespace Share.ApiTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IServiceBaseNeo4j<CustomerNeo4j, string> _baseService;
        private readonly IRedisStreamService _redisStreamService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IServiceBaseNeo4j<CustomerNeo4j, string> baseService, IRedisStreamService redisStreamService)
        {
            _logger = logger;
            _baseService = baseService;
            _redisStreamService = redisStreamService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            //string json = "{\"relations\":[{\"type\":\"HAS_BUCKET\"}]}";
            //string json = "{\"relations\":[{\"type\":\"HAS_BUCKET\"},{\"type\":\"DIEU_HANH\"}]}";
            //string json = "{\"node\":\"TaxPayer\",\"filter\":{\"taxCode\":{\"$eq\":\"A\"}},\"relations\":[{\"type\":\"HAS_BUCKET\"}]}";
            //string json = "{\"node\":\"TaxPayer\",\"filter\":{\"taxCode\":{\"$eq\":\"A\"}},\"relations\":[{\"type\":\"HAS_BUCKET\",\"direction\":\"out\",\"depth\":{\"min\":1,\"max\":50}},{\"type\":\"TO\",\"direction\":\"in\"}]}";
            //string json = "{\"node\":\"TaxPayer\",\"filter\":{\"taxCode\":{\"$eq\":\"A\"}},\"relations\":[{\"type\":\"HAS_BUCKET\",\"direction\":\"out\",\"target\":\"InvoiceBucket\",\"depth\":{\"min\":1,\"max\":3}},{\"type\":\"TO\",\"direction\":\"in\",\"target\":\"Invoice\"}]}";
            //string json = "{\"node\":\"TaxPayer\"}";
            //string json = "{\"node\":\"TaxPayer\",\"target\":{\"node\":\"InvoiceBucket\"}}";
            //string json = "";
            //Utils utils = new();
            //var cypher = utils.Parse(json);

            //var result = await _baseService.SearchNode(new Shared.Database.Neo4j.Responses.CypherQuery
            //{
            //    Query = cypher.Query,
            //    Params = cypher.Params  
            //});
            const string stream = "trade.test";
            const string group = "order-group";
            await _redisStreamService.AddAsync<object>(stream, "test message redis");
            return Ok("ok");
        }
    }
}

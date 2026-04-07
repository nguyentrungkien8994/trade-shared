using Microsoft.AspNetCore.Mvc;
using Shared.Database.Neo4j.Service;

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

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IServiceBaseNeo4j<CustomerNeo4j, string> baseService)
        {
            _logger = logger;
            _baseService = baseService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            string query = "MATCH (a:TaxPayer {taxCode:'A'})\r\nMATCH (b:TaxPayer {taxCode:'D'})\r\nMATCH path = allShortestPaths((a)-[*]-(b))\r\nRETURN path;";
            //string query = "MATCH(m:TaxPayer) RETURN m";
            var result = await _baseService.SearchNode(new Shared.Database.Neo4j.Responses.CypherQuery
            {
                Query = query
            });
            return Ok(result);
        }
    }
}

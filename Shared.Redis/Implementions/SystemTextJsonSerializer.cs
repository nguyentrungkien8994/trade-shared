
using Newtonsoft.Json;

namespace Shared.Redis
{
    public class SystemTextJsonSerializer : IRedisSerializer
    {

        public string Serialize<T>(T value)
            => JsonConvert.SerializeObject(value);

        public T? Deserialize<T>(string value)
        {
            var _options = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            return JsonConvert.DeserializeObject<T>(value, _options);
        }
    }
}

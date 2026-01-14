using Newtonsoft.Json;
using System.Text;

namespace Shared.Telegram
{
    public class TelegramTopicService : ITelegramTopicService
    {
        private readonly HttpClient _httpClient;
        private readonly TelegramConfig _config;

        public TelegramTopicService(
            TelegramConfig config)
        {
            _httpClient = new HttpClient();
            _config = config;
        }

        private string BaseUrl =>
            $"https://api.telegram.org/bot{_config.BotToken}";

        // ===============================
        // CREATE TOPIC
        // ===============================
        public async Task<CreateResult?> CreateTopicAsync(
            string topicName,
            CancellationToken ct = default)
        {
            var url = $"{BaseUrl}/createForumTopic";

            var payload = new
            {
                chat_id = _config.ChatId,
                name = topicName
            };

            var content = BuildJsonContent(payload);

            var response = await _httpClient.PostAsync(url, content, ct);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();
            CreateTopicResult? res = JsonConvert.DeserializeObject<CreateTopicResult>(body);
            return res?.result;
        }

        // ===============================
        // SEND MESSAGE TO TOPIC
        // ===============================
        public async Task SendMessageAsync(
            int topicId,
            string message,
            CancellationToken ct = default)
        {
            var url = $"{BaseUrl}/sendMessage";

            var payload = new
            {
                chat_id = _config.ChatId,
                message_thread_id = topicId,
                text = message
            };

            var content = BuildJsonContent(payload);

            var response = await _httpClient.PostAsync(url, content, ct);
            response.EnsureSuccessStatusCode();
        }

        // ===============================
        // HELPER
        // ===============================
        private HttpContent BuildJsonContent(object payload)
        {
            var json = JsonConvert.SerializeObject(payload);

            return new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );
        }
    }
}

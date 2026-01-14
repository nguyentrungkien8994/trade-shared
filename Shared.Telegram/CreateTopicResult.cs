namespace Shared.Telegram
{
    public class CreateTopicResult
    {
        public bool ok { get; set; }
        public CreateResult result { get; set; }
    }
    public class CreateResult
    {
        public int message_thread_id { get; set; }
        public string name { get; set; }
    }
}

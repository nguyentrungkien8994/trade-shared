using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Redis
{
    public interface IRedisPubSubService
    {
        Task PublishAsync<T>(string channel, T message);

        Task SubscribeAsync<T>(
            string channel,
            Func<T, Task> handler);
    }
}

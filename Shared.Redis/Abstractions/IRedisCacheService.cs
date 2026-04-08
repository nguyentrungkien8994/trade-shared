using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Redis
{
    public interface IRedisCacheService
    {
        Task<T?> GetAsync<T>(string key);

        Task<bool> SetAsync<T>(
            string key,
            T value,
            TimeSpan? expiry = null);

        Task<bool> RemoveAsync(string key);

        Task<bool> ExistsAsync(string key);
    }
}

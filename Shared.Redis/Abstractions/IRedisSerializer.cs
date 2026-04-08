using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Redis
{
    public interface IRedisSerializer
    {
        string Serialize<T>(T value);
        T? Deserialize<T>(string value);
    }
}

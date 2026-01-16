using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Helper;

public class ConfigHelper
{
    public static string GetConfigByKey(string key, IConfiguration _configuration)
    {
        string? strVal = Environment.GetEnvironmentVariable(key);
        strVal = strVal ?? _configuration[key];
        return strVal ?? "";
    }
}

using NLog.Config;
using NLog.Targets;

namespace TRADE.SHARED.KAFKA;

public class NLogConfiguration
{
    public static LoggingConfiguration GetConfig() {
        NLog.Config.LoggingConfiguration config = new NLog.Config.LoggingConfiguration();

        var consoleTarget = new ColoredConsoleTarget("console")
        {
            Layout =
            "${when:when=level==LogLevel.Fatal:inner=\u001b[1;37;41m}" +   // trắng trên nền đỏ
            "${when:when=level==LogLevel.Error:inner=\u001b[31m}" +        // đỏ
            "${when:when=level==LogLevel.Warn:inner=\u001b[33m}" +         // vàng
            "${when:when=level==LogLevel.Info:inner=\u001b[32m}" +         // xanh lá
            "${when:when=level==LogLevel.Debug:inner=\u001b[36m}" +        // cyan
            "${longdate} | ${level:uppercase=true} | ${message} ${exception:format=toString}\u001b[0m"
        };


        // Define targets for each log level
        string baseLogDirectory = Path.Combine(Environment.CurrentDirectory, "logs", "nlog");
        var infoTarget = CreateFileTarget("infoFile", Path.Combine(baseLogDirectory, "Info", "info-${shortdate}.log"));
        var warningTarget = CreateFileTarget("warningFile", Path.Combine(baseLogDirectory, "Warning", "warning-${shortdate}.log"));
        var errorTarget = CreateFileTarget("errorFile", Path.Combine(baseLogDirectory, "Error", "error-${shortdate}.log"));
        var debugTarget = CreateFileTarget("debugFile", Path.Combine(baseLogDirectory, "Debug", "debug-${shortdate}.log"));

        // Add rules for each level
        config.AddRuleForOneLevel(NLog.LogLevel.Info, infoTarget);
        config.AddRuleForOneLevel(NLog.LogLevel.Warn, warningTarget);
        config.AddRuleForOneLevel(NLog.LogLevel.Error, errorTarget);
        config.AddRuleForOneLevel(NLog.LogLevel.Debug, debugTarget);

        config.AddRuleForOneLevel(NLog.LogLevel.Info, consoleTarget);
        config.AddRuleForOneLevel(NLog.LogLevel.Warn, consoleTarget);
        config.AddRuleForOneLevel(NLog.LogLevel.Error, consoleTarget);
        config.AddRuleForOneLevel(NLog.LogLevel.Debug, consoleTarget);
        return config;
    }
    private static FileTarget CreateFileTarget(string name, string fileName)
    {
        var target = new FileTarget(name)
        {
            FileName = fileName.Replace("\\", "/"), // Ensure consistent path separators
            Layout = "${longdate} | ${level:uppercase=true} | ${logger} | ${message} ${exception:format=ToString,StackTrace}",
            KeepFileOpen = false,
            ArchiveEvery = FileArchivePeriod.Day,
            Encoding = System.Text.Encoding.UTF8
        };

        // Ensure directory exists
        var directory = Path.GetDirectoryName(fileName);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return target;
    }
}

using System;
using System.IO;
using System.Reflection;
using Serilog;
using Serilog.Events;
using MyApp.Core.Contracts;
using IAppLogger = MyApp.Core.Contracts.Services.ILogger;

namespace MyApp.Core.Impl.Services;

/// <summary>
/// Реализация ILogger в домене Core с записью в файл через Serilog.
/// </summary>
[OtherDomainImplementation]
public class RealLogger : MarshalByRefObject, IAppLogger
{
    private Serilog.Core.Logger? _serilogger;

    public RealLogger()
    {
        var basePathForLog = Path.GetDirectoryName(typeof(RealLogger).Assembly.Location);
        var logDirectory = Path.Combine(basePathForLog ?? Path.GetTempPath(), "logs");
        if (!Directory.Exists(logDirectory))
            Directory.CreateDirectory(logDirectory);

        _serilogger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(Path.Combine(logDirectory, "myapp_engine-.txt"), rollingInterval: RollingInterval.Day)
            .CreateLogger();

        Log("RealLogger инициализирован в отдельном домене");
    }

    public void Log(string message)
    {
        _serilogger?.Write(LogEventLevel.Information, "{Message}", message);
    }

    public void LogError(string message, Exception? ex = null)
    {
        if (ex != null)
            _serilogger?.Write(LogEventLevel.Error, ex, "{Message}", message);
        else
            _serilogger?.Write(LogEventLevel.Error, "{Message}", message);
    }
}

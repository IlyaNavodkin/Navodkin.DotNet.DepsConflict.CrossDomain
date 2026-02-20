using System;

namespace MyApp.Core.Contracts.Services;

/// <summary>
/// Абстракция логирования для вызова через границу AppDomain.
/// </summary>
public interface ILogger
{
    void Log(string message);
    void LogError(string message, Exception? ex = null);
}

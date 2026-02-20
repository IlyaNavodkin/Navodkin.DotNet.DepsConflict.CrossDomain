using Autodesk.Revit.UI;
using MyApp.Core.Contracts.Services;

namespace MyApp.Core.Contracts.RevitCommands.SampleCommand;

/// <summary>
/// Простой сервис примера: логирование и показ диалога приветствия.
/// </summary>
public class SimpleDimpleService
{
    private readonly ILogger? _logger;

    public SimpleDimpleService(ILogger? logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Пишет сообщение в лог и показывает диалог "Hello world".
    /// </summary>
    public void SayHello()
    {
        _logger?.Log("SimpleDimpleService.SayHello");
        TaskDialog.Show("Sample", "Hello world");
    }
}

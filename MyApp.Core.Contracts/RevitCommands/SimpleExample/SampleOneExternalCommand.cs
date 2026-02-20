using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MyApp.Core.Contracts.Services;
using MyApp.Core.Contracts.RevitCommands.SampleCommand;
using MyApp.Core.Contracts.DepContainer;

namespace MyApp.Core.Contracts.RevitCommands.SimpleExample;

/// <summary>
/// Команда ленты Revit: показывает приветствие (Hello world) и пишет в лог.
/// </summary>
[Transaction(TransactionMode.Manual)]
public sealed class SampleOneExternalCommand : IExternalCommand
{
    /// <summary>
    /// Выполняет команду: логирование и диалог приветствия.
    /// </summary>
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var logger = DIContainerContext.Current?.Resolve<ICoreModule>()?.Container.Resolve<ILogger>();
        logger?.Log("Run command SampleOneExternalCommand");

        try
        {
            var service = new SimpleDimpleService(logger);
            service.SayHello();
            return Result.Succeeded;
        }
        catch (Exception exception)
        {
            logger?.LogError("SampleOneExternalCommand failed", exception);
            return Result.Failed;
        }
    }
}

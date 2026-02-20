using System.Diagnostics;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MyApp.Core.Contracts;
using MyApp.Core.Contracts.DepContainer;
using MyApp.Core.Contracts.Services;
using MyApp.Modules.TestModule.Contracts;

namespace MyApp.Modules.TestModule.Contracts.RevitCommands.RandomDataPresent;

/// <summary>
/// Команда ленты Revit: открывает окно со случайными данными (домен Core).
/// </summary>
[Transaction(TransactionMode.ReadOnly)]
public sealed class ShowRandomDataWindowCommand : IExternalCommand
{
    /// <summary>Выполняет команду: вызов ITestSampleModule.ShowRandomDataWindow().</summary>
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var logger = DIContainerContext.Current?.Resolve<ICoreModule>()?.Container.Resolve<ILogger>();
        logger?.Log("Команда ShowRandomDataWindow выполнена");

        var testModule = DIContainerContext.Current?.Resolve<ITestSampleModule>();
        if (testModule == null)
        {
            message = "Внешний домен не инициализирован (Core домен).";
            return Result.Failed;
        }

        try
        {
            testModule.ShowRandomDataWindow();
            logger?.Log("ShowRandomDataWindow завершён (окно закрыто)");
            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            logger?.LogError("Ошибка открытия окна", ex);
            message = ex.Message;
            return Result.Failed;
        }
    }
}

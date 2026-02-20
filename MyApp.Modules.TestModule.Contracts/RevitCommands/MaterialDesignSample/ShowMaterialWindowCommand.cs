using System.Diagnostics;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MyApp.Core.Contracts;
using MyApp.Core.Contracts.DepContainer;
using MyApp.Core.Contracts.Services;
using MyApp.Modules.TestModule.Contracts;

namespace MyApp.Modules.TestModule.Contracts.RevitCommands.MaterialDesignSample;

/// <summary>
/// Команда ленты Revit: открывает демо-окно Material Design (модально, домен Core).
/// </summary>
[Transaction(TransactionMode.ReadOnly)]
public sealed class ShowMaterialWindowCommand : IExternalCommand
{
    /// <summary>Выполняет команду: вызов ITestSampleModule.ShowMaterialWindow().</summary>
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var logger = DIContainerContext.Current?.Resolve<ICoreModule>()?.Container.Resolve<ILogger>();
        logger?.Log("Команда ShowMaterialWindow выполнена");

        var testModule = DIContainerContext.Current?.Resolve<ITestSampleModule>();
        if (testModule == null)
        {
            message = "Внешний домен не инициализирован (Core домен).";
            return Result.Failed;
        }

        try
        {
            testModule.ShowMaterialWindow();
            logger?.Log("ShowMaterialWindow завершён (окно закрыто)");
            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            logger?.LogError("Ошибка открытия окна Material", ex);
            message = ex.Message;
            return Result.Failed;
        }
    }
}

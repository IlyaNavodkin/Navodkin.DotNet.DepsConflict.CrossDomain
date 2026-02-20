using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MyApp.Core.Contracts.Dto;
using MyApp.Core.Contracts.Services;
using MyApp.Modules.TestModule.Contracts;
using MyApp.Modules.TestModule.Contracts.ViewModels;
using MyApp.Modules.TestModule.Contracts.Data.Commands;
using MyApp.Modules.TestModule.Contracts.Data.Queries;
using System.ComponentModel.Design;
using MyApp.Core.Contracts.DepContainer;
using MyApp.Core.Contracts;
using MyApp.Modules.TestModule.Contracts.Data.Queries;
using MyApp.Modules.TestModule.Contracts.Data.Commands;


namespace MyApp.Modules.TestModule.Contracts.RevitCommands.NoWaitExecute;

/// <summary>
/// Команда ленты Revit: открывает немодальное окно HandyControl со списком стен, выделением и удалением.
/// </summary>
[Transaction(TransactionMode.Manual)]
public sealed class ShowHandyControlWindowCommand : IExternalCommand
{
    /// <summary>Выполняет команду: создание провайдеров и вызов ITestSampleModule.ShowHandyControlWindow().</summary>
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var logger = DIContainerContext.Current?.Resolve<ICoreModule>()?.Container.Resolve<ILogger>();
        logger?.Log("Команда ShowHandyControlWindow выполнена");

        var testModule = DIContainerContext.Current?.Resolve<ITestSampleModule>();
        if (testModule == null)
        {
            message = "Внешний домен не инициализирован (Core домен).";
            return Result.Failed;
        }

        var doc = commandData.Application.ActiveUIDocument.Document;

        var revitTask = DIContainerContext.Current?.Resolve<RevitTask>();
        if (revitTask == null)
        {
            message = "RevitTask не зарегистрирован в контейнере.";
            return Result.Failed;
        }

        var viewModel = new WallsViewModel();
        var wallsProvider = new WallsProvider(commandData.Application, doc, revitTask);
        var selectionService = new ElementSelectionService(commandData.Application, revitTask);
        var handler = new DeleteCommandHandler(commandData.Application, doc, revitTask);

        testModule.ShowHandyControlWindow(viewModel, wallsProvider, selectionService, handler);
        logger?.Log("Окно HandyControl открыто (немодальное)");
        return Result.Succeeded;
    }
}

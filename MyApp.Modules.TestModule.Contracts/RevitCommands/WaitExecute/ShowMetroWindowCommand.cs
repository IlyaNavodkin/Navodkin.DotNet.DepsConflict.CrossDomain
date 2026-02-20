using System.Diagnostics;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MyApp.Core.Contracts;
using MyApp.Core.Contracts.DepContainer;
using MyApp.Core.Contracts.Services;
using MyApp.Modules.TestModule.Contracts;
using MyApp.Modules.TestModule.Contracts.ViewModels;

namespace MyApp.Modules.TestModule.Contracts.RevitCommands.WaitExecute;

/// <summary>
/// Команда ленты Revit: открывает модальное окно MahApps.Metro со списком стен, после закрытия удаляет выбранную стену.
/// </summary>
[Transaction(TransactionMode.Manual)]
public sealed class ShowMetroWindowCommand : IExternalCommand
{
    /// <summary>Выполняет команду: сбор списка стен, вызов ShowMetroWindow(), удаление выбранной стены.</summary>
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var logger = DIContainerContext.Current?.Resolve<ICoreModule>()?.Container.Resolve<ILogger>();
        logger?.Log("Команда ShowMetroWindow выполнена");

        var testModule = DIContainerContext.Current?.Resolve<ITestSampleModule>();
        if (testModule == null)
        {
            message = "Внешний домен не инициализирован (Core домен).";
            return Result.Failed;
        }

        var doc = commandData.Application.ActiveUIDocument?.Document;
        if (doc == null)
        {
            message = "Нет активного документа.";
            return Result.Failed;
        }

        var viewModel = CollectWallsViewModel(doc);

        try
        {
            var selectedId = testModule.ShowMetroWindow(viewModel);
            logger?.Log("ShowMetroWindow завершён (окно закрыто)");

            if (!string.IsNullOrEmpty(selectedId))
            {
                DeleteWallById(doc, selectedId);
                logger?.Log($"Стена {selectedId} удалена.");
            }

            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            logger?.LogError("Ошибка открытия окна Metro", ex);
            message = ex.Message;
            return Result.Failed;
        }
    }

    private static WallsViewModel CollectWallsViewModel(Document doc)
    {
        var viewModel = new WallsViewModel();
        var collector = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_Walls)
            .WhereElementIsNotElementType();
        foreach (Element el in collector)
        {
            viewModel.Walls.Add(new WallItem
            {
                Id = el.Id.IntegerValue.ToString(),
                Name = el.Name ?? string.Empty
            });
        }
        return viewModel;
    }

    private static void DeleteWallById(Document doc, string? wallId)
    {
        if (string.IsNullOrEmpty(wallId))
            return;
        var elementId = new ElementId(int.Parse(wallId));
        var element = doc.GetElement(elementId);
        if (element == null)
            return;
        using var tx = new Transaction(doc, "Удаление стены");
        tx.Start();
        doc.Delete(element.Id);
        tx.Commit();
    }
}

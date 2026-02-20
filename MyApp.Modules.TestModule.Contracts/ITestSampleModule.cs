using MyApp.Core.Contracts;
using MyApp.Modules.TestModule.Contracts.ViewModels;
using MyApp.Modules.TestModule.Contracts.Data.Queries;
using MyApp.Modules.TestModule.Contracts.Data.Commands;

namespace MyApp.Modules.TestModule.Contracts;

/// <summary>
/// Контроллер модуля в домене Core: открытие окон, работа с Excel. Вызывается из домена Revit через прокси.
/// </summary>
public interface ITestSampleModule : IOtherDomainModule
{
    /// <summary>Открывает окно со случайными данными (немодально).</summary>
    void ShowRandomDataWindow();

    /// <summary>Открывает демо-окно Material Design (модально).</summary>
    void ShowMaterialWindow();

    /// <summary>Открывает окно со списком стен (MahApps.Metro), блокирует до закрытия; возвращает выбранный id стены или null.</summary>
    string? ShowMetroWindow(WallsViewModel viewModel);

    /// <summary>Открывает окно HandyControl без ожидания; wallsProvider — список стен, selectionService — выделение в Revit, deleteReceiver — удаление элемента.</summary>
    void ShowHandyControlWindow(WallsViewModel viewModel, IWallsProvider wallsProvider, IElementSelectionService selectionService, IDeleteCommandHandler deleteReceiver);

    /// <summary>Создаёт временный Excel-файл в указанной папке.</summary>
    void CreateTempExcelFile(string targetDirectory);
}

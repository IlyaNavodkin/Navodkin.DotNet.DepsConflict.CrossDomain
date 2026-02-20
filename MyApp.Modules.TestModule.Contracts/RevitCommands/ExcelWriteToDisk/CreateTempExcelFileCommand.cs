using System.Diagnostics;
using System.IO;
using System.Reflection;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MyApp.Core.Contracts;
using MyApp.Core.Contracts.DepContainer;
using MyApp.Core.Contracts.Services;
using MyApp.Modules.TestModule.Contracts;

namespace MyApp.Modules.TestModule.Contracts.RevitCommands.ExcelWriteToDisk;

/// <summary>
/// Команда ленты Revit: создаёт временный Excel-файл в папке сборки и открывает проводник.
/// </summary>
[Transaction(TransactionMode.ReadOnly)]
public sealed class CreateTempExcelFileCommand : IExternalCommand
{
    /// <summary>Выполняет команду: вызов ITestSampleModule.CreateTempExcelFile() и открытие проводника.</summary>
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var logger = DIContainerContext.Current?.Resolve<ICoreModule>()?.Container.Resolve<ILogger>();
        logger?.Log("Команда CreateTempExcelFile выполнена");

        var testModule = DIContainerContext.Current?.Resolve<ITestSampleModule>();
        if (testModule == null)
        {
            message = "Внешний домен не инициализирован (Core домен).";
            return Result.Failed;
        }

        string targetDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            ?? Path.GetTempPath();

        try
        {
            testModule.CreateTempExcelFile(targetDir);
            logger?.Log("CreateTempExcelFile завершён");
            Process.Start("explorer.exe", $"/select,\"{targetDir}\"");
            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            logger?.LogError("Ошибка создания Excel", ex);
            message = ex.Message;
            return Result.Failed;
        }
    }
}

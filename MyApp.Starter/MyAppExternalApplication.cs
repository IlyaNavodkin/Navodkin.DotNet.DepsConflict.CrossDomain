using Autodesk.Revit.UI;
using MyApp.Core.Contracts.Config;
using MyApp.Core.Contracts.Services;
using MyApp.Core.Contracts.DepContainer;
using MyApp.Core.Contracts.RevitCommands.SimpleExample;
using MyApp.Modules.TestModule.Contracts;
using MyApp.Modules.TestModule.Contracts.RevitCommands.ExcelWriteToDisk;
using MyApp.Modules.TestModule.Contracts.RevitCommands.MaterialDesignSample;
using MyApp.Modules.TestModule.Contracts.RevitCommands.NoWaitExecute;
using MyApp.Modules.TestModule.Contracts.RevitCommands.RandomDataPresent;
using MyApp.Modules.TestModule.Contracts.RevitCommands.WaitExecute;
using MyApp.Core.Contracts;
using MyApp.Core.Contracts.Domain;

namespace MyApp.Starter;

/// <summary>
/// Точка входа плагина Revit: инициализация домена Core, DI-контейнера и ленты с командами.
/// </summary>
public class MyAppExternalApplication : IExternalApplication
{
    private const string TabName = "MyApp";
    private const string PanelName = "Инструменты";
    private static readonly string CoreCommandsAssemblyPath = typeof(SampleOneExternalCommand).Assembly.Location;
    private static readonly string TestModuleCommandsAssemblyPath = typeof(ShowHandyControlWindowCommand).Assembly.Location;

    private static ServiceContainer? _container;

    /// <summary>
    /// Вызывается при завершении Revit: логирование, выгрузка домена Core, сброс контейнера.
    /// </summary>
    public Result OnShutdown(UIControlledApplication application)
    {
        try
        {
            var container = _container;
            if (container != null)
            {
                container.Resolve<ICoreModule>()?
                    .Container.Resolve<ILogger>()?
                    .Log("Revit плагин MyApp завершает работу");
            }
            OtherAppDomainManager.Shutdown();
        }
        finally
        {
            DIContainerContext.SetCurrent(null);
            _container?.Dispose();
            _container = null;
        }
        return Result.Succeeded;
    }

    /// <summary>
    /// Вызывается при запуске Revit: создание контейнера, инициализация домена Core, создание ленты, регистрация RevitTask.
    /// </summary>
    public Result OnStartup(UIControlledApplication application)
    {
        try
        {
            _container = new ServiceContainer();

            DIContainerContext.SetCurrent(_container);

            OtherAppDomainManager.Initialize(AppDomainManagerConfig.Default);

            var logger = _container.Resolve<ICoreModule>()?.Container.Resolve<ILogger>();
            if (logger == null)
            {
                TaskDialog.Show("MyApp", "Не удалось инициализировать логгер.");
                return Result.Failed;
            }

            logger.Log("Revit плагин MyApp запущен");

            CreateRibbon(application);

            logger.Log("Startup complete");

            var controller = _container.Resolve<ITestSampleModule>();
            controller?.ShowMaterialWindow();

            var revitTask = new RevitTask();

            _container.AddSingleton<RevitTask>(_ => revitTask);

            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            TaskDialog.Show("Ribbon Sample", ex.ToString());
            return Result.Failed;
        }
    }

    /// <summary>
    /// Создаёт вкладку и панель ленты Revit с кнопками команд (данные, Material, Metro, HandyControl, Excel, Hello).
    /// </summary>
    private static void CreateRibbon(UIControlledApplication application)
    {
        application.CreateRibbonTab(TabName);
        var panel = application.CreateRibbonPanel(TabName, PanelName);

        var showWindowButton = new PushButtonData(
            "MyApp_ShowRandomDataWindow",
            "Открыть окно\nданных", 
            TestModuleCommandsAssemblyPath,
            typeof(ShowRandomDataWindowCommand).FullName!)
        {
            ToolTip = "Открыть окно со случайными данными (ViewModel из Core)"
        };
        panel.AddItem(showWindowButton);

        var materialButton = new PushButtonData(
            "MyApp_ShowMaterialWindow",
            "Material",
            TestModuleCommandsAssemblyPath,
            typeof(ShowMaterialWindowCommand).FullName!)
        {
            ToolTip = "Окно Material Design"
        };
        panel.AddItem(materialButton);

        var metroButton = new PushButtonData(
            "MyApp_ShowMetroWindow",
            "Metro",
            TestModuleCommandsAssemblyPath,
            typeof(ShowMetroWindowCommand).FullName!)
        {
            ToolTip = "Окно MahApps.Metro"
        };
        panel.AddItem(metroButton);

        var handyControlButton = new PushButtonData(
            "MyApp_ShowHandyControlWindow",
            "HandyControl",
            TestModuleCommandsAssemblyPath,
            typeof(ShowHandyControlWindowCommand).FullName!)
        {
            ToolTip = "Окно HandyControl"
        };
        panel.AddItem(handyControlButton);

        var excelButton = new PushButtonData(
            "MyApp_CreateTempExcelFile",
            "Excel",
            TestModuleCommandsAssemblyPath,
            typeof(CreateTempExcelFileCommand).FullName!)
        {
            ToolTip = "Создать случайный Excel рядом со сборкой"
        };
        panel.AddItem(excelButton);

        var helloButton = new PushButtonData(
            "MyApp_SampleOne",
            "Hello",
            CoreCommandsAssemblyPath,
            typeof(SampleOneExternalCommand).FullName!)
        {
            ToolTip = "Показать приветствие"
        };
        panel.AddItem(helloButton);
    }
}

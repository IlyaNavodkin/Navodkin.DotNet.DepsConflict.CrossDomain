using System;
using System.Collections.Generic;
using MyApp.Core.Contracts.Services;
using MyApp.Modules.TestModule.Contracts;
using MyApp.Modules.TestModule.Contracts.ViewModels;
using MyApp.Modules.TestModule.Impl.ExcelWriteToDisk;
using MyApp.Modules.TestModule.Impl.NoWaitExecute;
using MyApp.Modules.TestModule.Impl.RandomDataPresent;
using MyApp.Modules.TestModule.Impl.MaterialDesignSample;
using MyApp.Modules.TestModule.Impl.WaitExecute;
using MyApp.Core.Contracts.DepContainer;
using MyApp.Core.Contracts.UI;
using MyApp.Core.Contracts;
using MyApp.Modules.TestModule.Contracts.Data.Queries;
using MyApp.Modules.TestModule.Contracts.Data.Commands;


namespace MyApp.Modules.TestModule.Impl;

/// <summary>
/// Реализация модуля в домене Core: создаёт ViewModel и открывает WPF-окна в общем STA-потоке (RunOnUi).
/// </summary>
[OtherDomainImplementation]
public class TestSampleModule : MarshalByRefObject, ITestSampleModule
{
    private ICoreModule? _coreModule;
    private ICoreDomainUiRunner? _uiRunner;
    private ServiceContainer? _container;
    private IModuleContainer? _containerProxy;

    public TestSampleModule(ICoreModule coreModule, ICoreDomainUiRunner uiRunner)
    {
        _coreModule = coreModule ?? throw new ArgumentNullException(nameof(coreModule));
        _uiRunner = uiRunner ?? throw new ArgumentNullException(nameof(uiRunner));
    }

    public void Initialize()
    {
        if (_container != null)
            return;
        if (_coreModule == null)
            throw new InvalidOperationException("CoreModule не задан.");

        _container = new ServiceContainer();
        var logger = _coreModule.Container.Resolve<ILogger>();
        _container.AddSingleton<ILogger>(_ => logger);
        _container.AddSingleton<ExcelService>(c => new ExcelService(c.Resolve<ILogger>()));

        _containerProxy = new ModuleContainerProxy(_container);
    }

    public IModuleContainer Container
    {
        get
        {
            if (_containerProxy == null)
                throw new InvalidOperationException("Вызовите Initialize() перед обращением к Container.");
            return _containerProxy;
        }
    }

    public void ShowRandomDataWindow()
    {
        var logger = Container.Resolve<ILogger>();
        logger.Log("Создание случайных данных для окна...");

        var random = new Random();
        var viewModel = new RandomDataViewModel
        {
            Title = $"Отчёт от {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            GeneratedAt = DateTime.Now,
            Items = new List<RandomItem>()
        };

        for (int i = 0; i < 10; i++)
        {
            viewModel.Items.Add(new RandomItem
            {
                Id = i + 1,
                Name = $"Элемент {i + 1}",
                Value = Math.Round(random.NextDouble() * 100, 2)
            });
        }

        logger.Log($"Сгенерировано {viewModel.Items.Count} элементов");

        _uiRunner!.RunOnUi(() =>
        {
            var window = new RandomDataWindow(viewModel);
            window.ShowDialog();
        });

        logger.Log("Окно закрыто");
    }

    public void ShowMaterialWindow()
    {
        var logger = Container.Resolve<ILogger>();
        logger.Log("Открытие окна Material Design...");
        _uiRunner!.RunOnUi(() =>
        {
            var window = new MaterialDemoWindow();
            window.ShowDialog();
        });
        logger.Log("Окно Material закрыто");
    }

    public string? ShowMetroWindow(WallsViewModel viewModel)
    {
        var logger = Container.Resolve<ILogger>();
        logger.Log("Открытие окна MahApps.Metro...");
        if (viewModel == null)
            throw new ArgumentNullException(nameof(viewModel));

        _uiRunner!.RunOnUi(() =>
        {
            var window = new MetroDemoWindow(viewModel);
            window.ShowDialog();
        });
        logger.Log("Окно Metro закрыто");
        return viewModel.SelectedWallId;
    }

    public void ShowHandyControlWindow(WallsViewModel viewModel, IWallsProvider wallsProvider, IElementSelectionService selectionService, IDeleteCommandHandler deleteReceiver)
    {
        var logger = Container.Resolve<ILogger>();
        logger.Log("Открытие окна HandyControl (немодальное)...");

        _uiRunner!.RunOnUi(() =>
        {
            try
            {
                var window = new HandyControlDemoWindow(viewModel, wallsProvider, selectionService, deleteReceiver);
                window.Show();
            }
            catch (Exception ex)
            {
                logger.LogError("Ошибка при открытии окна HandyControl", ex);
            }
        });
    }

    public void CreateTempExcelFile(string targetDirectory)
    {
        Container.Resolve<ExcelService>().CreateTempExcelFile(targetDirectory);
    }

}

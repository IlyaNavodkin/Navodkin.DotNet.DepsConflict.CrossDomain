using System;
using MyApp.Core.Contracts.Services;
using MyApp.Core.Impl.UI;
using MyApp.Core.Impl.Services;
using MyApp.Core.Contracts.DepContainer;
using MyApp.Core.Contracts.UI;
using MyApp.Core.Contracts;

namespace MyApp.Core.Impl;

/// <summary>
/// Модуль ядра домена Core: в контейнере регистрируются ILogger и другие общие сервисы.
/// </summary>
[OtherDomainImplementation]
public sealed class CoreModule : MarshalByRefObject, ICoreModule
{
    private ServiceContainer? _container;
    private ModuleContainerProxy? _containerProxy;

    public void Initialize()
    {
        if (_container != null)
            return;

        new CoreDomainExceptionHandler().Register();

        var bootstrap = new CoreDomainDispatcherBootstrap();
        bootstrap.Start();

        _container = new ServiceContainer();
        _container.AddSingleton<ILogger>(_ => new RealLogger());
        _container.AddSingleton<ICoreDomainUiRunner>(_ => bootstrap);
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
}

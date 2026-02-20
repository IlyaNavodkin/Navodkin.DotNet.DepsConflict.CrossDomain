using System;
using MyApp.Core.Contracts.DepContainer;

namespace MyApp.Core.Contracts.Services;

/// <summary>
/// Прокси контейнера модуля (MarshalByRefObject) для вызова через границу AppDomain.
/// </summary>
public sealed class ModuleContainerProxy : MarshalByRefObject, IModuleContainer
{
    private readonly ServiceContainer _container;

    public ModuleContainerProxy(ServiceContainer container)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
    }

    public T Resolve<T>() where T : class?
    {
        return _container.Resolve<T>();
    }

    public override object? InitializeLifetimeService()
    {
        return null;
    }
}

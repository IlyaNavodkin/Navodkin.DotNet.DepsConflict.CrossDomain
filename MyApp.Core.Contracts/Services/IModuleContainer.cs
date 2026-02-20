namespace MyApp.Core.Contracts.Services;

/// <summary>
/// Доступ к DI-контейнеру модуля через границу AppDomain.
/// </summary>
public interface IModuleContainer
{
    T Resolve<T>() where T : class?;
}

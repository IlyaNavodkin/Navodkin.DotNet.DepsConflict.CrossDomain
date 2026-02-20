namespace MyApp.Core.Contracts;

/// <summary>
/// Модуль ядра домена Core: в контейнере зарегистрированы ILogger и другие общие сервисы.
/// ILogger доступен только через контейнер этого модуля.
/// </summary>
public interface ICoreModule : IOtherDomainModule
{
}

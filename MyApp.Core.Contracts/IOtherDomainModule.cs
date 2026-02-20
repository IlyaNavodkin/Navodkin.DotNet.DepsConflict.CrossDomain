using MyApp.Core.Contracts.Services;

namespace MyApp.Core.Contracts;

/// <summary>
/// Базовый интерфейс модуля, работающего в отдельном AppDomain (домен Core).
/// В Initialize() реализация создаёт свой DI-контейнер и регистрирует зависимости.
/// </summary>
public interface IOtherDomainModule
{
    /// <summary>
    /// Инициализация модуля: создание DI-контейнера и регистрация зависимостей.
    /// Вызывается OtherAppDomainManager после создания прокси, до регистрации в основном контейнере.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Прокси контейнера модуля (контейнер создаётся в Initialize()).
    /// </summary>
    IModuleContainer Container { get; }
}

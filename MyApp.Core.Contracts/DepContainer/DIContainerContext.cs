namespace MyApp.Core.Contracts.DepContainer;

/// <summary>
/// Статический контекст для доступа к DI-контейнеру. MyAppExternalApplication выставляет Current в OnStartup.
/// Позволяет получать сервисы (ILogger, ICoreModule и др.) через Current.Resolve&lt;T&gt;() без прямой ссылки на стартер.
/// </summary>
public static class DIContainerContext
{
    private static ServiceContainer? _current;

    public static ServiceContainer? Current => _current;

    public static void SetCurrent(ServiceContainer? container)
    {
        _current = container;
    }
}

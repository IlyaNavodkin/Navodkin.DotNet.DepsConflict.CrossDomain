using System;

namespace MyApp.Core.Contracts.UI;

/// <summary>
/// Выполнение кода в STA-потоке домена Core (на общем Dispatcher).
/// Реализация создаётся при инициализации домена; модули используют для показа окон.
/// </summary>
public interface ICoreDomainUiRunner
{
    /// <summary>Выполняет действие в STA-потоке домена (синхронно).</summary>
    void RunOnUi(Action action);
}

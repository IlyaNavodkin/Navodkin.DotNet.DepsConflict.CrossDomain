using System;
using System.Threading;
using System.Windows.Threading;
using MyApp.Core.Contracts.UI;

namespace MyApp.Core.Impl.UI;

/// <summary>
/// Запускает единственный STA-поток в домене Core, регистрирует перехватчик необработанных исключений Dispatcher.
/// Позволяет выполнять код в этом потоке через RunOnUi (показ WPF-окон).
/// </summary>
public sealed class CoreDomainDispatcherBootstrap : MarshalByRefObject, ICoreDomainDispatcherBootstrap, ICoreDomainUiRunner
{
    private volatile Dispatcher? _dispatcher;

    /// <summary>
    /// Запускает STA-поток, регистрирует обработчик исключений Dispatcher и входит в цикл сообщений.
    /// Возвращает управление после того, как Dispatcher создан в STA-потоке.
    /// </summary>
    public void Start()
    {
        var thread = new Thread(() =>
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            CoreDomainExceptionHandler.RegisterDispatcherUnhandledException(_dispatcher);
            Dispatcher.Run();
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        for (int i = 0; i < 50; i++)
        {
            if (_dispatcher != null) break;
            Thread.Sleep(20);
        }
    }

    /// <summary>
    /// Выполняет действие в STA-потоке домена Core (синхронно).
    /// </summary>
    public void RunOnUi(Action action)
    {
        _dispatcher.Invoke(action);
    }
}

using System;
using System.Windows;
using System.Windows.Threading;

namespace MyApp.Core.Impl.UI;

/// <summary>
/// Регистрирует глобальный перехватчик необработанных исключений в домене Core.
/// Создаётся и вызывается в целевом домене (через CreateInstanceFromAndUnwrap).
/// </summary>
public sealed class CoreDomainExceptionHandler : MarshalByRefObject
{
    /// <summary>
    /// Подписывается на UnhandledException в текущем AppDomain и показывает все необработанные исключения в MessageBox.
    /// </summary>
    public void Register()
    {
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }

    /// <summary>
    /// Подписывается на UnhandledException диспетчера WPF. Вызывать в STA-потоке (перед Dispatcher.Run).
    /// Перехватывает исключения из обработчиков событий, показывает MessageBox и помечает как Handled.
    /// </summary>
    public static void RegisterDispatcherUnhandledException(Dispatcher dispatcher)
    {
        if (dispatcher == null) return;
        dispatcher.UnhandledException += (_, e) =>
        {
            var ex = e.Exception;
            var message = ex != null
                ? $"{ex.GetType().Name}: {ex.Message}\n\n{ex.StackTrace}"
                : "Неизвестная ошибка";
            try
            {
                MessageBox.Show(message, "Ошибка (домен Core)", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine($"[Core Domain] Ошибка: {message}");
            }

            e.Handled = true;
        };
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        var message = ex != null
            ? $"{ex.GetType().Name}: {ex.Message}\n\n{ex.StackTrace}"
            : e.ExceptionObject?.ToString() ?? "Неизвестная ошибка";
        var caption = e.IsTerminating ? "Критическая ошибка (домен Core)" : "Ошибка (домен Core)";

        MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

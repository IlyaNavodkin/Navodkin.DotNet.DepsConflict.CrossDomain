using System;
using System.Threading;
using MyApp.Core.Contracts.Services;

namespace MyApp.Modules.TestModule.Impl.Helpers;

/// <summary>
/// Запуск модального окна в отдельном STA-потоке с ожиданием закрытия (thread.Join).
/// </summary>
public static class ModalWindowHelper
{
    public static void RunModalWindow(ILogger logger, Action showDialog)
    {
        var thread = new Thread(() =>
        {
            try
            {
                showDialog();
            }
            catch (Exception ex)
            {
                logger.LogError("Ошибка при открытии окна", ex);
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
    }
}

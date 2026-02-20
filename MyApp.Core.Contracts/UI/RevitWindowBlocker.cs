using System.Runtime.InteropServices;

namespace MyApp.Core.Contracts.UI;

/// <summary>
/// Блокировка/разблокировка окна по хендлу через WinAPI (user32) — симуляция модального поведения.
/// </summary>
public static class RevitWindowBlocker
{
    [DllImport("user32.dll")]
    private static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

    /// <summary>
    /// Заблокировать окно (отключить ввод).
    /// </summary>
    public static void Block(IntPtr windowHandle)
    {
        if (windowHandle != IntPtr.Zero)
            EnableWindow(windowHandle, false);
    }

    /// <summary>
    /// Разблокировать окно (включить ввод).
    /// </summary>
    public static void Unblock(IntPtr windowHandle)
    {
        if (windowHandle != IntPtr.Zero)
            EnableWindow(windowHandle, true);
    }
}

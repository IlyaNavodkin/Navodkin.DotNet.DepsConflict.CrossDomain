namespace MyApp.Core.Contracts.UI;

/// <summary>
/// Запуск STA-потока и регистрация перехватчика исключений при создании домена Core.
/// </summary>
public interface ICoreDomainDispatcherBootstrap
{
    void Start();
}

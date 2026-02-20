using System;
using System.Collections.Generic;

namespace MyApp.Core.Contracts.DepContainer;

/// <summary>
/// Время жизни сервиса в контейнере.
/// </summary>
public enum ServiceLifetime
{
    Singleton,
    Transient
}

/// <summary>
/// Минимальный DI-контейнер: Singleton, Transient и освобождение реализаций IDisposable при Dispose.
/// </summary>
public sealed class ServiceContainer : IDisposable
{
    private readonly Dictionary<Type, (Func<ServiceContainer, object?> Factory, ServiceLifetime Lifetime)> _registrations = new();
    private readonly Dictionary<Type, object?> _singletons = new();
    private bool _disposed;
    private readonly object _lock = new();

    /// <summary>
    /// Регистрирует сервис как Singleton. Экземпляр создаётся фабрикой при первом Resolve.
    /// </summary>
    public void AddSingleton<T>(Func<ServiceContainer, T> factory) where T : class?
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ServiceContainer));
        _registrations[typeof(T)] = (c => factory(c), ServiceLifetime.Singleton);
    }

    /// <summary>
    /// Регистрирует сервис как Singleton по типу (для динамической регистрации модулей).
    /// </summary>
    public void AddSingleton(Type serviceType, Func<ServiceContainer, object?> factory)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ServiceContainer));
        if (serviceType == null || factory == null)
            throw new ArgumentNullException(serviceType == null ? nameof(serviceType) : nameof(factory));
        _registrations[serviceType] = (factory, ServiceLifetime.Singleton);
    }

    /// <summary>
    /// Регистрирует сервис как Transient. При каждом Resolve вызывается фабрика.
    /// </summary>
    public void AddTransient<T>(Func<ServiceContainer, T> factory) where T : class?
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ServiceContainer));
        _registrations[typeof(T)] = (c => factory(c), ServiceLifetime.Transient);
    }

    /// <summary>
    /// Возвращает экземпляр сервиса.
    /// </summary>
    public T Resolve<T>() where T : class?
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ServiceContainer));
        return (T)Resolve(typeof(T))!;
    }

    /// <summary>
    /// Возвращает экземпляр сервиса по типу.
    /// </summary>
    public object? Resolve(Type serviceType)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ServiceContainer));
        if (!_registrations.TryGetValue(serviceType, out var reg))
            throw new InvalidOperationException($"Сервис не зарегистрирован: {serviceType.FullName}");

        var (factory, lifetime) = reg;
        if (lifetime == ServiceLifetime.Singleton)
        {
            lock (_lock)
            {
                if (!_singletons.TryGetValue(serviceType, out var instance))
                {
                    instance = factory(this);
                    _singletons[serviceType] = instance;
                }
                return instance;
            }
        }
        return factory(this);
    }

    public void Dispose()
    {
        if (_disposed) return;
        lock (_lock)
        {
            foreach (var instance in _singletons.Values)
            {
                if (instance is IDisposable d)
                {
                    try { d.Dispose(); } catch { /* Игнорируем ошибки при освобождении сервиса. */ }
                }
            }
            _singletons.Clear();
            _disposed = true;
        }
    }
}

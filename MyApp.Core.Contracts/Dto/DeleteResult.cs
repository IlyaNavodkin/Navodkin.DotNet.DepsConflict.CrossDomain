using System;

namespace MyApp.Core.Contracts.Dto;

/// <summary>
/// Сериализуемый результат операции для передачи через границу AppDomain.
/// </summary>
/// <typeparam name="T">Тип полезных данных результата.</typeparam>
[Serializable]
public sealed class DoimainResult<T>
{
    /// <summary>Признак успешного выполнения.</summary>
    public bool Success { get; set; }

    /// <summary>Сообщение об ошибке или пояснение.</summary>
    public string? Message { get; set; }

    /// <summary>Данные результата.</summary>
    public T? Data { get; set; }

    /// <summary>Создаёт успешный результат с данными.</summary>
    public static DoimainResult<T> Ok(T data) => new() { Success = true, Data = data };

    /// <summary>Создаёт результат с ошибкой.</summary>
    public static DoimainResult<T> Fail(string message) => new() { Success = false, Message = message };
}

using System;
using System.IO;
using System.Reflection;

namespace MyApp.Core.Contracts.Domain;

/// <summary>
/// Вызывается первым в домене Core: подписывается на AssemblyResolve и загружает сборки только из указанной папки.
/// Исключает загрузку чужих версий зависимостей (например Serilog) из папки Revit или других аддинов.
/// </summary>
public class CoreDomainSetup : MarshalByRefObject
{
    private string? _basePath;

    /// <summary>
    /// Подписаться на AssemblyResolve в текущем домене и загружать сборки только из basePath.
    /// Вызывать до создания RealLogger.
    /// </summary>
    public void SetResolveOnlyFromBasePath(string basePath)
    {
        _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
    }

    private Assembly? CurrentDomain_AssemblyResolve(object? sender, ResolveEventArgs args)
    {
        if (string.IsNullOrEmpty(_basePath)) return null;
        var name = new AssemblyName(args.Name).Name;
        if (string.IsNullOrEmpty(name)) return null;
        var path = Path.Combine(_basePath, name + ".dll");
        return File.Exists(path) ? Assembly.LoadFrom(path) : null;
    }

    public override object? InitializeLifetimeService() => null;
}

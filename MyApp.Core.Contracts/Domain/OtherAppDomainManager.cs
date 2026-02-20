using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MyApp.Core.Contracts.Config;
using MyApp.Core.Contracts.DepContainer;
using MyApp.Core.Contracts.Services;
using MyApp.Core.Contracts.UI;

namespace MyApp.Core.Contracts.Domain;

/// <summary>
/// Создаёт отдельный AppDomain (домен Core) и регистрирует модули (ICoreModule и др.) через IOtherDomainModule.
/// После создания прокси вызывается Initialize(), затем модуль регистрируется в DIContainerContext как синглтон.
/// </summary>
public static class OtherAppDomainManager
{
    private static AppDomain? _workerDomain;
    private static bool _isInitialized;

    /// <summary>
    /// Создаёт домен Core, настраивает загрузку сборок, регистрирует ICoreModule и остальные модули по паттерну в DIContainerContext.
    /// </summary>
    /// <param name="config">Настройки путей и имён; при null используется AppDomainManagerConfig.Default.</param>
    public static void Initialize(AppDomainManagerConfig? config = null)
    {
        config ??= AppDomainManagerConfig.Default;
        if (_isInitialized) return;

        try
        {
            var starterDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var corePath = Path.Combine(starterDir ?? Path.GetTempPath(), config.CorePathRelative);

            if (!Directory.Exists(corePath))
            {
                WriteError($"{DateTime.Now}: Папка не найдена: {corePath}");
                throw new InvalidOperationException($"Папка не найдена: {corePath}. Разверните сборки Core в эту папку.");
            }

            var setup = new AppDomainSetup
            {
                ApplicationBase = corePath,
                PrivateBinPath = corePath,
                ShadowCopyFiles = "true",
                LoaderOptimization = LoaderOptimization.MultiDomainHost
            };

            _workerDomain = AppDomain.CreateDomain(config.DomainName, null, setup);

            var contractsDllPath = Path.Combine(corePath, $"{config.ContractCoreAssemblyPath}.dll");
            var domainSetup = (CoreDomainSetup)_workerDomain.CreateInstanceFromAndUnwrap(
                contractsDllPath,
                typeof(CoreDomainSetup).FullName!);
            domainSetup.SetResolveOnlyFromBasePath(corePath);

            var container = DIContainerContext.Current;
            if (container == null)
                throw new InvalidOperationException("DI-контейнер должен быть создан и установлен в DIContainerContext до вызова AppDomainManager.Initialize().");

            // 1. Регистрируем ICoreModule первым (интерфейс известен в домене Revit)
            var coreDllPath = Path.Combine(corePath, $"{config.ImplCoreAssemblyPath}.dll");
            var coreAsm = Assembly.LoadFrom(coreDllPath);
            var coreImplType = FindEntryPoint(coreAsm, typeof(ICoreModule));
            var coreModuleProxy = (IOtherDomainModule)_workerDomain.CreateInstanceFromAndUnwrap(
                coreDllPath,
                coreImplType.FullName!,
                false,
                BindingFlags.Default,
                null,
                Array.Empty<object>(),
                null,
                null);
            coreModuleProxy.Initialize();
            container.AddSingleton(typeof(ICoreModule), _ => coreModuleProxy);

            var uiRunner = coreModuleProxy.Container.Resolve<ICoreDomainUiRunner>();

            // 2. Остальные модули — по паттерну имён сборок (MyApp.Modules.*)
            var otherAssemblyNames = GetAssemblyNamesMatchingPattern(corePath, config.OtherModuleAssemblyPattern);
            foreach (var assemblyName in otherAssemblyNames)
            {
                var dllPath = Path.Combine(corePath, $"{assemblyName}.dll");
                if (!File.Exists(dllPath))
                {
                    WriteError($"{DateTime.Now}: Сборка не найдена: {dllPath}");
                    throw new InvalidOperationException($"Сборка не найдена: {dllPath}");
                }

                var asm = Assembly.LoadFrom(dllPath);
                foreach (var (interfaceType, implType) in DiscoverOtherDomainModules(asm))
                {
                    var ctorArgs = ResolveCtorArgs(implType, coreModuleProxy, uiRunner);
                    var proxy = (IOtherDomainModule)_workerDomain.CreateInstanceFromAndUnwrap(
                        dllPath,
                        implType.FullName!,
                        false,
                        BindingFlags.Default,
                        null,
                        ctorArgs,
                        null,
                        null);
                    proxy.Initialize();
                    container.AddSingleton(interfaceType, _ => proxy);
                }
            }

            var logger = coreModuleProxy.Container.Resolve<ILogger>();
            logger.Log("Модули успешно инициализированы");

            _isInitialized = true;
        }
        catch (Exception ex)
        {
            WriteError($"{DateTime.Now}: Ошибка инициализации AppDomain: {ex}");
            throw;
        }
    }

    /// <summary>Возвращает имена DLL в corePath, совпадающие с паттерном (например MyApp.Modules.*).</summary>
    private static IEnumerable<string> GetAssemblyNamesMatchingPattern(string corePath, string pattern)
    {
        if (!Directory.Exists(corePath))
            yield break;
        foreach (var file in Directory.EnumerateFiles(corePath, "*.dll"))
        {
            var assemblyName = Path.GetFileNameWithoutExtension(file);
            if (AssemblyNameMatchesPattern(assemblyName, pattern))
                yield return assemblyName;
        }
    }

    /// <summary>Проверяет совпадение имени сборки с паттерном (поддержка суффикса *).</summary>
    private static bool AssemblyNameMatchesPattern(string assemblyName, string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            return false;
        if (pattern.EndsWith("*", StringComparison.Ordinal))
        {
            var prefix = pattern.TrimEnd('*');
            return assemblyName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }
        return string.Equals(assemblyName, pattern, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Находит в сборке класс с OtherDomainImplementationAttribute, реализующий указанный интерфейс.</summary>
    private static Type FindEntryPoint(Assembly asm, Type interfaceType)
    {
        Type[] types;
        try
        {
            types = asm.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            types = e.Types ?? Array.Empty<Type>();
        }

        var type = types
            .Where(t => t != null && !t.IsInterface && !t.IsAbstract)
            .FirstOrDefault(t => interfaceType.IsAssignableFrom(t!) &&
                t!.GetCustomAttribute<OtherDomainImplementationAttribute>() != null);

        return type ?? throw new InvalidOperationException(
            $"Не найден класс с атрибутом {nameof(OtherDomainImplementationAttribute)} " +
            $"и реализующий {interfaceType.Name} в {asm.FullName}");
    }

    /// <summary>Находит в сборке пары (интерфейс, реализация) для модулей с OtherDomainImplementationAttribute, кроме ICoreModule.</summary>
    private static IEnumerable<(Type InterfaceType, Type ImplType)> DiscoverOtherDomainModules(Assembly asm)
    {
        Type[] types;
        try
        {
            types = asm.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            types = e.Types ?? Array.Empty<Type>();
        }

        foreach (var t in types.Where(t => t != null && !t.IsInterface && !t.IsAbstract))
        {
            if (t!.GetCustomAttribute<OtherDomainImplementationAttribute>() == null)
                continue;
            if (!typeof(IOtherDomainModule).IsAssignableFrom(t))
                continue;

            var iface = t.GetInterfaces()
                .FirstOrDefault(i => i != typeof(IOtherDomainModule) && typeof(IOtherDomainModule).IsAssignableFrom(i));
            if (iface == null || iface == typeof(ICoreModule))
                continue;

            yield return (iface, t);
        }
    }

    /// <summary>Подбирает аргументы конструктора реализации: (ICoreModule) или (ICoreModule, ICoreDomainUiRunner).</summary>
    private static object?[] ResolveCtorArgs(Type implType, IOtherDomainModule coreModuleProxy, ICoreDomainUiRunner uiRunner)
    {
        var ctors = implType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
        foreach (var c in ctors)
        {
            var ps = c.GetParameters();
            if (ps.Length == 2 && ps[0].ParameterType == typeof(ICoreModule) && ps[1].ParameterType == typeof(ICoreDomainUiRunner))
                return new object[] { coreModuleProxy, uiRunner };
            if (ps.Length == 1 && ps[0].ParameterType == typeof(ICoreModule))
                return new object[] { coreModuleProxy };
        }
        return Array.Empty<object>();
    }

    /// <summary>Выгружает домен Core и сбрасывает состояние менеджера.</summary>
    public static void Shutdown()
    {
        if (_workerDomain == null) return;
        try
        {
            try
            {
                var core = DIContainerContext.Current?.Resolve<ICoreModule>();
                core?.Container.Resolve<ILogger>()?.Log("Выгрузка домена Core...");
            }
            catch
            {
                // Игнорируем ошибки при логировании перед выгрузкой домена.
            }

            AppDomain.Unload(_workerDomain);
            _workerDomain = null;
            _isInitialized = false;
        }
        catch (Exception ex)
        {
            WriteError($"{DateTime.Now}: Ошибка выгрузки домена: {ex}");
        }
    }

    /// <summary>Дописывает сообщение об ошибке в файл myapp_error.txt во временной папке.</summary>
    internal static void WriteError(string text)
    {
        var path = Path.Combine(Path.GetTempPath(), "myapp_error.txt");
        File.AppendAllText(path, text + Environment.NewLine);
    }
}

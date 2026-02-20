namespace MyApp.Core.Contracts.Config;

/// <summary>
/// Настройки для OtherAppDomainManager: пути к сборкам, имя домена Core, паттерн поиска модулей.
/// </summary>
public class AppDomainManagerConfig
{
    /// <summary>Имя сборки контрактов (для CoreDomainSetup).</summary>
    public string ContractCoreAssemblyPath { get; set; } = "MyApp.Core.Contracts";

    /// <summary>Имя сборки с реализацией ICoreModule.</summary>
    public string ImplCoreAssemblyPath { get; set; } = "MyApp.Core.Impl";

    /// <summary>Имя создаваемого AppDomain.</summary>
    public string DomainName { get; set; } = "MyAppCoreDomain";

    /// <summary>Относительный путь к папке со сборками Core (от каталога стартера).</summary>
    public string CorePathRelative { get; set; } = "Core";

    /// <summary>Паттерн имён сборок для поиска остальных модулей (например MyApp.Modules.*).</summary>
    public string OtherModuleAssemblyPattern { get; set; } = "MyApp.Modules.*";

    /// <summary>Конфиг с значениями по умолчанию.</summary>
    public static AppDomainManagerConfig Default { get; } = new AppDomainManagerConfig();
}

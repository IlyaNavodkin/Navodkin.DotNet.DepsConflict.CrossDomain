using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.Core.Contracts;

/// <summary>
/// Маркирует класс как реализацию модуля, создаваемую в отдельном AppDomain (домен Core).
/// Используется OtherAppDomainManager для поиска типов при инициализации.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class OtherDomainImplementationAttribute : Attribute { }

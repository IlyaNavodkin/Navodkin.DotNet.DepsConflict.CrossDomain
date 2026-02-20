using System;
using System.Collections.Generic;

namespace MyApp.Modules.TestModule.Contracts.ViewModels;

/// <summary>Модель представления окна со случайными данными (сериализуемая).</summary>
[Serializable]
public class RandomDataViewModel
{
    public string Title { get; set; } = string.Empty;
    public List<RandomItem> Items { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

/// <summary>Элемент списка случайных данных.</summary>
[Serializable]
public class RandomItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Value { get; set; }
}

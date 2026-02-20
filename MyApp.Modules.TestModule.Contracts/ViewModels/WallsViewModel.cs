using System;
using System.Collections.Generic;

namespace MyApp.Modules.TestModule.Contracts.ViewModels;

/// <summary>Элемент списка стен (id и имя) для передачи через границу AppDomain.</summary>
[Serializable]
public class WallItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

/// <summary>Модель представления списка стен и выбранного id для окон Metro/HandyControl.</summary>
[Serializable]
public class WallsViewModel
{
    public List<WallItem> Walls { get; set; } = new();
    public string? SelectedWallId { get; set; }
}

using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MyApp.Core.Contracts.Dto;
using MyApp.Modules.TestModule.Contracts.Data.Queries;
using MyApp.Modules.TestModule.Contracts.Data.Commands;
using MyApp.Modules.TestModule.Contracts.ViewModels;

namespace MyApp.Modules.TestModule.Contracts.Data.Queries;

/// <summary>
/// Провайдер списка стен из документа Revit. Реализация в домене Revit (MarshalByRefObject);
/// вызывается из домена Core по кнопке «Получить стены».
/// </summary>
public interface IWallsProvider
{
    WallsViewModel GetWalls();
}


/// <summary>
/// Провайдер списка стен из документа Revit. GetWalls() выполняется на потоке Revit через RevitTask.
/// </summary>
public sealed class WallsProvider : MarshalByRefObject, IWallsProvider
{
    private readonly RevitTask _revitTask;

    public WallsProvider(UIApplication application, Document document, RevitTask revitTask)
    {
        _ = document ?? throw new ArgumentNullException(nameof(document));
        _revitTask = revitTask ?? throw new ArgumentNullException(nameof(revitTask));
    }

    public WallsViewModel GetWalls()
    {
        try
        {
            return _revitTask.Run((app) =>
            {
                var doc = app.ActiveUIDocument?.Document;
                var vm = new WallsViewModel();
                if (doc != null && doc.IsValidObject)
                {
                    var collector = new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_Walls)
                        .WhereElementIsNotElementType();
                    foreach (Element el in collector)
                    {
                        vm.Walls.Add(new WallItem
                        {
                            Id = el.Id.IntegerValue.ToString(),
                            Name = el.Name ?? string.Empty
                        });
                    }
                }
                return vm;
            }).GetAwaiter().GetResult();
        }
        catch (Exception)
        {
            return new WallsViewModel();
        }
    }
}

using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MyApp.Modules.TestModule.Contracts.Data.Queries;
using MyApp.Modules.TestModule.Contracts.Data.Commands;
using MyApp.Modules.TestModule.Contracts.ViewModels;


namespace MyApp.Modules.TestModule.Contracts.Data.Commands;

/// <summary>
/// Выделение элемента в Revit по id. Реализация в домене Revit (MarshalByRefObject);
/// вызывается из домена Core при выборе строки в таблице.
/// </summary>
public interface IElementSelectionService
{
    void SelectElement(string? elementId);
}


/// <summary>
/// Выделение элемента в Revit по id. Выполняется на потоке Revit через RevitTask.
/// </summary>
public sealed class ElementSelectionService : MarshalByRefObject, IElementSelectionService
{
    private readonly RevitTask _revitTask;

    public ElementSelectionService(UIApplication application, RevitTask revitTask)
    {
        _revitTask = revitTask ?? throw new ArgumentNullException(nameof(revitTask));
    }

    public void SelectElement(string? elementId)
    {
        if (string.IsNullOrEmpty(elementId))
            return;

        var id = elementId;
        _ = _revitTask.Run((app) =>
        {
            var doc = app.ActiveUIDocument?.Document;
            if (doc == null || !doc.IsValidObject)
                return;

            try
            {
                var elementId = new ElementId(int.Parse(id));
                var element = doc.GetElement(elementId);
                if (element != null)
                    app.ActiveUIDocument!.Selection.SetElementIds(new[] { element.Id });
            }
            catch
            {
                // Элемент мог быть удалён или документ закрыт — игнорируем.
            }
        });
    }
}

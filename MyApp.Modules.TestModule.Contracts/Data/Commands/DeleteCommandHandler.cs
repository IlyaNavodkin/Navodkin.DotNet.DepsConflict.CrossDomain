using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MyApp.Core.Contracts.Dto;
using MyApp.Modules.TestModule.Contracts.Data.Queries;
using MyApp.Modules.TestModule.Contracts.Data.Commands;
using MyApp.Modules.TestModule.Contracts.ViewModels;
using System.Diagnostics;

namespace MyApp.Modules.TestModule.Contracts.Data.Commands;

/// <summary>
/// Приёмник запроса на удаление элемента в Revit. Реализация в домене Revit (MarshalByRefObject);
/// вызывается из домена Core при нажатии «Удалить» в окне.
/// </summary>
public interface IDeleteCommandHandler
{
    /// <summary>Синхронный запрос на удаление элемента. Возвращает сериализуемый результат для передачи через границу AppDomain.</summary>
    DoimainResult<int> RequestDelete(string? elementId);
}


/// <summary>
/// Обработчик удаления элемента в Revit. Выполняется на потоке Revit через RevitTask.
/// </summary>
public sealed class DeleteCommandHandler : MarshalByRefObject, IDeleteCommandHandler
{
    private readonly RevitTask _revitTask;

    public DeleteCommandHandler(UIApplication application, Document document, RevitTask revitTask)
    {
        Document = document ?? throw new ArgumentNullException(nameof(document));
        _revitTask = revitTask;
    }

    public Document Document { get; }

    public DoimainResult<int> RequestDelete(string? elementIdInt)
    {
        try
        {
            if (string.IsNullOrEmpty(elementIdInt))
                return DoimainResult<int>.Fail("Не указан id элемента");

            var result = _revitTask.Run((app) =>
            {
                var doc = app.ActiveUIDocument.Document;
                var elementId = new ElementId(int.Parse(elementIdInt));
                var element = doc.GetElement(elementId);
                if (element == null)
                    return DoimainResult<int>.Fail("Не удалось удалить элемент");

                using var tx = new Transaction(doc, "Удаление элемента");
                if (tx.Start() == TransactionStatus.Started)
                {
                    doc.Delete(element.Id);
                    tx.Commit();
                }

                return DoimainResult<int>.Ok(1);
            }).GetAwaiter().GetResult();

            return result;
        }
        catch (Exception ex)
        {
            TaskDialog.Show("Удаление элемента", ex.Message);
            return DoimainResult<int>.Fail(ex.Message);
        }
    }
}

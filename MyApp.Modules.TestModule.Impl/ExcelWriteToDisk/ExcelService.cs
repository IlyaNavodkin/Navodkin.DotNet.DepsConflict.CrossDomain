using System;
using System.IO;
using ClosedXML.Excel;
using MyApp.Core.Contracts.Services;
using MyApp.Modules.TestModule.Contracts;
using MyApp.Modules.TestModule.Contracts.ViewModels;
using MyApp.Modules.TestModule.Contracts.Data.Commands;
using MyApp.Modules.TestModule.Contracts.Data.Queries;

namespace MyApp.Modules.TestModule.Impl.ExcelWriteToDisk;

/// <summary>
/// Создание временного Excel-файла в указанной папке (ClosedXML).
/// </summary>
public class ExcelService
{
    private readonly ILogger _logger;

    public ExcelService(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Создаёт xlsx-файл со случайными данными в targetDirectory.</summary>
    public void CreateTempExcelFile(string targetDirectory)
    {
        if (string.IsNullOrWhiteSpace(targetDirectory) || !Directory.Exists(targetDirectory))
        {
            _logger.LogError("CreateTempExcelFile: неверная папка " + targetDirectory);
            return;
        }

        var random = new Random();
        var fileName = $"TempExcel_{DateTime.Now:yyyyMMdd_HHmmss}_{random.Next(1000, 9999)}.xlsx";
        var filePath = Path.Combine(targetDirectory, fileName);

        try
        {
            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Лист1");
            sheet.Cell(1, 1).Value = "Случайные данные";
            sheet.Cell(2, 1).Value = "Дата";
            sheet.Cell(2, 2).Value = DateTime.Now;
            sheet.Cell(3, 1).Value = "Значения";
            for (int r = 4; r <= 13; r++)
            {
                for (int c = 1; c <= 5; c++)
                    sheet.Cell(r, c).Value = Math.Round(random.NextDouble() * 100, 2);
            }
            workbook.SaveAs(filePath);
            _logger.Log($"Создан файл: {filePath}");
        }
        catch (Exception ex)
        {
            _logger.LogError("Ошибка создания Excel", ex);
            throw;
        }
    }
}

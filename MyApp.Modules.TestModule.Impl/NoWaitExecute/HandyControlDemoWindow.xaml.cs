using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MyApp.Modules.TestModule.Contracts.Data.Queries;
using MyApp.Modules.TestModule.Contracts.Data.Commands;
using MyApp.Modules.TestModule.Contracts.ViewModels;

namespace MyApp.Modules.TestModule.Impl.NoWaitExecute;

/// <summary>
/// Немодальное окно HandyControl: список стен, выделение в Revit, удаление через IDeleteCommandHandler.
/// </summary>
public partial class HandyControlDemoWindow : Window
{
    private readonly IWallsProvider _wallsProvider;
    private readonly IElementSelectionService _selectionService;
    private readonly IDeleteCommandHandler _deleteReceiver;
    private readonly ObservableCollection<WallItem> _displayWalls;

    public HandyControlDemoWindow(WallsViewModel viewModel, 
        IWallsProvider wallsProvider, 
        IElementSelectionService selectionService, 
        IDeleteCommandHandler deleteReceiver)
    {
        _ = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _wallsProvider = wallsProvider ?? throw new ArgumentNullException(nameof(wallsProvider));
        _selectionService = selectionService ?? throw new ArgumentNullException(nameof(selectionService));
        _deleteReceiver = deleteReceiver ?? throw new ArgumentNullException(nameof(deleteReceiver));
        _displayWalls = new ObservableCollection<WallItem>();
        InitializeComponent();
        WallsDataGrid.ItemsSource = _displayWalls;
    }

    private void GetWallsButton_Click(object sender, RoutedEventArgs e)
    {
        var vm = _wallsProvider.GetWalls();
        _displayWalls.Clear();
        foreach (var w in vm.Walls)
            _displayWalls.Add(w);
    }

    private void WallsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (WallsDataGrid?.SelectedItem is WallItem selected)
            _selectionService.SelectElement(selected.Id);
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (WallsDataGrid?.SelectedItem is not WallItem selected)
            return;

        var result = _deleteReceiver.RequestDelete(selected.Id);
        if (result.Success)
        {
            MessageBox.Show("Стена удалена", "Удаление стены", MessageBoxButton.OK, MessageBoxImage.Information);
            _displayWalls.Remove(selected);
        }
        else if (!string.IsNullOrEmpty(result.Message))
        {
            MessageBox.Show(result.Message, "Удаление стены", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
    }
}

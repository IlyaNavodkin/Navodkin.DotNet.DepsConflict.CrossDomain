using System.Windows;
using MahApps.Metro.Controls;
using MyApp.Modules.TestModule.Contracts.ViewModels;

namespace MyApp.Modules.TestModule.Impl.WaitExecute;

/// <summary>
/// Модальное окно MahApps.Metro со списком стен; выбранный id возвращается через WallsViewModel.SelectedWallId.
/// </summary>
public partial class MetroDemoWindow : MetroWindow
{
    private readonly WallsViewModel _viewModel;

    public MetroDemoWindow(WallsViewModel viewModel)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        ApplySelection();
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        ApplySelection();
        base.OnClosed(e);
    }

    private void ApplySelection()
    {
        if (_viewModel != null && WallsDataGrid?.SelectedItem is WallItem selected)
            _viewModel.SelectedWallId = selected.Id;
    }
}

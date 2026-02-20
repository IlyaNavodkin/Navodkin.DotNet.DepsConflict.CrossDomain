using System.Windows;
using MyApp.Modules.TestModule.Contracts.ViewModels;

namespace MyApp.Modules.TestModule.Impl.RandomDataPresent;

/// <summary>
/// Окно отображения случайных данных (RandomDataViewModel).
/// </summary>
public partial class RandomDataWindow : Window
{
    public RandomDataWindow(RandomDataViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

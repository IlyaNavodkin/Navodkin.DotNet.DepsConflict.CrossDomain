using System.Windows;

namespace MyApp.Modules.TestModule.Impl.MaterialDesignSample;

/// <summary>
/// Демо-окно в стиле Material Design (модальное).
/// </summary>
public partial class MaterialDemoWindow : Window
{
    public MaterialDemoWindow()
    {
        InitializeComponent();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

using Avalonia.Controls;
using Avalonia.Styling;
using SunFlower.Client.ViewModel;

namespace SunFlower.Client.View;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        Loaded += (_, _) =>
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.MainWindow = this;
                
                // Apply persisted theme on startup
                var settings = vm.SettingsService.Current;
                vm.ThemeService.SetTheme(settings.Theme);
            }
        };
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            _ = vm.OnApplicationExitAsync();
        }
    }
}
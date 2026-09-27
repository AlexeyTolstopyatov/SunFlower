using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SunFlower.Client.Service;
using SunFlower.Client.View;
using SunFlower.Client.ViewModel;

namespace SunFlower.Client;

public partial class App : Application
{
    public static PluginService PluginService { get; } = new();
    private static RecentFilesService RecentFilesService { get; } = new();
    private static ProjectService ProjectService { get; } = new();
    private static WorkspaceService WorkspaceService { get; } = new(PluginService, ProjectService);
    public static DisassemblingService DisassemblingService { get; } = new(WorkspaceService);
    private static WindowService WindowService { get; } = new();
    private static SettingsService SettingsService { get; } = new();
    public static ThemeService ThemeService { get; private set; } = new();
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (IsFileSystemSecured(out var s))
        {
            Console.WriteLine($"File {s} is read-only or doesn't exists. Make it with read-write permissions or create new.");
            Environment.Exit(-1);
            return;
        }
        
        // All children (windows in the App collection) must change theme by request
        ThemeService = new ThemeService();
        ThemeService.ThemeChanged += variant =>
        {
            RequestedThemeVariant = variant;
        };
        
        var mainViewModel = new MainWindowViewModel(
            PluginService,
            RecentFilesService,
            WorkspaceService,
            ProjectService,
            WindowService,
            SettingsService,
            ThemeService
        );

        // Show the main window first, then initialize plugins/recent files in the
        // background so the UI thread stays responsive during startup.
        _ = InitializeWhenReadyAsync(mainViewModel);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };

            desktop.Exit += async (_, _) =>
            {
                await mainViewModel.OnApplicationExitAsync();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Runs startup initialization in the background. The window is already visible,
    /// and heavy plugin activation is offloaded to the thread pool by PluginService,
    /// so this never blocks the UI thread. Exceptions are caught instead of being lost.
    /// </summary>
    private static async Task InitializeWhenReadyAsync(MainWindowViewModel mainViewModel)
    {
        try
        {
            await mainViewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Window init failed: {ex}");
        }
    }
    /// <summary>
    /// Necessary files which stores in the root already is recent files collection
    /// This is a little but important file
    /// </summary>
    /// <returns></returns>
    private static bool IsFileSystemSecured(out string reason)
    {
        var registry = new FileInfo(Path.Combine(AppContext.BaseDirectory, "Registry", "recent.json"));
        var settings = new FileInfo(Path.Combine(AppContext.BaseDirectory, "Registry", "settings.json"));
        reason = string.Empty;

        if (!registry.Exists || registry.IsReadOnly)
        {
            reason = "[Registry/recent.json]";
            return true;
        }
        
        if (!settings.Exists || settings.IsReadOnly)
        {
            reason = "[Registry/settings.json]";
            return true;
        }
        
        return false;
    }
}
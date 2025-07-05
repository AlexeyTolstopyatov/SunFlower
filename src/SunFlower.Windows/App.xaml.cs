using System.Configuration;
using System.Data;
using System.Windows;
using HandyControl.Data;
using HandyControl.Themes;

namespace SunFlower.Windows;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    // Application Theme changes when application starts.
    // There is a one way, I've found.

    protected override void OnStartup(StartupEventArgs e)
    {
        
        base.OnStartup(e);
    }
}
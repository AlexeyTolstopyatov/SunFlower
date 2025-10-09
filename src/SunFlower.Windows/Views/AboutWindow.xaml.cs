using System.IO;

namespace SunFlower.Windows.Views;

public partial class AboutWindow : HandyControl.Controls.Window
{
    public AboutWindow()
    {
        InitializeComponent();
        try
        {
            var content = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SunFlower.runtimeconfig.dll"));
            AboutBlock.Text = content;
        }
        catch
        {
            AboutBlock.Text = "Couldn't find resources";
        }
    }
}
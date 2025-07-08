using System.Diagnostics;
using HandyControl.Data;
using HandyControl.Themes;
using SunFlower.Abstractions.Types;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System.IO;
using System.Windows;

namespace SunFlower.Windows.Services;
public class MonacoEditorService
{
    private readonly WebView2 _webView;
    private bool _isWebViewInitialized;

    public MonacoEditorService(WebView2 webView)
    {
        _webView = webView ?? throw new ArgumentNullException(nameof(webView));
        InitializeAsync();
        
        _webView.CoreWebView2InitializationCompleted += (s, e) => 
        {
            if (e.IsSuccess)
            {
                _webView.CoreWebView2.WebMessageReceived += WebMessageReceivedHandler;
            }
        };
    }

    private void WebMessageReceivedHandler(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        Debug.WriteLine($"WebMessage received: {e.TryGetWebMessageAsString()}");
    }

    private async void InitializeAsync()
    {
        await InitializeWebView2();
    }
    
    private async Task InitializeWebView2()
    {
        try
        {
            string userDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Sunflower.Windows",
                "WebView2Cache");

            CoreWebView2Environment? env = await CoreWebView2Environment.CreateAsync(
                userDataFolder: userDataFolder);

            await _webView.EnsureCoreWebView2Async(env);
            _isWebViewInitialized = true;
            
            _webView.CoreWebView2.NavigationCompleted += (s, e) => 
            {
                if (e.IsSuccess)
                {
                    Debug.WriteLine("Page navigation completed");
                }
            };

            string htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Monaco", "index.html");
            if (File.Exists(htmlPath))
            {
                _webView.CoreWebView2.Navigate(htmlPath);
                SetTheme();
            }
            else
            {
                _webView.CoreWebView2.NavigateToString("<html><body><h1>Monaco template not found</h1></body></html>");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"WebView2 initialization failed: {ex.Message}");
        }
    }

    public async Task UpdateMarkdownReportAsync(List<FlowerSeedResult> results)
    {
        if (!_isWebViewInitialized || _webView.CoreWebView2 == null)
        {
            Debug.WriteLine("WebView2 not ready. Retrying in 500ms...");
            await Task.Delay(500);
            await UpdateMarkdownReportAsync(results);
            return;
        }
        
        try
        {
            string markdownContent = MarkdownGenerator.GenerateReport(results);
            
            _webView.CoreWebView2.PostWebMessageAsString(markdownContent);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error updating report: {ex.Message}");
        }
    }
    
    public async Task UpdateMarkdownReportAsync(IEnumerable<FlowerSeedResult> results)
    {
        if (!_isWebViewInitialized || _webView.CoreWebView2 == null)
            return;
        
        await _webView.CoreWebView2.ExecuteScriptAsync("showLoadingIndicator(true);");
        
        try
        {
            // make MDBook
            string markdownContent = MarkdownGenerator.GenerateReport(results);
            string escapedContent = System.Web.HttpUtility.JavaScriptStringEncode(markdownContent);

            _webView.CoreWebView2.PostWebMessageAsString(markdownContent);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error updating report: {ex.Message}");
        }
        finally
        {
            _webView.CoreWebView2.PostWebMessageAsString("showLoadingIndicator(false);");
        }
        
    }

    private void SetTheme()
    {
        string themeName = (Theme.GetSkin(App.Current.MainWindow) == SkinType.Dark) ? "vs-dark" : "vs-light";
        Console.Error.WriteLine(themeName);
        
        _webView.CoreWebView2.ExecuteScriptAsync($"editor.updateOptions({{ theme: '{themeName}' }});");
    }
    
}
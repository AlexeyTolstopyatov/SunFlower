using System.Diagnostics;
using SunFlower.Abstractions.Types;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System.IO;
using System.Windows;

namespace SunFlower.Windows.Services;
public class MonacoEditorManager
{
    private readonly WebView2 _webView;
    private bool _isWebViewInitialized;

    public MonacoEditorManager(WebView2 webView)
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
            var userDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Sunflower.Windows",
                "WebView2Cache");

            var env = await CoreWebView2Environment.CreateAsync(
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

            var htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Monaco", "index.html");
            if (File.Exists(htmlPath))
            {
                _webView.CoreWebView2.Navigate(htmlPath);
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
            var markdownContent = MarkdownGenerator.Generate(results);
            
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
            var markdownContent = MarkdownGenerator.Generate(results);
            var escapedContent = System.Web.HttpUtility.JavaScriptStringEncode(markdownContent);

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

    public async void SwitchDarkTheme()
    {
        await _webView.CoreWebView2.ExecuteScriptAsync($"editor.updateOptions({{theme:'vs-dark'}})");
    }

    public async void SwitchLightTheme()
    {
        await _webView.CoreWebView2.ExecuteScriptAsync("editor.updateOptions({theme:'vs-light'})");
    }
}
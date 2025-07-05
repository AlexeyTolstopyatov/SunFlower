using System.Windows;
using Newtonsoft.Json;
using SunFlower.Windows.Services;

namespace SunFlower.Windows.Views;

public partial class MonacoWindow : Window, IEditorService
{
    public MonacoWindow()
    {
        InitializeComponent();
    }
    
    #region IEditorService members
    public async Task UpdateEditorContentAsync(string content)
    {
        if (View2.CoreWebView2 == null) 
            await View2.EnsureCoreWebView2Async();

        string escaped = JsonConvert.ToString(content);
        string script = $"setEditorContent({escaped})";
        await View2.CoreWebView2!.ExecuteScriptAsync(script);
    }

    public async Task<string> GetEditorContentAsync()
    {
        if (View2.CoreWebView2 == null) 
            await View2.EnsureCoreWebView2Async();

        string result = await View2.CoreWebView2!.ExecuteScriptAsync("getEditorContent()");
        return JsonConvert.DeserializeObject<string>(result)!;
    }
    #endregion
}
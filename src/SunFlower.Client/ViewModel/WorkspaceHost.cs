// CoffeeLake (C) 2026-*
// 
// The WorkspaceHost.cs represents <what?>
// 
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com

using Dock.Model.Controls;
using SunFlower.Client.Model;
using SunFlower.Client.Service;
using SunFlower.Client.ViewModel.Dock;
using SunFlower.Kernel.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;

namespace SunFlower.Client.ViewModel;

public sealed class WorkspaceHost : IWorkspaceHost
{
    private readonly WorkspaceLayoutFactory _factory;
    private readonly IDocumentDock _contentDock;
    private readonly IFlowerExecutable _fe;
    private readonly SettingsService _settings;

    public event EventHandler? OpenedDocumentsChanged;

    public WorkspaceHost(
        WorkspaceLayoutFactory factory,
        IDocumentDock contentDock,
        IFlowerExecutable runner,
        SettingsService settings)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _contentDock = contentDock ?? throw new ArgumentNullException(nameof(contentDock));
        _fe = runner ?? throw new ArgumentNullException(nameof(runner));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));

        _factory.DockableAdded += (_, _) => OpenedDocumentsChanged?.Invoke(this, EventArgs.Empty);
        _factory.DockableClosed += (_, _) => OpenedDocumentsChanged?.Invoke(this, EventArgs.Empty);
    }

    public Task OpenAsync(string filePath)
    {
        var detection = ContentTypeDetector.Detect(filePath);
        return detection.SubType switch
        {
            ContentSubType.Markdown => OpenMarkdownAsync(filePath),
            ContentSubType.Text or ContentSubType.PlainText => OpenTextAsync(filePath),
            _ => OpenHexAsync(filePath),
        };
    }

    public async Task ExecuteFlowerAsync(FlowerData flower, string filePath)
    {
        var result = await _fe.ExecuteFlowerAsync(flower, filePath);

        switch (result.Kind)
        {
            case "Text":
                await OpenTextAsync(filePath);
                break;
            case "Markdown":
                await OpenMarkdownAsync(filePath);
                break;
            default:
                await OpenHexAsync(filePath); 
                break;
        }
    }
    private Task OpenTextAsync(string filePath) =>
        GetOrCreateAsync<TextDocumentViewModel>(filePath, "Text", async doc =>
        {
            var (family, size) = ReadFontConfig(_settings.Current.TextControl);
            doc.TextFontFamily = new FontFamily(family);
            doc.TextFontSize = size;
            doc.TextDocument = new AvaloniaEdit.Document.TextDocument(
                await File.ReadAllTextAsync(filePath));
        });

    private Task OpenHexAsync(string filePath) =>
        GetOrCreateAsync<HexDocumentViewModel>(filePath, "Hex", async doc =>
        {
            var (family, size) = ReadFontConfig(_settings.Current.HexControl);
            doc.HexFontFamily = new FontFamily(family);
            doc.HexFontSize = size;
            var bytes = await File.ReadAllBytesAsync(filePath);
            doc.BinaryDocument = new AvaloniaHex.Document.MemoryBinaryDocument(bytes);
        });

    private Task OpenMarkdownAsync(string filePath) =>
        GetOrCreateAsync<MarkdownDocumentViewModel>(filePath, "Page", async doc =>
        {
            doc.Content = await File.ReadAllTextAsync(filePath);
        });
    
    // private async Task OpenTextAsync(string filePath)
    // {
    //     var doc = GetOrCreate<TextDocumentViewModel>(filePath, "Text");
    //     var (family, size) = ReadFontConfig(_settings.Current.TextControl);
    //     doc.TextFontFamily = new FontFamily(family);
    //     doc.TextFontSize = size;
    //
    //     doc.TextDocument ??= new AvaloniaEdit.Document.TextDocument(
    //         await File.ReadAllTextAsync(filePath));
    //
    //     doc.Context = doc;
    //     
    //     Activate(doc);
    // }
    //
    // private async Task OpenHexAsync(string filePath)
    // {
    //     var doc = GetOrCreate<HexDocumentViewModel>(filePath, "Hex");
    //     var (family, size) = ReadFontConfig(_settings.Current.HexControl);
    //     doc.HexFontFamily = new FontFamily(family);
    //     doc.HexFontSize = size;
    //
    //     if (doc.BinaryDocument is null)
    //     {
    //         var bytes = await File.ReadAllBytesAsync(filePath);
    //         doc.BinaryDocument = new AvaloniaHex.Document.MemoryBinaryDocument(bytes);
    //     }
    //
    //     doc.Context = doc;
    //
    //     Activate(doc);
    // }
    //
    // private async Task OpenMarkdownAsync(string filePath)
    // {
    //     var doc = GetOrCreate<MarkdownDocumentViewModel>(filePath, "Page");
    //     doc.Content = await File.ReadAllTextAsync(filePath);
    //
    //     doc.Context = doc;
    //     
    //     Activate(doc);
    // }

    private T GetOrCreate<T>(string filePath, string suffix) where T : FileDocument, new()
    {
        // The layout is the truth. Closed tabs are gone from VisibleDockables,
        // so GetOrCreate recreates them -> no phantom entries possible.
        var existing = _contentDock.VisibleDockables?
            .OfType<T>()
            .FirstOrDefault(d => string.Equals(d.FilePath, filePath, StringComparison.Ordinal));

        if (existing is not null)
            return existing;
        // it sets Factory on the instance, which `new T()` does not.
        // Without a factory, Dock silently fails to render/activate the tab.
        var doc = new T(); //_factory.CreateDockable<T>(Guid.NewGuid().ToString());
                           // member is missing but Dock.Avalonia decompilation told that
                           // no special mechanics to create IDocument-like instance 
        doc.Title = $"{Path.GetFileName(filePath)} [{suffix}]";
        doc.FilePath = filePath;
        doc.CanFloat = false;

        _contentDock.AddDocument(doc);
        return doc;
    }
    
    private async Task<T> GetOrCreateAsync<T>(
        string filePath, string suffix, Func<T, Task> populate)
        where T : FileDocument, new()
    {
        var existing = _contentDock.VisibleDockables?
            .OfType<T>()
            .FirstOrDefault(d => string.Equals(d.FilePath, filePath, StringComparison.Ordinal));

        if (existing is not null)
            return existing;

        _factory.CreateDocument();
        var doc = new T // Exactly the same with decompiled IFactory::CreateDocument()
                        // Returns the new Document() instance adopted for workspace host 
        {
            Id = Guid.NewGuid().ToString(),
            FilePath = filePath,
            CanFloat = false,
            Title = $"{Path.GetFileName(filePath)} [{suffix}]"
        };
        
        doc.FilePath = filePath;
        doc.Context = doc;
        doc.CanFloat = false;

        await populate(doc);
        
        _contentDock.AddDocument(doc);
        return doc;
    }

    private static (string Family, double Size) ReadFontConfig(Font? cfg) =>
        (cfg?.Family ?? "Consolas", cfg?.Size > 0 ? cfg.Size : 13.0);
}
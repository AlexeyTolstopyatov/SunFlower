// CoffeeLake (C) 2026-*
// 
// The WorkspaceHost.cs represents <what?>
// 
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaHex.Document;
using Dock.Model.Controls;
using SunFlower.Client.Model;
using SunFlower.Client.Service;
using SunFlower.Client.ViewModel.Dock;
using SunFlower.Kernel.Services;

namespace SunFlower.Client.ViewModel;

public sealed class WorkspaceHost : IWorkspaceHost
{
    private readonly IDocumentDock _contentDock;
    private readonly WorkspaceLayoutFactory _factory;
    private readonly IFlowerExecutable _fe;
    private readonly SettingsService _settings;

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

    public event EventHandler? OpenedDocumentsChanged;

    public Task OpenAsync(string filePath, bool isPermanent)
    {
        var detection = ContentTypeDetector.Detect(filePath);
        return detection.SubType switch
        {
            ContentSubType.Markdown => OpenMarkdownAsync(filePath, isPermanent),
            ContentSubType.Text or ContentSubType.PlainText => OpenTextAsync(filePath, isPermanent),
            _ => OpenHexAsync(filePath, isPermanent)
        };
    }

    public async Task ExecuteFlowerAsync(FlowerData flower, string filePath)
    {
        PluginContentView result;
        try
        {
            result = await _fe.ExecuteFlowerAsync(flower, filePath);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync(
                $"[{nameof(ExecuteFlowerAsync)}] {flower.Instance.Name} failed: {ex}");
            return;
        }

        if (result.HasError)
            await Console.Error.WriteLineAsync(
                $"[{nameof(ExecuteFlowerAsync)}] {flower.Instance.Name}: {result.ErrorMessage}");

        // Open the *produced* result file (e.g. <target>_<plugin>.md),
        // not the original target that was analyzed.
        var resultPath = result.FilePath;

        if (result.IsMarkdown)
            await OpenMarkdownAsync(resultPath);
        else if (result.IsAssembly)
            await OpenTextAsync(resultPath);
        else
            await OpenMarkdownAsync(resultPath);
    }

    /// <summary> Opens a TextEditor tab in the document dock </summary>
    /// <param name="filePath">opens for a given file </param>
    /// <param name="isPermanent"></param>
    /// <returns></returns>
    private Task OpenTextAsync(string filePath, bool isPermanent = false)
    {
        return GetOrCreateAsync<TextDocumentViewModel>(filePath, "Text", async doc =>
        {
            var (family, size) = ReadFontConfigurationBlock(_settings.Current.TextControl);
            doc.TextFontFamily = new FontFamily(family);
            doc.TextFontSize = size;
            
            doc.CanDrag = !isPermanent;
            doc.CanClose = !isPermanent;
            
            doc.TextDocument = new TextDocument(
                await File.ReadAllTextAsync(filePath));
        });
    }
    /// <summary> Opens hex tab </summary>
    /// <param name="filePath"></param>
    /// <param name="isPermanent">Can tab miss a move and close permissions</param>
    /// <returns></returns>
    private Task OpenHexAsync(string filePath, bool isPermanent = false)
    {
        return GetOrCreateAsync<HexDocumentViewModel>(filePath, "Hex", async doc =>
        {
            var (family, size) = ReadFontConfigurationBlock(_settings.Current.HexControl);
            doc.HexFontFamily = new FontFamily(family);
            doc.HexFontSize = size;
            
            doc.CanClose = !isPermanent;
            doc.CanDrag = !isPermanent;
            
            var bytes = await File.ReadAllBytesAsync(filePath);
            doc.BinaryDocument = new MemoryBinaryDocument(bytes);
        });
    }

    private Task OpenMarkdownAsync(string filePath, bool isPermanent = false)
    {
        return GetOrCreateAsync<MarkdownDocumentViewModel>(filePath, "Page",
            async doc =>
            {
                doc.CanDrag = !isPermanent;
                doc.CanClose = !isPermanent;

                doc.Content = await File.ReadAllTextAsync(filePath);
            });
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
        // Exactly the same with decompiled IFactory::CreateDocument()
        // Returns the new Document() instance adopted for workspace host 
        var doc = new T
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

    public static (string Family, double Size) ReadFontConfigurationBlock(Font? cfg)
    {
        return (cfg?.Family ?? "Consolas", cfg?.Size > 0 ? cfg.Size : 13.0);
    }
}
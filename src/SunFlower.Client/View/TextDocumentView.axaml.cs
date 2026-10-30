using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;

namespace SunFlower.Client.View;

public partial class TextDocumentView : UserControl
{
    public TextDocumentView()
    {
        InitializeComponent();

        Loaded += OnLoaded;
        App.ThemeService.EditorChanged += OnEditorThemeChanged;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        InstallGrammar();
    }

    private void OnEditorThemeChanged(object? sender, ThemeName variant)
    {
        InstallGrammar();
    }

    private void InstallGrammar()
    {
        var inst = Editor.InstallTextMate(new RegistryOptions(App.ThemeService.TextEditorVariant));
        var grammar = Path.Combine(AppContext.BaseDirectory, "Grammar", "intel.json");
        inst.SetGrammarFile(grammar);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        // Less CPU bound when object is hidden
        App.ThemeService.EditorChanged -= OnEditorThemeChanged;
        base.OnDetachedFromVisualTree(e);
    }
}
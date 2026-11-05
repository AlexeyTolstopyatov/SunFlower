using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;

namespace SunFlower.Client.View.Dock;

public partial class TextDocumentView : UserControl
{
    /// <summary>
    /// Extern grammars are located in /Grammar subdirectory/catalog of sunflower client
    /// Unfortunately it needs to bound here.
    ///
    /// TODO: inspect + fill grammars during a file operation executing 
    /// </summary>
    private static readonly Dictionary<string, string> ExternGrammarByExtension = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".asm", "source.x86" },
        { ".x86", "source.x86" },

        { ".arm", "source.arm" },
        { ".arm7", "source.arm" },
    };
    /// <summary>
    /// Grammars embedded in the TextEditor
    /// (was decompiled and moved because of it was a private _availableGrammars class member)
    /// </summary>
    private static readonly Dictionary<string, string> GrammarByExtension = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".cmd",  "Bat" },
        { ".bat",  "Bat" },

        { ".c",    "Cpp" },
        { ".h",    "Cpp" },
        { ".cpp",  "Cpp" },
        { ".cc",   "Cpp" },
        { ".cxx",  "Cpp" },
        { ".hpp",  "Cpp" },
        { ".hxx",  "Cpp" },
        { ".m",    "ObjectiveC" },

        { ".pas",  "Pascal" },
        { ".lrp",  "Pascal" },
        { ".drp",  "Pascal" },
        
        { ".go",  "Go" },
        
        { ".cs",   "CSharp" },
        { ".fs",   "FSharp"},
        { ".java", "Java" },

        { ".js",   "Javascript" },
        { ".mjs",  "Javascript" },
        { ".cjs",  "Javascript" },
        { ".jsx",  "Javascript" },
        { ".ts",   "TypescriptBasics" },
        { ".tsx",  "TypescriptBasics" },

        { ".json", "Json" },

        { ".py",   "Python" },
        { ".pyw",  "Python" },
        
        { ".R",    "R"},
        
        { ".hls",  "HLSL" },
        { ".hlsl",  "HLSL" },
        { ".ini",  "Ini" },
        { ".cfg",  "Ini" },
        { ".conf", "Ini" },
        
        { ".html", "HTML" },
        { ".htm",  "HTML" },
        { ".css",  "CSS" },

        { ".rs",  "Rust" },

        { ".ps1",  "PowerShell" },
        
        { ".sh",  "ShellScript" },
        { ".csh",  "ShellScript" },
        { ".fish",  "ShellScript" },
        { ".bash",  "ShellScript" },
        { ".ksh",  "ShellScript" },
        
        { ".sql",  "SQL" },
        
        { ".bas",  "VB"},
        { ".frm",  "VB"},
        { ".cls",  "VB"},
        { ".vb",   "VB"},
        
        { ".xml",  "XML" },
        { ".xsd",  "XML" },
        { ".xsl",  "XML" },
        { ".xslt", "XML" },
        { ".xaml", "XML" },
        { ".yml",  "YAML"},
        { ".yaml",  "YAML"},
    };

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
        
        var filePath = (DataContext as ViewModel.Dock.FileDocument)?.FilePath;
        var grammarScope = string.Empty;

        if (!string.IsNullOrEmpty(filePath))
        {
            var ext = Path.GetExtension(filePath);
            
            if (GrammarByExtension.TryGetValue(ext, out var scope))
                grammarScope = scope;
        }
        else
        {
            return; // Bad execution result
        }

        // Try built-in TextMate grammar by scope name first (base set bundled with AvaloniaEdit.TextMate).
        // If unavailable or no match, fall back to the bundled intel.json grammar file.
        if (!string.IsNullOrEmpty(grammarScope) && grammarScope != "text.plain")
        {
            inst.SetGrammar(grammarScope);
            return;
        }
        
        if (ExternGrammarByExtension.TryGetValue(Path.GetExtension(filePath), out var externScope))
            grammarScope = externScope;
        
        // mips/arm/x86 extern text mates 
        if (grammarScope == "source.x86")
        {
            var grammarFile = Path.Combine(AppContext.BaseDirectory, "Grammar", "intel.json");
            if (File.Exists(grammarFile))
                inst.SetGrammarFile(grammarFile);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        App.ThemeService.EditorChanged -= OnEditorThemeChanged;
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        App.ThemeService.EditorChanged += OnEditorThemeChanged;
        base.OnAttachedToVisualTree(e);
    }
}
// CoffeeLake (C) 2026-*
// StructLayoutDialogViewModel - dialog context for the Struct Layout dialog.
// Reinterprets selected bytes as a packed struct described by a JSONC template.
// Left side
//      template editor;
//      right side field name/offset/type/size/value table;
// Flower toolkit:
//      FlowerReflection API,
//      BinaryReaderExtension
// @creator: atolstopyatov2017@vk.com

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SunFlower.Client.ViewModel;

public sealed partial class StructLayoutDialogViewModel : DialogViewModel
{
    private static readonly Dictionary<string, (Type Type, int Size)> TypeTable =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["byte"] = (typeof(byte), 1),
            ["word"] = (typeof(ushort), 2),
            ["dword"] = (typeof(uint), 4),
            ["qword"] = (typeof(ulong), 8)
        };

    private readonly byte[] _bytes;
    private readonly Window? _ownerWindow;

    [ObservableProperty] private string _bytesInfo = string.Empty;

    [ObservableProperty] private string? _errorMessage;

    [ObservableProperty] private FontFamily _fontFamily;

    [ObservableProperty] private double _fontSize;

    [ObservableProperty] private bool _hasError;

    [ObservableProperty] private bool _isParsed;

    [ObservableProperty] private string _templateText;

    [ObservableProperty] private string _title = "Struct Layout";

    public StructLayoutDialogViewModel(
        FontFamily? fontFamily,
        double? fontSize,
        string? templateText = null,
        Window? ownerWindow = null,
        byte[]? selectedBytes = null
    )
    {
        _fontFamily = fontFamily ?? new FontFamily("Consolas");
        _fontSize = fontSize ?? 13.0;

        _ownerWindow = ownerWindow;
        _bytes = selectedBytes ?? [];
        _templateText = templateText ?? DefaultTemplate;
        TemplateText = _templateText;
        UpdateBytesInfo();
        Parse();
    }

    public ObservableCollection<StructLayoutField> Fields { get; } = [];

    private static string DefaultTemplate => """
                                             [
                                               // This code is an example how the 32-bit integer might be packed
                                               { "name": "bCaught", "type": "BYTE" },
                                               { "name": "bCaught", "type": "BYTE" },
                                               { "name": "wCaught", "type": "WORD" },
                                               { "name": "dwCaught","type": "DWORD" }
                                             ]
                                             """;

    private void UpdateBytesInfo()
    {
        BytesInfo = _bytes.Length == 0
            ? "No bytes selected"
            : $"Selected bytes: {_bytes.Length} (0x0 .. 0x{_bytes.Length - 1:X})";
    }

    partial void OnTemplateTextChanged(string value)
    {
        Parse();
    }

    [RelayCommand]
    private void Parse()
    {
        ErrorMessage = null;
        HasError = false;
        Fields.Clear();

        if (_bytes.Length == 0)
        {
            ErrorMessage = "No bytes selected. Select a byte range first.";
            HasError = true;
            IsParsed = false;
            return;
        }

        var template = ParseTemplate(TemplateText);
        if (template == null)
        {
            IsParsed = false;
            return;
        }

        using var stream = new MemoryStream(_bytes);
        using var reader = new BinaryReader(stream);
        var offset = 0;

        foreach (var entry in template)
        {
            if (!TypeTable.TryGetValue(entry.Type, out var field))
            {
                ErrorMessage = $"Unknown type '{entry.Type}' for field '{entry.Name}'. " +
                               $"Supported: {string.Join(", ", TypeTable.Keys)}.";
                HasError = true;
                IsParsed = false;
                return;
            }

            var (clrType, size) = field;

            if (offset + size > _bytes.Length)
            {
                ErrorMessage = $"Field '{entry.Name}' (offset 0x{offset:X}, size {size}) " +
                               $"exceeds the selected bytes range ({_bytes.Length}).";
                HasError = true;
                IsParsed = false;
                return;
            }

            stream.Position = offset;
            object value;
            try
            {
                value = clrType switch
                {
                    _ when clrType == typeof(byte) => reader.ReadByte(),
                    _ when clrType == typeof(ushort) => reader.ReadUInt16(),
                    _ when clrType == typeof(uint) => reader.ReadUInt32(),
                    _ when clrType == typeof(ulong) => reader.ReadUInt64(),
                    _ => throw new NotSupportedException(clrType.FullName ?? clrType.Name)
                };
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to read field '{entry.Name}': {ex.Message}";
                HasError = true;
                IsParsed = false;
                return;
            }

            Fields.Add(new StructLayoutField
            {
                Name = entry.Name,
                Offset = offset,
                Type = entry.Type.ToUpperInvariant(),
                Size = size,
                Value = FormatValue(value, size)
            });

            offset += size;
        }

        IsParsed = true;
    }

    private static string FormatValue(object value, int size)
    {
        return value switch
        {
            byte b => $"0x{b:X2}",
            ushort w => $"0x{w:X4}",
            uint d => $"0x{d:X8}",
            ulong q => $"0x{q:X16}",
            _ => value.ToString() ?? string.Empty
        };
    }

    private TemplateEntry[]? ParseTemplate(string text)
    {
        // jsonc drop whole-line [//] comments, then parse allowing trailing commas.
        var cleaned = new StringBuilder();
        foreach (var line in text.Split('\n'))
        {
            if (line.TrimStart().StartsWith("//", StringComparison.Ordinal))
                continue;
            cleaned.AppendLine(line);
        }

        var options = new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        };

        try
        {
            using var doc = JsonDocument.Parse(cleaned.ToString(), options);
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
            {
                ErrorMessage = "Template must be a JSON array of field descriptors.";
                HasError = true;
                return null;
            }

            var entries = new List<TemplateEntry>();
            foreach (var item in doc.RootElement.EnumerateArray())
            {
                var name = item.GetProperty("name").GetString() ?? string.Empty;
                var type = item.TryGetProperty("type", out var typeProp)
                    ? typeProp.GetString() ?? string.Empty
                    : string.Empty;

                if (string.IsNullOrWhiteSpace(name))
                {
                    ErrorMessage = "Entry must have a non-empty \"name\".";
                    HasError = true;
                    return null;
                }

                if (string.IsNullOrWhiteSpace(type))
                {
                    ErrorMessage = $"\"{name}\" is missing a \"type\".";
                    HasError = true;
                    return null;
                }

                entries.Add(new TemplateEntry(name, type));
            }

            return entries.ToArray();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Invalid template JSON: {ex.Message}";
            HasError = true;
            return null;
        }
    }

    private static IStorageProvider? GetStorageProvider(Window? owner)
    {
        return owner == null
            ? null
            : TopLevel.GetTopLevel(owner)?.StorageProvider;
    }

    private static async Task<IStorageFile?> PickOpenFileAsync(Window? owner, string title, string[] patterns)
    {
        var storage = GetStorageProvider(owner);
        if (storage == null)
            return null;

        var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title
        });

        return files.Count > 0 ? files[0] : null;
    }

    private static async Task<IStorageFile?> PickSaveFileAsync(Window? owner, string title, string defaultExtension,
        string[] patterns)
    {
        var storage = GetStorageProvider(owner);
        if (storage == null)
            return null;

        return await storage.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            DefaultExtension = defaultExtension,
            FileTypeChoices = [new FilePickerFileType("Files") { Patterns = patterns }]
        });
    }

    [RelayCommand]
    private async Task From()
    {
        // Import a template (*.jsonc / *.json / *.txt) from disk.
        var file = await PickOpenFileAsync(_ownerWindow, "Import struct layout template",
            ["*.jsonc", "*.json", "*.txt"]);
        if (file == null)
            return;

        var path = file.TryGetLocalPath();
        if (string.IsNullOrEmpty(path))
            return;

        try
        {
            TemplateText = await File.ReadAllTextAsync(path);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to read template: {ex.Message}";
            HasError = true;
        }
    }

    [RelayCommand]
    private async Task SaveTemplate()
    {
        var file = await PickSaveFileAsync(_ownerWindow, "Save layout template", ".jsonc", ["*.jsonc", "*.json"]);
        if (file == null)
            return;

        var path = file.TryGetLocalPath();
        if (string.IsNullOrEmpty(path))
            return;

        try
        {
            await File.WriteAllTextAsync(path, TemplateText);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to save template: {ex.Message}";
            HasError = true;
        }
    }

    [RelayCommand]
    private async Task SaveLayout()
    {
        if (!IsParsed || Fields.Count == 0)
        {
            ErrorMessage = "Nothing to save: parse the template first.";
            HasError = true;
            return;
        }

        var file = await PickSaveFileAsync(
            _ownerWindow,
            "Save struct layout",
            ".json",
            ["*.json", "*.jsonc"]
        );

        if (file == null)
            return;

        var path = file.TryGetLocalPath();
        if (string.IsNullOrEmpty(path))
            return;

        try
        {
            var payload = Fields.Select(f => new
            {
                f.Name,
                f.Offset,
                f.Type,
                f.Size,
                f.Value
            });
            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(path, json);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to save layout: {ex.Message}";
            HasError = true;
        }
    }

    [RelayCommand]
    private void Accept()
    {
        DialogService?.CloseDialog(true);
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogService?.CloseDialog(false);
    }

    public sealed class StructLayoutField
    {
        public string Name { get; init; } = string.Empty;
        public int Offset { get; init; }
        public string Type { get; init; } = string.Empty;
        public int Size { get; init; }
        public string Value { get; init; } = string.Empty;
    }

    private sealed class TemplateEntry(string name, string type)
    {
        public string Name { get; } = name;
        public string Type { get; } = type;
    }
}
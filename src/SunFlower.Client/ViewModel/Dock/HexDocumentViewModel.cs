// CoffeeLake (C) 2026-*
// 
// The HexDocumentViewModel.cs represents <what?>
// 
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com

using System;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaHex;
using AvaloniaHex.Document;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SunFlower.Client.ViewModel.Dock;

public partial class HexDocumentViewModel : FileDocument
{
    [ObservableProperty] private IBinaryDocument? _binaryDocument;

    [ObservableProperty] private FontFamily? _hexFontFamily;

    [ObservableProperty] private double _hexFontSize;

    /// <summary>
    ///     Visibility of the individual hex editor columns. Bound from the view.
    /// </summary>
    [ObservableProperty] private bool _showOffsetColumn = true;

    [ObservableProperty] private bool _showHexColumn = true;

    [ObservableProperty] private bool _showBinaryColumn = true;

    [ObservableProperty] private bool _showAsciiColumn;

    /// <summary>
    ///     Fixed number of bytes per line, or null for dynamic/automatic layout.
    /// </summary>
    [ObservableProperty] private int? _bytesPerLine;

    public HexEditor? Editor { get; set; }

    partial void OnBytesPerLineChanged(int? value)
    {
        if (Editor is not null)
            Editor.HexView.BytesPerLine = value;
    }

    /// <summary>
    ///     Jumps the caret to the given byte offset in the HexEditor and brings it into view.
    /// </summary>
    public void JumpToOffset(ulong offset, bool isRelative)
    {
        if (Editor is null || BinaryDocument is null)
            return;

        if (offset >= BinaryDocument.Length || offset > long.MaxValue)
            return;

        if (!isRelative)
            Editor.Caret.GoToStartOfDocument();

        Editor.Caret.GoForward(offset);
        Editor.BringIntoView();
    }

    /// <summary>
    ///     Returns the currently selected bytes in the HexEditor, or null when nothing is selected.
    /// </summary>
    public byte[]? GetSelectedBytes()
    {
        if (Editor is null || BinaryDocument is null)
            return null;

        if (Editor.Selection.Range.ByteLength == 0)
            return null;

        var start = Math.Min(
            Editor.Selection.Range.Start.ByteIndex,
            Editor.Selection.Range.End.ByteIndex
        );
        var end = Math.Max(Editor.Selection.Range.Start.ByteIndex, Editor.Selection.Range.End.ByteIndex);
        var length = (int)(end - start);

        if (length <= 0)
            return null;

        var buffer = new byte[length];

        BinaryDocument.ReadBytes(start, buffer);
        return buffer;
    }
}
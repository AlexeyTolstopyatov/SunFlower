// CoffeeLake (C) 2026-*
//
// The BytesDialogViewModel.cs represents a converter of selected bytes array.
// Expecting destination
//      Integers converting         -> LSB/MSB Int (Decimal)
//      Integers converting         -> LSB/MSB Int (Hexadecimal)
//      ASCII converting            -> string
//      WTF (Windows wide encoding) -> string
//      UTF8 converting             -> string
//      IBM codepage converting     -> string
//
// Condition: make it using FlowerReport.SafeString because of it escapes string  
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SunFlower.Abstractions;

namespace SunFlower.Client.ViewModel;

public sealed partial class BytesDialogViewModel : DialogViewModel
{
    private readonly byte[] _bytes;

    [ObservableProperty] private string _bytesInfo = string.Empty;

    [ObservableProperty] private FontFamily _font;

    [ObservableProperty] private string _hexDump = string.Empty;

    [ObservableProperty] private double _size;

    [ObservableProperty] private string _title = "Bytes Converter";

    public BytesDialogViewModel(FontFamily? font, double? size, byte[]? selectedBytes = null)
    {
        _bytes = selectedBytes ?? [];
        _font = font ?? new FontFamily("Consolas");
        _size = size ?? 13.0;

        UpdateBytesInfo();
        HexDump = BuildHexDump(_bytes);
        BuildFormats();
    }

    public ObservableCollection<ByteFormatView> Formats { get; } = [];

    private void UpdateBytesInfo()
    {
        BytesInfo = _bytes.Length == 0
            ? "No bytes selected"
            : $"Selected bytes: {_bytes.Length} (0x0 .. 0x{_bytes.Length - 1:X})";
    }

    private static string BuildHexDump(byte[] bytes)
    {
        if (bytes.Length == 0)
            return string.Empty;

        var sb = new StringBuilder();
        for (var i = 0; i < bytes.Length; i += 16)
        {
            var chunk = bytes.Skip(i).Take(16).ToArray();
            sb.Append($"{i:X8}:  ");
            for (var j = 0; j < 16; j++)
                if (j < chunk.Length)
                    sb.Append($"{chunk[j]:X2} ");
                else
                    sb.Append("   ");

            sb.Append("  ");
            foreach (var b in chunk) sb.Append(b >= 32 && b < 127 ? (char)b : '.');

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private void BuildFormats()
    {
        Formats.Clear();

        if (_bytes.Length == 0)
        {
            Formats.Add(new ByteFormatView
            {
                Name = "No bytes selected",
                Value = string.Empty
            });
            return;
        }

        Formats.Add(new ByteFormatView
        {
            Name = "ASCII",
            Value = FlowerReport.SafeString(Encoding.ASCII.GetString(_bytes))
        });

        Formats.Add(new ByteFormatView
        {
            Name = "UTF-8",
            Value = FlowerReport.SafeString(Encoding.UTF8.GetString(_bytes))
        });

        Formats.Add(new ByteFormatView
        {
            Name = "UTF-16/WTF",
            Value = FlowerReport.SafeString(Encoding.Unicode.GetString(_bytes))
        });

        foreach (var cp in new[] { 437, 866, 1251, 1252 })
            try
            {
                var enc = Encoding.GetEncoding(cp);
                Formats.Add(new ByteFormatView
                {
                    Name = $"cp{cp}",
                    Value = FlowerReport.SafeString(enc.GetString(_bytes))
                });
            }
            catch
            {
                // Encoding not available
            }

        AddIntFormats(_bytes.Length);
    }

    private void AddIntFormats(int size)
    {
        if (_bytes.Length < size)
            return;

        var span = _bytes.AsSpan(0, size);

        var lsbWord = ReadUnsigned(span, false);
        var msbWord = ReadUnsigned(span, true);

        var lsbSword = SignExtend((long)lsbWord, size * 8);
        var msbSword = SignExtend((long)msbWord, size * 8);

        var postfix = $"X{size * 2}";
        Formats.Add(new ByteFormatView
        {
            Name = $"{size * 8}-bit WORD LSB",
            Value = "0x" + lsbWord.ToString(postfix),
            SignedValue = string.Empty
        });
        Formats.Add(new ByteFormatView
        {
            Name = $"{size * 8}-bit WORD MSB",
            Value = "0x" + msbWord.ToString(postfix),
            SignedValue = string.Empty
        });
        Formats.Add(new ByteFormatView
        {
            Name = $"u{size * 8}/i{size * 8} LSB",
            Value = lsbWord.ToString(),
            SignedValue = lsbSword.ToString()
        });
        Formats.Add(new ByteFormatView
        {
            Name = $"u{size * 8}/i{size * 8} MSB",
            Value = msbWord.ToString(),
            SignedValue = msbSword.ToString()
        });
    }

    private static ulong ReadUnsigned(ReadOnlySpan<byte> data, bool bigEndian)
    {
        ulong v = 0;
        if (bigEndian)
            foreach (var b in data)
                v = (v << 8) | b;
        else
            for (var i = data.Length - 1; i >= 0; i--)
                v = (v << 8) | data[i];
        return v;
    }

    private static long SignExtend(long value, int bits)
    {
        if (bits >= 64)
            return value;
        var signBit = 1L << (bits - 1);

        return (value & signBit) != 0
            ? value - (signBit << 1)
            : value;
    }

    [RelayCommand]
    private void CopyHex()
    {
        // Stub: the hex dump text can be copied directly from the text box.
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

    public sealed class ByteFormatView
    {
        public string Name { get; init; } = string.Empty;
        public string Value { get; init; } = string.Empty;
        public string SignedValue { get; init; } = string.Empty;
    }
}
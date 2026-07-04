//
// CoffeeLake (C) 2026-*
//
using System;
using System.IO;
using System.Linq;

namespace SunFlower.Client.Services;

public class ContentDetectionResult
{
    public bool IsBinary { get; set; }
    public bool IsText { get; set; }
    public ContentSubType SubType { get; set; } = ContentSubType.Unknown;
    public string Description { get; set; } = "Unknown";
    public string MimeType { get; set; } = "application/octet-stream";
}

public enum ContentSubType
{
    Unknown,
    Markdown,
    Assembly,
    PlainText,
    Binary,
    Image
}

public static class ContentTypeDetector
{
    /// <summary>
    /// Maximum slice to the check 
    /// </summary>
    private const int ProbeSize = 8192;
    private const double BinaryThreshold = 0.05;

    public static ContentDetectionResult Detect(string filePath)
    {
        if (!File.Exists(filePath))
            return new ContentDetectionResult { Description = "File not found" };

        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        var result = new ContentDetectionResult();
        // Fast check
        if (IsTextExtension(ext))
        {
            result.IsText = true;
            result.IsBinary = false;
            result.SubType = GetTextSubType(ext);
            result.Description = GetTextDescription(ext);
            result.MimeType = GetTextMimeType(ext);
            return result;
        }

        using var stream = File.OpenRead(filePath);
        var probeBuffer = new byte[Math.Min(ProbeSize, stream.Length)];
        _ = stream.Read(probeBuffer, 0, probeBuffer.Length);

        var magicResult = CheckMagicBytes(probeBuffer);
        if (magicResult != null)
            return magicResult;

        var nullByteCount = probeBuffer.Count(t => t == 0);

        var nullRatio = (double)nullByteCount / probeBuffer.Length;

        if (nullRatio > BinaryThreshold)
        {
            result.IsBinary = true;
            result.IsText = false;
            result.SubType = ContentSubType.Binary;
            result.Description = "Binary data";
            result.MimeType = "application/octet-stream";
        }
        else
        {
            // ASCII
            var printableCount = probeBuffer
                .Count(b => b >= 32 && b <= 126 || b == '\n' || b == '\r' || b == '\t');

            var printableRatio = (double)printableCount / probeBuffer.Length;
            if (printableRatio > 0.8)
            {
                result.IsText = true;
                result.IsBinary = false;
                result.SubType = ContentSubType.PlainText;
                result.Description = "Plain text";
                result.MimeType = "text/plain";
            }
            else
            {
                result.IsBinary = true;
                result.IsText = false;
                result.SubType = ContentSubType.Binary;
                result.Description = "Binary data";
                result.MimeType = "application/octet-stream";
            }
        }

        return result;
    }

    private static ContentDetectionResult? CheckMagicBytes(byte[] header)
    {
        if (header.Length < 4) return null;
        
        // Intel HEX
        if (header[0] == 0x3A && header[1] == 0x30) // ":0" — start of Intel HEX record
            return new ContentDetectionResult
            {
                IsBinary = false, IsText = true,
                SubType = ContentSubType.PlainText,
                Description = "Intel HEX",
                MimeType = "text/plain"
            };

        // PNG
        if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47)
            return new ContentDetectionResult
            {
                IsBinary = true, IsText = false,
                SubType = ContentSubType.Image,
                Description = "PNG Image",
                MimeType = "image/png"
            };

        return null;
    }

    private static bool IsTextExtension(string ext) => ext switch
    {
        ".md" or ".markdown" => true,
        ".asm" or ".s" or ".inc" => true,
        ".txt" or ".text" or ".log" => true,
        ".xml" or ".json" or ".yaml" or ".yml" => true,
        ".html" or ".htm" or ".xhtml" => true,
        ".csv" or ".tsv" => true,
        ".cfg" or ".conf" or ".ini" => true,
        ".cs" or ".fs" or ".vb" or ".rs" or ".py" or ".js" or ".ts" or ".c" or ".h" or ".cpp" or ".hpp" or "zig" => true,
        ".md" or ".rst" or ".adoc" => true,
        _ => false
    };

    private static ContentSubType GetTextSubType(string ext) => ext switch
    {
        ".md" or ".markdown" => ContentSubType.Markdown,
        ".asm" or ".s" or ".inc" => ContentSubType.Assembly,
        _ => ContentSubType.PlainText
    };

    private static string GetTextDescription(string ext) => ext switch
    {
        ".md" or ".markdown" => "Markdown",
        ".asm" or ".s" or ".inc" => "Assembly",
        _ => "Plain text"
    };

    private static string GetTextMimeType(string ext) => ext switch
    {
        ".md" => "text/markdown",
        ".asm" => "text/x-asm",
        ".html" => "text/html",
        ".json" => "application/json",
        _ => "text/plain"
    };
}
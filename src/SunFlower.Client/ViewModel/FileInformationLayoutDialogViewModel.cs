// CoffeeLake (C) 2026-*
// 
// The FileInformationLayoutDialogViewModel.cs represents <what?>
// 
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com


using SunFlower.Client.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SunFlower.Kernel.Readers;

namespace SunFlower.Client.ViewModel;

public partial class FileInformationLayoutDialogViewModel(string path) : DialogViewModel
{
    [ObservableProperty] private FlowerFileInfo? _flowerFileInfo = FlowerBinarySeeker.Get(path);

    [ObservableProperty] private bool _isFileSystemBusy;
    [ObservableProperty] private bool _isStringsBusy;

    [ObservableProperty] private FileSystemSummary? _fileSystem;
    [ObservableProperty] private StringsSummary? _strings;
    
    public IReadOnlyList<InfoRow> FileSystemRows => _fileSystem is { } fs
        ? new[]
        {
            new InfoRow { Key = "Full path", Value = fs.FullPath },
            new InfoRow { Key = "Size", Value = fs.SizeHuman },
            new InfoRow { Key = "Created", Value = FormatDate(fs.Created) },
            new InfoRow { Key = "Last access", Value = FormatDate(fs.LastAccess) },
            new InfoRow { Key = "Last write", Value = FormatDate(fs.LastWrite) },
            new InfoRow { Key = "Attributes", Value = fs.Attributes },
            new InfoRow { Key = "Unix permissions", Value = fs.UnixPermissions },
            new InfoRow { Key = "Owner", Value = fs.Owner },
            new InfoRow { Key = "SHA-256", Value = fs.Sha256 },
            new InfoRow { Key = "SHA-1", Value = fs.Sha1 },
            new InfoRow { Key = "MD5", Value = fs.Md5 },
        }.Where(r => !string.IsNullOrWhiteSpace(r.Value)).ToArray()
        : [];

    public IReadOnlyList<InfoRow> StringsRows => _strings is { } s
        ? new[]
        {
            new InfoRow { Key = "ASCII strings", Value = s.AsciiCount.ToString() },
            new InfoRow { Key = "UTF-16 strings", Value = s.Utf16Count.ToString() },
            new InfoRow { Key = "URLs", Value = FormatList(s.Urls) },
            new InfoRow { Key = "IP addresses", Value = FormatList(s.Ips) },
            new InfoRow { Key = "Domains", Value = FormatList(s.Domains) },
            new InfoRow { Key = "Emails", Value = FormatList(s.Emails) },
            new InfoRow { Key = "Windows paths", Value = FormatList(s.WindowsPaths) },
            new InfoRow { Key = "Unix paths", Value = FormatList(s.UnixPaths) },
        }.Where(r => !string.IsNullOrWhiteSpace(r.Value)).ToArray()
        : [];

    public async Task LoadAsync()
    {
        await Task.WhenAll(LoadFileSystemAsync(), LoadStringsAsync());
        OnPropertyChanged(nameof(FileSystemRows));
        OnPropertyChanged(nameof(StringsRows));
    }

    private async Task LoadFileSystemAsync()
    {
        _isFileSystemBusy = true;
        try
        {
            var fi = new FileInfo(path);
            if (!fi.Exists) { _fileSystem = new FileSystemSummary { Error = "File not found" }; return; }

            var attrs = fi.Attributes.ToString();
            var unix = TryGetUnixMode(fi.FullName);
            var owner = TryGetOwner(fi.FullName);

            var (sha256, sha1, md5) = await ComputeHashesAsync(fi.FullName);

            _fileSystem = new FileSystemSummary
            {
                FullPath = fi.FullName,
                SizeBytes = fi.Length,
                SizeHuman = FormatSize(fi.Length),
                Created = fi.CreationTimeUtc,
                LastAccess = fi.LastAccessTimeUtc,
                LastWrite = fi.LastWriteTimeUtc,
                Attributes = attrs,
                UnixPermissions = unix,
                Owner = owner,
                Sha256 = sha256,
                Sha1 = sha1,
                Md5 = md5
            };
        }
        catch (Exception ex) { _fileSystem = new FileSystemSummary { Error = ex.Message }; }
        finally { _isFileSystemBusy = false; }
    }

    private static async Task<(string Sha256, string Sha1, string Md5)> ComputeHashesAsync(string file)
    {
        await using var stream = File.OpenRead(file);
        
        using var sha256 = SHA256.Create();
        using var sha1 = SHA1.Create();
        using var md5 = MD5.Create();

        var buffer = new byte[81920];
        int read;
        while ((read = await stream.ReadAsync(buffer)) > 0)
        {
            sha256.TransformBlock(buffer, 0, read, null, 0);
            sha1.TransformBlock(buffer, 0, read, null, 0);
            md5.TransformBlock(buffer, 0, read, null, 0);
        }
        sha256.TransformFinalBlock(buffer, 0, 0);
        sha1.TransformFinalBlock(buffer, 0, 0);
        md5.TransformFinalBlock(buffer, 0, 0);

        return (ToHex(sha256.Hash!), ToHex(sha1.Hash!), ToHex(md5.Hash!));
    }

    private static string ToHex(byte[] bytes) => Convert
        .ToHexString(bytes)
        .ToLowerInvariant();

    private static string FormatSize(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
        _ => $"{bytes / (1024.0 * 1024):F1} MB"
    };

    private static string FormatDate(DateTimeOffset? dt) =>
        dt?.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") ?? "?";

    private async Task LoadStringsAsync()
    {
        _isStringsBusy = true;
        try
        {
            byte[] bytes;
            try
            {
                bytes = await File.ReadAllBytesAsync(path);
            }
            catch (Exception ex)
            {
                _strings = new StringsSummary { Error = ex.Message };
                return;
            }

            var ascii = ExtractAsciiStrings(bytes, 4);
            var utf16 = ExtractUtf16Strings(bytes, 4);

            // no matches at the array bounds 
            var combined = string.Join('\n', ascii.Concat(utf16));

            _strings = new StringsSummary
            {
                AsciiCount = ascii.Count,
                Utf16Count = utf16.Count,
                SampleAscii = ascii.Take(20).ToArray(),
                SampleUtf16 = utf16.Take(20).ToArray(),
                Urls = ExtractUrls(combined),
                Ips = ExtractIps(combined),
                Domains = ExtractDomains(combined),
                Emails = ExtractEmails(combined),
                WindowsPaths = ExtractWindowsPaths(combined),
                UnixPaths = ExtractUnixPaths(combined),
            };
        }
        catch (Exception ex)
        {
            _strings = new StringsSummary { Error = ex.Message };
        }
        finally
        {
            _isStringsBusy = false;
        }
    }

    private static List<string> ExtractAsciiStrings(byte[] bytes, int minLen)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        foreach (var b in bytes)
        {
            if (b >= 0x20 && b <= 0x7E) sb.Append((char)b);
            else { if (sb.Length >= minLen) result.Add(sb.ToString()); sb.Clear(); }
        }
        if (sb.Length >= minLen) result.Add(sb.ToString());
        return result;
    }

    private static List<string> ExtractUtf16Strings(byte[] bytes, int minLen)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        for (var i = 0; i + 1 < bytes.Length; i += 2)
        {
            var c = (char)(bytes[i] | (bytes[i + 1] << 8));
            
            if (c >= 0x20 && c <= 0x7E)
            {
                sb.Append(c);
            }
            else
            {
                if (sb.Length >= minLen) 
                    result.Add(sb.ToString()); sb.Clear();
            }
        }
        if (sb.Length >= minLen) result.Add(sb.ToString());
        return result;
    }
    
    private static readonly Regex UrlRegex = new(@"https?://[^\s""'<>]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex IpRegex = new(@"\b(?:\d{1,3}\.){3}\d{1,3}\b", RegexOptions.Compiled);
    private static readonly Regex EmailRegex = new(@"[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex WinPathRegex = new(@"[A-Za-z]:\\[^\s""'<>|]+", RegexOptions.Compiled);
    private static readonly Regex UnixPathRegex = new(@"(?<!\w)/(?:[\w.-]+/)*[\w.-]+", RegexOptions.Compiled);

    private static string[] ExtractUrls(string text) => UrlRegex.Matches(text).Select(m => m.Value).Distinct().ToArray();
    private static string[] ExtractIps(string text) => IpRegex.Matches(text).Select(m => m.Value).Distinct().ToArray();
    private static string[] ExtractEmails(string text) => EmailRegex.Matches(text).Select(m => m.Value).Distinct().ToArray();
    private static string[] ExtractWindowsPaths(string text) => WinPathRegex.Matches(text).Select(m => m.Value).Distinct().ToArray();
    private static string[] ExtractUnixPaths(string text) => UnixPathRegex.Matches(text).Select(m => m.Value).Distinct().ToArray();

    private static string?[] ExtractDomains(string text)
    {
        var fromUrls = UrlRegex.Matches(text)
            .Select(m => Uri.TryCreate(m.Value, UriKind.Absolute, out var uri) ? uri.Host : null)
            .Where(h => !string.IsNullOrEmpty(h));
        var standalone = Regex
            .Matches(text, @"\b(?:[a-z0-9-]+\.)+[a-z]{2,}\b", RegexOptions.IgnoreCase)
            .Select(m => m.Value);
        return fromUrls
            .Concat(standalone)
            .Distinct()
            .ToArray();
    }

    private static string FormatList(IReadOnlyList<string> items) =>
        items.Count == 0 ? "" : string.Join(", ", items.Take(10)) + (items.Count > 10 ? " ..." : "");
    
    private static string TryGetUnixMode(string file)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) 
            return "";
        try
        {
            return File.GetUnixFileMode(file).ToString();
        }
        catch
        {
            return "";
        }
    }
    private static string TryGetOwner(string file)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return "";
        try
        {
            // Unix: stat -c %U. <-- wait with no cancellation token
            // Windows: shell api? /Native/File static class
            return "";
        }
        catch { return ""; }
    }

    [RelayCommand]
    private void Exit() => DialogService?.CloseDialog();
}
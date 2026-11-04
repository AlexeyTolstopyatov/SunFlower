// CoffeeLake (C) 2026-*
// 
// The FileSystemSummary.cs represents <what?>
// 
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com

using System;

namespace SunFlower.Client.Model;

public sealed class FileSystemSummary
{
    public string FullPath { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
    public string SizeHuman { get; init; } = string.Empty;
    public DateTimeOffset? Created { get; init; }
    public DateTimeOffset? LastAccess { get; init; }
    public DateTimeOffset? LastWrite { get; init; }
    public string Attributes { get; init; } = string.Empty;
    public string UnixPermissions { get; init; } = string.Empty;
    public string Owner { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public string Sha1 { get; init; } = string.Empty;
    public string Md5 { get; init; } = string.Empty;
    public string Error { get; init; } = string.Empty;
}
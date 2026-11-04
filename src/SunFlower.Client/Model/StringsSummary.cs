// CoffeeLake (C) 2026-*
// 
// The StringsSummary.cs represents <what?>
// 
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com

using System.Collections.Generic;

namespace SunFlower.Client.Model;

public sealed class StringsSummary
{
    public int AsciiCount { get; init; }
    public int Utf16Count { get; init; }
    public IReadOnlyList<string> SampleAscii { get; init; } = [];
    public IReadOnlyList<string> SampleUtf16 { get; init; } = [];
    public IReadOnlyList<string> Urls { get; init; } = [];
    public IReadOnlyList<string> Ips { get; init; } = [];
    public IReadOnlyList<string> Domains { get; init; } = [];
    public IReadOnlyList<string> Emails { get; init; } = [];
    public IReadOnlyList<string> WindowsPaths { get; init; } = [];
    public IReadOnlyList<string> UnixPaths { get; init; } = [];
    public string Error { get; init; } = "";
}
//
// CoffeeLake (C) 2026-*
//
// DisassemblingService provides a direct integration of SunFlower.Dasm
// into the client UI, independent of the plugin system.
//

using System;
using System.IO;
using Sunflower.Dasm;

namespace SunFlower.Client.Service;

/// <summary>
/// Target CPU architecture for disassembly.
/// </summary>
public enum DisassemblerArchitecture
{
    I8086,
    I80186,
    I80286,
    I80386
}

public class DisassemblingService
{
    private readonly WorkspaceService _workspaceService;

    public DisassemblingService(WorkspaceService workspaceService)
    {
        _workspaceService = workspaceService;
    }

    /// <summary>
    /// Disassemble a byte range, starting at the given offset.
    /// </summary>
    public string DisassembleRange(byte[] bytes, int startOffset,
        DisassemblerArchitecture arch = DisassemblerArchitecture.I80286)
    {
        if (bytes.Length == 0)
            return "; (empty range)";

        int[] entryPoints = [startOffset & 0xFFFF];
        var interruptsPath = ResolveInterruptsPath();

        return arch switch
        {
            DisassemblerArchitecture.I8086 =>
                I8086Decoder.decodeRecursive(interruptsPath, bytes, entryPoints),
            DisassemblerArchitecture.I80186 =>
                I80186Decoder.decodeRecursive(interruptsPath, bytes, entryPoints),
            DisassemblerArchitecture.I80286 =>
                I80286Decoder.decodeRecursive(interruptsPath, bytes, entryPoints),
            DisassemblerArchitecture.I80386 =>
                I80386Decoder.decodeRecursive(interruptsPath, bytes, entryPoints),
            _ => I80286Decoder.decodeRecursive(interruptsPath, bytes, entryPoints)
        };
    }

    /// <summary>
    /// Resolve path to the DOS interrupt table descriptor.
    /// </summary>
    private static string ResolveInterruptsPath()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var relative = Path.Combine("Interrupt", "dos.json");
        var fromBase = Path.Combine(baseDir, relative);

        if (File.Exists(fromBase))
            return fromBase;

        var dasmDir = Path.GetDirectoryName(
            typeof(I8086Decoder).Assembly.Location);
        if (dasmDir != null)
        {
            var fromDasm = Path.Combine(dasmDir, relative);
            if (File.Exists(fromDasm))
                return fromDasm;
        }

        return string.Empty;
    }
}
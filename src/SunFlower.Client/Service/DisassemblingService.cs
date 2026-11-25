//
// CoffeeLake (C) 2026-*
//
// DisassemblingService provides a direct integration of SunFlower.Dasm
// into the client UI, independent of the plugin system.
//

using System;
using System.IO;
using Sunflower.Dasm;
using Sunflower.Dasm.Intel.Core;

namespace SunFlower.Client.Service;


public class DisassemblingService(WorkspaceService workspaceService)
{
    private readonly WorkspaceService _workspaceService = workspaceService;

    /// <summary>
    ///     Disassemble a byte range, starting at the given offset.
    /// </summary>
    public string DisassembleRange(byte[] bytes, int startOffset,
        CpuModeType arch = CpuModeType.I286)
    {
        if (bytes.Length == 0)
            return "; (empty range)";

        int[] entryPoints = [startOffset & 0xFFFF];
        var interruptsPath = ResolveInterruptsPath();

        return "Under Construction";
    }

    /// <summary>
    ///     Resolve path to the DOS interrupt table descriptor.
    /// </summary>
    private static string ResolveInterruptsPath()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var relative = Path.Combine("Interrupt", "dos.json");
        var fromBase = Path.Combine(baseDir, relative);

        if (File.Exists(fromBase))
            return fromBase;
        
        var fromDasm = Path.Combine(baseDir, relative);
        return File.Exists(fromDasm) 
            ? fromDasm 
            : string.Empty;
    }
}
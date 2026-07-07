using System;

namespace SunFlower.Client.Service;

public static class ArrayExtension
{
    /// <summary>
    /// Large memory segments are unable to index the
    /// standard ways which .NET represents.
    /// </summary>
    /// <param name="start">Offset in the source array</param>
    /// <param name="length">Length of selection in the source array</param>
    /// <param name="source">Reference to the source array </param>
    /// <param name="result">Reference to the array with extracted bytes</param>
    public static void ExtractBytes(ulong start, ulong length, byte[] source, out byte[] result)
    {
        if (start + length > (ulong)source.LongLength)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Specified range is out of the source array bounds.");
        }
        Span<byte> sourceSpan = source;
        var slice = sourceSpan.Slice((int)start, (int)length);

        result = new byte[length];
        slice.CopyTo(result);
    }
}

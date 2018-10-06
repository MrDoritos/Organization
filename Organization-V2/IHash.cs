using System;
using System.Collections.Generic;
using System.Text;

namespace Organization_V2
{
    public interface IHashable
    {
        SHA256 SHA256 { get; }
        byte[] SHA1 { get; }
        byte[] CRC32 { get; }
        byte[] CRC64 { get; }
        bool Compare(IHashable h);
    }

    public interface IHash
    {
        bool Compare(IHash h);
        byte this[int i] { get; }
        byte[] Hash { get; }
        int Length { get; }
        byte Type { get; }
    }
}

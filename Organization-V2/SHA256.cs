using System;
using System.Collections.Generic;
using System.Text;

namespace Organization_V2
{
    public class SHA256 : IHash
    {
        public SHA256(byte[] hash) { Change(hash); }
        public int Length { get; } = 32; //32 bytes, not bits
        public byte Type { get; } = 1;
        public byte[] Hash { get; private set; }
        public byte this[int i] { get => Hash[i]; }
        public void Change(byte[] hash)
        {
            if (hash.Length != 32) throw new ArgumentException("Hash should be 256 bits or 32 bytes in size", "hash");
            Hash = hash;
        }
        public bool Compare(SHA256 h)
        {            
            for (int i = 0; i < 32; i++)
                if (this[i] != h[i]) return false;
            return true;
        }

        public bool Compare(IHash h)
        {
            if (h.Type != Type) throw new ArgumentException("Not the same hashing heuristic", "h");
            for (int i = 0; i < 32; i++)
                if (this[i] != h[i]) return false;
            return true;
        }
    }
}

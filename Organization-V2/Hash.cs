using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace Organization_V2
{
    class Hash : IHashable
    {
        public Hash(string filepath) { HashFile(filepath); }

        public SHA256 SHA256 { get; private set; }

        public byte[] SHA1 { get; private set; }

        public byte[] CRC32 { get; private set; }

        public byte[] CRC64 { get; private set; }

        public bool Compare(IHashable h)
        {
            if (h == null || SHA256 == null) return false;
            if (h.SHA256.Compare(SHA256)) return true; else return false;
        }

        public void HashFile(string filepath)
        {
            using (FileStream str = File.OpenRead(filepath))
            {
                using (SHA256Managed b = new SHA256Managed())
                {
                    SHA256 = new SHA256(b.ComputeHash(str));
                }
            }
        }
    }
}

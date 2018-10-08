using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Organization_V2
{
    public static class Extension
    {
        static public byte[] Combine(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }

        static public byte[] GetBytes(this int i)
        {
            return new byte[] { (byte)(i >> 24), (byte)(i >> 16), (byte)(i >> 8), (byte)i };
        }

        static public byte[] GetBytes(this short i)
        {
            return new byte[] { (byte)(i >> 8), (byte)(i) };
        }

        static public byte[] GetBytes(this ushort i)
        {
            return new byte[] { (byte)(i >> 8), (byte)(i) };
        }

        static public byte[] GetBytes(this long i)
        {
            return new byte[] { (byte)(i >> 54), (byte)(i >> 48), (byte)(i >> 40), (byte)(i >> 32), (byte)(i >> 24), (byte)(i >> 16), (byte)(i >> 8), (byte)i };
        }

        static public byte[] GetBytes(this byte i)
        {
            return new byte[] { i };
        }

        static public byte[] GetBytes(this bool i)
        {
            return new byte[] { (byte)(i ? 0 : 1) };
        }

        static public bool Equals(this byte[] i, byte[] a)
        {
            bool equal = true;
            for (int it = 0; !equal && it < i.Length && it < a.Length; it++)
                equal = (i[it] == a[it]);
            return equal;
        }

        /// <summary>
        /// Broken ATM, since the SHL and SHR operators are for 32 bit integers only
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="pos">0 based index of the long</param>
        /// <returns></returns>
        static public long GetLong(byte[] buffer, ref int pos)
        {
            pos += 8;
            return (((long)buffer[pos - 8] << 54) | ((long)buffer[pos - 7] << 48) | ((long)buffer[pos - 6] << 40) | ((long)buffer[pos - 5] << 32) | ((long)buffer[pos - 4] << 24) | ((long)buffer[pos - 3] << 16) | ((long)buffer[pos - 2] << 8) | ((long)buffer[pos - 1]));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="pos">0 based index of the integer</param>
        /// <returns></returns>
        static public int GetInt(byte[] buffer, ref int pos)
        {
            pos += 4;
            return ((buffer[pos - 4] << 24) | (buffer[pos - 3] << 16) | (buffer[pos - 2] << 8) | (buffer[pos - 1]));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="pos">0 based index of the short</param>
        /// <returns></returns>
        static public short GetShort(byte[] buffer, ref int pos)
        {
            pos += 2;
            return (short)((buffer[pos - 2] << 8) | (buffer[(pos - 1)]));
        }

        static public ushort GetUshort(byte[] buffer, ref int pos)
        {
            pos += 2;
            return (ushort)((buffer[pos - 2] << 8) | (buffer[pos - 1]));
        }
        
        static public ushort GetUshort(byte[] buffer)
        {
            return (ushort)((buffer[0] << 8) | (buffer[1]));
        }

        static public short GetShort(byte[] buffer)
        {
            return (short)((buffer[0] << 8) | (buffer[1]));
        }

        static public int GetInt(byte[] buffer)
        {
            return ((buffer[0] << 24) | (buffer[1] << 16) | (buffer[2] << 8) | (buffer[3]));
        }

        static public byte GetByte(byte[] buffer, ref int pos)
        {
            pos++;
            return buffer[pos - 1];
        }

        /// <summary>
        /// Compute the distance between two strings.
        /// </summary>
        public static int Compute(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }
    }
}

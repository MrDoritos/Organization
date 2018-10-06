using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Organization_V2
{
    public static class Database
    {
        public static CentralDirectory Load(string filepath)
        {
            using (FileStream fs = File.OpenRead(filepath))
                return Load(fs);
        }

        public static void Save(string filepath, CentralDirectory i)
        {
            using (FileStream fs = File.OpenWrite(filepath))
                Save(fs, i);    
        }

        private static CentralDirectory Load(FileStream fs)
        {
            var a = new Loader(fs);
            var b = LoadCentralIndex(a);
            var c = LoadSoftDir(a, b).SubDirectories;
            return new CentralDirectory(b, c);
        }

        private static void Save(FileStream fs, CentralDirectory i)
        {
            var a = new UnLoader(fs);
            SaveCentralIndex(a, i);
            var b = i.Directories;
            foreach (var c in b)
                SaveSoftDir(a, c);
        }

        private static CentralIndex LoadCentralIndex(Loader a)
        {
            int nextid = a.NextInt;
            int numoffiles = a.NextInt;
            
            SoftFile[] cindex = new SoftFile[numoffiles];
            for (int i = 0; i < numoffiles; i++)
                cindex[i] = LoadSoftFile(a);
            return new CentralIndex(nextid, cindex);
        }

        private static void SaveCentralIndex(UnLoader a, CentralIndex s)
        {
            a.PackInt(s.CurId);
            var files = s.Files;
            int fileCount = files.Count;
            a.PackInt(fileCount);

            for (int i = 0; i < fileCount; i++)
                SaveSoftFile(a, files[i]);
        }

        private static SoftDirectory LoadSoftDir(Loader a, CentralIndex s)
        {
            int id = a.NextInt;
            int subDirCount = a.NextInt;
            int fileCount = a.NextInt;
            string name = a.NextString;
            string[] tags = Tags(a);
            SoftDirectory[] subDirs = new SoftDirectory[subDirCount];
            SoftFile[] softFiles = new SoftFile[fileCount];
            for (int i = 0; i < subDirCount; i++)
                subDirs[i] = LoadSoftDir(a, s);
            for (int i = 0; i < fileCount; i++)
                softFiles[i] = s.FirstOrDefault(a.NextInt);
            return new SoftDirectory(id, name, subDirs, softFiles, tags);
        }

        private static void SaveSoftDir(UnLoader a, SoftDirectory i)
        {
            var subdirs = i.SubDirectories;
            var files = i.SoftFiles;
            int subdircount = subdirs.Count;
            int filecount = files.Count;
            a.PackInt(i.Id);
            a.PackInt(subdircount);
            a.PackInt(filecount);
            a.PackString(i.Name);
            Tags(a, i.TagArray);
            for (int b = 0; b < subdircount; b++)
                SaveSoftDir(a, subdirs[b]);
            for (int b = 0; b < filecount; b++)
                a.PackInt(files[b].Id);            
        }

        private static SoftFile LoadSoftFile(Loader a)
        {
            int id = a.NextInt;
            string name = a.NextString;
            string thumbnailpath = a.NextString;
            string[] tags = Tags(a);
            SHA256 a1 = LoadSHA256(a);
            return new SoftFile(id, name, thumbnailpath, tags, a1);
        }   

        private static void SaveSoftFile(UnLoader a, SoftFile i)
        {
            a.PackInt(i.Id);
            a.PackString(i.Name);
            a.PackString(i.ThumbnailPath);
            Tags(a, i.TagArray);
            SaveSHA256(a, i.SHA256);
        }

        private static SHA256 LoadSHA256(Loader a)
        {
            if (a.NextByte < 32) return null;
            else return new SHA256(a.Load(32));
        }

        private static void SaveSHA256(UnLoader a, SHA256 i)
        {
            if (i == null || i.Hash == null) a.PackByte(0);
            else { a.Pack(i.Hash); }
        }
        
        private static string[] Tags(Loader a)
        {
            int count = a.NextInt;
            string[] vs = new string[count];
            for (int i = 0; i < count; i++)
                vs[i] = a.NextString;
            return vs;
        }

        private static void Tags(UnLoader a, string[] i)
        {
            a.PackInt(i.Length);
            for (int b = 0; b < i.Length; b++)
                a.PackString(i[b]);
        }
    }

    class Loader
    {
        private FileStream _fs;

        //private int _pos;

        //public int Position { get => _pos; }

        public Loader(FileStream fs)
        {
            _fs = fs;
            //_pos = 0;
        }

        public int NextInt { get => Extension.GetInt(Load(4)); }

        public short NextShort { get => Extension.GetShort(Load(2)); }

        public byte NextByte { get => (byte)_fs.ReadByte(); }

        public string NextString { get => LoadString(); }

        public string LoadString(int len, Encoding e)
        {
            return e.GetString(Load(len));
        }

        /// <summary>
        /// Loads the next string in the buffer, with length defined as an integer, and the encoding as the default windows encoding
        /// </summary>
        /// <returns></returns>
        private string LoadString()
        {
            return Encoding.Default.GetString(Load(NextInt));
        }

        public byte[] Load(int len)
        {
            byte[] vs = new byte[len];
            _fs.Read(vs, 0, len);
            return vs;
        }
    }

    class UnLoader
    {
        private FileStream _fs;

        public UnLoader(FileStream fs)
        {
            _fs = fs;
        }

        public void PackInt(int i) { _fs.Write(Extension.GetBytes(i), 0, 4); }

        public void PackByte(byte i) { _fs.WriteByte(i); }

        public void PackShort(short i) { _fs.Write(Extension.GetBytes(i), 0, 2); }
        
        public void PackString(string i, Encoding e) { var a = e.GetBytes(i); PackInt(a.Length); _fs.Write(a, 0, a.Length); }        

        public void PackString(string i) { var a = Encoding.Default.GetBytes(i); PackInt(a.Length); _fs.Write(a, 0, a.Length); }

        public void Pack(byte[] i) { _fs.Write(i, 0, i.Length); }
    }
}

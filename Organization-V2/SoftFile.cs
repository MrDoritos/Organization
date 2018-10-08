using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Organization_V2
{
    public class SoftFile : IFile, ITaggable, IID, IHashable, IThumbable
    {
        private List<string> _tags;

        private List<SoftFile> _references;

        //public static bool operator !=(SoftFile i, SoftFile b)
        //{
        //    return ((!i.Compare(b) && i.Id != b.Id));
        //}        

        //public static bool operator== (SoftFile i, SoftFile b)
        //{
        //    return (i.Compare(b) && i.Id == b.Id);
        //}

        public bool Equals(SoftFile obj)
        {
            return ((Compare(obj) && Id == obj.Id));
        }

        public bool Equals(int id)
        {
            return Id == id;
        }

        public bool Equals(IHashable i)
        {
            return (Compare(i));
        }

        public SoftFile(int id, string name, string thumbnailpath, string[] tags)
        {
            Name = name;
            Id = id;
            ThumbnailPath = thumbnailpath ?? "";
            _tags = new List<string>(tags ?? new string[0]);
            _references = new List<SoftFile>();
        }
        public SoftFile(int id, string name, string thumbnailpath, string[] tags, SHA256 hash)
        {
            Name = name;
            Id = id;
            ThumbnailPath = thumbnailpath ?? "";
            _tags = new List<string>(tags ?? new string[0]);
            SHA256 = hash;
            _references = new List<SoftFile>();
        }
        public SoftFile(int id, string name)
        {
            Name = name;
            Id = id;
            ThumbnailPath = "";
            _tags = new List<string>();
            _references = new List<SoftFile>();
        }

        public string Hashes =>
            $"SHA256: {(SHA256 != null ? BitConverter.ToString(SHA256.Hash).Replace("-", "").ToLower() : "none")}";    

        public override string ToString()
        {
            return Name;
        }

        public string Name { get; set; }
        public int Id { get; }
        public string ThumbnailPath { get; set; }
        public bool ThumbnailExists { get => (ThumbnailPath.Length > 0 && File.Exists(ThumbnailPath)); }
        public IReadOnlyList<string> Tags => _tags;
        public IReadOnlyList<SoftFile> References => _references;
        public string[] TagArray => _tags.ToArray();

        public SHA256 SHA256 { get; private set; }

        public byte[] SHA1 => throw new NotImplementedException();

        public byte[] CRC32 => throw new NotImplementedException();

        public byte[] CRC64 => throw new NotImplementedException();

        public FileStream Thumbnail => File.OpenRead(ThumbnailPath);

        public void Hash(string filepath)
        {
            using (FileStream str = File.OpenRead(filepath))
            {
                using (SHA256Managed b = new SHA256Managed())
                {
                    SHA256 = new SHA256(b.ComputeHash(str));
                }
            }
        }

        public void AddTag(string tag)
        {
            _tags.Add(tag.ToLower());
        }

        public void AddTags(params string[] tags)
        {
            _tags.AddRange(new List<string>(tags.Select(n => n.ToLower())));
        }

        public void RemoveTag(string tag)
        {
            _tags.RemoveAll(n => n == tag);
        }

        public void RemoveTags(params string[] tags)
        {
            _tags.RemoveAll(n => tags.Any(m => m.ToLower().Equals(n)));
        }

        public void RemoveTags()
        {
            _tags = new List<string>();
        }

        public bool Compare(IHashable h)
        {
            if (h == null || SHA256 == null) return false;
            if (h.SHA256.Compare(SHA256)) return true; else return false;
        }

        public void AddReference(SoftFile i)
        {
           if (!ReferenceExists(i)) { _references.Add(i); }
        }

        public bool ReferenceExists(IID i)
        {
            return _references.Exists(n => n.Id == i.Id);
        }

        public void RemoveReference(IID i)
        {
            _references.RemoveAll(n => n.Id == i.Id);
        }        
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;

namespace Organization.Objects
{
    public class SoftFile
    {
        public SoftFile(string name, uint id) { Name = name ?? ""; Id = id; _tags = new List<string>(); Description = ""; }
        public SoftFile(string name, string thumbnail, uint id) { Name = name ?? ""; Thumbnail = thumbnail ?? ""; Description = ""; Id = id; _tags = new List<string>(); }
        public SoftFile(string name, string thumbnail, uint id, string description, string[] tags, string hash) { Name = name ?? ""; Thumbnail = thumbnail ?? ""; Id = id; Description = description ?? ""; _tags = (tags ?? new string[0]).ToList(); }

        public uint Id { get; }
        public string Name { get; private set; }
        public string Thumbnail { get; private set; }
        public bool ThumbnailExists { get => File.Exists(Thumbnail); }
        public string Description { get; set; }
        public string Hash { get; private set; } = "";
        private List<string> _tags;

        public void AddTag(string tag)
        {
            tag = tag.ToLower();
            if (!_tags.Contains(tag))
            {
                _tags.Add(tag);
            }
        }

        public void AddTags(string[] tag)
        {
            foreach (var a in tag)
                if (!_tags.Contains(a)) _tags.Add(a);
        }

        public void RemoveTag(string tag)
        {
            _tags.RemoveAll(n => n == tag);
        }

        public IReadOnlyList<string> Tags { get => _tags; }

        public void GetHash(string file)
        {
            Hash = Convert.ToBase64String(MD5.Create().ComputeHash(File.OpenRead(file)));
        }
        public void Rename(string newName) { Name = newName ?? Name; }
        public void ChangeThumbPath(string newPath) { Thumbnail = newPath ?? ""; }

        public override string ToString()
        {
            return Id.ToString();
        }
    }
}

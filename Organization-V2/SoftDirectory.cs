using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Organization_V2
{
    public class SoftDirectory : IID, ITaggable, IDirectory
    {
        public SoftDirectory(int id, string name)
        {
            Name = name;
            _subDirs = new List<SoftDirectory>();
            _softFiles = new List<SoftFile>();
            _tags = new List<string>();
            Id = id;
        }

        public SoftDirectory(int id, string name, SoftDirectory[] subDirs, SoftFile[] files, string[] tags)
        {
            Name = name;
            _subDirs = new List<SoftDirectory>(subDirs);
            _softFiles = new List<SoftFile>(files);
            _tags = new List<string>(tags);
            Id = id;
        }

        public string Name { get; }

        private List<SoftDirectory> _subDirs;

        private List<SoftFile> _softFiles;

        private List<string> _tags;

        public IReadOnlyList<string> Tags => _tags;

        public string[] TagArray => _tags.ToArray();

        public int Id { get; }

        public IReadOnlyList<SoftDirectory> SubDirectories => _subDirs;

        public IReadOnlyList<SoftFile> SoftFiles => _softFiles;

        public SoftDirectory Parent { get; private set; }
        
        public SoftDirectory AddDirectory(SoftDirectory i)
        {
            i.Parent = this;
            _subDirs.Add(i);
            return i;
        }

        public SoftFile AddFile(SoftFile i, bool check = true)
        {
            if (!check || (check && !Exists(i.Id)))
                _softFiles.Add(i);
            else
                throw new InvalidOperationException("File already exists");
            return i;
        }

        public bool Exists(int id)
        {
            return _softFiles.Exists(n => n.Id == id);
        }

        public bool Exists(IHashable i)
        {
            return _softFiles.Exists(n => n.Compare(i));
        }

        public bool Exists(SoftFile i)
        {
            return _softFiles.Exists(n => n.Id == i.Id);
        }

        public void AddTag(string tag)
        {
            if (!TagExists(tag = tag.ToLower())) _tags.Add(tag);
        }

        public bool TagExists(string lowercaseTag)
        {
            return _tags.Any(lowercaseTag.Equals);
        }

        public void AddTags(params string[] tags)
        {
            foreach (var a in tags)
                AddTag(a);
        }

        public void RemoveFile(IID i)
        {
            throw new NotImplementedException();
        }

        public void RemoveSubDirectory(IID i)
        {
            throw new NotImplementedException();
        }

        public void RemoveTag(string tag)
        {
            throw new NotImplementedException();
        }

        public void RemoveTags(params string[] tags)
        {
            throw new NotImplementedException();
        }

        public void RemoveTags()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            if (Parent == null) return $"/{Name}"; else return Parent.ToString() + $"/{Name}";
        }

        public string URI()
        {
            if (Parent == null) return $"/{Id}"; else return Parent.URI() + $"/{Id}";
        }

        public void RemoveDirectory(SoftDirectory i)
        {
            throw new NotImplementedException();
        }

        public void RemoveDirectory(IID i)
        {
            throw new NotImplementedException();
        }

        public void AddFiles(IEnumerable<SoftDirectory> i)
        {
            _subDirs.AddRange(i);
        }        

        public SoftDirectory FindDir(string path)
        {
            if (path.StartsWith('/')) return null;
            return RecursiveSearch(path.Trim('/').ToLower().Split('/'), this);            
        }
        
        public SoftDirectory FindDir(int[] idpath, int pos = 0)
        {
            var cur = _subDirs.FirstOrDefault(n => n.Id == idpath[pos]);
            if (cur == null || pos > idpath.Length - 2) return cur;
            return cur.FindDir(idpath, pos + 1);
        }

        public SoftDirectory RecursiveSearch(string[] paths, SoftDirectory last, int cur = 0)
        {
            if (cur > paths.Length - 1)
                return last;
            var a = GetChild(paths[cur]);
            if (a == null) return null;
            return RecursiveSearch(paths, a, cur + 1);
        }

        public SoftDirectory GetChild(string name, bool casesens = false)
        {
            if (!casesens) name = name.ToLower();
            if (name == "..") return Parent;
            if (name == ".") return this;
            if (!casesens)
                return _subDirs.FirstOrDefault(n => n.Name.ToLower() == name);
            else
                return _subDirs.FirstOrDefault(n => n.Name == name);
        }
    }
}

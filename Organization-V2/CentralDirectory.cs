using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Organization_V2
{
    public class CentralDirectory : CentralIndex, ILinks<SoftFile>, ICentralDirectory
    {
        public CentralDirectory()
        {
            _directories = new List<SoftDirectory>();
        }

        public CentralDirectory(CentralIndex i, IEnumerable<SoftDirectory> directories) : base(i.CurId, i.Files.ToArray())
        {
            _directories = new List<SoftDirectory>(directories);
        }

        private List<SoftDirectory> _directories;

        public new IReadOnlyList<IID> Children => throw new NotImplementedException();

        public IReadOnlyList<SoftDirectory> Directories { get => _directories; }

        public SoftDirectory Self => GetSelf();

        private SoftDirectory GetSelf()
        {
            var a = new SoftDirectory(0, "Central Directory");
            a.AddFiles(_directories);
            return a;
        }

        public SoftDirectory AddDirectory(SoftDirectory i)
        {
            _directories.Add(i);
            return i;
        }

        public SoftDirectory CreateDirectory(string name)
        {
            return new SoftDirectory(NextId, name);
        }        

        public IReadOnlyList<IID> Find(IID i)
        {
            throw new NotImplementedException();
        }        

        public SoftFile GetFile(int id)
        {
            return base.FirstOrDefault(id);
        }

        public IThumbable FindThumbnail(string path)
        {
            string[] vs = path.Trim('/').Split('/');
            int[] s = new int[vs.Length];
            for (int i = 0; i < vs.Length; i++)
                if (int.TryParse(vs[i], out int t)) { s[i] = t; } else { return null; }
            var a = Files.FirstOrDefault(n => n.Id == s[0]);
            return a != null ? a : FindThumbnail(s);
        }

        public IThumbable FindThumbnail(int id)
        {
            var a = Files.FirstOrDefault(n => n.Id == id);
            return a != null ? a : FindThumbRec(id, Self);
        }

        public IThumbable FindThumbRec(int id, SoftDirectory i)
        {
            if (i.Id == id) return i;
            foreach (var a in i.SoftFiles)
                if (a.Id == id) return a;
            foreach (var a in i.SubDirectories)
            {
                var asd = FindThumbRec(id, a);
                if (asd != null) return asd;
            }
            return null;
        }

        public IThumbable FindThumbnail(int[] idpath, int pos = 0)
        {

            var cur = _directories.FirstOrDefault(n => n.Id == idpath[pos]);

            if (cur == null || pos > idpath.Length - 2) return cur;
            return cur.FindThumb(idpath, pos + 1);
        }
        
        public SoftDirectory FindDir(string path)
        {
            string[] vs = path.Trim('/').Split('/');
            int[] s = new int[vs.Length];
            for (int i = 0; i < vs.Length; i++) 
                if (int.TryParse(vs[i], out int t)) { s[i] = t; } else { return null; }
            return FindDir(s);
        }

        public SoftFile FindFile(string path)
        {
            string[] vs = path.Trim('/').Split('/');
            int[] s = new int[vs.Length];
            for (int i = 0; i < vs.Length; i++)
                if (int.TryParse(vs[i], out int t)) { s[i] = t; } else { return null; }
            return FindFile(s);
        }

        public SoftDirectory FindDir(int[] idpath, int pos = 0)
        {
            var cur = _directories.FirstOrDefault(n => n.Id == idpath[pos]);
            if (cur == null || pos > idpath.Length - 2) return cur;
            return cur.FindDir(idpath, pos + 1);
        }

        public SoftFile FindFile(int[] idpath, int pos = 0, SoftDirectory cur = null)
        {
            var las = cur;
            cur = _directories.FirstOrDefault(n => n.Id == idpath[pos]);
            if (cur == null || pos > idpath.Length - 1)
            {
                if (las != null)
                    return las.SoftFiles.FirstOrDefault(n => n.Id == idpath[pos]);
                else
                    return null;
            }
            return FindFile(idpath, pos + 1, cur);
        }

        public new IID FirstOrDefault(IID i)
        {
            throw new NotImplementedException();
        }

        public SoftFile FirstOrDefault(int id)
        {
            return Files.FirstOrDefault(n => n.Id == id);
        }

        public void RemoveDirectory(SoftDirectory i)
        {
            _directories.RemoveAll(n => n.Id == i.Id);
        }

        public void RemoveDirectory(IID i)
        {
            _directories.RemoveAll(n => n.Id == i.Id);
        }
    }
}

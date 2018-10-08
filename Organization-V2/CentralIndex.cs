using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Nessos.LinqOptimizer.CSharp;

namespace Organization_V2
{
    public class CentralIndex : ILinks<SoftFile>, IIndex
    {
        public CentralIndex(int nextid, params SoftFile[] softFiles)
        {
            id = nextid + 1;
            _softFiles = new List<SoftFile>(softFiles);
        }

        public CentralIndex()
        {
            id = 1;
            //NextId = 1;
            _softFiles = new List<SoftFile>();
        }

        private List<SoftFile> _softFiles;

        private int id;

        public int NextId { get => id++; }

        public int CurId { get => id; }

        public SoftFile this[int i] { get => _softFiles[i]; }
        
        public SoftFile this[string i] { get => _softFiles.First(n => n.Name == i); }

        public SoftFile this[IID i] { get => _softFiles.First(n => n.Id == i.Id); }
        
        public IReadOnlyList<SoftFile> Children => _softFiles;

        public IReadOnlyList<SoftFile> Files => _softFiles;

        IReadOnlyList<SoftFile> ILinks<SoftFile>.Children => throw new NotImplementedException();

        public IID FirstOrDefault(IID i)
        {
            throw new NotImplementedException();
        }
        
        IReadOnlyList<SoftFile> ILinks<SoftFile>.Find(IID i)
        {
            throw new NotImplementedException();
        }

        public SoftFile AddFile(SoftFile i)
        {
            _softFiles.Add(i);
            return i;
        }

        public SoftFile AddFile(string name)
        {
            var a = new SoftFile(NextId, name);
            _softFiles.Add(a);
            return a;
        }

        public SoftFile CreateFile(string name)
        {            
            return AddFile(new SoftFile(NextId, name));            
        }

        public void AddFiles(params SoftFile[] i)
        {
            _softFiles.AddRange(i);
        }

        public void RemoveFile(IID i)
        {
            _softFiles.RemoveAll(n => n.Id == i.Id);
        }

        public void RemoveFiles(IID i)
        {
            RemoveFile(i);
        }

        public void RemoveFiles()
        {
            _softFiles = new List<SoftFile>();
        }

        SoftFile ILinks<SoftFile>.FirstOrDefault(int i)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<SoftFile> Find(IID i)
        {
            throw new NotImplementedException();
        }

        public SoftFile FirstOrDefault(int i)
        {
            return _softFiles.FirstOrDefault(n => n.Id == i);
        }

        public bool Exists(int i)
        {
            return _softFiles.Exists(n => n.Id == i);
        }

        public bool Exists(IHashable i)
        {
            return _softFiles.Any(n => n.Compare(i));
        }

        public SoftFile Find(IHashable i)
        {
            return _softFiles.FirstOrDefault(n => n.Compare(i));
        }

        public SoftFile OpenCLFirstOrDefault(int i, bool wr = false)
        {
            var query = (from num in _softFiles.AsQueryExpr()
                         where num.Id == i
                         select num);
            var res = query.Run();
            if (wr) {
                Console.WriteLine($"{res.Count()} Results");
            foreach (var a in res)
                    Console.WriteLine($"[{a.Id}] {a.Name}"); }
            return res.FirstOrDefault();
        }
    }
}

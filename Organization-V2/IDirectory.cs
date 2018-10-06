using System;
using System.Collections.Generic;
using System.Text;

namespace Organization_V2
{
    public interface IDirectory : IHierarchy
    {
        string Name { get; }        
        SoftFile AddFile(SoftFile i, bool check = true);
        bool Exists(SoftFile i);
        bool Exists(int id);
        bool Exists(IHashable i);
        void AddFiles(IEnumerable<SoftDirectory> i);
        //SoftDirectory AddDirectory(SoftDirectory i);
        IReadOnlyList<SoftDirectory> SubDirectories { get; }
        IReadOnlyList<SoftFile> SoftFiles { get; }
        SoftDirectory Parent { get; }
        void RemoveFile(IID i);
        //void RemoveSubDirectory(IID i);
    }
}

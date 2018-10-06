using System;
using System.Collections.Generic;
using System.Text;

namespace Organization_V2
{
    public interface IIndex
    {
        SoftFile AddFile(SoftFile i);
        SoftFile AddFile(string name);
        void AddFiles(params SoftFile[] i);        
        void RemoveFile(IID i);
        void RemoveFiles(IID i);
        void RemoveFiles();       
        IReadOnlyList<SoftFile> Files { get; }
    }
}

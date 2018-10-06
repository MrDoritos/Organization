using System;
using System.Collections.Generic;
using System.Text;

namespace Organization_V2
{
    public interface ICentralDirectory : IHierarchy
    {
        SoftDirectory Self { get; }
        IReadOnlyList<SoftDirectory> Directories { get; }
    }
}

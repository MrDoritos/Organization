using System;
using System.Collections.Generic;
using System.Text;

namespace Organization_V2
{
    public interface IHierarchy
    {
        SoftDirectory AddDirectory(SoftDirectory i);
        void RemoveDirectory(SoftDirectory i);
        void RemoveDirectory(IID i);
    }
}

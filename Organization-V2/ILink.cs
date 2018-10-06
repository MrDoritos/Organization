using System;
using System.Collections.Generic;
using System.Text;

namespace Organization_V2
{
    public interface ILinks<T>
    {
        IReadOnlyList<T> Children { get; }
        IReadOnlyList<T> Find(IID i);
        T FirstOrDefault(int i);
    }

    public interface IID
    {
        int Id { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Organization_V2
{
    public interface IFile
    {
        string Name { get; }
        string ThumbnailPath { get; }
        bool ThumbnailExists { get; }
    }
}

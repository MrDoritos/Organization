using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Organization_V2
{
    public interface IThumbable
    {
        bool ThumbnailExists { get; }
        string ThumbnailPath { get; set; }
        FileStream Thumbnail { get; }
        string Name { get; set; }
    }
}

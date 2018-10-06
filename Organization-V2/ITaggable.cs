using System;
using System.Collections.Generic;
using System.Text;

namespace Organization_V2
{
    public interface ITaggable
    {
        IReadOnlyList<string> Tags { get; }
        string[] TagArray { get; }
        void AddTag(string tag);
        void RemoveTag(string tag);
        void AddTags(params string[] tags);
        void RemoveTags(params string[] tags);
        void RemoveTags();
    }
}

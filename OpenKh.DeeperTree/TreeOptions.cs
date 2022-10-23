using System;
using System.Collections.Generic;
using System.Text;

namespace OpenKh.DeeperTree
{
    public class TreeOptions
    {
        public IDictionary<string, Type> ObjectTypes { get; set; } = new SortedDictionary<string, Type>();

        public TreeOptions AddType(string name, Type type)
        {
            ObjectTypes[name] = type;
            return this;
        }
    }
}

using System;
using System.Collections.Generic;

namespace OpenKh.DeeperTree
{
    public class TreeReaderBuilder
    {
        public IDictionary<string, Type> ObjectTypes { get; set; } = new SortedDictionary<string, Type>();

        public TreeReaderBuilder AddType(string name, Type type)
        {
            ObjectTypes[name] = type;
            return this;
        }

        public TreeReader Build()
        {
            return new TreeReader(this);
        }
    }
}

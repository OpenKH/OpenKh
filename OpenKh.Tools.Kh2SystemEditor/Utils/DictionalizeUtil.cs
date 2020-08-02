using OpenKh.Tools.Kh2SystemEditor.Attributes;
using OpenKh.Tools.Kh2SystemEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OpenKh.Tools.Kh2SystemEditor.Utils
{
    static class DictionalizeUtil
    {
        internal static IList<IDictionary<string, object>> ToDictList<T>(IEnumerable<T> inList)
        {
            var outList = new List<IDictionary<string, object>>();

            var type = typeof(T);
            var props = type.GetProperties()
                .Where(prop => prop.GetCustomAttribute<ExportTargetAttribute>() != null)
                .ToArray();

            foreach (var item in inList)
            {
                var dict = new Dictionary<string, object>();

                foreach (var prop in props)
                {
                    dict[prop.Name] = prop.GetValue(item);
                }

                outList.Add(dict);
            }

            return outList;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2MdlxEditor.Utils
{
    internal class EnumPairingUtils
    {
        public static KeyValuePair<KeyType, string>[] GetEnumDict<KeyType, EnumType>() where KeyType : struct
        {
            var type = typeof(EnumType);
            return type.GetMembers(BindingFlags.Public | BindingFlags.Static)
                .Cast<FieldInfo>()
                .Select(fieldInfo => new KeyValuePair<KeyType, string>(
                    (KeyType)Convert.ChangeType(fieldInfo.GetValue(null), typeof(KeyType))!,
                    fieldInfo.Name
                ))
                .ToArray();
        }
    }
}

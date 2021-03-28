using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace OpenKh.Command.TexFooter.TypeConverters
{
    class UseJsonStyleArray<T> : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type.HasElementType && type.GetElementType() == typeof(T);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            throw new NotImplementedException();
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            if (value == null)
            {
                emitter.Emit(new Scalar(null, null, "null", ScalarStyle.Plain, true, false));
            }
            else
            {
                emitter.Emit(new SequenceStart(null, null, true, SequenceStyle.Flow));
                foreach (var it in (IEnumerable)value)
                {
                    emitter.Emit(new Scalar((it == null) ? null : (it + "")));
                }
                emitter.Emit(new SequenceEnd());
            }
        }
    }
}

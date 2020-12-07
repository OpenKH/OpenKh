using OpenKh.Kh2;
using OpenKh.Research.Kh2Anim.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using static OpenKh.Kh2.Motion;

namespace OpenKh.Research.Kh2Anim.TypeConverters
{
    class TimelineTableConverter : TypeConverter
    {
        public TimelineTableConverter()
        {

        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var tokens = (value + "").Split(',');
            return new TimelineTable
            {
                Interpolation = (Interpolation)Enum.Parse(typeof(Interpolation), tokens[0], true),
                KeyFrame = Convert.ToSingle(tokens[1]),
                Value = Convert.ToSingle(tokens[2]),
                TangentEaseIn = Convert.ToSingle(tokens[3]),
                TangentEaseOut = Convert.ToSingle(tokens[4]),
            };
        }
    }
}

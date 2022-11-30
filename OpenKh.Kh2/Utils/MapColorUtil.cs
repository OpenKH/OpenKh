using OpenKh.Kh2.Models.MapColorModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xe.BinaryMapper;
using Xe.Graphics;

namespace OpenKh.Kh2.Utils
{
    public class MapColorUtil
    {
        private static readonly Lazy<IBinaryMapping> lazyMapper = new Lazy<IBinaryMapping>(
            () =>
            {
                var config = MappingConfiguration.DefaultConfiguration()
                    .ForType<Color>(
                        x =>
                        {
                            return new Color(
                                r: x.Reader.ReadByte(),
                                g: x.Reader.ReadByte(),
                                b: x.Reader.ReadByte(),
                                a: x.Reader.ReadByte()
                            );
                        },
                        x =>
                        {
                            var it = (Color)x.Item;
                            x.Writer.Write(it.r);
                            x.Writer.Write(it.g);
                            x.Writer.Write(it.b);
                            x.Writer.Write(it.a);
                        }
                    );
                return config.Build();
            }
        );

        public MapColor Read(Stream stream)
        {
            return lazyMapper.Value.ReadObject<MapColor>(stream);
        }

        public void Write(Stream stream, MapColor it)
        {
            lazyMapper.Value.WriteObject(stream, it);
        }
    }
}

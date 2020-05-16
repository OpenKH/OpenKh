using System;
using System.Collections.Generic;
using System.Text;

namespace OpenKh.Kh2.SrkAlternatives
{
    public class BitConverter : IDisposable
    {
        public void Dispose()
        {
            Array.Clear(this.Data, 0, this.Data.Length);
            this.Data = null;
        }

        public BitConverter()
        {

        }
        public BitConverter(ref byte[] data)
        {
            this.Data = data;
        }

        public byte[] Data;
        public Int32 Int32(int position)
        {
            return global::System.BitConverter.ToInt32(this.Data, position);
        }

        public Int16 Int16(int position)
        {
            return global::System.BitConverter.ToInt16(this.Data, position);
        }

        public Single Single(int position)
        {
            return global::System.BitConverter.ToSingle(this.Data, position);
        }
    }
}

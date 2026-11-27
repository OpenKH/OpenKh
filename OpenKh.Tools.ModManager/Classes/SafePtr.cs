using System;
using System.Text;
using System.Runtime.InteropServices;

namespace OpenKh.Tools.ModManager.Classes
{
    public class SafePtr : IDisposable
    {
        protected IntPtr _internalPtr;
        protected Type _internalType;

        public SafePtr(object value)
        {
            _internalType = value.GetType();
            _internalPtr = Marshal.AllocHGlobal(value is string ? 255 : Marshal.SizeOf(_internalType));

            if (value is byte)
                Marshal.WriteByte(_internalPtr, (byte)value);

            else if (value is short)
                Marshal.WriteInt16(_internalPtr, (short)value);

            else if (value is int)
                Marshal.WriteInt32(_internalPtr, (int)value);

            else if (value is long)
                Marshal.WriteInt64(_internalPtr, (long)value);

            else if (value is double)
                Marshal.WriteInt64(_internalPtr, BitConverter.DoubleToInt64Bits((double)value));

            else if (value is string)
            {
                var _fetchString = (value as string) + "\x00";
                var _fetchEncoding = Encoding.Default.GetBytes(_fetchString);
                Marshal.Copy(_fetchEncoding, 0, _internalPtr, _fetchEncoding.Length);
            }

            else
                throw new InvalidCastException("SafePtr does not support this type!");
        }

        public static implicit operator SafePtr(byte value) => new SafePtr(value);
        public static implicit operator SafePtr(short value) => new SafePtr(value);
        public static implicit operator SafePtr(int value) => new SafePtr(value);
        public static implicit operator SafePtr(long value) => new SafePtr(value);
        public static implicit operator SafePtr(string value) => new SafePtr(value);
        public static implicit operator SafePtr(double value) => new SafePtr(value);

        public void operator /= (byte value) => Marshal.WriteByte(_internalPtr, value);
        public void operator /= (short value) => Marshal.WriteInt16(_internalPtr, value);
        public void operator /= (int value) => Marshal.WriteInt32(_internalPtr, value);
        public void operator /= (long value) => Marshal.WriteInt64(_internalPtr, value);
        public void operator /= (double value) => Marshal.WriteInt64(_internalPtr, BitConverter.DoubleToInt64Bits(value));
        public void operator /= (string value)
        {
            var _fetchString = value + "\x00";
            var _fetchEncoding = Encoding.Default.GetBytes(_fetchString);
            Marshal.Copy(_fetchEncoding, 0, _internalPtr, _fetchEncoding.Length);
        }

        public static implicit operator byte?(SafePtr value) => value == null || value._internalPtr == IntPtr.Zero ? null : Marshal.ReadByte(value._internalPtr);
        public static implicit operator short?(SafePtr value) => value == null || value._internalPtr == IntPtr.Zero ? null : Marshal.ReadInt16(value._internalPtr);
        public static implicit operator int?(SafePtr value) => value == null || value._internalPtr == IntPtr.Zero ? null : Marshal.ReadInt32(value._internalPtr);
        public static implicit operator long?(SafePtr value) => value == null || value._internalPtr == IntPtr.Zero ? null : Marshal.ReadInt64(value._internalPtr);
        public static implicit operator double?(SafePtr value) => value == null || value._internalPtr == IntPtr.Zero ? null : BitConverter.Int64BitsToDouble(Marshal.ReadInt64(value._internalPtr));
        public static implicit operator string?(SafePtr value)
        {
            var _fetchValue = new byte[0xFF];

            if (value == null || value._internalPtr == IntPtr.Zero)
                return null;

            Marshal.Copy(value._internalPtr, _fetchValue, 0, 0xFF);
            return Encoding.Default.GetString(_fetchValue, 0x00, _fetchValue.IndexOf<byte>(0x00));
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(_internalPtr);
            _internalPtr = IntPtr.Zero;
        }
    }
}

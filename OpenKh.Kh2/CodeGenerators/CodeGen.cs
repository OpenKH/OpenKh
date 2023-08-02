
// This is an auto generated source code with Text Template Transformation Toolkit.

using OpenKh.Common.Utils;

namespace OpenKh.Kh2
{
    public partial class Motion
    {
        public partial class Key
        {
            public Interpolation Type
            {
                get => (Interpolation)BitsUtil.Int.GetBits(Type_Time, 0, 2);
                set => Type_Time = (short)BitsUtil.Int.SetBits((int)Type_Time, 0, 2, (int)value);
            }
            
            public short Time
            {
                get => (short)BitsUtil.Int.GetBits(Type_Time, 2, 14);
                set => Type_Time = (short)BitsUtil.Int.SetBits((int)Type_Time, 2, 14, (int)value);
            }
            
        }
        
        public partial class InitialPose
        {
            public Channel ChannelValue
            {
                get => (Channel)BitsUtil.Int.GetBits(Channel, 0, 4);
                set => Channel = (short)BitsUtil.Int.SetBits((int)Channel, 0, 4, (int)value);
            }
            
        }
        
        public partial class FCurve
        {
            public Channel ChannelValue
            {
                get => (Channel)BitsUtil.Int.GetBits(Channel, 0, 4);
                set => Channel = (byte)BitsUtil.Int.SetBits((int)Channel, 0, 4, (int)value);
            }
            
            public CycleType Pre
            {
                get => (CycleType)BitsUtil.Int.GetBits(Channel, 4, 2);
                set => Channel = (byte)BitsUtil.Int.SetBits((int)Channel, 4, 2, (int)value);
            }
            
            public CycleType Post
            {
                get => (CycleType)BitsUtil.Int.GetBits(Channel, 6, 2);
                set => Channel = (byte)BitsUtil.Int.SetBits((int)Channel, 6, 2, (int)value);
            }
            
        }
        
        public partial class ExpressionNode
        {
            public int Element
            {
                get => (int)BitsUtil.Int.GetBits(Data, 16, 16);
                set => Data = (int)BitsUtil.Int.SetBits((int)Data, 16, 16, (int)value);
            }
            
            public byte Type
            {
                get => (byte)BitsUtil.Int.GetBits(Data, 0, 8);
                set => Data = (int)BitsUtil.Int.SetBits((int)Data, 0, 8, (int)value);
            }
            
            public bool IsGlobal
            {
                get => BitsUtil.Int.GetBit(Data, 8);
                set => Data = (int)BitsUtil.Int.SetBit((int)Data, 8, value);
            }
            
        }
        
        public partial class IKHelper
        {
            public int Unknown
            {
                get => (int)BitsUtil.Int.GetBits(Flags, 22, 10);
                set => Flags = (int)BitsUtil.Int.SetBits((int)Flags, 22, 10, (int)value);
            }
            
            public bool Terminate
            {
                get => BitsUtil.Int.GetBit(Flags, 0);
                set => Flags = (int)BitsUtil.Int.SetBit((int)Flags, 0, value);
            }
            
            public bool Below
            {
                get => BitsUtil.Int.GetBit(Flags, 1);
                set => Flags = (int)BitsUtil.Int.SetBit((int)Flags, 1, value);
            }
            
            public bool EnableBias
            {
                get => BitsUtil.Int.GetBit(Flags, 2);
                set => Flags = (int)BitsUtil.Int.SetBit((int)Flags, 2, value);
            }
            
        }
        
        public partial class Joint
        {
            public byte IK
            {
                get => (byte)BitsUtil.Int.GetBits(Flags, 6, 2);
                set => Flags = (byte)BitsUtil.Int.SetBits((int)Flags, 6, 2, (int)value);
            }
            
            public bool ExtEffector
            {
                get => BitsUtil.Int.GetBit(Flags, 0);
                set => Flags = (byte)BitsUtil.Int.SetBit((int)Flags, 0, value);
            }
            
            public bool CalcMatrix2Rot
            {
                get => BitsUtil.Int.GetBit(Flags, 1);
                set => Flags = (byte)BitsUtil.Int.SetBit((int)Flags, 1, value);
            }
            
            public bool Calculated
            {
                get => BitsUtil.Int.GetBit(Flags, 2);
                set => Flags = (byte)BitsUtil.Int.SetBit((int)Flags, 2, value);
            }
            
            public bool Fixed
            {
                get => BitsUtil.Int.GetBit(Flags, 3);
                set => Flags = (byte)BitsUtil.Int.SetBit((int)Flags, 3, value);
            }
            
            public bool Rotation
            {
                get => BitsUtil.Int.GetBit(Flags, 4);
                set => Flags = (byte)BitsUtil.Int.SetBit((int)Flags, 4, value);
            }
            
            public bool Trans
            {
                get => BitsUtil.Int.GetBit(Flags, 5);
                set => Flags = (byte)BitsUtil.Int.SetBit((int)Flags, 5, value);
            }
            
        }
        
        public partial class Limiter
        {
            public LimiterType Type
            {
                get => (LimiterType)BitsUtil.Int.GetBits(Flags, 29, 3);
                set => Flags = (int)BitsUtil.Int.SetBits((int)Flags, 29, 3, (int)value);
            }
            
            public bool HasXMin
            {
                get => BitsUtil.Int.GetBit(Flags, 22);
                set => Flags = (int)BitsUtil.Int.SetBit((int)Flags, 22, value);
            }
            
            public bool HasXMax
            {
                get => BitsUtil.Int.GetBit(Flags, 23);
                set => Flags = (int)BitsUtil.Int.SetBit((int)Flags, 23, value);
            }
            
            public bool HasYMin
            {
                get => BitsUtil.Int.GetBit(Flags, 24);
                set => Flags = (int)BitsUtil.Int.SetBit((int)Flags, 24, value);
            }
            
            public bool HasYMax
            {
                get => BitsUtil.Int.GetBit(Flags, 25);
                set => Flags = (int)BitsUtil.Int.SetBit((int)Flags, 25, value);
            }
            
            public bool HasZMin
            {
                get => BitsUtil.Int.GetBit(Flags, 26);
                set => Flags = (int)BitsUtil.Int.SetBit((int)Flags, 26, value);
            }
            
            public bool HasZMax
            {
                get => BitsUtil.Int.GetBit(Flags, 27);
                set => Flags = (int)BitsUtil.Int.SetBit((int)Flags, 27, value);
            }
            
            public bool Global
            {
                get => BitsUtil.Int.GetBit(Flags, 28);
                set => Flags = (int)BitsUtil.Int.SetBit((int)Flags, 28, value);
            }
            
        }
        
    }
    
}


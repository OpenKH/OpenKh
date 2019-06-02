namespace OpenKh.Kh2.Messages
{
    public class MessageCommandModel
    {
        public MessageCommand Command { get; set; }
        public byte[] Data { get; set; }
        public string Text { get; set; }

        public short PositionX
        {
            get => (short)(Data[0] | (Data[1] << 8));
            set
            {
                var rawValue = (ushort)value;
                Data[0] = (byte)(rawValue & 0xFF);
                Data[1] = (byte)((rawValue >> 8) & 0xFF);
            }
        }

        public short PositionY
        {
            get => (short)(Data[2] | (Data[3] << 8));
            set
            {
                var rawValue = (ushort)value;
                Data[2] = (byte)(rawValue & 0xFF);
                Data[3] = (byte)((rawValue >> 8) & 0xFF);
            }
        }
    }
}

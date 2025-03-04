using System.Text;
using Xe.BinaryMapper;
using YamlDotNet.Serialization;

namespace OpenKh.Command.Bdxio.Models
{
    public class BdxHeader
    {
        [Data(Count = 16), YamlIgnore()] public byte[] RawName { get; set; } = new byte[0];
        [Data] public int WorkSize { get; set; }
        [Data] public int StackSize { get; set; }
        [Data] public int TempSize { get; set; }
        public Trigger[] Triggers { get; set; } = new Trigger[0];

        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)] public bool IsEmpty { get; set; }

        public string Name
        {
            get => Encoding.ASCII.GetString(RawName).Split('\0')[0];
            set => RawName = Encoding.ASCII.GetBytes((value ?? "").PadRight(16, '\0'), 0, 16);
        }

        public class IntTrigger
        {
            [Data] public int Key { get; set; }
            [Data] public int Addr { get; set; }
        }

        public class Trigger
        {
            public int Key { get; set; }
            public string Addr { get; set; }
        }
    }
}

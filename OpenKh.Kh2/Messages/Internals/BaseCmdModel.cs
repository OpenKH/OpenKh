namespace OpenKh.Kh2.Messages.Internals
{
    internal class BaseCmdModel
    {
        public MessageCommand Command { get; set; }

        public int Length { get; set; }

        public string Text { get; set; }

        public byte RawData { get; set; }
    }
}

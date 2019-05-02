namespace kh.kh2.Messages.Internals
{
    internal class UnsupportedCmdModel : BaseCmdModel
    {
        public UnsupportedCmdModel(byte data)
        {
            Command = MessageCommand.Unsupported;
            RawData = data;
        }
    }
}

namespace kh.kh2.Messages.Internals
{
    internal class UnsupportedCmdModel : SingleDataCmdModel
    {
        public UnsupportedCmdModel() :
            base(MessageCommand.Unsupported)
        { }
    }
}

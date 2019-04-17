namespace kh.kh2.Messages.Internals
{
    internal class UnsupportedCmdModel : SingleDataCmdModel
    {
        public UnsupportedCmdModel(byte chData) :
            base(MessageCommand.Unsupported, chData)
        {

        }
    }
}

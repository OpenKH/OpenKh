namespace OpenKh.Kh2.Messages.Internals
{
    internal class SingleDataCmdModel : DataCmdModel
    {
        public SingleDataCmdModel(MessageCommand command) :
            base(command, 1)
        { }
    }
}

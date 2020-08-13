namespace OpenKh.Kh2.Messages.Internals
{
    internal class DataCmdModel : BaseCmdModel
    {
        public DataCmdModel(MessageCommand command, int lenght)
        {
            Command = command;
            Length = lenght;
        }
    }
}

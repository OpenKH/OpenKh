namespace kh.kh2.Messages.Internals
{
    internal class SingleDataCmdModel : MessageCommandModel
    {
        public SingleDataCmdModel(MessageCommand command, BaseMessageDecoder msgParser) :
            this(command, msgParser.Next())
        { }

        public SingleDataCmdModel(MessageCommand command, byte data)
        {
            Command = command;
            Data = new byte[] { data };
        }
    }
}

using System.Linq;

namespace kh.kh2.Messages.Internals
{
    internal class MultipleDataCmdModel : MessageCommandModel
    {
        public MultipleDataCmdModel(MessageCommand command, BaseMessageDecoder msgParser, int count)
        {
            Command = command;
            Data = Enumerable.Range(0, count)
                .Select(x => msgParser.Next())
                .ToArray();
        }
    }
}

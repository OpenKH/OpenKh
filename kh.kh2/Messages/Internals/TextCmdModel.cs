namespace kh.kh2.Messages.Internals
{
    internal class TextCmdModel : BaseCmdModel
    {
        public TextCmdModel(byte chData) :
            this((char)chData)
        { }

        public TextCmdModel(char ch) :
            this($"{ch}")
        { }

        public TextCmdModel(string str)
        {
            Command = MessageCommand.PrintText;
            Text = $"{str}";
        }
    }
}

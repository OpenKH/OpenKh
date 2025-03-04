namespace OpenKh.Command.Bdxio.Utils
{
    public class BdxAsciiModel
    {
        public bool IsEmpty { get; set; }

        public string? Header { get; set; }
        public string? Script { get; set; }

        public int FirstHeaderLineNumber { get; set; }
        public int FirstScriptLineNumber { get; set; }

        public string GetLineNumberRetainedScriptBody() => new string('\n', FirstScriptLineNumber) + Script;

        public static BdxAsciiModel ParseText(string body)
        {
            var lines = body.Replace("\r\n", "\n").Split('\n');
            int separator1 = Array.IndexOf(lines, "---");
            if (separator1 < 0)
            {
                throw new InvalidDataException();
            }
            int separator2 = Array.IndexOf(lines, "---", separator1 + 1);
            if (separator2 < 0)
            {
                throw new InvalidDataException();
            }

            var header = string.Join("\n", lines, separator1 + 1, separator2 - separator1 - 1);
            var script = string.Join("\n", lines.Skip(separator2 + 1));

            return new BdxAsciiModel
            {
                FirstHeaderLineNumber = 1 + separator1,
                FirstScriptLineNumber = 1 + separator2,
                IsEmpty = header.Trim().Length == 0 && script.Trim().Length == 0,
                Header = header,
                Script = script,
            };
        }
    }
}

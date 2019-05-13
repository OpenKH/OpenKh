using System;

namespace OpenKh.Common.Exceptions
{
    public class ParseException : Exception
    {
        public ParseException(string value, int index, string reason) :
            base(GetExceptionMessage(value, index, reason))
        { }

        private static string GetExceptionMessage(string value, int index, string reason) =>
            $"Parse error on {value} at {index} due to the following reason:\n{reason}";
    }
}

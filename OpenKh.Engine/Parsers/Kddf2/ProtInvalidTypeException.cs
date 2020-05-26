using System;

namespace OpenKh.Engine.Parsers.Kddf2
{
    public class ProtInvalidTypeException : ApplicationException
    {
        public ProtInvalidTypeException() : base("Has to be typ1 or typ2") { }
    }
}

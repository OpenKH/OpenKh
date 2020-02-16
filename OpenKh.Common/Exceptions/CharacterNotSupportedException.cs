using System;

namespace OpenKh.Common.Exceptions
{
    public class CharacterNotSupportedException : ArgumentException
    {
        public CharacterNotSupportedException(char ch) :
            base($"The character {ch} it is not supported by the specified encoding.")
        { }
    }
}

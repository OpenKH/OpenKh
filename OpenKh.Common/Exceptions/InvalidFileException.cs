using System;

namespace OpenKh.Common.Exceptions
{
    public class InvalidFileException<T> : Exception
    {
        public InvalidFileException() :
            base($"The specified file is not recognized as {typeof(T).Name}.")
        { }
    }

    public class InvalidFileException : Exception
    {
        public InvalidFileException() :
            base("The specified file is not recognized.")
        { }
    }
}

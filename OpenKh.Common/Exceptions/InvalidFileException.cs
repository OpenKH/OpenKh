using System;

namespace OpenKh.Common.Exceptions
{
    public class InvalidFileException<T> : InvalidFileException
    {
        public InvalidFileException() :
            base(typeof(T))
        { }
    }


    public class InvalidFileException : Exception
    {
        public InvalidFileException(Type type) :
            base($"The specified file is not recognized as {type.Name}.")
        { }

        public InvalidFileException() :
            base("The specified file is not recognized.")
        { }
    }
}

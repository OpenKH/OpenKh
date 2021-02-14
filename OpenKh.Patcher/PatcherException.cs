using System;

namespace OpenKh.Patcher
{
    public class PatcherException : AggregateException
    {
        public PatcherException(Metadata patch, Exception innerException) :
            base($"The mod '{patch.Title}' generated an error.", innerException)
        {
            Patch = patch;
        }

        public Metadata Patch { get; }
    }
}

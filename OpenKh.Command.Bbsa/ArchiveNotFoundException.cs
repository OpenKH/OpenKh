using System;

namespace OpenKh.Command.Bbsa
{
    public class ArchiveNotFoundException : ArgumentException
    {
        public ArchiveNotFoundException(string path, int archiveIndex) :
            base($"The path {path} does not contain BBS{archiveIndex}.DAT.")
        { }
    }
}

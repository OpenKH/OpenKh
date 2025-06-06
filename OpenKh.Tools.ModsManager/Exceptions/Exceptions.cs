using System;

namespace OpenKh.Tools.ModsManager.Exceptions
{
    public class ModAlreadyExistsExceptions : Exception
    {
        public string ModName { get; }

        public ModAlreadyExistsExceptions(string modName) :
            base($"The previous install of '{modName}' will not be overwritten. The installation will now stop.")
        {
            ModName = modName;
        }
    }

    public class ModNotValidException : Exception
    {
        public string ModName { get; }

        public ModNotValidException(string modName) :
            base($"The mod '{modName}' does not contain a valid OpenKH compatible mod due to the missing 'mod.yml' file.")
        {
            ModName = modName;
        }
    }

    public class RepositoryNotFoundException : Exception
    {
        public string RepositoryName { get; set; }

        public RepositoryNotFoundException(string repositoryName) :
            base($"Repository '{repositoryName}' not found.")
        {

        }
    }

    public class ModMovedWithoutGameException : Exception
    {
        public string ModName { get; }

        public ModMovedWithoutGameException(string modName) :
            base($"The mod '{modName}' has been changed from a collection but a base game was not specified and could not be moved, it will be removed. Please reinstall in the appropriate launch game.")
        {
            ModName = modName;
        }
    }
}

using System;

namespace OpenKh.Tools.ModsManager.Exceptions
{
    public class ModAlreadyExistsExceptions : Exception
    {
        public string ModName { get; }

        public ModAlreadyExistsExceptions(string modName) :
            base($"A mod with the name '{modName}' already exists. The installation will now stop.")
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
}

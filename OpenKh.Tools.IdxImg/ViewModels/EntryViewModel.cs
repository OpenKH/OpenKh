namespace OpenKh.Tools.IdxImg.ViewModels
{
    public abstract class EntryViewModel
    {
        public string Name { get; }

        internal EntryViewModel(string name)
        {
            Name = name;
        }

        public abstract void Extract(string outputPath);
    }
}

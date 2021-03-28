namespace OpenKh.Tools.Kh2SystemEditor.Models
{
    public class ObjectModel
    {
        public ObjectModel(int value, string name)
        {
            Name = name;
            Value = (short)value;
        }

        public string Name { get; }
        public short Value { get; }
    }
}

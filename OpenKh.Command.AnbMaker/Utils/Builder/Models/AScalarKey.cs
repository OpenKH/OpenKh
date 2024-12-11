namespace OpenKh.Command.AnbMaker.Utils.Builder.Models
{
    public class AScalarKey
    {
        public float Time { get; set; }
        public float Value { get; set; }

        public override string ToString() => $"{Time}, {Value}";
    }
}

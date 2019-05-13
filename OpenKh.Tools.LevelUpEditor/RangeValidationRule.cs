using System.Windows.Controls;

namespace OpenKh.Tools.LevelUpEditor
{
    public class RangeValidationRule : ValidationRule
    {
        public int MinValue { get; set; }
        public int MaxValue { get; set; }

        public override ValidationResult Validate(
          object value, System.Globalization.CultureInfo cultureInfo)
        {
            string text = $"Must be between {MinValue} and {MaxValue}";
            if (!int.TryParse(value.ToString(), out int intValue))
                return new ValidationResult(false, "Not an integer");
            if (intValue < MinValue)
                return new ValidationResult(false, $"Too small. {text}");
            if (intValue > MaxValue)
                return new ValidationResult(false, $"Too large. {text}");
            return ValidationResult.ValidResult;
        }
    }
}
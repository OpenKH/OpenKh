using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace OpenKh.Tools.Kh1TextEditor.Views
{
    public partial class LanguageSelectionWindow : Window
    {
        private sealed class LanguageOption
        {
            public string Code { get; init; }
            public string DisplayName { get; init; }
        }

        private static readonly IReadOnlyDictionary<string, string> LanguageNames =
            new Dictionary<string, string>
            {
                ["SP"] = "Español",
                ["UK"] = "English (Europe)",
                ["US"] = "English (North America)",
                ["FR"] = "Français",
                ["GR"] = "Deutsch",
                ["IT"] = "Italiano",
                ["JP"] = "日本語",
                ["FM"] = "Final Mix",
            };

        public LanguageSelectionWindow(IEnumerable<string> availableLanguages, string preferredLanguage)
        {
            InitializeComponent();
            var options = availableLanguages
                .Distinct()
                .OrderBy(x => x == "SP" ? 0 : 1)
                .ThenBy(x => x)
                .Select(x => new LanguageOption
                {
                    Code = x,
                    DisplayName = LanguageNames.TryGetValue(x, out var name) ? $"{x} — {name}" : x,
                })
                .ToList();
            options.Add(new LanguageOption { Code = null, DisplayName = "All languages (slower)" });
            LanguageComboBox.ItemsSource = options;
            LanguageComboBox.SelectedItem = options.FirstOrDefault(x => x.Code == preferredLanguage) ?? options[0];
        }

        public string SelectedLanguage { get; private set; }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            SelectedLanguage = (LanguageComboBox.SelectedItem as LanguageOption)?.Code;
            DialogResult = true;
        }
    }
}

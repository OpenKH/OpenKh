using Avalonia.Controls;
using Avalonia.Interactivity;
using OpenKh.Tools.Common.Avalonia;
using OpenKh.Tools.ModsManager.ViewModels;
using System;
using System.Collections.Generic;

namespace OpenKh.Tools.ModsManager.Views
{
    /// <summary>
    /// Avalonia port of the setup wizard. The Xceed Wizard control is
    /// replaced by a simple page host: all pages live in a Panel and only the
    /// current one is visible; Back/Next follow the same ViewModel-driven
    /// branching (WizardPageAfterIntro etc.) and PageStack history the WPF
    /// wizard uses, so SetupWizardViewModel is shared unchanged.
    /// </summary>
    public partial class SetupWizardWindow : DialogWindowBase
    {
        private sealed record PageInfo(
            string Title,
            string Description,
            Func<object> GetNextPage,
            Func<bool> CanNext,
            Func<bool> CanBackAndCancel);

        private readonly SetupWizardViewModel _vm;
        private readonly Dictionary<Control, PageInfo> _pages;
        private Control _currentPage;
        private bool _updatingButtons;

        public SetupWizardWindow()
        {
            InitializeComponent();
            _vm = new SetupWizardViewModel();

            // Assign the page tokens BEFORE setting DataContext: some shared
            // ViewModel getters (e.g. LaunchOption) assign the WizardPageAfter*
            // properties from these tokens as a side effect, and Avalonia
            // evaluates bindings synchronously when DataContext changes. With
            // the tokens still null, navigation would dead-end.
            _vm.PageIsoSelection = PageIsoSelection;
            _vm.PageEosInstall = PageEosInstall;
            _vm.PageRegion = PageRegion;
            _vm.PageGameData = PageGameData;
            _vm.PageSteamAPITrick = PageSteamAPITrick;
            _vm.LastPage = LastPage;

            DataContext = _vm;

            _pages = new Dictionary<Control, PageInfo>
            {
                [PageGameEdition] = new PageInfo(
                    "Game edition",
                    "Selected the preferred edition to launch the game",
                    () => _vm.WizardPageAfterIntro,
                    () => _vm.IsGameSelected,
                    () => true),
                [PageIsoSelection] = new PageInfo(
                    "Configure the game you want to mod",
                    "Do not worry, you can change this option later",
                    () => PageGameData,
                    () => true,
                    () => true),
                [PageGameData] = new PageInfo(
                    "Set Game Data Location",
                    "It might be necessary to extract game's data.",
                    () => _vm.WizardPageAfterGameData,
                    () => _vm.IsGameDataFound,
                    () => _vm.IsNotExtracting),
                [PageRegion] = new PageInfo(
                    "Set your preferred region",
                    "This will instruct the game to force to load specific languages",
                    () => LastPage,
                    () => _vm.IsGameDataFound,
                    () => true),
                [PageEosInstall] = new PageInfo(
                    "Install OpenKH Panacea (Optional and Experimental)",
                    "Install automatic mod loading support into the game's folder.",
                    () => PageLuaBackendInstall,
                    () => true,
                    () => true),
                [PageLuaBackendInstall] = new PageInfo(
                    "Install Lua Backend",
                    "Lua Backend allows you to use Lua Scripts with the PC version of Kingdom Hearts.",
                    () => _vm.WizardPageAfterLuaBackend,
                    () => true,
                    () => true),
                [PageSteamAPITrick] = new PageInfo(
                    "Launch Games Directly (Steam)",
                    "Steam allows you to launch the exes directly through a one line text file located in the games install folder.",
                    () => PageGameData,
                    () => true,
                    () => true),
                [LastPage] = new PageInfo(
                    "You're set!",
                    "You successfully configured OpenKH Mods Manager.",
                    () => null,
                    () => false,
                    () => true),
            };

            _vm.PropertyChanged += (_, _) => UpdateButtons();
            _vm.PageStack.PropertyChanged += (_, _) => UpdateButtons();

            NavigateTo(PageGameEdition);

            Closed += (sender, e) => _vm.SetAborted();
        }

        private void NavigateTo(Control page)
        {
            if (page is null)
                return;

            if (_currentPage is not null)
                _currentPage.IsVisible = false;
            _currentPage = page;
            _currentPage.IsVisible = true;

            _vm.PageStack.OnPageChanged(page);

            var info = _pages[page];
            HeaderTitle.Text = info.Title;
            HeaderDescription.Text = info.Description;

            UpdateButtons();
        }

        private void UpdateButtons()
        {
            if (_currentPage is null)
                return;

            // Some shared-ViewModel getters (e.g. GameEdition) assign other
            // properties as a side effect, which raises PropertyChanged and
            // would re-enter this method endlessly without the guard.
            if (_updatingButtons)
                return;
            _updatingButtons = true;
            try
            {
                var info = _pages[_currentPage];
                NextButton.IsVisible = !ReferenceEquals(_currentPage, LastPage);
                NextButton.IsEnabled = info.CanNext() && info.GetNextPage() is not null;
                BackButton.IsEnabled = info.CanBackAndCancel() && _vm.PageStack.Back is Control;
                CancelButton.IsEnabled = info.CanBackAndCancel();
                FinishButton.IsVisible = ReferenceEquals(_currentPage, LastPage);
            }
            finally
            {
                _updatingButtons = false;
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.PageStack.Back is Control previous)
                NavigateTo(previous);
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            var info = _pages[_currentPage];
            if (info.CanNext() && info.GetNextPage() is Control next)
                NavigateTo(next);
        }

        private void Finish_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close(true);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) =>
            Close();
    }
}

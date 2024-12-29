using OpenKh.Bbs;
using OpenKh.Common;
using OpenKh.Tools.CtdEditor.Helpers;
using OpenKh.Tools.CtdEditor.Services;
using OpenKh.Tools.CtdEditor.ViewModels;
using Reactive.Bindings.Extensions;
using SharpDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;

namespace OpenKh.Tools.CtdEditor.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private static readonly IEnumerable<FileDialogFilter> CtdFilter = FileDialogFilterComposer.Compose()
            .AddExtensions("CTD message", "ctd")
            .AddAllFiles();
        private static readonly IEnumerable<FileDialogFilter> FontArcFilter = FileDialogFilterComposer.Compose()
            .AddExtensions("Font archive", "arc")
            .AddAllFiles();

        public MainViewModel VM => (MainViewModel)DataContext;

        public MainWindow(
            MainViewModel vm,
            AppInfoService appInfoService,
            Func<FontEditorViewModel, FontWindow> newFontWindow,
            Func<LayoutEditorViewModel, LayoutWindow> newLayoutWindow)
        {
            InitializeComponent();

            vm
                .ObserveProperty(it => it.FileName)
                .ObserveOn(Scheduler.Immediate)
                .Select(fileName => $"{Path.GetFileName(fileName) ?? "untitled"} | {appInfoService.ApplicationName}")
                .Subscribe(title => Title = title)
                .AddTo(_disposables);

            vm.OpenCommand = new RelayCommand(x =>
                FileDialog.OnOpen(fileName => OpenFile(fileName), CtdFilter), x => true);

            vm.SaveCommand = new RelayCommand(x =>
            {
                if (!string.IsNullOrEmpty(vm.FileName))
                {
                    SaveFile(vm.FileName, vm.FileName);
                }
                else
                {
                    vm.SaveAsCommand.Execute(x);
                }
            }, x => true);

            vm.SaveAsCommand = new RelayCommand(x =>
                FileDialog.OnSave(fileName => SaveFile(vm.FileName, fileName), CtdFilter), x => true);

            vm.ExitCommand = new RelayCommand(x =>
            {
                Close();
            }, x => true);

            vm.OpenFontCommand = new RelayCommand(x =>
                FileDialog.OnOpen(fileName => OpenFontFile(fileName), FontArcFilter),
                x => vm.CtdViewModel != null);

            vm.OpenFontEditorCommand = new RelayCommand(x =>
                newFontWindow(new FontEditorViewModel(vm.Fonts))
                    .ShowDialog(),
                x => vm.Fonts != null);

            vm.OpenLayoutEditorCommand = new RelayCommand(x =>
                newLayoutWindow(new LayoutEditorViewModel(vm.Ctd))
                    .ShowDialog(),
                x => vm.Ctd != null
            );

            vm.UseInternationalEncodingCommand = new RelayCommand(x =>
            {
                vm.MessageConverter = MessageConverter.Default;
                vm.UseInternationalEncoding = true;
                vm.UseJapaneseEncoding = false;
            });

            vm.UseJapaneseEncodingCommand = new RelayCommand(x =>
            {
                vm.MessageConverter = MessageConverter.ShiftJIS;
                vm.UseInternationalEncoding = false;
                vm.UseJapaneseEncoding = true;
            });

            vm.AboutCommand = new RelayCommand(x =>
            {
                var about = new AboutDialog(Assembly.GetExecutingAssembly());
                about.Author = "Open KH Team";
                about.AuthorWebsite = "https://www.openkh.dev";
                about.ShowDialog();
            }, x => true);

            DataContext = vm;

            string[] args = Environment.GetCommandLineArgs();
            for (int a = 0; a < args.Length; a++)
            {
                bool isArg(string arg) => string.Equals(args[a], arg, StringComparison.InvariantCultureIgnoreCase);

                if (isArg("--font") || isArg("-f"))
                {
                    a++;
                    OpenFontFile(args[a]);
                }
                else if (isArg("--message") || isArg("-m"))
                {
                    a++;
                    OpenFile(args[a]);
                }
            }
        }

        private bool OpenFile(string fileName) => File.OpenRead(fileName).Using(stream =>
        {
            if (!Ctd.IsValid(stream))
            {
                MessageBox.Show(this, $"{Path.GetFileName(fileName)} is not a valid CTD file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            VM.Ctd = Ctd.Read(stream);
            VM.FileName = fileName;
            return true;
        });

        private void OpenFontFile(string fileName) => File.OpenRead(fileName).Using(stream =>
        {
            if (!Arc.IsValid(stream))
                throw new Exception("Not a valid ARC file");

            VM.Fonts = FontsArc.Read(stream);
            VM.FontName = fileName;
        });

        private void SaveFile(string previousFileName, string fileName)
        {
            File.Create(fileName).Using(stream =>
            {
                VM.Ctd.Write(stream);
            });
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            _disposables.Dispose();
        }
    }
}

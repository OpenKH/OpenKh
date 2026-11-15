using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static OpenKh.Patcher.Metadata;

namespace OpenKh.Tools.ModManager.Models
{
    public partial class ModModel : ObservableObject
    {
        [ObservableProperty]
        private string? _modTitle;

        [ObservableProperty]
        private string? _modAuthor;

        [ObservableProperty]
        private string? _modGitAddress;

        [ObservableProperty]
        private string? _modPlatform;

        [ObservableProperty]
        private List<PreferenceModel>? _modPreferences;

        [ObservableProperty]
        private string? _modDescription;

        [ObservableProperty]
        private string[]? _modDependencies;

        [ObservableProperty]
        private string[]? _modFilesList;

        [ObservableProperty]
        private Bitmap? _modIcon;

        [ObservableProperty]
        private string? _modPath;

        [ObservableProperty]
        private bool _modActive;

        [ObservableProperty]
        private bool _modValid;

        [ObservableProperty]
        private Uri? _modSource;

        [ObservableProperty]
        private Uri? _modIssues;

        [ObservableProperty]
        private PresetModel? _modLinkPreset;

        [ObservableProperty]
        private DateTime _modLatestCommit;

        [ObservableProperty]
        private bool _modUpdate;
    }
}

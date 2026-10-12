using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Octokit;
using OpenKh.Patcher;
using OpenKh.Tools.ModManager.Classes;
using OpenKh.Tools.ModManager.Models;
using OpenKh.Tools.ModManager.Services;
using OpenKh.Tools.ModManager.Views;
using SharpYaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace OpenKh.Tools.ModManager.ViewModels
{
    public partial class CatalogViewModel : ViewModelBase
    {
        [ObservableProperty]
        private bool _initialized = false;

        [ObservableProperty]
        private ModModel? _currentMod = null;

        [ObservableProperty]
        private ObservableCollection<ModModel>? _allFetchedMods = null;

        [ObservableProperty]
        private ObservableCollection<ModModel>? _filterFetchedMods = null;

        [ObservableProperty]
        private ObservableCollection<string>? _selectedMods = null;

        [ObservableProperty]
        private Config? _currentConfig = null;

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        partial void OnSearchQueryChanged(string value)
        {
            var _fetchValueRegex = new Regex("^(?:(?=(?:.*?\\s)?author:(?<author>\\S+)))?(?:author:\\S+\\s*)?(?<title>.*?)(?:\\s*author:\\S+)?$");
            var _fetchSearchMatch = _fetchValueRegex.Match(value.ToLower());

            if (_fetchSearchMatch.Success)
            {
                var _fetchGroupTitle = _fetchSearchMatch.Groups["title"].Value;
                var _fetchGroupAuthor = _fetchSearchMatch.Groups["author"].Value;

                var _fetchFilter = AllFetchedMods.Where(x => x.ModTitle.ToLower().Contains(_fetchGroupTitle) && x.ModAuthor.ToLower().Contains(_fetchGroupAuthor));

                var _lockObject = new object();
                FilterFetchedMods.Clear();

                foreach (var _fetchMod in _fetchFilter)
                    FilterFetchedMods.Add(_fetchMod);
            }
        }

        public TopLevel? FetchTopLevel()
        {
            var _fetchApplication = Avalonia.Application.Current;

            if (_fetchApplication == null)
                return null;

            var _fetchLifetime = _fetchApplication.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            var _fetchMainView = _fetchLifetime != null ? _fetchLifetime.Windows.FirstOrDefault(x => x.GetType() == typeof(CatalogView)) : null;

            return _fetchMainView;
        }

        public async Task<bool> InitializeView(bool selectLast = false)
        {
            Initialized = false;

            CurrentMod = null;
            SelectedMods = null;
            AllFetchedMods = null;
            FilterFetchedMods = null;

            if (CurrentConfig != null)
            {
                var _lockObject = new object();

                AllFetchedMods = new ObservableCollection<ModModel>();
                FilterFetchedMods = new ObservableCollection<ModModel>();

                SelectedMods = new ObservableCollection<string>();

                var _fetchHeader = new ProductHeaderValue("OpenKH-ModSearcher");
                var _fetchClient = new GitHubClient(_fetchHeader);

                var _fetchGame = Config.GameShorthand[CurrentConfig.Frontend.TargetGame];

                // 2. Define the search request using the "topic:topic-name" qualifier
                var _fetchInquiry = new SearchRepositoriesRequest($"topic:openkh-mod topic:{_fetchGame}");
                var _fetchResult = await _fetchClient.Search.SearchRepo(_fetchInquiry);

                await Task.Run(async () =>
                {
                    while (_fetchResult.Items != null && _fetchResult.Items.Count != 0)
                    {
                        await Parallel.ForEachAsync(_fetchResult.Items.AsParallel(), async (_fetchMod, _fetchToken) =>
                        {
                            var _fetchMetadataURL = $"https://raw.githubusercontent.com/{_fetchMod.FullName}/{_fetchMod.DefaultBranch}/mod.yml";
                            var _fetchIconURL = $"https://raw.githubusercontent.com/{_fetchMod.FullName}/{_fetchMod.DefaultBranch}/icon.png";

                            using var _makeClient = new HttpClient();

                            using var _fetchMetadataOK = await _makeClient.GetAsync(_fetchMetadataURL, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None);
                            using var _fetchImageOK = await _makeClient.GetAsync(_fetchIconURL, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None);

                            // If the file doesn't exist, abort.
                            if (_fetchMetadataOK.StatusCode == HttpStatusCode.OK)
                            {
                                var _fetchContent = await _makeClient.GetStringAsync(_fetchMetadataURL, CancellationToken.None);
                                var _fetchImageRAW = _fetchImageOK.StatusCode == HttpStatusCode.OK ? await _makeClient.GetByteArrayAsync(_fetchIconURL, CancellationToken.None) : null;

                                var _fetchMetadata = Metadata.Read(new StringReader(_fetchContent));

                                Dispatcher.UIThread.Post(async () =>
                                {
                                    if (_fetchMetadata != null && _fetchMetadata.IsValid)
                                    {
                                        var _modModel = new ModModel
                                        {
                                            ModTitle = !String.IsNullOrEmpty(_fetchMetadata.Title) ? _fetchMetadata.Title : _fetchMod.Name,
                                            ModAuthor = !String.IsNullOrEmpty(_fetchMetadata.OriginalAuthor) ? _fetchMetadata.OriginalAuthor : _fetchMod.Owner.Login,
                                            ModDescription = !String.IsNullOrEmpty(_fetchMetadata.Description) ? _fetchMetadata.Description : "This mod does not have a description, but we believe it's pretty cool.",
                                            ModFilesList = _fetchMetadata.Assets.Select(x => x.Name).ToArray(),
                                            ModIcon = _fetchImageRAW != null ? new Bitmap(new MemoryStream(_fetchImageRAW)) : null,
                                            ModPath = _fetchMod.FullName,
                                            ModActive = false,
                                            ModValid = true
                                        };

                                        lock (_lockObject)
                                        {
                                            FilterFetchedMods.Add(_modModel);
                                            AllFetchedMods.Add(_modModel);
                                        }
                                    }
                                });
                            }
                        });

                        _fetchInquiry = new SearchRepositoriesRequest($"topic:openkh-mod topic:{_fetchGame}") { PerPage = 100, Page = _fetchInquiry.Page + 1 };
                        _fetchResult = await _fetchClient.Search.SearchRepo(_fetchInquiry);
                    }
                });

                return true;
            }

            return false;
        }

        [RelayCommand]
        private void Toggle(Tuple<ModModel, bool> inputParameter)
        {
            if (CurrentConfig != null && SelectedMods != null)
            {
                var _fetchTargetMod = inputParameter.Item1;
                var _fetchIsChecked = inputParameter.Item2;

                var _fetchSenderURL = _fetchTargetMod.ModPath;
                var _fetchFromList = SelectedMods.FirstOrDefault(x => x == _fetchSenderURL);

                if (_fetchFromList != null && !_fetchIsChecked)
                    SelectedMods.Remove(_fetchSenderURL);

                else if (_fetchFromList == null && _fetchIsChecked)
                    SelectedMods.Add(_fetchSenderURL);
            }
        }

        [RelayCommand]
        private void Install()
        {
            var _fetchTopLevel = FetchTopLevel() as Window;
            if (SelectedMods != null && _fetchTopLevel != null)
            {
                var _fetchReturn = SelectedMods.Count > 0 ? String.Join(';', SelectedMods) : null;
                _fetchTopLevel.Close(_fetchReturn);
            }
        }
    }
}

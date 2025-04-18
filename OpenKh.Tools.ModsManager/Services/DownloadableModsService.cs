using Newtonsoft.Json;
using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Patcher;
using OpenKh.Tools.ModsManager.Models;
using OpenKh.Tools.ModsManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace OpenKh.Tools.ModsManager.Services
{
    public static class DownloadableModsService
    {
        private const string DEFAULT_MODS_JSON_URL = "https://raw.githubusercontent.com/OpenKH/mods-manager-feed/refs/heads/main/downloadable-mods.json";
        private static string CachePath => Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "downloadable-mods.json");
        private static HttpClient _httpClient = new HttpClient();
        private static string _modDirectoryPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "mods");

        public class DownloadableModsData
        {
            public Dictionary<string, List<DownloadableModEntry>> Mods { get; set; }
        }

        public class DownloadableModEntry
        {
            public string Repo { get; set; }
        }

        public static async Task<IEnumerable<DownloadableModModel>> GetDownloadableModsForGame(string game)
        {
            try
            {
                var data = await GetDownloadableModsData();

                if (data?.Mods == null || !data.Mods.ContainsKey(game))
                    return Enumerable.Empty<DownloadableModModel>();

                var mods = new List<DownloadableModModel>();
                var tasks = new List<Task<DownloadableModModel>>();

                // Start all API requests in parallel
                foreach (var entry in data.Mods[game])
                {
                    // Check if the mod is already installed
                    var modPath = GetModPath(entry.Repo);
                    if (Directory.Exists(modPath))
                        continue;

                    tasks.Add(FetchModInfo(entry.Repo));
                }

                // Wait for all tasks to complete
                var results = await Task.WhenAll(tasks);

                // Add only non-null results
                foreach (var modInfo in results)
                {
                    if (modInfo != null)
                    {
                        mods.Add(modInfo);
                    }
                }

                return mods;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting downloadable mods: {ex.Message}");
                return Enumerable.Empty<DownloadableModModel>();
            }
        }

        public static string GetModPath(string repositoryName)
        {
            var path = repositoryName.Split('/');
            if (path.Length != 2)
                return null;

            return Path.Combine(
                ConfigurationService.ModCollectionPath,
                path[0],
                path[1]);
        }

        private static async Task<DownloadableModModel> FetchModInfo(string repo)
        {
            try
            {
                var path = repo.Split('/');
                if (path.Length != 2)
                    return null;

                var userName = path[0];
                var repoName = path[1];

                var yamlContent = await TryFetchFile(GetMetadataJsonUrl(repo));

                if (string.IsNullOrEmpty(yamlContent))
                {
                    yamlContent = await TryFetchFile($"https://raw.githubusercontent.com/{userName}/{repoName}/master/mod.yml");
                    if (string.IsNullOrEmpty(yamlContent))
                        return null;
                }

                // Deserialize the YAML content using MemoryStream
                Metadata metadata;
                using (var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(yamlContent)))
                {
                    metadata = Metadata.Read(ms);
                }

                // Try to fetch GitHub repository description
                string repoDescription = null;
                try
                {
                    string repoApiUrl = $"https://api.github.com/repos/{userName}/{repoName}";
                    var response = await TryFetchFile(repoApiUrl);

                    if (!string.IsNullOrEmpty(response))
                    {
                        var repoInfo = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
                        if (repoInfo.ContainsKey("description") && repoInfo["description"] != null)
                        {
                            repoDescription = repoInfo["description"].ToString();
                        }
                    }
                }
                catch
                {
                    // Ignore GitHub API errors
                }

                // Try to get icon.png from main branch first, then master
                BitmapImage iconImage = null;
                BitmapImage previewImage = null;
                try
                {
                    byte[] imageBytes = await TryFetchBinaryFile(GetIconUrl(repo));

                    if (imageBytes == null || imageBytes.Length == 0)
                    {
                        imageBytes = await TryFetchBinaryFile($"https://raw.githubusercontent.com/{userName}/{repoName}/master/icon.png");
                    }

                    if (imageBytes != null && imageBytes.Length > 0)
                    {
                        try
                        {
                            var ms = new MemoryStream(imageBytes);
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.StreamSource = ms;
                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.EndInit();
                            ms.Position = 0;
                            iconImage = image;

                            // Guardar la imagen en disco para futuras referencias
                            string localModPath = Path.Combine(_modDirectoryPath, userName, repoName);
                            Directory.CreateDirectory(localModPath);
                            File.WriteAllBytes(Path.Combine(localModPath, "icon.png"), imageBytes);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error creating icon image for {userName}/{repoName}: {ex.Message}");
                        }
                    }

                    // Try to get preview.png
                    byte[] previewBytes = await TryFetchBinaryFile(GetPreviewUrl(repo));

                    if (previewBytes == null || previewBytes.Length == 0)
                    {
                        previewBytes = await TryFetchBinaryFile($"https://raw.githubusercontent.com/{userName}/{repoName}/master/preview.png");
                    }

                    if (previewBytes != null && previewBytes.Length > 0)
                    {
                        try
                        {
                            var ms = new MemoryStream(previewBytes);
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.StreamSource = ms;
                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.EndInit();
                            ms.Position = 0;
                            previewImage = image;

                            // Guardar la imagen en disco para futuras referencias
                            string localModPath = Path.Combine(_modDirectoryPath, userName, repoName);
                            Directory.CreateDirectory(localModPath);
                            File.WriteAllBytes(Path.Combine(localModPath, "preview.png"), previewBytes);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error creating preview image for {userName}/{repoName}: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error fetching icon for {repo}: {ex.Message}");
                    // Continue without icon
                }

                return new DownloadableModModel
                {
                    Repository = repo,
                    Name = metadata.Title ?? "Unknown",
                    Author = metadata.OriginalAuthor ?? userName,
                    Description = metadata.Description ?? repoDescription ?? $"A mod for {metadata.Game ?? "Kingdom Hearts"}",
                    Game = metadata.Game,
                    IconImageSource = null, // No used
                    PreviewImageSource = null, // Not used
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching mod info for {repo}: {ex.Message}");
                return null;
            }
        }

        public static string GetIconUrl(string repo)
        {
            if (string.IsNullOrEmpty(repo))
                return null;

            return $"{GetRawBaseUrl(repo)}/icon.png";
        }

        public static string GetPreviewUrl(string repo)
        {
            if (string.IsNullOrEmpty(repo))
                return null;

            return $"{GetRawBaseUrl(repo)}/preview.png";
        }

        public static string GetMetadataJsonUrl(string repo)
        {
            // Cambiado para apuntar directamente a la URL raw de GitHub
            return $"https://raw.githubusercontent.com/{repo}/main/mod.yml";
        }

        public static string GetRawBaseUrl(string repo)
        {
            var path = repo.Split('/');
            if (path.Length != 2)
                return null;

            return $"https://raw.githubusercontent.com/{path[0]}/{path[1]}/main";
        }

        private static async Task<string> TryFetchFile(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private static async Task<byte[]> TryFetchBinaryFile(string url)
        {
            try
            {
                Console.WriteLine($"Fetching binary file from: {url}");
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    Console.WriteLine($"Successfully downloaded {bytes.Length} bytes from {url}");
                    return bytes;
                }
                Console.WriteLine($"Failed to download from {url}, status code: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading from {url}: {ex.Message}");
                return null;
            }
        }

        public static async Task<DownloadableModsData> GetDownloadableModsData()
        {
            try
            {
                // First try to fetch from remote URL
                string json = await TryFetchFile(DEFAULT_MODS_JSON_URL);

                // If remote fetch fails, try to use local JSON
                if (string.IsNullOrEmpty(json))
                {
                    var localJsonPath = Path.Combine(
                        Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                        "downloadable-mods.json");

                    if (File.Exists(localJsonPath))
                    {
                        json = File.ReadAllText(localJsonPath);
                    }
                }

                // If local fetch fails, try to use cache
                if (string.IsNullOrEmpty(json) && File.Exists(CachePath))
                {
                    json = File.ReadAllText(CachePath);
                }

                // If we have json, deserialize and save to cache
                if (!string.IsNullOrEmpty(json))
                {
                    try
                    {
                        // Save to cache
                        File.WriteAllText(CachePath, json);
                    }
                    catch { /* Ignore cache saving failures */ }

                    return JsonConvert.DeserializeObject<DownloadableModsData>(json);
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting mods data: {ex.Message}");
                return null;
            }
        }

        public static async Task<bool> InstallMod(string repo, Action<string> progressOutput = null)
        {
            try
            {
                progressOutput?.Invoke($"Installing mod from {repo}...");
                await ModsService.InstallMod(repo, false, false, progressOutput);
                progressOutput?.Invoke($"Mod installed successfully!");
                return true;
            }
            catch (Exception ex)
            {
                progressOutput?.Invoke($"Error installing mod: {ex.Message}");
                return false;
            }
        }

        public static List<ModViewModel> GetInstalledMods()
        {
            // Debería retornar la lista real, pero usamos una lista vacía por simplicidad
            return new List<ModViewModel>();
        }

        public static async Task<DownloadableModViewModel> CreateModViewModelFromRepo(string game, string repo, List<ModViewModel> installedMods)
        {
            try
            {
                string downloadUrl = GetRawBaseUrl(repo);
                string metadataUrl = $"{downloadUrl}/mod.yml";
                string iconUrl = $"{downloadUrl}/icon.png";
                string previewUrl = $"{downloadUrl}/preview.png";

                var modYmlContent = await TryFetchTextFile(metadataUrl);
                if (string.IsNullOrEmpty(modYmlContent))
                {
                    Log.Warn($"Failed to download mod.yml from {metadataUrl}");
                    return null;
                }

                var metadata = DeserializeModModel(modYmlContent);
                if (metadata == null)
                {
                    Log.Warn($"Failed to deserialize mod.yml content from {metadataUrl}");
                    return null;
                }

                BitmapImage iconImage = await GetImageFromUrl(iconUrl);
                BitmapImage previewImage = await GetImageFromUrl(previewUrl);

                return new DownloadableModViewModel(metadata, iconImage, previewImage, repo, game, installedMods);
            }
            catch (Exception ex)
            {
                Log.Err($"Error creating mod view model for repo {repo}: {ex.Message}");
                return null;
            }
        }

        private static DownloadableModModel DeserializeModModel(string modYmlContent)
        {
            try
            {
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(modYmlContent));
                var metadata = Metadata.Read(stream);

                var model = new DownloadableModModel
                {
                    Name = metadata.Title,
                    Author = metadata.OriginalAuthor,
                    Description = metadata.Description,
                    Game = metadata.Game
                    // Añadir más propiedades según sea necesario
                };

                return model;
            }
            catch (Exception ex)
            {
                Log.Err($"Error deserializing mod.yml: {ex.Message}");
                return null;
            }
        }

        private static async Task<string> TryFetchTextFile(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public static async Task<BitmapImage> GetImageFromUrl(string url)
        {
            try
            {
                var bytes = await TryFetchBinaryFile(url);
                if (bytes == null || bytes.Length == 0)
                {
                    return null;
                }

                return await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    var ms = new MemoryStream(bytes);
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.StreamSource = ms;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.EndInit();
                    image.Freeze(); // Important to freeze the image so it can be used across threads
                    ms.Position = 0;
                    return image;
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching icon for {url}: {ex.Message}");
                return null;
            }
        }
    }
}

using OpenKh.Patcher;
using OpenKh.Tools.ModsManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace OpenKh.Tools.ModsManager.Services
{
    public class DownloadableModsService
    {
        /// <summary>
        /// Delegate and event for status updates notification
        /// </summary>
        /// <param name="status"></param>
        public delegate void StatusUpdateHandler(string status);

        /// <summary>
        /// Notify for status updates
        /// </summary>
        public event StatusUpdateHandler OnStatusUpdate;

        /// <summary>
        /// Notify for diagnostic log messages
        /// </summary>
        /// <param name="status"></param>
        public delegate void DiagLogHandler(string status);

        /// <summary>
        /// Notify for diagnostic log messages
        /// </summary>
        public event DiagLogHandler OnDiagLog;

        private const string DownloadableModsJsonUrl = "https://raw.githubusercontent.com/OpenKH/mods-manager-feed/main/downloadable-mods.json";

        private const string ModMetadataFileName = "mod.yml";

        /// <summary>
        /// Different file name variants for better compatibility
        /// </summary>
        private static readonly string[] ModIconFileNames = { "icon.png", "Icon.png", "ICON.png", "Icon.PNG", "icon.PNG" };

        private static readonly string[] ModPreviewFileNames = { "preview.png", "Preview.png", "PREVIEW.png", "Preview.PNG", "preview.PNG" };

        /// <summary>
        /// A cache directory name for cache mechanism of this DownloadableModsService.
        /// This directory will be placed like `%LOCALAPPDATA%/OpenKh/downloadable-mods-cache`
        /// </summary>
        private const string CacheDirectoryName = "downloadable-mods-cache";

        /// <summary>
        /// HTTP request timeout (5 seconds)
        /// </summary>
        private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Cache of downloadable mods by game
        /// </summary>
        private readonly Dictionary<string, List<DownloadableModModel>> _modsCache = new Dictionary<string, List<DownloadableModModel>>();

        /// <summary>
        /// Cache expiration time (1 day)
        /// </summary>
        private static TimeSpan CacheExpiration = TimeSpan.FromDays(1);

        private static Lazy<DownloadableModsService> _lazyDefault = new Lazy<DownloadableModsService>(
            () => new DownloadableModsService()
        );

        /// <summary>
        /// Provide a singleton default instance
        /// </summary>
        public static DownloadableModsService Default => _lazyDefault.Value;

        /// <summary>
        /// Encodes the repository path to handle special characters in URLs
        /// </summary>
        /// <param name="repositoryPath">Repository path in username/repository format</param>
        /// <returns>URL-safe encoded path</returns>
        private string EncodeRepositoryPath(string repo)
        {
            if (string.IsNullOrEmpty(repo))
                return string.Empty;

            try
            {
                // Encode each part of the repository (username/repository-name) separately
                var parts = repo.Split('/');
                if (parts.Length != 2)
                    return repo;

                string encodedOwner = Uri.EscapeDataString(parts[0]);
                string encodedRepo = Uri.EscapeDataString(parts[1]);

                return $"{encodedOwner}/{encodedRepo}";
            }
            catch (Exception ex)
            {
                OnDiagLog?.Invoke($"Error encoding repository path: {ex.Message}");
                return repo; // In case of error, return the original
            }
        }

        /// <summary>
        /// Attempts to load an image by trying different filename variants and branches
        /// </summary>
        private async Task TryLoadImageWithVariants(
            DownloadableModModel mod,
            string cachePath,
            string encodedRepo,
            string[] fileNameVariants,
            Action<BitmapImage> setImage,
            CancellationToken cancellationToken = default)
        {
            // Repository branches to try
            string[] branches = { "main", "master" };
            bool success = false;

            foreach (var branch in branches)
            {
                if (success)
                    break;

                foreach (var fileName in fileNameVariants)
                {
                    try
                    {
                        // Build the URL and specific cache path for this combination
                        string url = $"https://raw.githubusercontent.com/{encodedRepo}/{branch}/{fileName}";
                        string specificCachePath = Path.Combine(
                            Path.GetDirectoryName(cachePath),
                            $"{branch}_{fileName}_{Path.GetFileName(cachePath)}");

                        OnDiagLog?.Invoke($"Trying URL: {url}");

                        // Try to load this variant
                        await LoadImageWithCache(mod, specificCachePath, url, setImage, cancellationToken);

                        // If we get here, we assume it was successful
                        OnDiagLog?.Invoke($"Success with URL: {url}");
                        success = true;
                        break;
                    }
                    catch (Exception ex)
                    {
                        OnDiagLog?.Invoke($"Error with variant {fileName} in branch {branch}: {ex.Message}");
                        // Continue with the next variant
                    }
                }
            }

            if (!success)
            {
                OnDiagLog?.Invoke($"Could not load any image variant for {mod.Repo}");
            }
        }

        /// <summary>
        /// Last cache update time by game
        /// </summary>
        private readonly Dictionary<string, DateTime> _lastCacheUpdate = new Dictionary<string, DateTime>();

        /// <summary>
        /// Base directory for file cache
        /// </summary>
        private readonly string _cacheDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "OpenKh", CacheDirectoryName);

        public DownloadableModsService(
            string cacheDirectory = null)
        {
            _cacheDirectory = cacheDirectory ?? _cacheDirectory;

            if (!Directory.Exists(_cacheDirectory))
                Directory.CreateDirectory(_cacheDirectory);
        }

        /// <summary>
        /// Get the list of downloadable mods for a specific game
        /// </summary>
        public async Task GetDownloadableModsForGameAsync(
            string gameId,
            Func<DownloadableModModel, Task> emitAsync,
            bool fallbackToLocalCache = true,
            CancellationToken cancellationToken = default)
        {
            OnStatusUpdate?.Invoke("Starting mod loading...");

            await GetDownloadableModsForGameInternalAsync(gameId, emitAsync, fallbackToLocalCache, cancellationToken);
        }

        private async Task GetDownloadableModsForGameInternalAsync(
            string gameId,
            Func<DownloadableModModel, Task> emitAsync,
            bool fallbackToLocalCache = true,
            CancellationToken cancellationToken = default
        )
        {
            // Check if cancellation is requested
            cancellationToken.ThrowIfCancellationRequested();
            OnStatusUpdate?.Invoke("Checking available cache...");

            // Check if we have valid cache
            if (_modsCache.TryGetValue(gameId, out var cachedMods) &&
                _lastCacheUpdate.TryGetValue(gameId, out var lastUpdate) &&
                DateTime.Now - lastUpdate < CacheExpiration)
            {
                OnStatusUpdate?.Invoke("Using cached data (less than 1 hour old)...");

                // Cache is up to date, filter out any mod that's already installed
                var installedMods = ModsService.Mods.ToHashSet();
                var filteredMods = cachedMods
                    .Where(mod => !installedMods.Contains(mod.Repo))
                    .ToList();

                OnStatusUpdate?.Invoke($"Found {filteredMods.Count} available mods in cache");

                foreach (var mod in filteredMods)
                {
                    await emitAsync(mod);
                }
            }

            // No cache or it's expired, load from network

            try
            {
                // Check if cancellation is requested
                cancellationToken.ThrowIfCancellationRequested();

                OnStatusUpdate?.Invoke($"Downloading mod list for {gameId}...");

                // Get the list of available mods from the JSON
                using var client = new HttpClient();
                client.Timeout = RequestTimeout;

                // Use GetAsync with HttpCompletionOption.ResponseHeadersRead for better performance
                using var response = await client.GetAsync(DownloadableModsJsonUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();

                var jsonContent = await response.Content.ReadAsStringAsync();

                // Save JSON to cache
                try
                {
                    OnStatusUpdate?.Invoke("Saving data to cache...");
                    var jsonCachePath = Path.Combine(_cacheDirectory, "downloadable-mods.json");
                    await File.WriteAllTextAsync(jsonCachePath, jsonContent, cancellationToken);
                }
                catch (Exception ex)
                {
                    OnDiagLog?.Invoke($"Error caching JSON: {ex.Message}");
                }

                // Check if cancellation is requested
                cancellationToken.ThrowIfCancellationRequested();

                OnStatusUpdate?.Invoke("Processing server data...");

                using var modData = System.Text.Json.JsonDocument.Parse(jsonContent);

                if (true
                    && modData.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object
                    && modData.RootElement.TryGetProperty("mods", out var modsElements)
                    && modsElements.ValueKind == System.Text.Json.JsonValueKind.Object
                    && modsElements.TryGetProperty(gameId, out var gameModsElement)
                    && gameModsElement.ValueKind == System.Text.Json.JsonValueKind.Array
                )
                {
                    // Get list of installed mods to filter
                    var installedMods = ModsService.Mods.ToHashSet();
                    var modTasks = new List<Task<DownloadableModModel>>();
                    var allMods = new List<DownloadableModModel>(); // List of all mods (installed or not) for caching

                    int totalMods = gameModsElement.EnumerateArray().Count();

                    OnStatusUpdate?.Invoke($"Found {totalMods} mods for {gameId}. Loading details...");

                    // Check if cancellation is requested
                    cancellationToken.ThrowIfCancellationRequested();

                    var modElements = gameModsElement.EnumerateArray().ToArray();

                    var numEmitted = 0;

                    // Process each mod entry and create tasks for parallel loading
                    foreach (var (modElement, processedMods) in modElements.Select((modElement, processedMods) => (modElement, processedMods)))
                    {
                        // Check if cancellation is requested
                        cancellationToken.ThrowIfCancellationRequested();

                        // Update status before processing batch
                        OnStatusUpdate?.Invoke($"Loading mod details... ({processedMods}/{totalMods})");

                        var repo = modElement.GetProperty("repo").GetString();

                        // Create base mod entry for all mods (for caching)
                        var mod = new DownloadableModModel
                        {
                            Repo = repo,
                            Game = gameId
                        };

                        allMods.Add(mod);

                        // Skip already installed mods from the result list but still process for caching
                        bool isInstalled = installedMods.Contains(repo);
                        bool isBlacklisted = ConfigurationService.BlacklistedMods != null &&
                                           ConfigurationService.BlacklistedMods.Contains(repo);

                        // Add task to load this mod
                        try
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            // Use file cache for metadata/images
                            await LoadMetadataWithCache(mod, cancellationToken);
                        }
                        catch (OperationCanceledException)
                        {
                            throw; // Re-throw cancellation exceptions
                        }
                        catch (Exception ex)
                        {
                            OnDiagLog?.Invoke($"Error loading mod {repo}: {ex.Message}");
                        }

                        if (isInstalled || isBlacklisted)
                        {
                            continue;
                        }
                        else
                        {
                            await emitAsync(mod);
                            numEmitted += 1;
                        }
                    }

                    // Update cache
                    _modsCache[gameId] = allMods;
                    _lastCacheUpdate[gameId] = DateTime.Now;

                    OnStatusUpdate?.Invoke($"Loading completed. {numEmitted} mods available for installation.");
                }
            }
            catch (Exception ex)
            {
                OnDiagLog?.Invoke($"Error loading downloadable mods: {ex.Message}");

                if (fallbackToLocalCache)
                {
                    // Try to load from local cache in case of network error
                    try
                    {
                        foreach (var it in await GetDownloadableModsLocallyAsync(gameId))
                        {
                            await emitAsync(it);
                        }
                    }
                    catch (Exception cacheEx)
                    {
                        OnDiagLog?.Invoke($"Error loading from cache: {cacheEx.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Get downloadable mods from local cache only
        /// </summary>
        public async Task<List<DownloadableModModel>> GetDownloadableModsLocallyAsync(string gameId)
        {
            var mods = new List<DownloadableModModel>();

            var jsonCachePath = Path.Combine(_cacheDirectory, "downloadable-mods.json");
            if (File.Exists(jsonCachePath))
            {
                var jsonContent = await File.ReadAllTextAsync(jsonCachePath);
                var modData = System.Text.Json.JsonDocument.Parse(jsonContent);
                var gameModsElement = modData.RootElement.GetProperty("mods").GetProperty(gameId);

                if (gameModsElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    var installedMods = ModsService.Mods.ToHashSet();
                    foreach (var modElement in gameModsElement.EnumerateArray())
                    {
                        var repo = modElement.GetProperty("repo").GetString();
                        if (installedMods.Contains(repo) ||
                            (ConfigurationService.BlacklistedMods != null &&
                             ConfigurationService.BlacklistedMods.Contains(repo)))
                            continue;

                        var mod = new DownloadableModModel
                        {
                            Repo = repo,
                            Game = gameId,
                            Title = repo.Split('/').Last(),
                            Description = "Loaded from local cache. Limited information available."
                        };
                        mods.Add(mod);
                    }
                }
            }

            return mods;
        }

        private async Task LoadMetadataWithCache(DownloadableModModel mod, CancellationToken cancellationToken = default)
        {
            try
            {
                // Create cache directory for this specific repository
                string modCacheDir = Path.Combine(_cacheDirectory, mod.Repo.Replace('/', '_'));
                string metadataPath = Path.Combine(modCacheDir, ModMetadataFileName);
                string iconPath = Path.Combine(modCacheDir, ModIconFileNames[0]);
                string previewPath = Path.Combine(modCacheDir, ModPreviewFileNames[0]);

                if (!Directory.Exists(modCacheDir))
                    Directory.CreateDirectory(modCacheDir);

                // Load metadata
                Metadata metadata = null;

                // Try loading from cache first
                if (File.Exists(metadataPath) && (DateTime.Now - File.GetLastWriteTime(metadataPath) < CacheExpiration))
                {
                    try
                    {
                        using var stream = File.OpenRead(metadataPath);
                        metadata = Metadata.Read(stream);
                    }
                    catch (Exception ex)
                    {
                        OnDiagLog?.Invoke($"Error reading cached metadata: {ex.Message}");
                    }
                }

                // If unable to load from cache, load from network
                if (metadata == null)
                {
                    using var client = new HttpClient();
                    client.Timeout = RequestTimeout;

                    // Load metadata file
                    var modYmlUrl = $"https://raw.githubusercontent.com/{mod.Repo}/main/{ModMetadataFileName}";
                    var metadataContent = await client.GetStringAsync(modYmlUrl);

                    // Parse YAML to get metadata
                    using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(metadataContent));
                    metadata = Metadata.Read(stream);

                    // Save to cache
                    try
                    {
                        await File.WriteAllTextAsync(metadataPath, metadataContent);
                    }
                    catch (Exception ex)
                    {
                        OnDiagLog?.Invoke($"Error caching metadata: {ex.Message}");
                    }
                }

                // Map metadata to model
                mod.Title = metadata.Title ?? mod.RepoName;
                mod.OriginalAuthor = metadata.OriginalAuthor ?? mod.RepoOwner;
                mod.Description = metadata.Description ?? "No description available.";

                // Encode repository path to handle special characters
                string encodedRepo = EncodeRepositoryPath(mod.Repo);

                // DEBUG: Show all possible paths for debugging
                OnDiagLog?.Invoke($"Original repository: {mod.Repo}");
                OnDiagLog?.Invoke($"Encoded repository: {encodedRepo}");

                // Load the icon by trying different variants
                await TryLoadImageWithVariants(mod, iconPath, encodedRepo, ModIconFileNames, image => mod.IconImage = image, cancellationToken);

                // Load the preview image by trying different variants
                await TryLoadImageWithVariants(mod, previewPath, encodedRepo, ModPreviewFileNames, image => mod.ScreenshotImageSource = image, cancellationToken);
            }
            catch (Exception ex)
            {
                OnDiagLog?.Invoke($"Error loading metadata for {mod.Repo}: {ex.Message}");
                // Set default values if metadata loading fails
                mod.Title = mod.Title ?? mod.RepoName;
                mod.OriginalAuthor = mod.OriginalAuthor ?? mod.RepoOwner;
                mod.Description = mod.Description ?? $"Could not load description for {mod.Repo}. Check your internet connection or try again later.";

                // Create default image if needed
                if (mod.IconImage == null)
                {
                    try
                    {
                        mod.IconImage = GetTextBasedAvatarImageOf(mod.RepoName ?? "?");
                    }
                    catch (Exception exIconImage)
                    {
                        OnDiagLog?.Invoke($"Error creating placeholder: {exIconImage.Message}");
                    }
                }
            }
        }

        private BitmapImage GetTextBasedAvatarImageOf(string name)
        {
            // Create a simple colored rectangle as placeholder
            var drawingVisual = new System.Windows.Media.DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawRectangle(
                    new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(100, 149, 237)), // Cornflower blue
                    null,
                    new System.Windows.Rect(0, 0, 64, 64));

                // Add text with repo name
                var formattedText = new System.Windows.Media.FormattedText(
                    (name + "  ").Substring(0, 2),
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    new System.Windows.Media.Typeface("Arial"),
                    24,
                    System.Windows.Media.Brushes.White,
                    1.0);

                drawingContext.DrawText(formattedText,
                    new System.Windows.Point((64 - formattedText.Width) / 2, (64 - formattedText.Height) / 2));
            }

            var renderTarget = new System.Windows.Media.Imaging.RenderTargetBitmap(
                64, 64, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
            renderTarget.Render(drawingVisual);
            renderTarget.Freeze();

            // Convert RenderTargetBitmap to BitmapImage
            var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(renderTarget));

            // Don't dispose the stream until after the BitmapImage has loaded from it
            var memoryStream = new System.IO.MemoryStream();
            encoder.Save(memoryStream);
            memoryStream.Position = 0;

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            // Safe to dispose now that image is frozen
            memoryStream.Dispose();

            return bitmapImage;
        }

        private async Task LoadImageWithCache(DownloadableModModel mod, string cachePath, string url, Action<BitmapImage> setImage, CancellationToken cancellationToken = default)
        {
            try
            {
                // Always show a placeholder first
                CreatePlaceholderImage(mod, setImage);

                byte[] imageData = null;

                // IMPORTANT: Force image downloads to solve cache problems
                bool forceRefresh = true; // Temporarily always download images

                // Extra debugging for URL
                OnDiagLog?.Invoke($"Attempting to load image from URL: {url}");
                OnDiagLog?.Invoke($"Cache path: {cachePath}");

                // Try to load from cache first if not forcing refresh
                if (!forceRefresh && File.Exists(cachePath))
                {
                    try
                    {
                        imageData = File.ReadAllBytes(cachePath);
                        OnDiagLog?.Invoke($"Image loaded from cache: {cachePath}");
                    }
                    catch (Exception ex)
                    {
                        OnDiagLog?.Invoke($"Error reading cached image: {ex.Message}");
                        // Delete corrupt cache file
                        try
                        { File.Delete(cachePath); }
                        catch { }
                    }
                }

                // If not in cache, cache read failed, or forcing refresh, download it
                if (imageData == null || forceRefresh)
                {
                    try
                    {
                        using var client = new HttpClient();
                        client.Timeout = TimeSpan.FromSeconds(20); // Longer timeout

                        // Add User-Agent to avoid potential blocks
                        client.DefaultRequestHeaders.Add("User-Agent", "OpenKh-ModManager/1.0");

                        OnDiagLog?.Invoke($"Downloading image from URL: {url}");
                        var response = await client.GetAsync(url, cancellationToken);

                        if (response.IsSuccessStatusCode)
                        {
                            imageData = await response.Content.ReadAsByteArrayAsync();
                            OnDiagLog?.Invoke($"Image downloaded successfully: {imageData.Length} bytes");

                            try
                            {
                                // Create cache directory if it doesn't exist
                                var directory = Path.GetDirectoryName(cachePath);
                                if (!Directory.Exists(directory))
                                {
                                    Directory.CreateDirectory(directory);
                                }

                                // Save to cache
                                File.WriteAllBytes(cachePath, imageData);
                            }
                            catch (Exception ex)
                            {
                                OnDiagLog?.Invoke($"Error saving image to cache: {ex.Message}");
                            }
                        }
                        else
                        {
                            OnDiagLog?.Invoke($"Failed to download image: {response.StatusCode} for {url}");
                        }
                    }
                    catch (Exception ex)
                    {
                        OnDiagLog?.Invoke($"Error downloading image: {ex.Message} for {url}");
                    }
                }

                // If we have image data, create a BitmapImage from it
                if (imageData != null)
                {
                    try
                    {
                        var bitmap = new BitmapImage();
                        using (var ms = new MemoryStream(imageData))
                        {
                            bitmap.BeginInit();
                            bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.StreamSource = ms;
                            bitmap.EndInit();
                            bitmap.Freeze(); // Important for cross-thread usage
                        }

                        setImage(bitmap);
                        OnDiagLog?.Invoke($"Image set successfully to UI");
                    }
                    catch (Exception ex)
                    {
                        OnDiagLog?.Invoke($"Error creating BitmapImage: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                OnDiagLog?.Invoke($"Unhandled error in LoadImageWithCache: {ex.Message}");
            }
        }

        private void CreatePlaceholderImage(DownloadableModModel mod, Action<BitmapImage> setImage)
        {
            try
            {
                // Create colored rectangle with text
                var drawingVisual = new System.Windows.Media.DrawingVisual();
                using (var drawingContext = drawingVisual.RenderOpen())
                {
                    // Pick a color based on repository name
                    byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(mod.Repo ?? "unknown");
                    var r = (byte)((nameBytes.Length > 0 ? nameBytes[0] : 100) % 200 + 55);
                    var g = (byte)((nameBytes.Length > 1 ? nameBytes[1] : 149) % 200 + 55);
                    var b = (byte)((nameBytes.Length > 2 ? nameBytes[2] : 237) % 200 + 55);

                    var brush = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(r, g, b));

                    drawingContext.DrawRectangle(brush, null, new System.Windows.Rect(0, 0, 64, 64));

                    // Get first two letters of repo name as text
                    string initials = "?";
                    if (!string.IsNullOrEmpty(mod.RepoName))
                    {
                        initials = mod.RepoName.Substring(0, Math.Min(2, mod.RepoName.Length)).ToUpper();
                    }

                    var formattedText = new System.Windows.Media.FormattedText(
                        initials,
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Windows.FlowDirection.LeftToRight,
                        new System.Windows.Media.Typeface("Arial"),
                        24,
                        System.Windows.Media.Brushes.White,
                        System.Windows.Media.VisualTreeHelper.GetDpi(drawingVisual).PixelsPerDip);

                    // Center the text
                    drawingContext.DrawText(formattedText,
                        new System.Windows.Point((64 - formattedText.Width) / 2, (64 - formattedText.Height) / 2));
                }

                // Render to bitmap
                var renderTarget = new System.Windows.Media.Imaging.RenderTargetBitmap(
                    64, 64, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
                renderTarget.Render(drawingVisual);

                // Convert to BitmapImage
                var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
                encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(renderTarget));

                var memoryStream = new System.IO.MemoryStream();
                encoder.Save(memoryStream);
                memoryStream.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                memoryStream.Close();

                // Set the placeholder image
                setImage(bitmapImage);
            }
            catch (Exception ex)
            {
                OnDiagLog?.Invoke($"Error creating placeholder: {ex.Message}");
            }
        }
    }
}

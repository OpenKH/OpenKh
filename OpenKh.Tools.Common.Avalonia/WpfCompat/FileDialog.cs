// Compatibility reimplementation of Xe.Tools.Wpf.Dialogs.FileDialog and its
// filter helpers (from the XeEngine.Tools.Public submodule) on top of
// Avalonia's IStorageProvider. Declared in the same namespace so shared
// sources compile unchanged; only Avalonia executables reference this
// assembly.
//
// Like the WPF original, the On* methods are synchronous: the callback has
// already run (or not) by the time they return. This is implemented by
// pumping a nested dispatcher frame while the async picker is open.

using Avalonia.Platform.Storage;
using OpenKh.Tools.Common.Avalonia.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace Xe.Tools.Wpf.Dialogs
{
    public class FileDialogFilter
    {
        public string Name { get; }
        public string[] Patterns { get; }

        private FileDialogFilter(string name, IEnumerable<string> extensions)
        {
            Name = name;
            Patterns = extensions.ToArray();
        }

        public override string ToString() => $"{Name}|{string.Join(";", Patterns)}";

        internal FilePickerFileType ToFilePickerFileType() =>
            new FilePickerFileType(Name)
            {
                Patterns = Patterns
                    .Select(x => x.StartsWith("*") ? x : $"*.{x}")
                    .ToArray(),
            };

        public static FileDialogFilter ByAllFiles(string name = "All files") => ByPatterns(name, "*");
        public static FileDialogFilter ByExtensions(string name, params string[] extensions) => ByExtensions(name, extensions.AsEnumerable());
        public static FileDialogFilter ByExtensions(string name, IEnumerable<string> extensions) =>
            ByPatterns(name, extensions.Select(x => $"*.{x}"));
        public static FileDialogFilter ByPatterns(string name, params string[] patterns) => ByExtensions(name, patterns.AsEnumerable());
        public static FileDialogFilter ByPatterns(string name, IEnumerable<string> patterns) => new FileDialogFilter(name, patterns);
    }

    public static class FileDialogFilterComposer
    {
        public static List<FileDialogFilter> Compose() => new List<FileDialogFilter>();
        public static List<FileDialogFilter> AddAllFiles(this List<FileDialogFilter> filters, string name = "All files")
        {
            filters.Add(FileDialogFilter.ByAllFiles(name));
            return filters;
        }
        public static List<FileDialogFilter> AddExtensions(this List<FileDialogFilter> filters, string name, params string[] extensions)
        {
            filters.Add(FileDialogFilter.ByExtensions(name, extensions));
            return filters;
        }
        public static List<FileDialogFilter> AddExtensions(this List<FileDialogFilter> filters, string name, IEnumerable<string> extensions)
        {
            filters.Add(FileDialogFilter.ByExtensions(name, extensions));
            return filters;
        }
        public static List<FileDialogFilter> AddPatterns(this List<FileDialogFilter> filters, string name, params string[] patterns)
        {
            filters.Add(FileDialogFilter.ByPatterns(name, patterns));
            return filters;
        }
        public static List<FileDialogFilter> AddPatterns(this List<FileDialogFilter> filters, string name, IEnumerable<string> patterns)
        {
            filters.Add(FileDialogFilter.ByPatterns(name, patterns));
            return filters;
        }
    }

    public static class FileDialog
    {
        public static bool? OnOpen(
            Action<string> callback,
            IEnumerable<FileDialogFilter> filters = null,
            string defaultFileName = null,
            AvaloniaWindow parent = null)
        {
            var files = ShowAndWait(parent, provider => provider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                AllowMultiple = false,
                FileTypeFilter = ToFileTypes(filters),
                SuggestedFileName = defaultFileName,
            }));

            var path = files?.Select(TryGetLocalPath).FirstOrDefault(x => x != null);
            if (path == null)
                return null;

            callback(path);
            return true;
        }

        public static bool? OnOpenMultiple(
            Action<string[]> callback,
            IEnumerable<FileDialogFilter> filters = null,
            string defaultFileName = null,
            AvaloniaWindow parent = null)
        {
            var files = ShowAndWait(parent, provider => provider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                AllowMultiple = true,
                FileTypeFilter = ToFileTypes(filters),
                SuggestedFileName = defaultFileName,
            }));

            var paths = files?.Select(TryGetLocalPath).Where(x => x != null).ToArray();
            if (paths == null || paths.Length == 0)
                return null;

            callback(paths);
            return true;
        }

        public static bool? OnSave(
            Action<string> callback,
            IEnumerable<FileDialogFilter> filters = null,
            string defaultFileName = null,
            AvaloniaWindow parent = null)
        {
            var file = ShowAndWaitSingle(parent, provider => provider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                FileTypeChoices = ToFileTypes(filters),
                SuggestedFileName = defaultFileName,
                ShowOverwritePrompt = true,
            }));

            var path = file != null ? TryGetLocalPath(file) : null;
            if (path == null)
                return null;

            callback(path);
            return true;
        }

        public static bool? OnFolder(
            Action<string> callback,
            string defaultFileName = null,
            AvaloniaWindow parent = null)
        {
            var window = parent ?? WindowLocator.GetActiveWindow();
            var provider = window?.StorageProvider;
            if (provider == null)
                return null;

            var folders = WindowLocator.WaitOnUIThread(PickFolderAsync(provider, defaultFileName));
            var path = folders?.Select(TryGetLocalPath).FirstOrDefault(x => x != null);
            if (path == null)
                return null;

            callback(path);
            return true;
        }

        private static async Task<IReadOnlyList<IStorageFolder>> PickFolderAsync(
            IStorageProvider provider, string defaultDirectory)
        {
            IStorageFolder startLocation = null;
            if (!string.IsNullOrEmpty(defaultDirectory))
                startLocation = await provider.TryGetFolderFromPathAsync(defaultDirectory);

            return await provider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                AllowMultiple = false,
                SuggestedStartLocation = startLocation,
            });
        }

        private static IReadOnlyList<IStorageFile> ShowAndWait(
            AvaloniaWindow parent,
            Func<IStorageProvider, Task<IReadOnlyList<IStorageFile>>> pick)
        {
            var window = parent ?? WindowLocator.GetActiveWindow();
            var provider = window?.StorageProvider;
            if (provider == null)
                return null;
            return WindowLocator.WaitOnUIThread(pick(provider));
        }

        private static IStorageFile ShowAndWaitSingle(
            AvaloniaWindow parent,
            Func<IStorageProvider, Task<IStorageFile>> pick)
        {
            var window = parent ?? WindowLocator.GetActiveWindow();
            var provider = window?.StorageProvider;
            if (provider == null)
                return null;
            return WindowLocator.WaitOnUIThread(pick(provider));
        }

        private static List<FilePickerFileType> ToFileTypes(IEnumerable<FileDialogFilter> filters) =>
            filters?.Select(x => x.ToFilePickerFileType()).ToList();

        private static string TryGetLocalPath(IStorageItem item) =>
            item?.TryGetLocalPath();
    }
}

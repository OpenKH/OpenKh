using System;

namespace OpenKh.Tools.ModsManager.Services
{
    /// <summary>
    /// Single choke point for features that only exist on Windows. The Linux
    /// (Avalonia) build hides the related UI and short-circuits the command
    /// handlers through these flags instead of removing the Windows code.
    /// </summary>
    public static class PlatformCapabilities
    {
        /// <summary>
        /// Live PCSX2 memory patching relies on kernel32 OpenProcess/
        /// ReadProcessMemory/WriteProcessMemory (see OpenKh.Tools.Common's
        /// ProcessStream).
        /// </summary>
        public static bool SupportsPcsx2Injection => OperatingSystem.IsWindows();

        /// <summary>
        /// Panacea is a native Windows DLL that gets injected into the PC
        /// releases to redirect asset loading.
        /// </summary>
        public static bool SupportsPanacea => OperatingSystem.IsWindows();

        /// <summary>
        /// There is no Epic Games Store client for Linux; neither the
        /// com.epicgames.launcher:// protocol nor the EGS manifest folders
        /// exist there.
        /// </summary>
        public static bool SupportsEpicGamesStore => OperatingSystem.IsWindows();

        /// <summary>
        /// The in-place self-updater swaps files via a generated batch script
        /// and relaunches; the AppImage build is an immutable mount, so Linux
        /// falls back to pointing the user at the releases page.
        /// </summary>
        public static bool SupportsSelfUpdate => OperatingSystem.IsWindows();
    }
}

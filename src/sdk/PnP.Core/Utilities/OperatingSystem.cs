using System.Runtime.InteropServices;

namespace PnP.Core
{
    internal static class OperatingSystem
    {
        internal static bool IsWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        internal static bool IsMacOS() => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        internal static bool IsLinux() => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    }
}

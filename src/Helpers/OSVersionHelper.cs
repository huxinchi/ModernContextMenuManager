using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernContextMenuManager.Helpers
{
    internal static class OSVersionHelper
    {
        private static bool? isWindows10_17763_OrGreater;

        public static bool IsWindows10_17763_OrGreater => isWindows10_17763_OrGreater ??= Environment.OSVersion.Version >= new Version(10, 0, 17763, 0);
    }
}

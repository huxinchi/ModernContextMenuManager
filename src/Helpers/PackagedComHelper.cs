using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernContextMenuManager.Helpers
{
    public static class PackagedComHelper
    {
        private const string SubKey_PackagedCom_Package = "SOFTWARE\\Classes\\PackagedCom\\Package\\";
        private const string SubKey_BlockedClsids = "Software\\Microsoft\\Windows\\CurrentVersion\\Shell Extensions\\Blocked";

        public static ComPackage[] GetAllComPackages()
        {
            try
            {
                using (var subKey = Registry.LocalMachine.OpenSubKey(SubKey_PackagedCom_Package, false))
                {
                    if (subKey != null)
                    {
                        var names = subKey.GetSubKeyNames();
                        if (names.Length > 0)
                        {
                            var list = new List<ComPackage>(names.Length);
                            for (int i = 0; i < names.Length; i++)
                            {
                                using (var subKey2 = subKey.OpenSubKey($"{names[i]}\\Class", false))
                                {
                                    if (subKey2 != null)
                                    {
                                        var names2 = subKey2.GetSubKeyNames();
                                        var list2 = new List<ComPackageComInfo>(names2.Length);
                                        for (int j = 0; j < names2.Length; j++)
                                        {
                                            if (Guid.TryParse(names2[j], out var clsid))
                                            {
                                                using (var subKey3 = subKey2.OpenSubKey(names2[j], false))
                                                {
                                                    if (subKey3 != null)
                                                    {
                                                        var dllPath = (string)subKey3.GetValue("DllPath", "");
                                                        if (!string.IsNullOrEmpty(dllPath) && dllPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                                                        {
                                                            var serverId = (int)subKey3.GetValue("ServerId", 0);
                                                            var threading = (int)subKey3.GetValue("Threading", 0);
                                                            list2.Add(new ComPackageComInfo(clsid, dllPath, threading switch
                                                            {
                                                                0 => ApartmentState.STA,
                                                                1 => ApartmentState.MTA,
                                                                _ => ApartmentState.Unknown
                                                            }));
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        list.Add(new ComPackage(names[i], [.. list2]));
                                    }
                                }
                            }

                            return [.. list];
                        }
                    }
                }
            }
            catch { }
            return [];
        }

        public static BlockedClsid[] GetBlockedClsids()
        {
            List<BlockedClsid>? list = null;
            try
            {
                using (var subKey = Registry.LocalMachine.OpenSubKey(SubKey_BlockedClsids, false))
                {
                    if (subKey != null)
                    {
                        var names = subKey.GetValueNames();
                        for (int i = 0; i < names.Length; i++)
                        {
                            if (Guid.TryParse(names[i], out var clsid))
                            {
                                if (list == null) list = new List<BlockedClsid>(names.Length * 2);
                                list.Add(new BlockedClsid(clsid, BlockedClsidType.LocalMachine));
                            }
                        }
                    }
                }
            }
            catch { }
            try
            {
                using (var subKey = Registry.CurrentUser.OpenSubKey(SubKey_BlockedClsids, false))
                {
                    if (subKey != null)
                    {
                        var names = subKey.GetValueNames();
                        for (int i = 0; i < names.Length; i++)
                        {
                            if (Guid.TryParse(names[i], out var clsid))
                            {
                                if (list == null) list = new List<BlockedClsid>(names.Length * 2);
                                list.Add(new BlockedClsid(clsid, BlockedClsidType.CurrentUser));
                            }
                        }
                    }
                }
            }
            catch { }

            if (list != null)
            {
                return [.. list.Distinct()];
            }

            return [];
        }

        public static bool SetBlockedClsid(Guid clsid, BlockedClsidType type, bool blocked)
        {
            try
            {
                RegistryKey rootKey = type switch
                {
                    BlockedClsidType.LocalMachine => Registry.LocalMachine,
                    _ => Registry.CurrentUser
                };

                using (var subKey = rootKey.CreateSubKey(SubKey_BlockedClsids, true))
                {
                    var name = clsid.ToString("B").ToUpperInvariant();
                    if (blocked)
                    {
                        var oldValue = subKey.GetValue(name);
                        if (oldValue is null)
                        {
                            subKey.SetValue(name, "Blocked by ContextMenuManager");
                        }
                        return true;
                    }
                    else
                    {
                        subKey.DeleteValue(name);
                        return true;
                    }
                }
            }
            catch { }

            return false;
        }

        public record struct ComPackage(string PackageFullName, ComPackageComInfo[] Clsids);

        public record struct ComPackageComInfo(Guid Clsid, string DllPath, ApartmentState ThreadingMode);

        public record struct BlockedClsid(Guid Clsid, BlockedClsidType Type);

        public enum BlockedClsidType
        {
            CurrentUser,
            LocalMachine
        }
    }
}

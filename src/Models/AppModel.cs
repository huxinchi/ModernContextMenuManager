using ModernContextMenuManager.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernContextMenuManager.Models
{
    public class AppModel
    {
        public AppModel(
            AppInfo appInfo,
            PackageInfo packageInfo,
            Dictionary<Guid, PackagedComHelper.BlockedClsidType> blockedClsids)
        {
            AppInfo = appInfo;
            PackageInfo = packageInfo;
            Clsids = appInfo.ContextMenuGuids.Select(c =>
            {
                if (blockedClsids.TryGetValue(c, out var blockedClsidType))
                {
                    return new ClsidCheckModel(c, false, blockedClsidType != PackagedComHelper.BlockedClsidType.LocalMachine);
                }
                return new ClsidCheckModel(c, true, true);
            }).ToArray();

            if (string.IsNullOrEmpty(AppInfo.DisplayName))
            {
                DisplayName = $"[{PackageInfo.PackageFamilyName}]";
            }
            else
            {
                DisplayName = $"{AppInfo.DisplayName} [{PackageInfo.PackageFamilyName}]";
            }
        }

        public AppInfo AppInfo { get; }

        public PackageInfo PackageInfo { get; }

        public IReadOnlyList<ClsidCheckModel> Clsids { get; }

        public string DisplayName { get; }
    }
}

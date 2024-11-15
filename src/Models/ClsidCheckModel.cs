using ModernContextMenuManager.Base;
using ModernContextMenuManager.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernContextMenuManager.Models
{
    public partial class ClsidCheckModel : ObservableObject
    {
        private bool enabled;

        public ClsidCheckModel(Guid clsid, bool enabled, bool canModify)
        {
            Clsid = clsid;
            CanModify = canModify;
            this.enabled = enabled;
        }

        public Guid Clsid { get; }

        public bool CanModify { get; }

        public bool Enabled
        {
            get => enabled;
            set => SetProperty(ref enabled, value,
                onPropertyChanging: (oldValue, newValue) =>
                    PackagedComHelper.SetBlockedClsid(Clsid, PackagedComHelper.BlockedClsidType.CurrentUser, !newValue),
                notifyWhenNotChanged: true,
                asyncNotifyWhenNotChanged: true);
        }
    }
}

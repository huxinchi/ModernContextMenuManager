using ModernContextMenuManager.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernContextMenuManager.Models
{
    public class ClsidCheckModel
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
            set
            {
                var oldValue = enabled;
                enabled = value;
                if (!PackagedComHelper.SetBlockedClsid(Clsid, PackagedComHelper.BlockedClsidType.CurrentUser, !enabled))
                {
                    enabled = oldValue;
                }
            }
        }
    }
}

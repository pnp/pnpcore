using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    [SharePointType("SP.BasePermissions", Target = typeof(Web), Uri = "_api/Web/EffectiveBasePermissions)")]
    internal partial class BasePermissions : BaseDataModel<IBasePermissions>, IBasePermissions
    {
        #region Properties
        public long Low { get => GetValue<long>(); set => SetValue(value); }
        public long High { get => GetValue<long>(); set => SetValue(value); }

        [KeyProperty(nameof(High))]
        public override object Key { get => High; set => High = long.Parse(value.ToString()); }
        #endregion
        
        #region Extension methods
        public bool Has(PermissionKind perm)
        {
            switch (perm)
            {
                case PermissionKind.EmptyMask:
                    return true;
                case PermissionKind.FullMask:
                    if ((High & 0x7FFF) == 32767)
                    {
                        return Low == 65535;
                    }
                    return false;
                default:
                    {
                        int num = (int)(perm - 1);
                        uint num2 = 1u;
                        if (num >= 0 && num < 32)
                        {
                            num2 <<= num;
                            return 0 != (Low & num2);
                        }
                        if (num >= 32 && num < 64)
                        {
                            num2 <<= num - 32;
                            return 0 != (High & num2);
                        }
                        return false;
                    }
            }
        }

        public void Clear(PermissionKind permissionKind)
        {
            int num = (int)permissionKind;
            num--;
            uint num2 = 1;
            if (num >= 0 && num < 32)
            {
                num2 <<= num;
                num2 = ~num2;
                Low &= num2;
            }
            else if (num >= 32 && num < 64)
            {
                num2 <<= num - 32;
                num2 = ~num2;
                High &= num2;
            }
        }

        public void ClearAll()
        {
            High = 0;
            Low = 0;
        }

        public void Set(PermissionKind permissionKind)
        {
            if(!IsPropertyAvailable(l => l.Low) && !IsPropertyAvailable(l => l.High))
            {
                Low = 0;
                High = 0;
            }
            switch (permissionKind)
            {
                case PermissionKind.FullMask:
                    Low = 65535;
                    High = 32767;
                    return;
                case PermissionKind.EmptyMask:
                    Low = 0;
                    High = 0;
                    return;
            }
            int num = (int)permissionKind;
            num--;
            uint num2 = 1;
            if (num >= 0 && num < 32)
            {
                num2 <<= num;
                Low = Low | num2;
            }
            else if (num >= 32 && num < 64)
            {
                num2 <<= num - 32;
                High = High | num2;
            }
        }


        public bool HasPermissions(uint high, uint low)
        {
            if ((High & high) == high)
            {
                return (Low & low) == low;
            }
            return false;
        }
        #endregion

        public BasePermissions()
        {
            this.ClearAll(); // initialize the values
        }
    }
}

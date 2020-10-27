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

        public bool HasPermissions(uint high, uint low)
        {
            if ((High & high) == high)
            {
                return (Low & low) == low;
            }
            return false;
        }
        #endregion
    }
}

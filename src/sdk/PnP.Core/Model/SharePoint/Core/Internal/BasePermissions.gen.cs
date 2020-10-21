namespace PnP.Core.Model.SharePoint
{
    internal partial class BasePermissions : BaseDataModel<IBasePermissions>, IBasePermissions
    {
        public long Low { get => GetValue<long>(); set => SetValue(value); }
        public long High { get => GetValue<long>(); set => SetValue(value); }

        [KeyProperty("High")]
        public override object Key { get => this.High; set => this.High = long.Parse(value.ToString()); }
    }
}

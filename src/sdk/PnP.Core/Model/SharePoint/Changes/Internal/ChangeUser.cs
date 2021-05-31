namespace PnP.Core.Model.SharePoint
{
    [SharePointType("SP.ChangeUser")]
    internal partial class ChangeUser : Change, IChangeUser
    {
        public bool Activate { get => GetValue<bool>(); set => SetValue(value); }
        public int UserId { get => GetValue<int>(); set => SetValue(value); }
    }
}
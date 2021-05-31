namespace PnP.Core.Model.SharePoint
{
    [SharePointType("SP.ChangeGroup")]
    internal partial class ChangeGroup : Change, IChangeGroup
    {
        public int GroupId { get => GetValue<int>(); set => SetValue(value); }
    }
}
namespace PnP.Core.Model.SharePoint
{
    internal partial class ListItem : ExpandoBaseDataModel<IListItem>, IListItem
    {
        [SharePointProperty("Id", IsKey = true)]
        [GraphProperty("id", IsKey = true)]
        public int Id { get => GetValue<int>(); set => SetValue(value); }

        public bool CommentsDisabled { get => GetValue<bool>(); set => SetValue(value); }
        
        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public override object Key { get => this.Id; set => this.Id = (int)value; }
    }
}

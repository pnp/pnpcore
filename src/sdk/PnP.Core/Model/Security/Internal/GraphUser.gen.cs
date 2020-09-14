namespace PnP.Core.Model.Security
{
    internal partial class GraphUser : BaseDataModel<IGraphUser>, IGraphUser
    {
        public string Id { get => GetValue<string>(); set => SetValue(value); }

        public string UserPrincipalName { get => GetValue<string>(); set => SetValue(value); }

        public string OfficeLocation { get => GetValue<string>(); set => SetValue(value); }

        public string Mail { get => GetValue<string>(); set => SetValue(value); }

        [KeyProperty("Id")]
        public override object Key { get => this.Id; set => this.Id = value.ToString(); }
    }
}

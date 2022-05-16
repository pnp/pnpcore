namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal sealed class GraphPhysicalAddress : BaseDataModel<IGraphPhysicalAddress>, IGraphPhysicalAddress
    {
        #region Construction
        public GraphPhysicalAddress()
        {
        }
        #endregion

        #region Properties

        public string City { get => GetValue<string>(); set => SetValue(value); }
        public string CountryOrRegion { get => GetValue<string>(); set => SetValue(value); }
        public string PostalCode { get => GetValue<string>(); set => SetValue(value); }
        public string State { get => GetValue<string>(); set => SetValue(value); }
        public string Street { get => GetValue<string>(); set => SetValue(value); }

        #endregion
    }
}

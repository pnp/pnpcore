namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal sealed class GraphLocation : BaseDataModel<IGraphLocation>, IGraphLocation
    {
        #region Construction
        public GraphLocation()
        {
        }
        #endregion

        #region Properties

        public IGraphPhysicalAddress Address { get => GetModelValue<IGraphPhysicalAddress>(); set => SetModelValue(value); }

        public IGraphOutlookGeoCoordinates Coordinates { get => GetModelValue<IGraphOutlookGeoCoordinates>(); set => SetModelValue(value); }

        public string DisplayName { get => GetValue<string>(); set => SetValue(value); }

        public string LocationEmailAddress { get => GetValue<string>(); set => SetValue(value); }

        public string LocationUri { get => GetValue<string>(); set => SetValue(value); }

        public EventLocationType LocationType { get => GetValue<EventLocationType>(); set => SetValue(value); }

        public string UniqueId { get => GetValue<string>(); set => SetValue(value); }

        public string UniqueIdType { get => GetValue<string>(); set => SetValue(value); }

        #endregion
    }
}

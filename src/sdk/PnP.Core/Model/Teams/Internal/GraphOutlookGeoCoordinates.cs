namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal sealed class GraphOutlookGeoCoordinates : BaseDataModel<IGraphOutlookGeoCoordinates>, IGraphOutlookGeoCoordinates
    {
        #region Construction
        public GraphOutlookGeoCoordinates()
        {
        }
        #endregion

        #region Properties

        public double Accuracy { get => GetValue<double>(); set => SetValue(value); }

        public double Altitude { get => GetValue<double>(); set => SetValue(value); }

        public double AltitudeAccuracy { get => GetValue<double>(); set => SetValue(value); }

        public double Latitude { get => GetValue<double>(); set => SetValue(value); }

        public double Longitude { get => GetValue<double>(); set => SetValue(value); }

        #endregion
    }
}

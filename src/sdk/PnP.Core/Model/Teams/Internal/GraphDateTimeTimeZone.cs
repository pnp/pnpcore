namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal sealed class GraphDateTimeTimeZone : BaseDataModel<IGraphDateTimeTimeZone>, IGraphDateTimeTimeZone
    {
        #region Construction
        public GraphDateTimeTimeZone()
        {
        }
        #endregion

        #region Properties

        public string DateTime { get => GetValue<string>(); set => SetValue(value); }

        public string TimeZone { get => GetValue<string>(); set => SetValue(value); }

        #endregion
    }
}

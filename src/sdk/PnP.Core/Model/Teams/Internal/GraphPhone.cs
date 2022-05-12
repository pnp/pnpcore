namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal sealed class GraphPhone : BaseDataModel<IGraphPhone>, IGraphPhone
    {
        #region Construction
        public GraphPhone()
        {
        }
        #endregion

        #region Properties

        public string Number { get => GetValue<string>(); set => SetValue(value); }
        public EventPhoneType Type { get => GetValue<EventPhoneType>(); set => SetValue(value); }

        #endregion
    }
}

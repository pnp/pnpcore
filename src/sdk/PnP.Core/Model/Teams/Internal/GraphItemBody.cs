namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal sealed class GraphItemBody : BaseDataModel<IGraphItemBody>, IGraphItemBody
    {
        #region Construction
        public GraphItemBody()
        {
        }
        #endregion

        #region Properties
        public string Content { get => GetValue<string>(); set => SetValue(value); }

        public EventBodyType ContentType { get => GetValue<EventBodyType>(); set => SetValue(value); }
        #endregion
    }
}

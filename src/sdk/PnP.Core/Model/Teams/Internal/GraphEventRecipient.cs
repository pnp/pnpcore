namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal sealed class GraphRecipient : BaseDataModel<IGraphRecipient>, IGraphRecipient
    {
        #region Construction
        public GraphRecipient()
        {
        }
        #endregion

        #region Properties

        public IGraphEmailAddress EmailAddress { get => GetModelValue<IGraphEmailAddress>(); set => SetValue(value); }

        #endregion
    }
}

namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal sealed class GraphTimeSlot : BaseDataModel<IGraphTimeSlot>, IGraphTimeSlot
    {
        #region Construction
        public GraphTimeSlot()
        {
        }
        #endregion

        #region Properties

        public string Address { get => GetValue<string>(); set => SetValue(value); }
        public string Name { get => GetValue<string>(); set => SetValue(value); }

        #endregion
    }
}

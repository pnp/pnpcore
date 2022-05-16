namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal sealed class GraphPatternedRecurrence : BaseDataModel<IGraphPatternedRecurrence>, IGraphPatternedRecurrence
    {
        #region Construction
        public GraphPatternedRecurrence()
        {
        }
        #endregion

        #region Properties
        public IGraphRecurrencePattern Pattern { get => GetModelValue<IGraphRecurrencePattern>(); set => SetModelValue(value); }

        public IGraphRecurrenceRange Range { get => GetModelValue<IGraphRecurrenceRange>(); set => SetModelValue(value); }

        #endregion
    }
}

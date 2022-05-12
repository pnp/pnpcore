namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal sealed class GraphEventAttendee : BaseDataModel<IGraphEventAttendee>, IGraphEventAttendee
    {
        #region Construction
        public GraphEventAttendee()
        {
        }
        #endregion

        #region Properties
        public IGraphEmailAddress EmailAddress { get => GetModelValue<IGraphEmailAddress>(); set => SetModelValue(value); }

        public IGraphEventResponseStatus ResponseStatus { get => GetModelValue<IGraphEventResponseStatus>(); set => SetModelValue(value); }

        public IGraphTimeSlot TimeSlot { get => GetModelValue<IGraphTimeSlot>(); set => SetModelValue(value); }

        public EventAttendeeType Type { get => GetValue<EventAttendeeType>(); set => SetValue(value); }
        #endregion
    }
}

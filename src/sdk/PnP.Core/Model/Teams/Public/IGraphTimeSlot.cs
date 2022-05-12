namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Represents a time slot for a meeting.
    /// </summary>
    [ConcreteType(typeof(GraphTimeSlot))]
    public interface IGraphTimeSlot : IDataModel<IGraphTimeSlot>
    {
        /// <summary>
        /// The date, time, and time zone that a period ends.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// The date, time, and time zone that a period begins.
        /// </summary>
        public string Name { get; set; }
    }
}

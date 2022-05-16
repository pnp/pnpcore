namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Public interface to define a collection of attendees for an event
    /// </summary>
    [ConcreteType(typeof(GraphEventAttendeeCollection))]
    public interface IGraphEventAttendeeCollection : IDataModelCollection<IGraphEventAttendee>
    {
    }
}

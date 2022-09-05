using System.Collections.Generic;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Details for an attendee to join the meeting online.
    /// </summary>
    [ConcreteType(typeof(GraphOnlineMeetingInfo))]
    public interface IGraphOnlineMeetingInfo : IDataModel<IGraphOnlineMeetingInfo>
    {
        /// <summary>
        /// The ID of the conference.
        /// </summary>
        public string ConferenceId { get; }

        /// <summary>
        /// The external link that launches the online meeting. This is a URL that clients will launch into a browser and will redirect the user to join the meeting.
        /// </summary>
        public string JoinUrl { get; }

        /// <summary>
        /// All of the phone numbers associated with this conference.
        /// </summary>
        public IGraphPhoneCollection Phones { get; }

        /// <summary>
        /// The pre-formatted quickdial for this call.
        /// </summary>
        public string QuickDial { get; }

        /// <summary>
        /// The toll free numbers that can be used to join the conference.
        /// </summary>
        public List<string> TollFreeNumbers { get; }

        /// <summary>
        /// The toll number that can be used to join the conference.
        /// </summary>
        public string TollNumber { get; }

    }
}

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Defines the type of event
    /// </summary>
    public enum EventType
    {
        /// <summary>
        /// Event will only take place once
        /// </summary>
        SingleInstance,

        /// <summary>
        /// Event is of type Occurence and will repeat
        /// </summary>
        Occurrence,

        /// <summary>
        /// Exception
        /// </summary>
        Exception,

        /// <summary>
        /// SeriesMaster
        /// </summary>
        SeriesMaster
    }
}

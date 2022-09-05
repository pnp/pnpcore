namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Represents a phone number.
    /// </summary>
    [ConcreteType(typeof(GraphPhone))]
    public interface IGraphPhone : IDataModel<IGraphPhone>
    {
        /// <summary>
        /// The phone number.
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// The type of phone number.
        /// </summary>
        public EventPhoneType Type { get; set; }
    }
}

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// The name and email address of a contact or message recipient.
    /// </summary>
    [ConcreteType(typeof(GraphEmailAddress))]
    public interface IGraphEmailAddress : IDataModel<IGraphEmailAddress>
    {
        /// <summary>
        /// The email address of the person or entity.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// The display name of the person or entity.
        /// </summary>
        public string Name { get; set; }
    }
}

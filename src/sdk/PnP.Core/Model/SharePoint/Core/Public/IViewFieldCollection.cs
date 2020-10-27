namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a ViewFieldCollection object
    /// </summary>
    [ConcreteType(typeof(ViewFieldCollection))]
    public interface IViewFieldCollection : IDataModel<IViewFieldCollection>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string SchemaXml { get; set; }

        #endregion

    }
}

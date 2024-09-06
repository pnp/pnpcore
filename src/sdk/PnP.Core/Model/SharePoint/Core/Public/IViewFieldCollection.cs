using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// View fields model
    /// </summary>
    [ConcreteType(typeof(ViewFieldCollection))]
    public interface IViewFieldCollection : IDataModel<IViewFieldCollection>, IDataModelCollectionLoad<IViewFieldCollection>, ISupportModules<IViewFieldCollection>
    {
        /// <summary>
        /// List view fields
        /// </summary>
        public System.Collections.Generic.List<string> Items { get; }

        /// <summary>
        /// Listview fields schema xml
        /// </summary>
        public string SchemaXml { get; }
    }
}

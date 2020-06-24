using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents a Feature in SharePoint Online
    /// </summary>
    [ConcreteType(typeof(Feature))]
    public interface IFeature : IDataModel<IFeature>
    {
        /// <summary>
        /// The ID of the Feature
        /// </summary>
        public Guid DefinitionId { get; }

        /// <summary>
        /// The name of the feature
        /// </summary>
        public string DisplayName { get; }
    }
}

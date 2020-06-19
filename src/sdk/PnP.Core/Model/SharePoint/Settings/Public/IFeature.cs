using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// 
    /// </summary>
    [ConcreteType(typeof(Feature))]
    public interface IFeature : IDataModel<IFeature>
    {
        /// <summary>
        /// The ID of the Feature
        /// </summary>
        public Guid DefinitionId { get; }

        public string DisplayName { get; }
    }
}

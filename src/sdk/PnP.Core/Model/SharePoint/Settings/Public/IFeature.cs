using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// 
    /// </summary>
    [ConcreteType(typeof(Feature))]
    public interface IFeature : IDataModel<IFeature>,
        IDataModelGet,
        IDataModelDelete
    {
        /// <summary>
        /// The ID of the Feature
        /// </summary>
        public Guid Id { get; }
    
    }
}

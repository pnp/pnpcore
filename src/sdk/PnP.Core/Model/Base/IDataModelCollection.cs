using System.Collections.Generic;

namespace PnP.Core.Model
{
    /// <summary>
    /// Defines the very basic interface for every collection of Domain Model objects
    /// </summary>
    /// <typeparam name="TModel">The actual type of the Domain Model objects</typeparam>
    public interface IDataModelCollection<TModel> : IEnumerable<TModel>, IDataModelParent, IDataModelWithContext, IRequestableCollection
    {
       
    }
}

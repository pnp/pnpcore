using PnP.Core.Model.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Model.Me
{
    /// <summary>
    /// 
    /// </summary>
    [ConcreteType(typeof(ChatCollection))]
    public interface IChatCollection : IQueryable<IChat>, IAsyncEnumerable<IChat>, IDataModelCollection<IChat>, IDataModelCollectionLoad<IChat>, IDataModelCollectionDeleteByGuidId, ISupportModules<IChat>
    {

        /// <summary>
        /// Create a new chat between users
        /// </summary>
        /// <param name="chatOptions"></param>
        /// <returns></returns>
        public Task<IChat> AddAsync(ChatOptions chatOptions);

        /// <summary>
        /// Create a new chat between users
        /// </summary>
        /// <param name="chatOptions"></param>
        /// <returns></returns>
        public IChat Add(ChatOptions chatOptions);
    }
}

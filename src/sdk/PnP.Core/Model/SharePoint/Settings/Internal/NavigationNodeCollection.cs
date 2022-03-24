using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class NavigationNodeCollection : QueryableDataModelCollection<INavigationNode>, INavigationNodeCollection
    {
        public NavigationNodeCollection(PnPContext context, IDataModelParent parent, string memberName = null)
           : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }

        #region Get Methods
        public INavigationNode GetById(int id, params Expression<Func<INavigationNode, object>>[] selectors)
        {
            return GetByIdAsync(id, selectors).GetAwaiter().GetResult();
        }

        public async Task<INavigationNode> GetByIdAsync(int id, params Expression<Func<INavigationNode, object>>[] selectors)
        {
            return await this.QueryProperties(selectors).FirstOrDefaultAsync(n => n.Id == id).ConfigureAwait(false);
        }

        public List<INavigationNode> GetByTitle(string title, params Expression<Func<INavigationNode, object>>[] selectors)
        {
            return GetByTitleAsync(title, selectors).GetAwaiter().GetResult();
        }

        public async Task<List<INavigationNode>> GetByTitleAsync(string title, params Expression<Func<INavigationNode, object>>[] selectors)
        {
            return await this.QueryProperties(selectors).Where(n => n.Title == title).ToListAsync().ConfigureAwait(false);
        }
        #endregion

        #region Add Methods

        public INavigationNode Add(NavigationNodeOptions navigationNodeOptions)
        {
            return AddAsync(navigationNodeOptions).GetAwaiter().GetResult();
        }

        public async Task<INavigationNode> AddAsync(NavigationNodeOptions navigationNodeOptions)
        {
            if (navigationNodeOptions == null)
            {
                throw new ArgumentNullException(nameof(navigationNodeOptions));
            }

            var newNavigationNode = CreateNewAndAdd() as NavigationNode;

            // options as arguments for the add method
            var additionalInfo = new Dictionary<string, object>()
            {
                { NavigationNode.NavigationNodeOptionsAdditionalInformationKey, navigationNodeOptions}
            };

            return await newNavigationNode.AddAsync(additionalInfo).ConfigureAwait(false) as NavigationNode;
        }

        #endregion
    }
}

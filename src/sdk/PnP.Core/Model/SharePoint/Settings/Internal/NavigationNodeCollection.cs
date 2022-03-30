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
        public NavigationType NavigationType { get; set; }

        public NavigationNodeCollection(PnPContext context, IDataModelParent parent, string memberName = null)
           : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
            Enum.TryParse(memberName, out NavigationType tempNavigationType);
            NavigationType = tempNavigationType;
        }

        #region Get Methods
        public INavigationNode GetById(int id, params Expression<Func<INavigationNode, object>>[] selectors)
        {
            return GetByIdAsync(id, selectors).GetAwaiter().GetResult();
        }

        public async Task<INavigationNode> GetByIdAsync(int id, params Expression<Func<INavigationNode, object>>[] selectors)
        {
            var apiCall = new ApiCall($"_api/web/navigation/GetNodeById('{id}')", ApiType.SPORest);
            return await BaseDataModelExtensions.BaseGetAsync(this, apiCall, selectors).ConfigureAwait(false);
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
                { NavigationNode.NavigationNodeOptionsAdditionalInformationKey, navigationNodeOptions },
                { NavigationNode.NavigationTypeKey, NavigationType}
            };

            return await newNavigationNode.AddAsync(additionalInfo).ConfigureAwait(false) as NavigationNode;
        }

        #endregion

        #region Delete Methods

        public void DeleteAllNodes()
        {
            DeleteAllNodesAsync().GetAwaiter().GetResult();
        }

        public async Task DeleteAllNodesAsync()
        {
            if (NavigationType == NavigationType.TopNavigationBar)
                await PnPContext.Web.Navigation.LoadAsync(p => p.TopNavigationBar).ConfigureAwait(false);
            else if (NavigationType == NavigationType.QuickLaunch)
                await PnPContext.Web.Navigation.LoadAsync(p => p.QuickLaunch).ConfigureAwait(false);

            foreach (var item in items)
                await item.DeleteBatchAsync().ConfigureAwait(false);

            await PnPContext.ExecuteAsync().ConfigureAwait(false);

        }

        #endregion
    }
}

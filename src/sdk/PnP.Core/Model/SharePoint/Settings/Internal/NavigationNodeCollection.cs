using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text.Json;
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
            _ = Enum.TryParse(memberName, out NavigationType tempNavigationType);
            NavigationType = tempNavigationType;
        }

        #region Get Methods
        public INavigationNode GetById(int id, params Expression<Func<INavigationNode, object>>[] selectors)
        {
            return GetByIdAsync(id, selectors).GetAwaiter().GetResult();
        }

        public async Task<INavigationNode> GetByIdAsync(int id, params Expression<Func<INavigationNode, object>>[] selectors)
        {
            var apiCall = new ApiCall($"{NavigationConstants.NavigationUri}/GetNodeById('{id}')", ApiType.SPORest);
            var navigationNode =  await BaseDataModelExtensions.BaseGetAsync(this, apiCall, selectors).ConfigureAwait(false) as NavigationNode;

            if (!navigationNode.Metadata.ContainsKey(PnPConstants.MetaDataRestId))
            {
                return null;
            }

            return navigationNode;
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

            if (navigationNodeOptions.Title == null)
            {
                throw new ArgumentNullException($"{nameof(navigationNodeOptions)}.{nameof(navigationNodeOptions.Title)}");
            }

            if (navigationNodeOptions.Url == null)
            {
                throw new ArgumentNullException($"{nameof(navigationNodeOptions)}.{nameof(navigationNodeOptions.Url)}");
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
            {
                await PnPContext.Web.Navigation.LoadAsync(p => p.TopNavigationBar).ConfigureAwait(false);
            }
            else if (NavigationType == NavigationType.QuickLaunch)
            {
                await PnPContext.Web.Navigation.LoadAsync(p => p.QuickLaunch).ConfigureAwait(false);
            }

            foreach (var item in items.ToList())
            {
                await item.DeleteAsync().ConfigureAwait(false);
            }
        }

        public void DeleteAllNodesBatch(Batch batch)
        {
            DeleteAllNodesBatchAsync(batch).GetAwaiter().GetResult();
        }

        public async Task DeleteAllNodesBatchAsync(Batch batch)
        {
            if (NavigationType == NavigationType.TopNavigationBar)
            {
                await PnPContext.Web.Navigation.LoadAsync(p => p.TopNavigationBar).ConfigureAwait(false);
            }
            else if (NavigationType == NavigationType.QuickLaunch)
            {
                await PnPContext.Web.Navigation.LoadAsync(p => p.QuickLaunch).ConfigureAwait(false);
            }

            foreach (var item in items.ToList())
            {
                await item.DeleteBatchAsync(batch).ConfigureAwait(false);
            }
        }

        public void DeleteAllNodesBatch()
        {
            DeleteAllNodesBatchAsync().GetAwaiter().GetResult();
        }

        public async Task DeleteAllNodesBatchAsync()
        {
            await DeleteAllNodesBatchAsync(PnPContext.CurrentBatch).ConfigureAwait(false);
        }
        #endregion

        #region Extension Methods

        public void MoveNodeAfter(INavigationNode nodeToMove, INavigationNode nodeToMoveAfter)
        {
            MoveNodeAfterAsync(nodeToMove, nodeToMoveAfter).GetAwaiter().GetResult();
        }

        public async Task MoveNodeAfterAsync(INavigationNode nodeToMove, INavigationNode nodeToMoveAfter)
        {
            var apiUrl = NavigationConstants.NavigationUri;
            if (NavigationType == NavigationType.QuickLaunch)
            {
                apiUrl += NavigationConstants.QuickLaunchUri;
            }
            else if (NavigationType == NavigationType.TopNavigationBar)
            {
                apiUrl += NavigationConstants.TopNavigationBarUri;
            }

            // Build body
            var requestBody = new
            {
                nodeId = nodeToMove.Id,
                previousNodeId = nodeToMoveAfter.Id
            }.AsExpando();
            string body = JsonSerializer.Serialize(requestBody, typeof(ExpandoObject), PnPConstants.JsonSerializer_IgnoreNullValues);

            var apiCall = new ApiCall(apiUrl + "/MoveAfter", ApiType.SPORest, body);

            var navigationNode = new NavigationNode()
            {
                PnPContext = PnPContext,
                Parent = this
            };
            await navigationNode.RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        #endregion
    }
}

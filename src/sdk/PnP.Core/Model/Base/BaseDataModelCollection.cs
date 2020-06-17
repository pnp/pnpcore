using PnP.Core.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Model
{
    /// <summary>
    /// Base abstract class for every Domain Model objects collection
    /// </summary>
    internal abstract class BaseDataModelCollection<TModel> : IDataModelCollection<TModel>, IManageableCollection<TModel>, ISupportPaging, IMetadataExtensible
    {
        private const string GraphNextLink = "@odata.nextLink";
        private const string SharePointRestListItemNextLink = "__next";

        #region Core properties

        /// <summary>
        /// Dictionary to access the domain model object Metadata
        /// </summary>
        public Dictionary<string, string> Metadata { get; } = new Dictionary<string, string>();

        /// <summary>
        /// List of items in the collection
        /// </summary>
        protected List<TModel> items = new List<TModel>();

        /// <summary>
        /// Each item and each collection requires a PnPContext
        /// </summary>
        public PnPContext PnPContext { get; set; }

        /// <summary>
        /// Parent of this collection
        /// </summary>
        public IDataModelParent Parent { get; set; }

        /// <summary>
        /// Number of items in the collection
        /// </summary>
        public int Count => this.items.Count;

        /// <summary>
        /// Was this collectioned loaded with data from a server request
        /// </summary>
        public bool Requested { get; set; } = false;

        /// <summary>
        /// Number of items in the collection
        /// </summary>
        public int Length => this.items.Count;

        /// <summary>
        /// Items in the collection
        /// </summary>
        public IEnumerable RequestedItems { get => this.items; }

        #endregion

        #region ICollection methods

        /// <summary>
        /// Standard add method to add a model to the collection
        /// </summary>
        /// <param name="model">Model to add</param>
        public virtual void Add(TModel item)
        {
            // Check that the item is not null
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            items.Add(item);
            Requested = true;
        }

        public virtual void Clear()
        {
            this.items.Clear();
        }

        public virtual bool Contains(TModel item)
        {
            return this.items.Contains(item);
        }

        public virtual void CopyTo(TModel[] array, int arrayIndex)
        {
            this.items.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Returns an enumerator, will also purge deleted items
        /// </summary>
        /// <returns>Enumerator for this collection</returns>
        public virtual IEnumerator<TModel> GetEnumerator()
        {
            PurgeDeletedItems();
            return items.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator, will also purge deleted items
        /// </summary>
        /// <returns>Enumerator for this collection</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Removes a model from the collection, will be called for example by the logic that consolidates batch requests
        /// </summary>
        /// <param name="item">Model to remove</param>
        public virtual bool Remove(TModel item)
        {
            // Check that the item is not null
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            return items.Remove(item);
        }

        #endregion

        #region Items management methods

        /// <summary>
        /// Purges the items marked as deleted
        /// </summary>
        internal void PurgeDeletedItems()
        {
            foreach (var item in items.Where(p => (p as TransientObject).Deleted).ToArray())
            {
                // remove item from collection as it was purged                       
                items.Remove(item);
            }
        }

        /// <summary>
        /// Parameter less new, needs to be overriden by the collections using this base class
        /// </summary>
        /// <returns>Model entity</returns>
        public virtual TModel CreateNew()
        {
            return default;
        }

        /// <summary>
        /// Parameter less new, needs to be overriden by the collections using this base class
        /// </summary>
        /// <returns>Model entity</returns>
        object IManageableCollection.CreateNew()
        {
            return this.CreateNew();
        }

        public void Add(object item)
        {
            // Check that the item is not null
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            // And that the item is of the correct type
            if (!(item is TModel))
            {
                throw new InvalidCastException("Invalid type for object to add to the collection");
            }

            // And if it is correct, add it through the typed method
            this.Add((TModel)item);
        }

        public void AddOrUpdate(TModel newItem, Predicate<TModel> selector)
        {
            AddOrUpdateInternal(newItem, selector);
        }

        public void AddOrUpdate(object newItem, Predicate<object> selector)
        {
            this.AddOrUpdateInternal((TModel)newItem, p => selector.Invoke(p));
        }

        public void AddOrUpdateInternal(TModel newItem, Predicate<TModel> selector)
        {
            // When the collection was never requested then just add the item, no point 
            // in trying to check if the item exists as collections that were not requested
            // typically do not have the key loaded. If the key property was not loaded the FindIndex will fail
            if (!Requested)
            {
                items.Add(newItem);
            }
            else
            {
                var itemToUpdateIndex = items.FindIndex(selector);
                if (itemToUpdateIndex >= 0)
                {
                    Replace(itemToUpdateIndex, newItem);
                }
                else
                {
                    items.Add(newItem);
                }
            }
        }

        public virtual void Replace(int itemIndex, TModel newItem)
        {
            items[itemIndex] = newItem;
        }

        /// <summary>
        /// Removes a model from the collection, will be called for example by the logic that consolidates batch requests
        /// </summary>
        /// <param name="item">Model to remove</param>
        public virtual bool Remove(object item)
        {
            return this.Remove((TModel)item);
        }

        #endregion

        #region Paging

        public bool CanPage
        {
            get
            {
                return Metadata.ContainsKey(GraphNextLink) || Metadata.ContainsKey(SharePointRestListItemNextLink);
            }
        }

        public async Task GetNextPageAsync()
        {

            if (CanPage)
            {
                // Get the parent (container) entity info
                var parentEntityInfo = EntityManager.Instance.GetStaticClassInfo(this.Parent.GetType());
                // and cast it to the IDataModelMappingHandler interface
                var parentEntityWithMappingHandlers = (IDataModelMappingHandler)this.Parent;

                // Determine the receiving property
                var receivingProperty = GetReceivingProperty(parentEntityInfo);
                if (string.IsNullOrEmpty(receivingProperty))
                {
                    throw new ClientException(ErrorType.ModelMetadataIncorrect, "Receiving property could not be determined, most likely the internal implemenation is not aligned with the interface naming");
                }

                // Prepare api call
                string nextLink;
                ApiType nextLinkApiType;
                // important: the skiptoken is case sensitive, so ensure to keep it the way is was provided to you by Graph
                if (Metadata.ContainsKey(GraphNextLink) && Metadata[GraphNextLink].Contains($"/{PnPConstants.GraphBetaEndpoint}/", StringComparison.InvariantCultureIgnoreCase))
                {
                    nextLink = Metadata[GraphNextLink].Replace($"{PnPConstants.MicrosoftGraphBaseUrl}{PnPConstants.GraphBetaEndpoint}/", "");
                    nextLinkApiType = ApiType.GraphBeta;
                }
                else if (Metadata.ContainsKey(GraphNextLink) && Metadata[GraphNextLink].Contains($"/{PnPConstants.GraphV1Endpoint}/", StringComparison.InvariantCultureIgnoreCase))
                {
                    nextLink = Metadata[GraphNextLink].Replace($"{PnPConstants.MicrosoftGraphBaseUrl}{PnPConstants.GraphV1Endpoint}/", "");
                    nextLinkApiType = ApiType.Graph;
                }
                else if (!string.IsNullOrEmpty(Metadata[SharePointRestListItemNextLink]))
                {
                    nextLink = CleanUpUrlParameters(Metadata[SharePointRestListItemNextLink]);
                    nextLinkApiType = ApiType.SPORest;
                }
                else
                {
                    // SPO Rest 
                    throw new ClientException(ErrorType.Unsupported, "Not yet supported");
                }

                PnPContext.CurrentBatch.Add(
                    this.Parent as TransientObject,
                    parentEntityInfo,
                    HttpMethod.Get,
                    new ApiCall // First option is Graph
                    {
                        Type = nextLinkApiType,
                        ReceivingProperty = receivingProperty,
                        Request = nextLink
                    },
                    default,
                    parentEntityWithMappingHandlers.MappingHandler,
                    parentEntityWithMappingHandlers.PostMappingHandler
                    );

                await PnPContext.ExecuteAsync().ConfigureAwait(false);
            }
            else
            {
                throw new ClientException(ErrorType.CollectionNotLoaded, "Please ensure you load this collection once before calling GetAllPages or GetNextPage");
            }

        }

        public async Task GetAllPagesAsync()
        {
            bool loadNextPage = true;
            int currentItemCount = items.Count;
            while (loadNextPage)
            {
                await GetNextPageAsync().ConfigureAwait(false);

                // Keep requesting pages until the total item count is not increasing anymore
                if (items.Count == currentItemCount)
                {
                    loadNextPage = false;

                    // Clear the MetaData paging links to avoid loading the collection again via paging
                    if (Metadata.ContainsKey(GraphNextLink))
                    {
                        Metadata.Remove(GraphNextLink);
                    }
                    else if (Metadata.ContainsKey(SharePointRestListItemNextLink))
                    {
                        Metadata.Remove(SharePointRestListItemNextLink);
                    }
                }
                else
                {
                    currentItemCount = items.Count;
                }
            }
        }

        private string GetReceivingProperty(EntityInfo parentEntityInfo)
        {
            var internalCollectionType = this.GetType();
            var publicCollectionType = Type.GetType($"{internalCollectionType.Namespace}.I{internalCollectionType.Name}");

            var propertyInParent = parentEntityInfo.Fields.FirstOrDefault(p => p.DataType == publicCollectionType);

            return propertyInParent?.Name;
        }

        private static string CleanUpUrlParameters(string url)
        {
            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out Uri uri))
            {
                NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(uri.Query);
                if (queryString["$skiptoken"] != null && queryString["$skip"] != null)
                {
                    // $skiptoken and $skip cannot be combined, removing $skip in this case
                    queryString.Remove("$skip");
                    return $"{uri.Scheme}://{uri.DnsSafeHost}{uri.AbsolutePath}?{queryString}";
                }
            }

            return url;
        }
        #endregion
    }
}

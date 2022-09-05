using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class ContentTypeCollection : QueryableDataModelCollection<IContentType>, IContentTypeCollection
    {
        public ContentTypeCollection(PnPContext context, IDataModelParent parent, string memberName = null)
            : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }

        #region Add
        public async Task<IContentType> AddBatchAsync(string id, string name, string description = null, string group = null)
        {
            return await AddBatchAsync(PnPContext.CurrentBatch, id, name, description, group).ConfigureAwait(false);
        }

        public IContentType AddBatch(string id, string name, string description = null, string group = null)
        {
            return AddBatchAsync(id, name, description, group).GetAwaiter().GetResult();
        }

        public async Task<IContentType> AddBatchAsync(Batch batch, string id, string name, string description = null, string group = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var newContentType = CreateNewAndAdd() as ContentType;

            newContentType.StringId = id;
            newContentType.Name = name;
            newContentType.Description = description;
            newContentType.Group = group;

            return await newContentType.AddBatchAsync(batch).ConfigureAwait(false) as ContentType;
        }

        public IContentType AddBatch(Batch batch, string id, string name, string description = null, string group = null)
        {
            return AddBatchAsync(batch, id, name, description, group).GetAwaiter().GetResult();
        }

        public async Task<IContentType> AddAsync(string id, string name, string description = null, string group = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var newContentType = CreateNewAndAdd() as ContentType;

            newContentType.StringId = id;
            newContentType.Name = name;
            newContentType.Description = description;
            newContentType.Group = group;

            return await newContentType.AddAsync().ConfigureAwait(false) as ContentType;
        }

        public IContentType Add(string id, string name, string description = null, string group = null)
        {
            return AddAsync(id, name, description, group).GetAwaiter().GetResult();
        }

        #endregion

        #region AddAvailableContentType
        public async Task<IContentType> AddAvailableContentTypeBatchAsync(string id)
        {
            return await AddAvailableContentTypeBatchAsync(PnPContext.CurrentBatch, id).ConfigureAwait(false);
        }

        public IContentType AddAvailableContentTypeBatch(string id)
        {
            return AddAvailableContentTypeBatchAsync(id).GetAwaiter().GetResult();
        }

        public async Task<IContentType> AddAvailableContentTypeBatchAsync(Batch batch, string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var newContentType = CreateNewAndAdd() as ContentType;
            newContentType.StringId = id;

            return await newContentType.AddAvailableContentTypeBatchAsync(batch, id).ConfigureAwait(false) as ContentType;
        }

        public IContentType AddAvailableContentTypeBatch(Batch batch, string id)
        {
            return AddAvailableContentTypeBatchAsync(batch, id).GetAwaiter().GetResult();
        }

        public async Task<IContentType> AddAvailableContentTypeAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var newContentType = CreateNewAndAdd() as ContentType;
            newContentType.StringId = id;

            return await newContentType.AddAvailableContentTypeAsync(id).ConfigureAwait(false) as ContentType;
        }

        public IContentType AddAvailableContentType(string id)
        {
            return AddAvailableContentTypeAsync(id).GetAwaiter().GetResult();
        }

        #endregion

        #region AddAvailableContentTypeFromHub

        public async Task<ILongRunningOperation> AddAvailableContentTypeFromHubAsync(string id, AddContentTypeFromHubOptions options = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var newContentType = CreateNewAndAdd() as ContentType;
            newContentType.StringId = id;

            return await newContentType.AddAvailableContentTypeFromHubAsync(id, options).ConfigureAwait(false);
        }

        public ILongRunningOperation AddAvailableContentTypeFromHub(string id, AddContentTypeFromHubOptions options = null)
        {
            return AddAvailableContentTypeFromHubAsync(id, options).GetAwaiter().GetResult();
        }

        #endregion


        #region Document Set

        public async Task<IDocumentSet> AddDocumentSetAsync(string id, string name, string description = null, string group = null, DocumentSetOptions options = null)
        {
            if (!id.StartsWith("0x0120D520"))
            {
                throw new ArgumentException(PnPCoreResources.Exception_ContentType_WrongDocumentSetId);
            }

            var contentType = await AddAsync(id, name, description, group).ConfigureAwait(false) as ContentType;

            var documentSet = await contentType.AsDocumentSetAsync().ConfigureAwait(false);

            if (options != null)
            {
                return await documentSet.UpdateAsync(options).ConfigureAwait(false);
            }

            return documentSet;
        }

        public IDocumentSet AddDocumentSet(string id, string name, string description = null, string group = null, DocumentSetOptions options = null)
        {
            return AddDocumentSetAsync(id, name, description, group, options).GetAwaiter().GetResult();
        }

        #endregion
    }
}

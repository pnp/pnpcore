using PnP.Core.QueryModel;
using PnP.Core.Services;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Requests.ContentTypes;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class FieldLinkCollection : QueryableDataModelCollection<IFieldLink>, IFieldLinkCollection
    {
        public FieldLinkCollection(PnPContext context, IDataModelParent parent, string memberName = null)
            : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }

        public async Task AddBatchAsync(Batch batch, IField field)
        {
            (Guid siteId, Guid webId) = await GetSiteAndWebIdsAsync(field).ConfigureAwait(false);

            ApiCall apiCall = BuildAddApiCall(field, siteId, webId);
            await (Parent as ContentType).RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void AddBatch(Batch batch, IField field)
        {
            AddBatchAsync(batch, field).GetAwaiter().GetResult();
        }

        public async Task AddBatchAsync(IField field)
        {
            await AddBatchAsync(PnPContext.CurrentBatch, field).ConfigureAwait(false);
        }

        public void AddBatch(IField field)
        {
            AddBatchAsync(field).GetAwaiter().GetResult();
        }

        public async Task<IFieldLink> AddAsync(IField field, string displayName = null, bool hidden = false, bool required = false, bool readOnly = false, bool showInDisplayForm = true)
        {
            (Guid siteId, Guid webId) = await GetSiteAndWebIdsAsync(field).ConfigureAwait(false);

            ApiCall apiCall = BuildAddApiCall(field, siteId, webId);

            var result = await (Parent as ContentType).RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
            Guid addedFieldLinkId = (Guid)result.ApiCall.CSOMRequests[0].Result;

            // Get the added field
            var addedFieldLink = await this.QueryProperties(p => p.All, p => p.ReadOnly, p => p.ShowInDisplayForm, p => p.DisplayName).FirstOrDefaultAsync(p => p.Id == addedFieldLinkId).ConfigureAwait(false);
            //var fieldLinks = await this.GetA

            if (addedFieldLink != null)
            {
                // Update the field if needed
                if (!string.IsNullOrEmpty(displayName))
                {
                    addedFieldLink.DisplayName = displayName;
                }
                addedFieldLink.Hidden = hidden;
                addedFieldLink.Required = required;
                addedFieldLink.ReadOnly = readOnly;
                addedFieldLink.ShowInDisplayForm = showInDisplayForm;

                if ((addedFieldLink as FieldLink).HasChanges)
                {
                    await addedFieldLink.UpdateAsync().ConfigureAwait(false);
                }
            }

            return addedFieldLink;
        }

        private async Task<(Guid,Guid)> GetSiteAndWebIdsAsync(IField field)
        {
            Guid siteId = Guid.Empty;
            Guid webId = Guid.Empty;
            if (field.Parent != null && field.Parent.Parent is ContentTypeHub hub)
            {
                hub.SiteId ??= await hub.GetSiteIdAsync().ConfigureAwait(false);

                if (!string.IsNullOrEmpty(hub.SiteId))
                {
                    var split = hub.SiteId.Split(',');
                    if (split.Length == 3)
                    {
                        siteId = Guid.Parse(split[1]);
                        webId = Guid.Parse(split[2]);
                    }
                }
                else
                {
                    throw new ClientException("No valid site id found");
                }
            }
            else
            {
                siteId = PnPContext.Site.Id;
                webId = PnPContext.Web.Id;
            }

            return (siteId, webId);
        }

        private ApiCall BuildAddApiCall(IField field, Guid siteId, Guid webId)
        {
            return new ApiCall(new List<IRequest<object>>
            {
                new AddFieldLinkRequest(Parent as IContentType, field, siteId, webId, true)
            });
        }

        public IFieldLink Add(IField field, string displayName = null, bool hidden = false, bool required = false, bool readOnly = false, bool showInDisplayForm = true)
        {
            return AddAsync(field, displayName, hidden, required, readOnly, showInDisplayForm).GetAwaiter().GetResult();
        }

    }
}

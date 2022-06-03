using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    [GraphType]
    internal sealed class DocumentSet : BaseDataModel<IDocumentSet>, IDocumentSet
    {

        #region Properties

        public string ContentTypeId { get => GetValue<string>(); set => SetValue(value); }

        public IList<IContentTypeInfo> AllowedContentTypes { get => GetValue<IList<IContentTypeInfo>>(); set => SetValue(value); }

        public IList<IDocumentSetContent> DefaultContents { get => GetValue<IList<IDocumentSetContent>>(); set => SetValue(value); }

        public bool ShouldPrefixNameToFile { get => GetValue<bool>(); set => SetValue(value); }

        public string WelcomePageUrl { get => GetValue<string>(); set => SetValue(value); }

        public IList<IField> SharedColumns { get => GetValue<IList<IField>>(); set => SetValue(value); }

        public IList<IField> WelcomePageColumns { get => GetValue<IList<IField>>(); set => SetValue(value); }

        [KeyProperty(nameof(ContentTypeId))]
        public override object Key { get => ContentTypeId; set => ContentTypeId = value.ToString(); }

        #endregion

        #region Methods

        public async Task<IDocumentSet> UpdateAsync(DocumentSetOptions options)
        {
            var parentCt = Parent as ContentType;

            dynamic body = new ExpandoObject();

            dynamic documentSet = new ExpandoObject();

            if (options.PropagateWelcomePageChanges.HasValue)
            {
                documentSet.propagateWelcomePageChanges = options.PropagateWelcomePageChanges;
            }

            if (options.ShouldPrefixNameToFile.HasValue)
            {
                documentSet.shouldPrefixNameToFile = options.ShouldPrefixNameToFile;
            }

            if (options.AllowedContentTypes != null && options.AllowedContentTypes.Count > 0)
            {
                var allowedContentTypes = new List<dynamic>();

                foreach (var allowedContentType in options.AllowedContentTypes)
                {
                    dynamic allowedCt = new ExpandoObject();

                    allowedCt.id = allowedContentType.Id;
                    allowedCt.name = allowedContentType.Name;

                    allowedContentTypes.Add(allowedCt);
                }

                if (options.KeepExistingContentTypes && AllowedContentTypes.Count > 0)
                {
                    foreach (var allowedContentType in AllowedContentTypes)
                    {
                        dynamic allowedCt = new ExpandoObject();

                        allowedCt.id = allowedContentType.Id;
                        allowedCt.name = allowedContentType.Name;

                        allowedContentTypes.Add(allowedCt);
                    }
                }

                documentSet.allowedContentTypes = allowedContentTypes;
            }

            if (options.SharedColumns != null && options.SharedColumns.Count > 0)
            {

                await parentCt.LoadAsync(p => p.Fields).ConfigureAwait(false);

                foreach (var field in options.SharedColumns)
                {
                    if (parentCt.Fields.AsRequested().FirstOrDefault(y => y.Id == field.Id) == null)
                    {
                        await parentCt.AddFieldAsync(field).ConfigureAwait(false);
                    }
                }

                var sharedColumns = new List<dynamic>();

                foreach (var column in options.SharedColumns)
                {
                    dynamic sharedColumn = new ExpandoObject();

                    sharedColumn.id = column.Id;
                    sharedColumn.name = column.InternalName;

                    sharedColumns.Add(sharedColumn);
                }

                if (options.KeepExistingSharedColumns && SharedColumns.Count > 0)
                {
                    foreach (var column in SharedColumns)
                    {
                        dynamic sharedColumn = new ExpandoObject();

                        sharedColumn.id = column.Id;
                        sharedColumn.name = column.InternalName;

                        sharedColumns.Add(sharedColumn);
                    }
                }

                documentSet.sharedColumns = sharedColumns;
            }

            if (options.WelcomePageColumns != null && options.WelcomePageColumns.Count > 0)
            {
                await parentCt.LoadAsync(p => p.Fields).ConfigureAwait(false);
                
                foreach (var field in options.WelcomePageColumns)
                {
                    // Check if field exists on CT, if not, add it. Otherwise --> Error
                    if (parentCt.Fields.AsRequested().FirstOrDefault(y => y.Id == field.Id) == null)
                    {
                        await parentCt.AddFieldAsync(field).ConfigureAwait(false);
                    }
                }

                var welcomePageColumns = new List<dynamic>();

                foreach (var column in options.WelcomePageColumns)
                {
                    dynamic welcomeColumn = new ExpandoObject();

                    welcomeColumn.id = column.Id;
                    welcomeColumn.name = column.InternalName;

                    welcomePageColumns.Add(welcomeColumn);
                }

                if (options.KeepExistingWelcomePageColumns && WelcomePageColumns.Count > 0)
                {
                    foreach (var column in WelcomePageColumns)
                    {
                        dynamic welcomeColumn = new ExpandoObject();

                        welcomeColumn.id = column.Id;
                        welcomeColumn.name = column.InternalName;

                        welcomePageColumns.Add(welcomeColumn);
                    }
                }

                documentSet.welcomePageColumns = welcomePageColumns;
            }

            if (options.DefaultContents != null && options.DefaultContents.Count > 0)
            {
                var defaultContents = new List<dynamic>();
                
                foreach (var defaultContent in options.DefaultContents)
                {
                    if (!defaultContent.FolderName.EndsWith("/"))
                    {
                        defaultContent.FolderName += "/";
                    }
                    
                    await defaultContent.File.EnsurePropertiesAsync(y => y.ListItemAllFields, y => y.ListId).ConfigureAwait(false);

                    await parentCt.AddFileToDefaultContentLocationAsync(defaultContent.File.ListId.ToString(), defaultContent.File.ListItemAllFields.Id.ToString(), defaultContent.FileName).ConfigureAwait(false);

                    dynamic defaultContentDynamic = new ExpandoObject();
                    defaultContentDynamic.fileName = defaultContent.FileName;
                    defaultContentDynamic.folderName = defaultContent.FolderName;
                    
                    dynamic contentTypeInfo = new ExpandoObject();
                    contentTypeInfo.id = defaultContent.ContentTypeId;
                    defaultContentDynamic.contentType = contentTypeInfo;

                    defaultContents.Add(defaultContentDynamic);
                }

                if (options.KeepExistingDefaultContent && DefaultContents.Count > 0)
                {
                    foreach (var dc in DefaultContents)
                    {
                        dynamic defaultContentDynamic = new ExpandoObject();
                        defaultContentDynamic.fileName = dc.FileName;
                        defaultContentDynamic.folderName = dc.FolderName;

                        dynamic contentTypeInfo = new ExpandoObject();
                        contentTypeInfo.id = dc.ContentType.Id;
                        defaultContentDynamic.contentType = contentTypeInfo;

                        defaultContents.Add(defaultContentDynamic);
                    }
                }

                documentSet.defaultContents = defaultContents;
            }

            body.documentSet = documentSet;
            
            var apiCall = new ApiCall($"sites/{PnPContext.Site.Id}/contenttypes/{ContentTypeId}", ApiType.Graph, jsonBody: JsonSerializer.Serialize(body, typeof(ExpandoObject), PnPConstants.JsonSerializer_IgnoreNullValues_CamelCase));
            var response = await RawRequestAsync(apiCall, new HttpMethod("PATCH")).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
            {
                return await parentCt.AsDocumentSetAsync().ConfigureAwait(false);
            }
            else
            {
                throw new Exception("Error occured during update");
            }
        }

        public IDocumentSet Update(DocumentSetOptions options)
        {
            return UpdateAsync(options).GetAwaiter().GetResult();
        }

        #endregion
    }
}

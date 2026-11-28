using AngleSharp.Html.Parser;
using Newtonsoft.Json;

using PnP.Core.Modernization.Cache;
using PnP.Core.Modernization.Entities;

using PnP.Core.Modernization.Functions;
using PnP.Core.Modernization.Telemetry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using System.Xml.Linq;

using System.IO;
using PnP.Core.Provisioning.Utilities;

namespace PnP.Core.Modernization.Transform
{
    /// <summary>
    /// Base page transformator class that contains logic that applies for all page transformations
    /// </summary>
    public abstract class BasePageTransformator : BaseTransform
    {
        internal PnPContext sourceClientContext;
        internal PnPContext targetClientContext;
        internal Stopwatch watch;
        internal const string ExecutionLog = "execution.csv";
        internal PageTransformation pageTransformation;
        internal UserTransformator userTransformator;
        internal TermTransformator termTransformator;
        internal string version = "undefined";
        internal PageTelemetry pageTelemetry;
        internal bool isRootPage = false;
        // source page information to "restore"
        internal IFieldUserValue SourcePageAuthor;
        internal IFieldUserValue SourcePageEditor;
        internal DateTime SourcePageCreated;
        internal DateTime SourcePageModified;

        #region Helper methods

        internal void SavePage(IPage page, string pageName, BaseTransformationInformation information,
            PnPContext context, Func<Uri, string> importExternalImage = null)
        {
            // A saved page ID is needed for SharePoint to select and retain the page-owned asset folder.
            page.Save(pageName);
            var textParts = page.Controls.OfType<IPageText>()
                .Where(part => !string.IsNullOrEmpty(part.Text) && part.Text.IndexOf("<img", StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
            if (textParts.Count == 0)
            {
                return;
            }

            if (importExternalImage == null)
            {
                var importer = new ExternalImageImporter(new Uri(context.Uri.AbsoluteUri), page.PageId.GetValueOrDefault(), pageName,
                    (method, request) => page.PnPContext.Web.ExecuteRequest(new ApiRequest(method, ApiRequestType.SPORest,
                        request, null, new Dictionary<string, string> { ["Accept"] = "application/json;odata=nometadata" })).Response,
                    RegisteredLogObservers);
                importExternalImage = importer.Import;
            }
            var functions = new BuiltIn(information, context, sourceClientContext, page, RegisteredLogObservers);
            bool changed = false;
            foreach (var part in textParts)
            {
                var updated = functions.TransformInlineImages(part.Text, externalImagesOnly: true, importExternalImage: importExternalImage);
                if (!string.Equals(updated, part.Text, StringComparison.Ordinal))
                {
                    part.Text = updated;
                    changed = true;
                }
            }
            if (changed)
            {
                page.Save(pageName);
            }
        }

        /// <summary>
        /// Loads the default webpart mapping model
        /// </summary>
        /// <returns></returns>
        public static PageTransformation LoadDefaultWebPartMapping()
        {
            // Load default webpartmapping file
            XmlSerializer xmlMapping = new XmlSerializer(typeof(PageTransformation));

            // Load the default one from resources into a model, no need for persisting this file
            string webpartMappingFileContents = LoadFile("PnP.Core.Modernization.webpartmapping.xml");

            PageTransformation webPartMappingModel = null;
            using (var stream = GenerateStreamFromString(webpartMappingFileContents))
            {
                webPartMappingModel = (PageTransformation)xmlMapping.Deserialize(stream);
            }

            return webPartMappingModel;
        }

        /// <summary>
        /// Loads the default webpart mapping model file
        /// </summary>
        /// <returns></returns>
        public static string LoadDefaultWebPartMappingFile()
        {
            return LoadFile("PnP.Core.Modernization.webpartmapping.xml"); ;
        }

        internal string GetFieldValue(BaseTransformationInformation baseTransformationInformation, string fieldName)
        {

            if (baseTransformationInformation.SourcePage != null)
            {
                return baseTransformationInformation.SourcePage[fieldName].ToString();
            }
            else
            {

                if (baseTransformationInformation.SourceFile != null)
                {
                    var fileServerRelativeUrl = baseTransformationInformation.SourceFile.EnsureProperty(p => p.ServerRelativeUrl);

                    // come up with equivalent field values for the page without listitem (so page living in the root folder of the site)
                    if (fieldName.Equals(Constants.FileRefField))
                    {
                        // e.g. /sites/espctest2/SitePages/demo16.aspx
                        return fileServerRelativeUrl;
                    }
                    else if (fieldName.Equals(Constants.FileDirRefField))
                    {
                        // e.g. /sites/espctest2/SitePages
                        return fileServerRelativeUrl.Replace($"/{Path.GetFileName(fileServerRelativeUrl)}", "");

                    }
                    else if (fieldName.Equals(Constants.FileLeafRefField))
                    {
                        // e.g. demo16.aspx
                        return Path.GetFileName(fileServerRelativeUrl);
                    }
                }
                return "";
            }
        }

        internal static IListItem LoadSourcePageFields(IListItem page)
        {
            page.EnsureProperties(item => item.ParentList.QueryProperties(list => list.Id));
            var list = page.ParentList;
            list.Fields.Load(field => field.InternalName, field => field.EntityPropertyName, field => field.TypeAsString);
            var schema = list.Fields.AsRequested().Select(field => new
            {
                field.InternalName, field.EntityPropertyName, field.TypeAsString
            }).ToArray();
            var view = new XElement("View",
                new XAttribute("Scope", "RecursiveAll"),
                new XElement("Query", new XElement("Where", new XElement("Eq",
                    new XElement("FieldRef", new XAttribute("Name", "ID")),
                    new XElement("Value", new XAttribute("Type", "Counter"), page.Id)))),
                new XElement("RowLimit", 1));
            var summaryLinks = schema.Where(field => field.TypeAsString == "SummaryLinks").ToArray();
            if (summaryLinks.Length > 0)
            {
                var editValues = SharePointRest.Request(page.PnPContext,
                    $"_api/web/lists(guid'{list.Id}')/items({page.Id})/FieldValuesForEdit?$select=" +
                    string.Join(",", summaryLinks.Select(field => field.EntityPropertyName)));
                if (summaryLinks.Any(field => !string.IsNullOrWhiteSpace(editValues.GetProperty(field.EntityPropertyName).GetString())))
                {
                    // REST exposes Summary Links display labels but loses their URLs, groups and formatting.
                    return list.LoadItemsByCamlQueryViaCsom(new CamlQueryOptions
                    {
                        ViewXml = view.ToString(SaveOptions.DisableFormatting), DatesInUtc = true
                    }).Items.Single();
                }
            }
            var nonDefaultFields = new[]
            {
                Constants.FileRefField, Constants.FileLeafRefField, Constants.FileDirRefField,
                Constants.HtmlFileTypeField, Constants.FileTypeField, Constants.ClientSideApplicationIdField,
                "PublishingPageImage", Constants.PublishingRollupImageField
            };
            var fields = schema
                .Where(field => nonDefaultFields.Contains(field.InternalName) || field.TypeAsString == "Image")
                .Select(field => field.InternalName).Distinct().ToArray();
            if (fields.All(page.Values.ContainsKey))
            {
                return page;
            }

            // The item REST endpoint omits publishing image fields even with an explicit projection.
            // RenderListDataAsStream supplies them and lets Core decode the field values without marking them dirty.
            view.Element("RowLimit").AddBeforeSelf(new XElement("ViewFields", schema.Select(field =>
                new XElement("FieldRef", new XAttribute("Name", field.InternalName)))));
            list.LoadListDataAsStream(new RenderListDataOptions
            {
                ViewXml = view.ToString(SaveOptions.DisableFormatting),
                RenderOptions = RenderListDataOptionsFlags.ListData,
                DatesInUtc = true
            });
            var loaded = list.Items.AsRequested().Single(item => item.Id == page.Id);
            // Restore raw rich text where REST supports it; stream rendering adds UI wrappers to those fields.
            var raw = SharePointRest.Request(page.PnPContext, $"_api/web/lists(guid'{list.Id}')/items({page.Id})");
            foreach (var field in schema)
            {
                if (raw.TryGetProperty(field.EntityPropertyName, out _) ||
                    raw.TryGetProperty(field.EntityPropertyName + "Id", out _))
                {
                    loaded.Values.Remove(field.InternalName);
                }
            }
            loaded.Load(item => item.All);
            return loaded;
        }

        internal bool FieldExistsAndIsUsed(BaseTransformationInformation baseTransformationInformation, string fieldName)
        {
            if (baseTransformationInformation.SourcePage != null)
            {
                return baseTransformationInformation.SourcePage.FieldExistsAndUsed(fieldName);
            }
            else
            {
                return true;
            }
        }

        internal bool IsRootPage(IFile file)
        {
            if (file != null)
            {
                return true;
            }

            return false;
        }

        internal void RemoveEmptyTextParts(IPage targetPage)
        {
            var textParts = targetPage.Controls.Where(p => p is IPageText);
            if (textParts != null && textParts.Any())
            {
                HtmlParser parser = new HtmlParser(new HtmlParserOptions() { IsEmbedded = true });

                foreach (var textPart in textParts.ToList())
                {
                    using (var document = parser.ParseDocument(((IPageText)textPart).Text))
                    {
                        if (document.FirstChild != null && string.IsNullOrEmpty(document.FirstChild.TextContent) &&
                            document.QuerySelector("img, iframe, .imagePlugin") == null)
                        {
                            LogInfo(LogStrings.TransformRemovingEmptyWebPart, LogStrings.Heading_RemoveEmptyTextParts);
                            // Drop text part
                            targetPage.Controls.Remove(textPart);
                        }
                    }
                }
            }
        }

        internal void RemoveEmptySectionsAndColumns(IPage targetPage)
        {
            foreach (var section in targetPage.Sections.ToList())
            {
                // First remove all empty sections
                if (section.Controls.Count == 0)
                {
                    targetPage.Sections.Remove(section);
                }
            }

            // Remove empty columns
            foreach (var section in targetPage.Sections)
            {
                if (section.Type == CanvasSectionTemplate.TwoColumn ||
                    section.Type == CanvasSectionTemplate.TwoColumnLeft ||
                    section.Type == CanvasSectionTemplate.TwoColumnRight ||
                    section.Type == CanvasSectionTemplate.TwoColumnVerticalSection ||
                    section.Type == CanvasSectionTemplate.TwoColumnLeftVerticalSection ||
                    section.Type == CanvasSectionTemplate.TwoColumnRightVerticalSection)
                {
                    var emptyColumn = section.Columns.Where(p => p.Controls.Count == 0 && !p.IsVerticalSectionColumn).FirstOrDefault();
                    if (emptyColumn != null)
                    {
                        // drop the empty column and change to single column section
                        section.Columns.Remove(emptyColumn);

                        if (section.Type == CanvasSectionTemplate.TwoColumnVerticalSection ||
                            section.Type == CanvasSectionTemplate.TwoColumnLeftVerticalSection ||
                            section.Type == CanvasSectionTemplate.TwoColumnRightVerticalSection)
                        {
                            section.Type = CanvasSectionTemplate.OneColumnVerticalSection;
                        }
                        else
                        {
                            section.Type = CanvasSectionTemplate.OneColumn;
                        }

                        section.Columns.First().ResetColumn(0, 12);
                    }
                }
                else if (section.Type == CanvasSectionTemplate.ThreeColumn ||
                         section.Type == CanvasSectionTemplate.ThreeColumnVerticalSection)
                {
                    var emptyColumns = section.Columns.Where(p => p.Controls.Count == 0 && !p.IsVerticalSectionColumn);
                    if (emptyColumns != null)
                    {
                        if (emptyColumns.Any() && emptyColumns.Count() == 2)
                        {
                            // drop the two empty columns and change to single column section
                            foreach (var emptyColumn in emptyColumns.ToList())
                            {
                                section.Columns.Remove(emptyColumn);
                            }

                            if (section.Type == CanvasSectionTemplate.ThreeColumnVerticalSection)
                            {
                                section.Type = CanvasSectionTemplate.OneColumnVerticalSection;
                            }
                            else
                            {
                                section.Type = CanvasSectionTemplate.OneColumn;
                            }

                            section.Columns.First().ResetColumn(0, 12);
                        }
                        else if (emptyColumns.Any() && emptyColumns.Count() == 1)
                        {
                            // Remove the empty column and change to two column section
                            section.Columns.Remove(emptyColumns.First());

                            if (section.Type == CanvasSectionTemplate.ThreeColumnVerticalSection)
                            {
                                section.Type = CanvasSectionTemplate.TwoColumnVerticalSection;
                            }
                            else
                            {
                                section.Type = CanvasSectionTemplate.TwoColumn;
                            }

                            int i = 0;
                            foreach (var column in section.Columns.Where(p => !p.IsVerticalSectionColumn))
                            {
                                column.ResetColumn(i, 6);
                                i++;
                            }
                        }
                    }
                }                
            }
        }

        internal void ApplyItemLevelPermissions(bool hasTargetContext, IListItem item, ListItemPermission lip, bool alwaysBreakItemLevelPermissions = false)
        {

            if (lip == null || item == null)
            {
                return;
            }

            // Break permission inheritance on the item if not done yet
            if (alwaysBreakItemLevelPermissions || !item.HasUniqueRoleAssignments)
            {
                item.BreakRoleInheritance(false, false);
                item.PnPContext.Execute();
            }

            // Cross site collection flow (can be from SPO to SPO, but also from SP On-Premises to SPO)
            if (hasTargetContext)
            {
                try
                {

                    // Ensure principals are available in the target site
                    Dictionary<string, ISharePointPrincipal> targetPrincipals = new Dictionary<string, ISharePointPrincipal>(lip.Principals.Count);

                    foreach (var principal in lip.Principals)
                    {
                        ISharePointPrincipal targetPrincipal = GetPrincipal(this.targetClientContext.Web, principal.Key, hasTargetContext);
                        if (targetPrincipal != null)
                        {
                            if (!targetPrincipals.ContainsKey(principal.Key))
                            {
                                targetPrincipals.Add(principal.Key, targetPrincipal);
                            }
                        }
                    }

                    // Assign item level permissions          
                    foreach (IRoleAssignment roleAssignment in lip.RoleAssignments)
                    {
                        var sourcePrincipal = lip.Principals.FirstOrDefault(p => p.Value.Id == roleAssignment.PrincipalId);
                        if (sourcePrincipal.Key != null && targetPrincipals.TryGetValue(sourcePrincipal.Key, out ISharePointPrincipal principal))
                        {
                            foreach (IRoleDefinition roleDef in roleAssignment.RoleDefinitions.AsRequested())
                            {
                                if (roleDef.Id != 1073741825) // Limited Access permission
                                {
                                    var targetRoleDef = this.targetClientContext.Web.RoleDefinitions.AsRequested().FirstOrDefault(r => r.Name == roleDef.Name);
                                    if (targetRoleDef != null)
                                    {
                                        item.AddRoleDefinition(principal.Id, targetRoleDef);
                                    }
                                }
                            }

                            
                        }
                    }

                    this.targetClientContext.Execute();

                }
                catch (Exception ex)
                {
                    LogError(string.Format(LogStrings.Error_ApplyPermissionFailedToApplyPermissions, ex.Message), LogStrings.Heading_ApplyItemLevelPermissions, ex);
                }
            }
            else
            {
                try
                {
                    // In-place transformation

                    // Assign item level permissions
                    foreach (IRoleAssignment roleAssignment in lip.RoleAssignments)
                    {
                        var principal = lip.Principals.Values.FirstOrDefault(p => p.Id == roleAssignment.PrincipalId);
                        if (principal != null)
                        {
                            foreach (IRoleDefinition roleDef in roleAssignment.RoleDefinitions.AsRequested())
                            {
                                item.AddRoleDefinition(principal.Id, roleDef);
                            }

                        }
                    }

                    this.sourceClientContext.Execute();

                }
                catch (Exception ex)
                {
                    LogError(string.Format(LogStrings.Error_ApplyPermissionFailedToApplyPermissions, ex.Message), LogStrings.Heading_ApplyItemLevelPermissions, ex);
                }
            }

            LogInfo(LogStrings.TransformCopiedItemPermissions, LogStrings.Heading_ApplyItemLevelPermissions);
        }

        internal ListItemPermission GetItemLevelPermissions(bool hasTargetContext, IList pagesLibrary, IListItem source, IListItem target)
        {
            ListItemPermission lip = null;

            if (source.IsPropertyAvailable(p => p.HasUniqueRoleAssignments) && source.HasUniqueRoleAssignments)
            {
                // You need to have the ManagePermissions permission before item level permissions can be copied
                if (pagesLibrary.HasPermissions(PermissionKind.ManagePermissions))
                {
                    // Copy the unique permissions from source to target
                    // Get the unique permissions
                    source.LoadBatch(a => a.RoleAssignments.QueryProperties(roleAsg => roleAsg.PrincipalId,
                        roleAsg => roleAsg.RoleDefinitions.QueryProperties(roleDef => roleDef.Id, roleDef => roleDef.Name, roleDef => roleDef.Description)));
                    this.sourceClientContext.Execute();

                    if (source.HasPermissions(PermissionKind.ManagePermissions))
                    {
                        // Load the site groups
                        this.sourceClientContext.Web.SiteGroups.LoadBatch(g => g.LoginName);

                        // Get target page information
                        if (hasTargetContext)
                        {
                            target.LoadBatch(p => p.HasUniqueRoleAssignments,p => p.RoleAssignments);
                            this.targetClientContext.Web.LoadBatch(p => p.RoleDefinitions);
                            this.targetClientContext.Web.SiteGroups.LoadBatch(g => g.LoginName);
                        }
                        else
                        {
                            target.LoadBatch(p => p.HasUniqueRoleAssignments,p => p.RoleAssignments);
                        }

                        this.sourceClientContext.Execute();

                        if (hasTargetContext)
                        {
                            try
                            {
                                this.targetClientContext.Execute();
                            }
                            catch(Exception)
                            {
                                LogWarning(LogStrings.Warning_TransformGetItemPermissionsAccessDenied, LogStrings.Heading_ApplyItemLevelPermissions);
                                return lip;
                            }
                        }

                        Dictionary<string, ISharePointPrincipal> principals = new Dictionary<string, ISharePointPrincipal>(10);
                        lip = new ListItemPermission()
                        {
                            RoleAssignments = source.RoleAssignments,
                            Principals = principals
                        };

                        // Apply new permissions
                        foreach (IRoleAssignment roleAssignment in source.RoleAssignments)
                        {
                            var member = SharePointRest.Request(source.PnPContext,
                                $"_api/web/lists(guid'{source.ParentList.Id}')/items({source.Id})/roleassignments/getbyprincipalid({roleAssignment.PrincipalId})/member?$select=LoginName");
                            var loginName = member.GetProperty("LoginName").GetString();
                            ISharePointPrincipal principal = GetPrincipal(this.sourceClientContext.Web, loginName, hasTargetContext, true);
                            if (principal != null)
                            {
                                if (!lip.Principals.ContainsKey(loginName))
                                {
                                    lip.Principals.Add(loginName, principal);
                                }
                            }
                        }
                    }
                }
                else
                {
                    LogWarning(LogStrings.Warning_TransformGetItemPermissionsAccessDenied, LogStrings.Heading_ApplyItemLevelPermissions);
                    return lip;
                }
            }

            LogInfo(LogStrings.TransformGetItemPermissions, LogStrings.Heading_ApplyItemLevelPermissions);

            return lip;
        }

        internal ISharePointPrincipal GetPrincipal(IWeb web, string principalInput, bool hasTargetContext, bool reading = false)
        {

            //On-Prem User Mapping - Dont replace the source
            if (hasTargetContext && !reading)
            {
                principalInput = this.userTransformator.RemapPrincipal(principalInput);
            }

            ISharePointPrincipal principal = web.SiteGroups.AsRequested().FirstOrDefault(g => g.LoginName.Equals(principalInput, StringComparison.OrdinalIgnoreCase));

            if (principal == null)
            {
                if (principalInput.Contains("#ext#"))
                {
                    principal = web.SiteUsers.AsQueryable().FirstOrDefault(u => u.LoginName == principalInput);

                    if (principal == null)
                    {
                        //Skipping external user...
                    }
                }
                else
                {
                    try
                    {
                        principal = web.EnsureUser(principalInput);
                        web.PnPContext.Execute();
                    }
                    catch (Exception ex)
                    {
                        if (!hasTargetContext)
                        {
                            //Failed to EnsureUser, we're not failing for this, only log as error when doing an in site transformation as it's not expected to fail here
                            LogError(LogStrings.Error_GetPrincipalFailedEnsureUser, LogStrings.Heading_GetPrincipal, ex);
                        }

                        principal = null;
                    }
                }
            }

            return principal;
        }

        internal void CopyPageMetadata(PageTransformationInformation pageTransformationInformation, string pageType, IFile targetPage, IList targetPagesLibrary)
        {
            var fieldsToCopy = CacheManager.Instance.GetFieldsToCopy(this.sourceClientContext.Web, targetPagesLibrary, pageType);
            bool listItemWasReloaded = false;
            if (fieldsToCopy.Count > 0)
            {
                // Load the target page list item
                targetPage.ListItemAllFields.LoadBatch();
                targetPage.PnPContext.Execute();

                pageTransformationInformation.SourcePage.EnsureProperty(p => p.ContentType);

                // regular fields
                bool isDirty = false;
                bool isSourceInitialized = false;
                IList sourceSitesPagesLibrary = default;

                var sitePagesServerRelativeUrl = UrlUtility.Combine(targetPage.PnPContext.Web.ServerRelativeUrl.TrimEnd(new char[] { '/' }), "sitepages");
                IList targetSitePagesLibrary = (targetPage.PnPContext as PnPContext).Web.GetList(sitePagesServerRelativeUrl);
                targetSitePagesLibrary.Fields.LoadBatch(f => f.Id,f => f.Title,f => f.Hidden,f => f.InternalName,f => f.DefaultValue,f => f.Required,f => f.StaticName,f => f.TypeAsString);
                targetPage.PnPContext.Execute();

                

                string contentTypeId = CacheManager.Instance.GetContentTypeId(targetPage.ListItemAllFields.ParentList, pageTransformationInformation.SourcePage.ContentType.Name);
                if (!string.IsNullOrEmpty(contentTypeId))
                {
                    // Load the target page list item, needs to be loaded as it was previously saved and we need to avoid version conflicts
                    targetPage.ListItemAllFields.LoadBatch();
                    targetPage.PnPContext.Execute();
                    listItemWasReloaded = true;

                    targetPage.ListItemAllFields[Constants.ContentTypeIdField] = contentTypeId;
                    targetPage.ListItemAllFields.UpdateOverwriteVersion();
                    isDirty = true;
                }

                #region Taxonomy Fields

                foreach (var fieldToCopy in fieldsToCopy.Where(p => p.FieldType == "TaxonomyFieldTypeMulti" || p.FieldType == "TaxonomyFieldType"))
                {
                    try
                    {
                        if (!listItemWasReloaded)
                        {
                            // Load the target page list item, needs to be loaded as it was previously saved and we need to avoid version conflicts
                            targetPage.ListItemAllFields.LoadBatch();
                            targetPage.PnPContext.Execute();
                            listItemWasReloaded = true;
                        }

                        if (!isSourceInitialized)
                        {
                            if (pageTransformationInformation.IsCrossSiteTransformation)
                            {

                                //TODO: Check if there is a scenario where the source page may origniate from another library on same site/web
                                // This is needed to get the field term set id
                                sourceSitesPagesLibrary = pageTransformationInformation.SourcePage.ParentList;
                                sourceSitesPagesLibrary.Fields.LoadBatch(f => f.Id,f => f.Title,f => f.Hidden,f => f.InternalName,f => f.DefaultValue,f => f.Required,f => f.StaticName);
                                this.sourceClientContext.Execute();
                            }

                            InitializeTermMapping(pageTransformationInformation);

                            isSourceInitialized = true;
                        }

                        IField taxFieldBeforeCast = targetSitePagesLibrary.Fields.AsRequested().FirstOrDefault(p => p.StaticName.Equals(fieldToCopy.FieldName));
                        IField sourceTaxFieldBeforeCast = null;
                        if (pageTransformationInformation.IsCrossSiteTransformation)
                        {
                            sourceTaxFieldBeforeCast = sourceSitesPagesLibrary.Fields.AsRequested().FirstOrDefault(p => p.StaticName.Equals(fieldToCopy.FieldName));
                        }
                        else
                        {
                            sourceTaxFieldBeforeCast = taxFieldBeforeCast;
                        }

                        switch (fieldToCopy.FieldType)
                        {
                            case "TaxonomyFieldTypeMulti":
                                {
                                    if (taxFieldBeforeCast != null && sourceTaxFieldBeforeCast != null)
                                    {
                                        var taxField = taxFieldBeforeCast;
                                        var srcTaxField = sourceTaxFieldBeforeCast;
                                        taxField.EnsureProperties(f => f.TermSetId, f => f.SspId);
                                        srcTaxField.EnsureProperties(f => f.TermSetId, f => f.SspId);
                                        var isSP2010 = pageTransformationInformation.SourceVersion == SPVersion.SP2010;

                                        var sourceTermSetId = Guid.Empty;
                                        var sourceSsdId = Guid.Empty;

                                        if (isSP2010)
                                        {
                                            // 2010 doesnt appear to be able to cast this type via CSOM
                                            var extractedTermSetId = TermTransformator.ExtractTermSetIdOrSspIdFromXmlSchema(sourceTaxFieldBeforeCast.SchemaXml);
                                            Guid.TryParse(extractedTermSetId, out sourceTermSetId);
                                            var extractedSspId = TermTransformator.ExtractTermSetIdOrSspIdFromXmlSchema(sourceTaxFieldBeforeCast.SchemaXml, true);
                                            Guid.TryParse(extractedSspId, out sourceSsdId);
                                        }
                                        else
                                        {
                                            sourceTermSetId = srcTaxField.TermSetId;
                                            sourceSsdId = srcTaxField.SspId;
                                        }

                                        // If source and target field point to the same termset then termmapping is not needed
                                        bool skipTermMapping = ((sourceTermSetId == taxField.TermSetId) && string.IsNullOrEmpty(pageTransformationInformation.TermMappingFile));

                                        if (!skipTermMapping)
                                        {
                                            skipTermMapping = !pageTransformationInformation.IsCrossSiteTransformation;

                                            //Gather terms from the term store
                                            termTransformator.CacheTermsFromTermStore(sourceTermSetId, taxField.TermSetId, sourceSsdId, isSP2010);
                                        }

                                        if (pageTransformationInformation.SourcePage[fieldToCopy.FieldName] != null)
                                        {
                                            if (pageTransformationInformation.SourcePage[fieldToCopy.FieldName] is IFieldValueCollection)
                                            {
                                                IFieldValueCollection valueCollectionToCopy = (pageTransformationInformation.SourcePage[fieldToCopy.FieldName] as IFieldValueCollection);
                                                if (!skipTermMapping)
                                                {
                                                    //Term Transformator
                                                    var resultTermTransform = termTransformator.TransformCollection(valueCollectionToCopy);
                                                    valueCollectionToCopy = resultTermTransform.Item1;

                                                    var mappedValues = valueCollectionToCopy.Values.Cast<IFieldTaxonomyValue>().Except(resultTermTransform.Item2);
                                                    taxField.SetFieldValueByValueCollection(targetPage.ListItemAllFields, taxField.NewFieldValueCollection(mappedValues));

                                                    isDirty = true;
                                                    LogInfo($"{LogStrings.TransformCopyingMetaDataField} {fieldToCopy.FieldName}", LogStrings.Heading_CopyingPageMetadata);

                                                    if (resultTermTransform.Item2.Any())
                                                    {
                                                        resultTermTransform.Item2.ForEach(field =>
                                                        {
                                                            LogWarning(string.Format(LogStrings.TransformCopyingMetaDataTaxFieldValue, field.Label), LogStrings.Heading_CopyingPageMetadata);
                                                        });
                                                    }
                                                }
                                                else
                                                {
                                                    var valueCollection = valueCollectionToCopy;
                                                    taxField.SetFieldValueByValueCollection(targetPage.ListItemAllFields, valueCollection);
                                                    isDirty = true;
                                                    LogInfo($"{LogStrings.TransformCopyingMetaDataField} {fieldToCopy.FieldName}", LogStrings.Heading_CopyingPageMetadata);
                                                }
                                            }
                                            else if (pageTransformationInformation.SourcePage[fieldToCopy.FieldName] is Dictionary<string, object>)
                                            {
                                                var taxDictionaryList = (pageTransformationInformation.SourcePage[fieldToCopy.FieldName] as Dictionary<string, object>);
                                                var valueCollectionToCopy = taxDictionaryList["_Child_Items_"] as Object[];

                                                List<string> taxonomyFieldValueArray = new List<string>();
                                                for (int i = 0; i < valueCollectionToCopy.Length; i++)
                                                {
                                                    var taxDictionary = valueCollectionToCopy[i] as Dictionary<string, object>;
                                                    var label = taxDictionary["Label"].ToString();
                                                    var termGuid = new Guid(taxDictionary["TermGuid"].ToString());

                                                    if (!skipTermMapping)
                                                    {
                                                        //Term Transformator
                                                        var transformTerm = termTransformator.Transform(new TermData() { TermGuid = termGuid, TermLabel = label });

                                                        if (transformTerm.IsTermResolved)
                                                        {
                                                            taxonomyFieldValueArray.Add($"-1;#{taxDictionary["Label"].ToString()}|{taxDictionary["TermGuid"].ToString()}");
                                                        }
                                                        else
                                                        {
                                                            LogWarning(string.Format(LogStrings.TransformCopyingMetaDataTaxFieldValue, label), LogStrings.Heading_CopyingPageMetadata);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        taxonomyFieldValueArray.Add($"-1;#{label}|{termGuid}");
                                                    }
                                                }

                                                if (valueCollectionToCopy.Length > 0)
                                                {
                                                    var valueCollection = taxField.ParseTaxonomyValues(string.Join(";#", taxonomyFieldValueArray));
                                                    taxField.SetFieldValueByValueCollection(targetPage.ListItemAllFields, valueCollection);
                                                    isDirty = true;
                                                    LogInfo($"{LogStrings.TransformCopyingMetaDataField} {fieldToCopy.FieldName}", LogStrings.Heading_CopyingPageMetadata);
                                                }
                                                else
                                                {
                                                    // Field was empty, so let's skip the metadata copy
                                                    LogInfo(string.Format(LogStrings.TransformCopyingMetaDataTaxFieldEmpty, fieldToCopy.FieldName), LogStrings.Heading_CopyingPageMetadata);
                                                }
                                            }
                                            else if (pageTransformationInformation.SourcePage[fieldToCopy.FieldName] is Array && isSP2010)
                                            {

                                                var taxValueArray = (pageTransformationInformation.SourcePage[fieldToCopy.FieldName] as Array);

                                                List<string> taxonomyFieldValueArray = new List<string>();
                                                foreach (var taxValueItem in taxValueArray)
                                                {
                                                    var term = taxValueItem.ToString().Split('|');

                                                    var label = term[0].ToString();
                                                    var termGuid = new Guid(term[1]);

                                                    //Term Transformator
                                                    var transformTerm = termTransformator.Transform(new TermData() { TermGuid = termGuid, TermLabel = label });

                                                    if (transformTerm.IsTermResolved)
                                                    {
                                                        taxonomyFieldValueArray.Add($"-1;#{transformTerm.TermLabel}|{transformTerm.TermGuid.ToString()}");
                                                    }
                                                    else
                                                    {
                                                        LogWarning(string.Format(LogStrings.TransformCopyingMetaDataTaxFieldValue, label), LogStrings.Heading_CopyingPageMetadata);
                                                    }
                                                }

                                                if (taxValueArray.Length > 0)
                                                {
                                                    var valueCollection = taxField.ParseTaxonomyValues(string.Join(";#", taxonomyFieldValueArray));
                                                    taxField.SetFieldValueByValueCollection(targetPage.ListItemAllFields, valueCollection);
                                                    isDirty = true;
                                                    LogInfo($"{LogStrings.TransformCopyingMetaDataField} {fieldToCopy.FieldName}", LogStrings.Heading_CopyingPageMetadata);
                                                }
                                                else
                                                {
                                                    // Field was empty, so let's skip the metadata copy
                                                    LogInfo(string.Format(LogStrings.TransformCopyingMetaDataTaxFieldEmpty, fieldToCopy.FieldName), LogStrings.Heading_CopyingPageMetadata);
                                                }

                                            }
                                            else
                                            {
                                                // Field was empty, so let's skip the metadata copy
                                                LogInfo(string.Format(LogStrings.TransformCopyingMetaDataTaxFieldEmpty, fieldToCopy.FieldName), LogStrings.Heading_CopyingPageMetadata);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        LogWarning($"{LogStrings.TransformCopyingMetaDataFieldSkipped} {fieldToCopy.FieldName}", LogStrings.Heading_CopyingPageMetadata);
                                        break;
                                    }
                                    break;
                                }
                            case "TaxonomyFieldType":
                                {
                                    if (taxFieldBeforeCast != null)
                                    {
                                        var taxField = taxFieldBeforeCast;
                                        taxField.EnsureProperties(f => f.TermSetId, f => f.SspId);
                                        IFieldTaxonomyValue taxValue;

                                        if (pageTransformationInformation.SourcePage[fieldToCopy.FieldName] != null)
                                        {
                                            var srcTaxField = sourceTaxFieldBeforeCast;
                                            srcTaxField.EnsureProperties(f => f.TermSetId, f => f.SspId);

                                            var isSP2010 = pageTransformationInformation.SourceVersion == SPVersion.SP2010;

                                            var sourceTermSetId = Guid.Empty;
                                            var sourceSsdId = Guid.Empty;

                                            if (isSP2010)
                                            {
                                                // 2010 doesnt appear to be able to cast this type via CSOM
                                                var extractedTermSetId = TermTransformator.ExtractTermSetIdOrSspIdFromXmlSchema(sourceTaxFieldBeforeCast.SchemaXml);
                                                Guid.TryParse(extractedTermSetId, out sourceTermSetId);
                                                var extractedSspId = TermTransformator.ExtractTermSetIdOrSspIdFromXmlSchema(sourceTaxFieldBeforeCast.SchemaXml, true);
                                                Guid.TryParse(extractedSspId, out sourceSsdId);
                                            }
                                            else
                                            {
                                                sourceTermSetId = srcTaxField.TermSetId;
                                                sourceSsdId = srcTaxField.SspId;
                                            }

                                            // If source and target field point to the same termset then termmapping is not needed
                                            bool skipTermMapping = ((sourceTermSetId == taxField.TermSetId) && string.IsNullOrEmpty(pageTransformationInformation.TermMappingFile));

                                            if (!skipTermMapping)
                                            {
                                                skipTermMapping = !pageTransformationInformation.IsCrossSiteTransformation;
                                            }

                                            if (!skipTermMapping)
                                            {
                                                //Gather terms from the term store
                                                //TODO: Refine this, feels clunky implementation
                                                termTransformator.CacheTermsFromTermStore(sourceTermSetId, taxField.TermSetId, sourceSsdId, isSP2010);
                                            }

                                            if (pageTransformationInformation.SourcePage[fieldToCopy.FieldName] is IFieldTaxonomyValue)
                                            {
                                                var labelToSet = (pageTransformationInformation.SourcePage[fieldToCopy.FieldName] as IFieldTaxonomyValue).Label;
                                                var termGuidToSet = (pageTransformationInformation.SourcePage[fieldToCopy.FieldName] as IFieldTaxonomyValue).TermId;

                                                if (!skipTermMapping)
                                                {
                                                    //Term Transformator
                                                    var termTranform = termTransformator.Transform(new TermData() { TermGuid = termGuidToSet, TermLabel = labelToSet });
                                                    if (termTranform.IsTermResolved)
                                                    {
                                                        taxValue = taxField.NewFieldTaxonomyValue(termTranform.TermGuid, termTranform.TermLabel);
                                                        taxField.SetFieldValueByValue(targetPage.ListItemAllFields, taxValue);
                                                        isDirty = true;
                                                        LogInfo($"{LogStrings.TransformCopyingMetaDataField} {fieldToCopy.FieldName}", LogStrings.Heading_CopyingPageMetadata);
                                                    }
                                                    else
                                                    {
                                                        LogWarning(string.Format(LogStrings.TransformCopyingMetaDataTaxFieldValue, labelToSet), LogStrings.Heading_CopyingPageMetadata);
                                                    }
                                                }
                                                else
                                                {
                                                    taxValue = taxField.NewFieldTaxonomyValue(termGuidToSet, labelToSet);
                                                    taxField.SetFieldValueByValue(targetPage.ListItemAllFields, taxValue);
                                                    isDirty = true;
                                                    LogInfo($"{LogStrings.TransformCopyingMetaDataField} {fieldToCopy.FieldName}", LogStrings.Heading_CopyingPageMetadata);
                                                }
                                            }
                                            else if (pageTransformationInformation.SourcePage[fieldToCopy.FieldName] is Dictionary<string, object>)
                                            {
                                                var taxDictionary = (pageTransformationInformation.SourcePage[fieldToCopy.FieldName] as Dictionary<string, object>);
                                                var label = taxDictionary["Label"].ToString();
                                                var termGuid = taxDictionary["TermGuid"].ToString();

                                                if (!skipTermMapping)
                                                {
                                                    //Term Transformator
                                                    var transformTerm = termTransformator.Transform(new TermData() { TermGuid = new Guid(termGuid), TermLabel = label });
                                                    if (transformTerm.IsTermResolved)
                                                    {
                                                        taxValue = taxField.NewFieldTaxonomyValue(transformTerm.TermGuid, transformTerm.TermLabel);
                                                        taxField.SetFieldValueByValue(targetPage.ListItemAllFields, taxValue);
                                                        isDirty = true;
                                                        LogInfo($"{LogStrings.TransformCopyingMetaDataField} {fieldToCopy.FieldName}", LogStrings.Heading_CopyingPageMetadata);
                                                    }
                                                    else
                                                    {
                                                        LogWarning(string.Format(LogStrings.TransformCopyingMetaDataTaxFieldValue, label), LogStrings.Heading_CopyingPageMetadata);
                                                    }
                                                }
                                                else
                                                {
                                                    taxValue = taxField.NewFieldTaxonomyValue(Guid.Parse(termGuid), label);
                                                    taxField.SetFieldValueByValue(targetPage.ListItemAllFields, taxValue);
                                                    isDirty = true;
                                                    LogInfo($"{LogStrings.TransformCopyingMetaDataField} {fieldToCopy.FieldName}", LogStrings.Heading_CopyingPageMetadata);
                                                }
                                            }
                                            else
                                            {
                                                // Field was empty, so let's skip the metadata copy
                                                LogInfo(string.Format(LogStrings.TransformCopyingMetaDataTaxFieldEmpty, fieldToCopy.FieldName), LogStrings.Heading_CopyingPageMetadata);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        LogWarning($"{LogStrings.TransformCopyingMetaDataFieldSkipped} {fieldToCopy.FieldName}", LogStrings.Heading_CopyingPageMetadata);
                                        break;
                                    }
                                    break;
                                }
                        }

                    }
                    catch (Exception ex)
                    {
                        LogError(string.Format(LogStrings.Error_TransformingTaxonomyField, fieldToCopy.FieldName), LogStrings.Heading_CopyingPageMetadata, ex);
                    }
                }

                if (isDirty)
                {
                    // Added handling here to prevent entire transform process from failing.
                    try
                    {
                        targetPage.ListItemAllFields.UpdateOverwriteVersion();
                        targetPage.ListItemAllFields.LoadBatch();
                        targetPage.PnPContext.Execute();
                        isDirty = false;
                    }
                    catch (Exception ex)
                    {
                        LogError(LogStrings.Error_CommittingTaxonomyField, LogStrings.Heading_CopyingPageMetadata, ex);
                    }
                    finally
                    {
                        isDirty = false;
                    }
                }

                #endregion

                #region all other metadata except for taxonomy fields

                // This is all other metadata except for taxonomy fields
                foreach (var fieldToCopy in fieldsToCopy.Where(p => p.FieldType != "TaxonomyFieldTypeMulti" && p.FieldType != "TaxonomyFieldType"))
                {
                    IField targetField = targetSitePagesLibrary.Fields.AsRequested().FirstOrDefault(p => p.StaticName.Equals(fieldToCopy.FieldName));

                    if (targetField != null && pageTransformationInformation.SourcePage[fieldToCopy.FieldName] != null)
                    {
                        if (fieldToCopy.FieldType == "User" || fieldToCopy.FieldType == "UserMulti")
                        {
                            object fieldValueToSet = pageTransformationInformation.SourcePage[fieldToCopy.FieldName];
                            if (fieldValueToSet is IFieldUserValue)
                            {
                                try
                                {
                                    // Source User
                                    var fieldUser = (fieldValueToSet as IFieldUserValue).LookupValue;
                                    // Mapped target user
                                    if (this.userTransformator != null)
                                    {
                                        fieldUser = this.userTransformator.RemapPrincipal(this.sourceClientContext, (fieldValueToSet as IFieldUserValue));
                                    }

                                    // Ensure user exists on target site
                                    var ensuredUserOnTarget = CacheManager.Instance.GetEnsuredUser((targetPage.PnPContext as PnPContext), fieldUser);
                                    if (ensuredUserOnTarget != null)
                                    {
                                        // Prep a new FieldUserValue object instance and update the list item
                                        var newUser = targetField.NewFieldUserValue(ensuredUserOnTarget.Id);
                                        targetPage.ListItemAllFields[fieldToCopy.FieldName] = newUser;
                                    }
                                    else
                                    {
                                        // Clear target field - needed in overwrite scenarios
                                        targetPage.ListItemAllFields[fieldToCopy.FieldName] = null;
                                        LogWarning(string.Format(LogStrings.Warning_UserIsNotMappedOrResolving, (fieldValueToSet as IFieldUserValue).LookupValue, fieldToCopy.FieldName), LogStrings.Heading_CopyingPageMetadata);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LogWarning(string.Format(LogStrings.Warning_UserIsNotResolving, (fieldValueToSet as IFieldUserValue).LookupValue, ex.Message), LogStrings.Heading_CopyingPageMetadata);
                                }
                            }
                            else
                            {
                                List<IFieldUserValue> userValues = new List<IFieldUserValue>();
                                foreach (var currentUser in ((IFieldValueCollection)fieldValueToSet).Values)
                                {
                                    try
                                    {
                                        // Source User
                                        var fieldUser = (currentUser as IFieldUserValue).LookupValue;
                                        // Mapped target user
                                        if (this.userTransformator != null)
                                        {
                                            fieldUser = this.userTransformator.RemapPrincipal(this.sourceClientContext, (currentUser as IFieldUserValue));
                                        }

                                        // Ensure user exists on target site
                                        var ensuredUserOnTarget = CacheManager.Instance.GetEnsuredUser((targetPage.PnPContext as PnPContext), fieldUser);
                                        if (ensuredUserOnTarget != null)
                                        {
                                            // Prep a new FieldUserValue object instance
                                            var newUser = targetField.NewFieldUserValue(ensuredUserOnTarget.Id);

                                            userValues.Add(newUser);
                                        }
                                        else
                                        {
                                            LogWarning(string.Format(LogStrings.Warning_UserIsNotMappedOrResolving, (currentUser as IFieldUserValue).LookupValue, fieldToCopy.FieldName), LogStrings.Heading_CopyingPageMetadata);
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        LogWarning(string.Format(LogStrings.Warning_UserIsNotResolving, (currentUser as IFieldUserValue).LookupValue, ex.Message), LogStrings.Heading_CopyingPageMetadata);
                                    }
                                }

                                if (userValues.Count > 0)
                                {
                                    targetPage.ListItemAllFields[fieldToCopy.FieldName] = targetField.NewFieldValueCollection(userValues);
                                }
                                else
                                {
                                    // Clear target field - needed in overwrite scenarios
                                    targetPage.ListItemAllFields[fieldToCopy.FieldName] = null;
                                }
                            }
                        }
                        else
                        {
                            // Handling of "special" fields

                            // PostCategory is a default field on a blog post, but it's a lookup. Let's copy as regular field
                            if (fieldToCopy.FieldId.Equals(Constants.PostCategory))
                            {
                                string postCategoryFieldValue = null;
                                var categories = ((IFieldValueCollection)pageTransformationInformation.SourcePage[fieldToCopy.FieldName]).Values
                                    .Cast<IFieldLookupValue>().ToArray();
                                if (categories.Length > 1)
                                {
                                    postCategoryFieldValue += ";#";
                                    foreach (IFieldLookupValue fieldLookupValue in categories)
                                    {
                                        postCategoryFieldValue = postCategoryFieldValue + fieldLookupValue.LookupValue + ";#";
                                    }
                                }
                                else
                                {
                                    if (categories.Length == 1)
                                    {
                                        postCategoryFieldValue = categories[0].LookupValue;
                                    }
                                }

                                targetPage.ListItemAllFields[fieldToCopy.FieldName] = postCategoryFieldValue;
                            }
                            // Regular field handling
                            else
                            {
                                targetPage.ListItemAllFields[fieldToCopy.FieldName] = targetField.CopyFieldValue(pageTransformationInformation.SourcePage[fieldToCopy.FieldName]);
                            }
                        }

                        isDirty = true;
                        LogInfo($"{LogStrings.TransformCopyingMetaDataField} {fieldToCopy.FieldName}", LogStrings.Heading_CopyingPageMetadata);
                    }
                    else
                    {
                        LogWarning($"{LogStrings.TransformCopyingMetaDataFieldSkipped} {fieldToCopy.FieldName}", LogStrings.Heading_CopyingPageMetadata);
                    }
                }

                if (isDirty)
                {
                    targetPage.ListItemAllFields.UpdateOverwriteVersion();
                    targetPage.ListItemAllFields.LoadBatch();
                    targetPage.PnPContext.Execute();
                    isDirty = false;
                }

                #endregion
            }
        }

        /// <summary>
        /// Gets the version of the assembly
        /// </summary>
        /// <returns></returns>
        internal string GetVersion()
        {
            try
            {
                var coreAssembly = Assembly.GetExecutingAssembly();
                return ((AssemblyFileVersionAttribute)coreAssembly.GetCustomAttribute(typeof(AssemblyFileVersionAttribute))).Version.ToString();
            }
            catch (Exception ex)
            {
                LogError(LogStrings.Error_GetVersionError, LogStrings.Heading_GetVersion, ex, true);
            }

            return "undefined";
        }

        internal void InitMeasurement()
        {
            try
            {
                if (File.Exists(ExecutionLog))
                {
                    File.Delete(ExecutionLog);
                }
            }
            catch { }
        }

        internal void Start()
        {
            watch = Stopwatch.StartNew();
        }

        internal void Stop(string method)
        {
            watch.Stop();
            var elapsedTime = watch.ElapsedMilliseconds;
            File.AppendAllText(ExecutionLog, $"{method};{elapsedTime}{Environment.NewLine}");
        }

        /// <summary>
        /// Appends the .aspx extension to a target page name when it is missing, matching the name the page is
        /// actually saved under. The comparison is case insensitive, so an existing extension is never duplicated.
        /// Saving a client side page appends the extension on its own, so the collision probe, the save and the
        /// readback must all be given the same canonical name or they resolve to different files.
        /// </summary>
        /// <param name="targetPageName">Target page name supplied by the caller or derived from the source page</param>
        /// <returns>The target page name as it will exist in the Site Pages library</returns>
        internal static string EnsureAspxExtension(string targetPageName)
        {
            if (string.IsNullOrEmpty(targetPageName))
            {
                return targetPageName;
            }

            return targetPageName.EndsWith(".aspx", StringComparison.InvariantCultureIgnoreCase)
                ? targetPageName
                : $"{targetPageName}.aspx";
        }

        /// <summary>
        /// Ensures the Core context properties needed by a transformation are loaded.
        /// </summary>
        /// <param name="clientContext">Caller-owned Core context.</param>
        /// <param name="isTargetContext">Whether target-only web properties are required.</param>
        internal void LoadClientObject(PnPContext clientContext, bool isTargetContext)
        {
            if (clientContext != null)
            {
                SharePointOnlineSupport.Validate(clientContext);
                // Load all web properties needed further one
                clientContext.Web.GetUrl();
                if (isTargetContext)
                {
                    clientContext.Web.EnsureProperties(p => p.Id, p => p.ServerRelativeUrl,
                        p => p.RootFolder.QueryProperties(f => f.WelcomePage), p => p.Language, p => p.WebTemplate);
                }
                else
                {
                    clientContext.Web.EnsureProperties(p => p.Id, p => p.ServerRelativeUrl,
                        p => p.RootFolder.QueryProperties(f => f.WelcomePage), p => p.Language);
                }
                clientContext.Site.EnsureProperties(p => p.Id, p => p.Url,
                    p => p.RootWeb.QueryProperties(web => web.ServerRelativeUrl));
            }
        }

        internal void PopulateGlobalProperties(PnPContext sourceContext, PnPContext targetContext)
        {
            // Azure AD Tenant ID
            if (targetContext != null)
            {
                // Cache tenant id
                this.pageTelemetry.LoadAADTenantId(targetContext);
            }
            else
            {
                // Cache tenant id
                this.pageTelemetry.LoadAADTenantId(sourceContext);
            }
        }

        /// <summary>
        /// Validates settings when doing a cross farm transformation
        /// </summary>
        /// <param name="baseTransformationInformation">Transformation Information</param>
        /// <remarks>Will disable feature if not supported</remarks>
        internal void CrossFarmTransformationValidation(BaseTransformationInformation baseTransformationInformation)
        {
            // Source only context - allow item level permissions
            // Source to target same base address - allow item level permissions
            // Source to target difference base address - disallow item level permissions

            if (targetClientContext != null && sourceClientContext != null)
            {
                if (!sourceClientContext.Uri.AbsoluteUri.Equals(targetClientContext.Uri.AbsoluteUri, StringComparison.InvariantCultureIgnoreCase))
                {
                    baseTransformationInformation.IsCrossSiteTransformation = true;
                }

                var sourceUrl = sourceClientContext.Uri.AbsoluteUri.GetBaseUrl();
                var targetUrl = targetClientContext.Uri.AbsoluteUri.GetBaseUrl();

                // Override the setting for keeping item level permissions
                if (!sourceUrl.Equals(targetUrl, StringComparison.InvariantCultureIgnoreCase))
                {   
                    // Set a global flag to indicate this is a cross farm transformation (on-prem to SPO tenant or SPO Tenant A to SPO Tenant B)
                    baseTransformationInformation.IsCrossFarmTransformation = true;
                }
            }

            if (sourceClientContext != null)
            {
                baseTransformationInformation.SourceVersion = GetVersion(sourceClientContext);
                baseTransformationInformation.SourceVersionNumber = GetExactVersion(sourceClientContext);
            }

            if (targetClientContext != null)
            {
                baseTransformationInformation.TargetVersion = GetVersion(targetClientContext);
                baseTransformationInformation.TargetVersionNumber = GetExactVersion(targetClientContext);
            }

            if (sourceClientContext != null && targetClientContext == null)
            {
                baseTransformationInformation.TargetVersion = baseTransformationInformation.SourceVersion;
                baseTransformationInformation.TargetVersionNumber = baseTransformationInformation.SourceVersionNumber;
            }

        }

        internal bool IsWikiPage(string pageType)
        {
            return pageType.Equals("WikiPage", StringComparison.InvariantCultureIgnoreCase);
        }

        internal bool IsPublishingPage(string pageType)
        {
            return pageType.Equals("PublishingPage", StringComparison.InvariantCultureIgnoreCase);
        }

        internal bool IsWebPartPage(string pageType)
        {
            return pageType.Equals("WebPartPage", StringComparison.InvariantCultureIgnoreCase);
        }

        internal bool IsBlogPage(string pageType)
        {
            return pageType.Equals("BlogPage", StringComparison.InvariantCultureIgnoreCase);
        }

        internal bool IsDelveBlogPage(string pageType)
        {
            return pageType.Equals("DelveBlogPage", StringComparison.InvariantCultureIgnoreCase);
        }

        internal bool IsClientSidePage(string pageType)
        {
            return pageType.Equals("ClientSidePage", StringComparison.InvariantCultureIgnoreCase);
        }

        internal bool IsAspxPage(string pageType)
        {
            return pageType.Equals("AspxPage", StringComparison.InvariantCultureIgnoreCase);
        }

        internal void StoreSourcePageInformationToKeep(IListItem sourcePage)
        {
            this.SourcePageAuthor = sourcePage[Constants.CreatedByField] as IFieldUserValue;
            this.SourcePageEditor = sourcePage[Constants.ModifiedByField] as IFieldUserValue;

            // Ensure to interprete time correctly: SPO stores in UTC, but we'll need to push back in local
            if (DateTime.TryParse(sourcePage[Constants.CreatedField].ToString(), out DateTime created))
            {
                DateTime createdIsUtc = DateTime.SpecifyKind(created, DateTimeKind.Utc);
                this.SourcePageCreated = createdIsUtc.ToLocalTime();
            }
            if (DateTime.TryParse(sourcePage[Constants.ModifiedField].ToString(), out DateTime modified))
            {
                DateTime modifiedIsUtc = DateTime.SpecifyKind(modified, DateTimeKind.Utc);
                this.SourcePageModified = modifiedIsUtc.ToLocalTime();
            }
        }

        internal void UpdateTargetPageWithSourcePageInformation(IListItem targetPage, BaseTransformationInformation baseTransformationInformation, string serverRelativePathForModernPage, bool crossSiteTransformation)
        {
            try
            {
                IFieldUserValue pageAuthor = this.SourcePageAuthor;
                IFieldUserValue pageEditor = this.SourcePageEditor;

                if (crossSiteTransformation && baseTransformationInformation.KeepPageCreationModificationInformation)
                {
                    // If transformtion is cross site collection we'll need to lookup users again
                    // Using a cloned context to not mess up with the pending list item updates
                    using (PnPContext clonedTargetContext = targetClientContext.Clone(new Uri(targetClientContext.Web.GetUrl())))
                    {
                        var srcPageAuthor = this.userTransformator.RemapPrincipal(this.sourceClientContext, this.SourcePageAuthor);
                        var srcPageEditor = this.userTransformator.RemapPrincipal(this.sourceClientContext, this.SourcePageEditor);

                        ISharePointUser pageAuthorUser = clonedTargetContext.Web.EnsureUser(srcPageAuthor);
                        ISharePointUser pageEditorUser = clonedTargetContext.Web.EnsureUser(srcPageEditor);
                        pageAuthorUser.LoadBatch();
                        pageEditorUser.LoadBatch();
                        clonedTargetContext.Execute();

                        // Prep a new FieldUserValue object instance and update the list item
                        pageAuthor = targetPage.ParentList.Fields.GetByInternalNameOrTitle(Constants.CreatedByField).NewFieldUserValue(pageAuthorUser.Id);
                        pageEditor = targetPage.ParentList.Fields.GetByInternalNameOrTitle(Constants.ModifiedByField).NewFieldUserValue(pageEditorUser.Id);
                    }
                }

                if (baseTransformationInformation.KeepPageCreationModificationInformation || baseTransformationInformation.PostAsNews)
                {
                    if (baseTransformationInformation.KeepPageCreationModificationInformation)
                    {
                        // All 4 fields have to be set!
                        targetPage[Constants.CreatedByField] = pageAuthor;
                        targetPage[Constants.ModifiedByField] = pageEditor;
                        targetPage[Constants.CreatedField] = this.SourcePageCreated;
                        targetPage[Constants.ModifiedField] = this.SourcePageModified;
                    }

                    if (baseTransformationInformation.PostAsNews)
                    {
                        targetPage[Constants.PromotedStateField] = "2";

                        // Determine what will be the publishing date that will show up in the news rollup
                        if (baseTransformationInformation.KeepPageCreationModificationInformation)
                        {
                            targetPage[Constants.FirstPublishedDateField] = this.SourcePageModified;
                        }
                        else
                        {
                            targetPage[Constants.FirstPublishedDateField] = targetPage[Constants.ModifiedField];
                        }
                    }

                    targetPage.UpdateOverwriteVersion();

                    if (baseTransformationInformation.PublishCreatedPage)
                    {
                        IFile targetPageFile = ((targetPage.PnPContext) as PnPContext).Web.GetFileByServerRelativeUrl(serverRelativePathForModernPage);
                        targetPageFile.LoadBatch();
                        // Try to publish, if publish is not needed/possible (e.g. when no minor/major versioning set) then this will return an error that we'll be ignoring
                        targetPageFile.Publish(LogStrings.PublishMessage);
                    }
                }

                targetPage.PnPContext.Execute();
            }
            catch (Exception ex)
            {
                // Eat exceptions as this is not critical for the generated page
                LogWarning(string.Format(LogStrings.Warning_NonCriticalErrorDuringPublish, ex.Message), LogStrings.Heading_ArticlePageHandling);
            }
        }


        /// <summary>
        /// Loads the User Mapping Files
        /// </summary>
        /// <param name="baseTransformationInformation"></param>
        internal void InitializeUserMapping(BaseTransformationInformation baseTransformationInformation)
        {
            // Create an instance of the user transformation class
            this.userTransformator = new UserTransformator(baseTransformationInformation, sourceClientContext, targetClientContext, RegisteredLogObservers);
        }

        /// <summary>
        /// Loads the term mapping transform
        /// </summary>
        /// <param name="baseTransformationInformation"></param>
        internal void InitializeTermMapping(BaseTransformationInformation baseTransformationInformation)
        {
            this.termTransformator = new TermTransformator(baseTransformationInformation, sourceClientContext, targetClientContext, RegisteredLogObservers);
        }

        internal void SetAuthorInPageHeader(PnPContext targetContext, IPage targetClientSidePage)
        {
            try
            {
                string userToResolve = this.userTransformator.RemapPrincipal(this.sourceClientContext, this.SourcePageAuthor);

                var ensuredPageAuthorUser = CacheManager.Instance.GetEnsuredUser(targetContext, userToResolve);
                if (ensuredPageAuthorUser != null)
                {
                    var author = CacheManager.Instance.GetUserFromUserList(targetContext, ensuredPageAuthorUser.Id);

                    if (author != null)
                    {
                        if (!author.IsGroup)
                        {
                            // Don't serialize null values
                            var jsonSerializerSettings = new JsonSerializerSettings()
                            {
                                MissingMemberHandling = MissingMemberHandling.Ignore,
                                NullValueHandling = NullValueHandling.Ignore
                            };

                            var json = JsonConvert.SerializeObject(author, jsonSerializerSettings);

                            if (!string.IsNullOrEmpty(json))
                            {
                                targetClientSidePage.PageHeader.Authors = json;
                            }
                        }
                    }
                    else
                    {
                        this.LogWarning(string.Format(LogStrings.Warning_PageHeaderAuthorNotSet, $"Author {this.SourcePageAuthor.LookupValue} could not be resolved."), LogStrings.Heading_ArticlePageHandling);
                    }
                }
                else
                {
                    this.LogWarning(string.Format(LogStrings.Warning_PageHeaderAuthorNotSet, $"Author {this.SourcePageAuthor.LookupValue} could not be resolved."), LogStrings.Heading_ArticlePageHandling);
                }
            }
            catch (Exception ex)
            {
                this.LogWarning(string.Format(LogStrings.Warning_PageHeaderAuthorNotSet, ex.Message), LogStrings.Heading_ArticlePageHandling);
            }
        }

        internal static string LoadFile(string fileName)
        {
            var fileContent = "";
            using (Stream stream = typeof(BasePageTransformator).Assembly.GetManifestResourceStream(fileName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    fileContent = reader.ReadToEnd();
                }
            }

            return fileContent;
        }

        internal static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
        #endregion

    }
}

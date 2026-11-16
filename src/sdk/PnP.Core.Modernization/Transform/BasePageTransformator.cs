using AngleSharp.Html.Parser;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Taxonomy;
using Newtonsoft.Json;
using PnP.Core.Services;
using PnP.Core.Modernization.Cache;
using PnP.Core.Modernization.Entities;
using PnP.Core.Modernization.Extensions;
using PnP.Core.Modernization.Functions;
using PnP.Core.Modernization.Telemetry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using PnPCore = PnP.Core.Model.SharePoint;

namespace PnP.Core.Modernization.Transform
{
    /// <summary>
    /// Base page transformator class that contains logic that applies for all page transformations
    /// </summary>
    public abstract class BasePageTransformator : BaseTransform
    {
        internal ClientContext sourceClientContext;
        internal ClientContext targetClientContext;
        internal Stopwatch watch;
        internal const string ExecutionLog = "execution.csv";
        internal PageTransformation pageTransformation;
        internal UserTransformator userTransformator;
        internal TermTransformator termTransformator;
        internal string version = "undefined";
        internal PageTelemetry pageTelemetry;
        internal bool isRootPage = false;
        // source page information to "restore"
        internal FieldUserValue SourcePageAuthor;
        internal FieldUserValue SourcePageEditor;
        internal DateTime SourcePageCreated;
        internal DateTime SourcePageModified;

        #region Helper methods

        internal void SavePage(PnPCore.IPage page, string pageName, BaseTransformationInformation information,
            ClientContext context, Func<Uri, string> importExternalImage = null)
        {
            // A saved page ID is needed for SharePoint to select and retain the page-owned asset folder.
            page.Save(pageName);
            var textParts = page.Controls.OfType<PnPCore.IPageText>()
                .Where(part => !string.IsNullOrEmpty(part.Text) && part.Text.IndexOf("<img", StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
            if (textParts.Count == 0)
            {
                return;
            }

            if (importExternalImage == null)
            {
                var importer = new ExternalImageImporter(new Uri(context.Url), page.PageId.GetValueOrDefault(), pageName,
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
                        return fileServerRelativeUrl.Replace($"/{System.IO.Path.GetFileName(fileServerRelativeUrl)}", "");

                    }
                    else if (fieldName.Equals(Constants.FileLeafRefField))
                    {
                        // e.g. demo16.aspx
                        return System.IO.Path.GetFileName(fileServerRelativeUrl);
                    }
                }
                return "";
            }
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

        internal bool IsRootPage(File file)
        {
            if (file != null)
            {
                return true;
            }

            return false;
        }

        internal void RemoveEmptyTextParts(PnPCore.IPage targetPage)
        {
            var textParts = targetPage.Controls.Where(p => p is PnPCore.IPageText);
            if (textParts != null && textParts.Any())
            {
                HtmlParser parser = new HtmlParser(new HtmlParserOptions() { IsEmbedded = true });

                foreach (var textPart in textParts.ToList())
                {
                    using (var document = parser.ParseDocument(((PnPCore.IPageText)textPart).Text))
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

        internal void RemoveEmptySectionsAndColumns(PnPCore.IPage targetPage)
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
                if (section.Type == PnPCore.CanvasSectionTemplate.TwoColumn ||
                    section.Type == PnPCore.CanvasSectionTemplate.TwoColumnLeft ||
                    section.Type == PnPCore.CanvasSectionTemplate.TwoColumnRight ||
                    section.Type == PnPCore.CanvasSectionTemplate.TwoColumnVerticalSection ||
                    section.Type == PnPCore.CanvasSectionTemplate.TwoColumnLeftVerticalSection ||
                    section.Type == PnPCore.CanvasSectionTemplate.TwoColumnRightVerticalSection)
                {
                    var emptyColumn = section.Columns.Where(p => p.Controls.Count == 0 && !p.IsVerticalSectionColumn).FirstOrDefault();
                    if (emptyColumn != null)
                    {
                        // drop the empty column and change to single column section
                        section.Columns.Remove(emptyColumn);

                        if (section.Type == PnPCore.CanvasSectionTemplate.TwoColumnVerticalSection ||
                            section.Type == PnPCore.CanvasSectionTemplate.TwoColumnLeftVerticalSection ||
                            section.Type == PnPCore.CanvasSectionTemplate.TwoColumnRightVerticalSection)
                        {
                            section.Type = PnPCore.CanvasSectionTemplate.OneColumnVerticalSection;
                        }
                        else
                        {
                            section.Type = PnPCore.CanvasSectionTemplate.OneColumn;
                        }

                        section.Columns.First().ResetColumn(0, 12);
                    }
                }
                else if (section.Type == PnPCore.CanvasSectionTemplate.ThreeColumn ||
                         section.Type == PnPCore.CanvasSectionTemplate.ThreeColumnVerticalSection)
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

                            if (section.Type == PnPCore.CanvasSectionTemplate.ThreeColumnVerticalSection)
                            {
                                section.Type = PnPCore.CanvasSectionTemplate.OneColumnVerticalSection;
                            }
                            else
                            {
                                section.Type = PnPCore.CanvasSectionTemplate.OneColumn;
                            }

                            section.Columns.First().ResetColumn(0, 12);
                        }
                        else if (emptyColumns.Any() && emptyColumns.Count() == 1)
                        {
                            // Remove the empty column and change to two column section
                            section.Columns.Remove(emptyColumns.First());

                            if (section.Type == PnPCore.CanvasSectionTemplate.ThreeColumnVerticalSection)
                            {
                                section.Type = PnPCore.CanvasSectionTemplate.TwoColumnVerticalSection;
                            }
                            else
                            {
                                section.Type = PnPCore.CanvasSectionTemplate.TwoColumn;
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

        internal void ApplyItemLevelPermissions(bool hasTargetContext, ListItem item, ListItemPermission lip, bool alwaysBreakItemLevelPermissions = false)
        {

            if (lip == null || item == null)
            {
                return;
            }

            // Break permission inheritance on the item if not done yet
            if (alwaysBreakItemLevelPermissions || !item.HasUniqueRoleAssignments)
            {
                item.BreakRoleInheritance(false, false);
                item.Context.ExecuteQueryRetry();
            }

            // Cross site collection flow (can be from SPO to SPO, but also from SP On-Premises to SPO)
            if (hasTargetContext)
            {
                try
                {

                    // Ensure principals are available in the target site
                    Dictionary<string, Principal> targetPrincipals = new Dictionary<string, Principal>(lip.Principals.Count);

                    foreach (var principal in lip.Principals)
                    {
                        var targetPrincipal = GetPrincipal(this.targetClientContext.Web, principal.Key, hasTargetContext);
                        if (targetPrincipal != null)
                        {
                            if (!targetPrincipals.ContainsKey(principal.Key))
                            {
                                targetPrincipals.Add(principal.Key, targetPrincipal);
                            }
                        }
                    }

                    // Assign item level permissions          
                    foreach (var roleAssignment in lip.RoleAssignments)
                    {
                        if (targetPrincipals.TryGetValue(roleAssignment.Member.LoginName, out Principal principal))
                        {
                            var roleDefinitionBindingCollection = new RoleDefinitionBindingCollection(this.targetClientContext);
                            bool hasRoleAdded = false;
                            foreach (var roleDef in roleAssignment.RoleDefinitionBindings)
                            {
                                if (roleDef.Id != 1073741825) // Limited Access permission
                                {
                                    var targetRoleDef = this.targetClientContext.Web.RoleDefinitions.GetByName(roleDef.Name);
                                    if (targetRoleDef != null)
                                    {
                                        roleDefinitionBindingCollection.Add(targetRoleDef);
                                        hasRoleAdded = true;
                                    }
                                }
                            }

                            // Prevent referencing empty collections
                            if (hasRoleAdded)
                            {
                                item.RoleAssignments.Add(principal, roleDefinitionBindingCollection);
                            }
                            
                        }
                    }

                    this.targetClientContext.ExecuteQueryRetry();

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
                    foreach (var roleAssignment in lip.RoleAssignments)
                    {
                        if (lip.Principals.TryGetValue(roleAssignment.Member.LoginName, out Principal principal))
                        {
                            var roleDefinitionBindingCollection = new RoleDefinitionBindingCollection(this.sourceClientContext);
                            foreach (var roleDef in roleAssignment.RoleDefinitionBindings)
                            {
                                roleDefinitionBindingCollection.Add(roleDef);
                            }

                            item.RoleAssignments.Add(principal, roleDefinitionBindingCollection);
                        }
                    }

                    this.sourceClientContext.ExecuteQueryRetry();

                }
                catch (Exception ex)
                {
                    LogError(string.Format(LogStrings.Error_ApplyPermissionFailedToApplyPermissions, ex.Message), LogStrings.Heading_ApplyItemLevelPermissions, ex);
                }
            }

            LogInfo(LogStrings.TransformCopiedItemPermissions, LogStrings.Heading_ApplyItemLevelPermissions);
        }

        internal ListItemPermission GetItemLevelPermissions(bool hasTargetContext, List pagesLibrary, ListItem source, ListItem target)
        {
            ListItemPermission lip = null;

            if (source.IsPropertyAvailable("HasUniqueRoleAssignments") && source.HasUniqueRoleAssignments)
            {
                // You need to have the ManagePermissions permission before item level permissions can be copied
                if (pagesLibrary.EffectiveBasePermissions.Has(PermissionKind.ManagePermissions))
                {
                    // Copy the unique permissions from source to target
                    // Get the unique permissions
                    this.sourceClientContext.Load(source, a => a.EffectiveBasePermissions, a => a.RoleAssignments.Include(roleAsg => roleAsg.Member.LoginName,
                        roleAsg => roleAsg.RoleDefinitionBindings.Include(roleDef => roleDef.Id, roleDef => roleDef.Name, roleDef => roleDef.Description)));
                    this.sourceClientContext.ExecuteQueryRetry();

                    if (source.EffectiveBasePermissions.Has(PermissionKind.ManagePermissions))
                    {
                        // Load the site groups
                        this.sourceClientContext.Load(this.sourceClientContext.Web.SiteGroups, p => p.Include(g => g.LoginName));

                        // Get target page information
                        if (hasTargetContext)
                        {
                            this.targetClientContext.Load(target, p => p.HasUniqueRoleAssignments, p => p.RoleAssignments);
                            this.targetClientContext.Load(this.targetClientContext.Web, p => p.RoleDefinitions);
                            this.targetClientContext.Load(this.targetClientContext.Web.SiteGroups, p => p.Include(g => g.LoginName));
                        }
                        else
                        {
                            this.sourceClientContext.Load(target, p => p.HasUniqueRoleAssignments, p => p.RoleAssignments);
                        }

                        this.sourceClientContext.ExecuteQueryRetry();

                        if (hasTargetContext)
                        {
                            try
                            {
                                this.targetClientContext.ExecuteQueryRetry();
                            }
                            catch(Exception)
                            {
                                LogWarning(LogStrings.Warning_TransformGetItemPermissionsAccessDenied, LogStrings.Heading_ApplyItemLevelPermissions);
                                return lip;
                            }
                        }

                        Dictionary<string, Principal> principals = new Dictionary<string, Principal>(10);
                        lip = new ListItemPermission()
                        {
                            RoleAssignments = source.RoleAssignments,
                            Principals = principals
                        };

                        // Apply new permissions
                        foreach (var roleAssignment in source.RoleAssignments)
                        {
                            var principal = GetPrincipal(this.sourceClientContext.Web, roleAssignment.Member.LoginName, hasTargetContext, true);
                            if (principal != null)
                            {
                                if (!lip.Principals.ContainsKey(roleAssignment.Member.LoginName))
                                {
                                    lip.Principals.Add(roleAssignment.Member.LoginName, principal);
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

        internal Principal GetPrincipal(Web web, string principalInput, bool hasTargetContext, bool reading = false)
        {

            //On-Prem User Mapping - Dont replace the source
            if (hasTargetContext && !reading)
            {
                principalInput = this.userTransformator.RemapPrincipal(principalInput);
            }

            Principal principal = web.SiteGroups.FirstOrDefault(g => g.LoginName.Equals(principalInput, StringComparison.OrdinalIgnoreCase));

            if (principal == null)
            {
                if (principalInput.Contains("#ext#"))
                {
                    principal = web.SiteUsers.FirstOrDefault(u => u.LoginName.Equals(principalInput));

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
                        web.Context.ExecuteQueryRetry();
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

        internal void CopyPageMetadata(PageTransformationInformation pageTransformationInformation, string pageType, File targetPage, List targetPagesLibrary)
        {
            var fieldsToCopy = CacheManager.Instance.GetFieldsToCopy(this.sourceClientContext.Web, targetPagesLibrary, pageType);
            bool listItemWasReloaded = false;
            if (fieldsToCopy.Count > 0)
            {
                // Load the target page list item
                targetPage.Context.Load(targetPage.ListItemAllFields);
                targetPage.Context.ExecuteQueryRetry();

                pageTransformationInformation.SourcePage.EnsureProperty(p => p.ContentType);

                // regular fields
                bool isDirty = false;
                bool isSourceInitialized = false;
                List sourceSitesPagesLibrary = default;

                var sitePagesServerRelativeUrl = PnP.Framework.Utilities.UrlUtility.Combine((targetPage.Context as ClientContext).Web.ServerRelativeUrl.TrimEnd(new char[] { '/' }), "sitepages");
                List targetSitePagesLibrary = (targetPage.Context as ClientContext).Web.GetList(sitePagesServerRelativeUrl);
                targetPage.Context.Load(targetSitePagesLibrary, l => l.Fields.IncludeWithDefaultProperties(f => f.Id, f => f.Title, f => f.Hidden, f => f.InternalName, f => f.DefaultValue, f => f.Required, f => f.StaticName));
                targetPage.Context.ExecuteQueryRetry();

                

                string contentTypeId = CacheManager.Instance.GetContentTypeId(targetPage.ListItemAllFields.ParentList, pageTransformationInformation.SourcePage.ContentType.Name);
                if (!string.IsNullOrEmpty(contentTypeId))
                {
                    // Load the target page list item, needs to be loaded as it was previously saved and we need to avoid version conflicts
                    targetPage.Context.Load(targetPage.ListItemAllFields);
                    targetPage.Context.ExecuteQueryRetry();
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
                            targetPage.Context.Load(targetPage.ListItemAllFields);
                            targetPage.Context.ExecuteQueryRetry();
                            listItemWasReloaded = true;
                        }

                        if (!isSourceInitialized)
                        {
                            if (pageTransformationInformation.IsCrossSiteTransformation)
                            {

                                //TODO: Check if there is a scenario where the source page may origniate from another library on same site/web
                                // This is needed to get the field term set id
                                sourceSitesPagesLibrary = pageTransformationInformation.SourcePage.ParentList;
                                this.sourceClientContext.Load(sourceSitesPagesLibrary, l => l.Fields.IncludeWithDefaultProperties(f => f.Id, f => f.Title, f => f.Hidden, f => f.InternalName, f => f.DefaultValue, f => f.Required));
                                this.sourceClientContext.ExecuteQueryRetry();
                            }

                            InitializeTermMapping(pageTransformationInformation);

                            isSourceInitialized = true;
                        }

                        var taxFieldBeforeCast = targetSitePagesLibrary.Fields.Where(p => p.StaticName.Equals(fieldToCopy.FieldName)).FirstOrDefault();
                        Field sourceTaxFieldBeforeCast = null;
                        if (pageTransformationInformation.IsCrossSiteTransformation)
                        {
                            sourceTaxFieldBeforeCast = sourceSitesPagesLibrary.Fields.Where(p => p.StaticName.Equals(fieldToCopy.FieldName)).FirstOrDefault();
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
                                        var taxField = targetPage.Context.CastTo<TaxonomyField>(taxFieldBeforeCast);
                                        var srcTaxField = this.sourceClientContext.CastTo<TaxonomyField>(sourceTaxFieldBeforeCast);
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
                                            if (pageTransformationInformation.SourcePage[fieldToCopy.FieldName] is TaxonomyFieldValueCollection)
                                            {
                                                var valueCollectionToCopy = (pageTransformationInformation.SourcePage[fieldToCopy.FieldName] as TaxonomyFieldValueCollection);
                                                if (!skipTermMapping)
                                                {
                                                    //Term Transformator
                                                    var resultTermTransform = termTransformator.TransformCollection(valueCollectionToCopy);
                                                    valueCollectionToCopy = resultTermTransform.Item1;

                                                    var taxonomyFieldValueArray = valueCollectionToCopy.Except(resultTermTransform.Item2).Select(taxonomyFieldValue => $"{taxonomyFieldValue.Label}|{taxonomyFieldValue.TermGuid}");

                                                    //If not multi-valued exception is thrown
                                                    if (taxonomyFieldValueArray.Count() == 1)
                                                    {
                                                        taxField.SetFieldValueByValue(targetPage.ListItemAllFields, valueCollectionToCopy[0]);
                                                    }
                                                    else
                                                    {
                                                        taxField.SetFieldValueByLabelGuidPair(targetPage.ListItemAllFields, string.Join(";", taxonomyFieldValueArray));
                                                    }

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
                                                    var taxonomyFieldValueArray = valueCollectionToCopy.Select(taxonomyFieldValue => $"-1;#{taxonomyFieldValue.Label}|{taxonomyFieldValue.TermGuid}");
                                                    var valueCollection = new TaxonomyFieldValueCollection(targetPage.Context, string.Join(";#", taxonomyFieldValueArray), taxField);
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
                                                    var valueCollection = new TaxonomyFieldValueCollection(targetPage.Context, string.Join(";#", taxonomyFieldValueArray), taxField);
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
                                                    var valueCollection = new TaxonomyFieldValueCollection(targetPage.Context, string.Join(";#", taxonomyFieldValueArray), taxField);
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
                                        var taxField = targetPage.Context.CastTo<TaxonomyField>(taxFieldBeforeCast);
                                        var taxValue = new TaxonomyFieldValue();

                                        if (pageTransformationInformation.SourcePage[fieldToCopy.FieldName] != null)
                                        {
                                            var srcTaxField = this.sourceClientContext.CastTo<TaxonomyField>(sourceTaxFieldBeforeCast);

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

                                            if (pageTransformationInformation.SourcePage[fieldToCopy.FieldName] is TaxonomyFieldValue)
                                            {
                                                var labelToSet = (pageTransformationInformation.SourcePage[fieldToCopy.FieldName] as TaxonomyFieldValue).Label;
                                                var termGuidToSet = (pageTransformationInformation.SourcePage[fieldToCopy.FieldName] as TaxonomyFieldValue).TermGuid;

                                                if (!skipTermMapping)
                                                {
                                                    //Term Transformator
                                                    var termTranform = termTransformator.Transform(new TermData() { TermGuid = new Guid(termGuidToSet), TermLabel = labelToSet });
                                                    if (termTranform.IsTermResolved)
                                                    {
                                                        taxValue.Label = termTranform.TermLabel;
                                                        taxValue.TermGuid = termTranform.TermGuid.ToString();
                                                        taxValue.WssId = -1;
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
                                                    taxValue.Label = labelToSet;
                                                    taxValue.TermGuid = termGuidToSet;
                                                    taxValue.WssId = -1;
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
                                                        taxValue.Label = transformTerm.TermLabel;
                                                        taxValue.TermGuid = transformTerm.TermGuid.ToString();
                                                        taxValue.WssId = -1;
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
                                                    taxValue.Label = label;
                                                    taxValue.TermGuid = termGuid;
                                                    taxValue.WssId = -1;
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
                        targetPage.Context.Load(targetPage.ListItemAllFields);
                        targetPage.Context.ExecuteQueryRetry();
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
                    var targetField = targetSitePagesLibrary.Fields.Where(p => p.StaticName.Equals(fieldToCopy.FieldName)).FirstOrDefault();

                    if (targetField != null && pageTransformationInformation.SourcePage[fieldToCopy.FieldName] != null)
                    {
                        if (fieldToCopy.FieldType == "User" || fieldToCopy.FieldType == "UserMulti")
                        {
                            object fieldValueToSet = pageTransformationInformation.SourcePage[fieldToCopy.FieldName];
                            if (fieldValueToSet is FieldUserValue)
                            {
                                try
                                {
                                    // Source User
                                    var fieldUser = (fieldValueToSet as FieldUserValue).LookupValue;
                                    // Mapped target user
                                    if (this.userTransformator != null)
                                    {
                                        fieldUser = this.userTransformator.RemapPrincipal(this.sourceClientContext, (fieldValueToSet as FieldUserValue));
                                    }

                                    // Ensure user exists on target site
                                    var ensuredUserOnTarget = CacheManager.Instance.GetEnsuredUser((targetPage.Context as ClientContext), fieldUser);
                                    if (ensuredUserOnTarget != null)
                                    {
                                        // Prep a new FieldUserValue object instance and update the list item
                                        var newUser = new FieldUserValue()
                                        {
                                            LookupId = ensuredUserOnTarget.Id
                                        };
                                        targetPage.ListItemAllFields[fieldToCopy.FieldName] = newUser;
                                    }
                                    else
                                    {
                                        // Clear target field - needed in overwrite scenarios
                                        targetPage.ListItemAllFields[fieldToCopy.FieldName] = null;
                                        LogWarning(string.Format(LogStrings.Warning_UserIsNotMappedOrResolving, (fieldValueToSet as FieldUserValue).LookupValue, fieldToCopy.FieldName), LogStrings.Heading_CopyingPageMetadata);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LogWarning(string.Format(LogStrings.Warning_UserIsNotResolving, (fieldValueToSet as FieldUserValue).LookupValue, ex.Message), LogStrings.Heading_CopyingPageMetadata);
                                }
                            }
                            else
                            {
                                List<FieldUserValue> userValues = new List<FieldUserValue>();
                                foreach (var currentUser in (fieldValueToSet as Array))
                                {
                                    try
                                    {
                                        // Source User
                                        var fieldUser = (currentUser as FieldUserValue).LookupValue;
                                        // Mapped target user
                                        if (this.userTransformator != null)
                                        {
                                            fieldUser = this.userTransformator.RemapPrincipal(this.sourceClientContext, (currentUser as FieldUserValue));
                                        }

                                        // Ensure user exists on target site
                                        var ensuredUserOnTarget = CacheManager.Instance.GetEnsuredUser((targetPage.Context as ClientContext), fieldUser);
                                        if (ensuredUserOnTarget != null)
                                        {
                                            // Prep a new FieldUserValue object instance
                                            var newUser = new FieldUserValue()
                                            {
                                                LookupId = ensuredUserOnTarget.Id
                                            };

                                            userValues.Add(newUser);
                                        }
                                        else
                                        {
                                            LogWarning(string.Format(LogStrings.Warning_UserIsNotMappedOrResolving, (currentUser as FieldUserValue).LookupValue, fieldToCopy.FieldName), LogStrings.Heading_CopyingPageMetadata);
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        LogWarning(string.Format(LogStrings.Warning_UserIsNotResolving, (currentUser as FieldUserValue).LookupValue, ex.Message), LogStrings.Heading_CopyingPageMetadata);
                                    }
                                }

                                if (userValues.Count > 0)
                                {
                                    targetPage.ListItemAllFields[fieldToCopy.FieldName] = userValues.ToArray();
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
                                if (((FieldLookupValue[])pageTransformationInformation.SourcePage[fieldToCopy.FieldName]).Length > 1)
                                {
                                    postCategoryFieldValue += ";#";
                                    foreach (var fieldLookupValue in (FieldLookupValue[])pageTransformationInformation.SourcePage[fieldToCopy.FieldName])
                                    {
                                        postCategoryFieldValue = postCategoryFieldValue + fieldLookupValue.LookupValue + ";#";
                                    }
                                }
                                else
                                {
                                    if (((FieldLookupValue[])pageTransformationInformation.SourcePage[fieldToCopy.FieldName]).Length == 1)
                                    {
                                        postCategoryFieldValue = ((FieldLookupValue[])pageTransformationInformation.SourcePage[fieldToCopy.FieldName])[0].LookupValue;
                                    }
                                }

                                targetPage.ListItemAllFields[fieldToCopy.FieldName] = postCategoryFieldValue;
                            }
                            // Regular field handling
                            else
                            {
                                targetPage.ListItemAllFields[fieldToCopy.FieldName] = pageTransformationInformation.SourcePage[fieldToCopy.FieldName];
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
                    targetPage.Context.Load(targetPage.ListItemAllFields);
                    targetPage.Context.ExecuteQueryRetry();
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
                if (System.IO.File.Exists(ExecutionLog))
                {
                    System.IO.File.Delete(ExecutionLog);
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
            System.IO.File.AppendAllText(ExecutionLog, $"{method};{elapsedTime}{Environment.NewLine}");
        }

        /// <summary>
        /// Loads the telemetry and properties for the client object
        /// </summary>
        /// <param name="clientContext"></param>
        /// <param name="isTargetContext"></param>
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

        internal void LoadClientObject(ClientContext clientContext, bool isTargetContext)
        {
            if (clientContext != null)
            {
                clientContext.ClientTag = $"SPDev:PageTransformator";
                // Load all web properties needed further one
                clientContext.Web.GetUrl();
                if (isTargetContext)
                {
                    clientContext.Load(clientContext.Web, p => p.Id, p => p.ServerRelativeUrl, p => p.RootFolder.WelcomePage, p => p.Language, p => p.WebTemplate);
                }
                else
                {
                    clientContext.Load(clientContext.Web, p => p.Id, p => p.ServerRelativeUrl, p => p.RootFolder.WelcomePage, p => p.Language);
                }
                clientContext.Load(clientContext.Site, p => p.Id, p => p.Url);
                // Load RootWeb as its own CSOM object instead of expanding it through Site.
                clientContext.Load(clientContext.Site.RootWeb, p => p.ServerRelativeUrl);
                // Use regular ExecuteQuery as we want to send this custom clienttag
                clientContext.ExecuteQuery();
            }
        }

        internal void PopulateGlobalProperties(ClientContext sourceContext, ClientContext targetContext)
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
                if (!sourceClientContext.Url.Equals(targetClientContext.Url, StringComparison.InvariantCultureIgnoreCase))
                {
                    baseTransformationInformation.IsCrossSiteTransformation = true;
                }

                var sourceUrl = sourceClientContext.Url.GetBaseUrl();
                var targetUrl = targetClientContext.Url.GetBaseUrl();

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

        internal void StoreSourcePageInformationToKeep(ListItem sourcePage)
        {
            this.SourcePageAuthor = sourcePage[Constants.CreatedByField] as FieldUserValue;
            this.SourcePageEditor = sourcePage[Constants.ModifiedByField] as FieldUserValue;

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

        internal void UpdateTargetPageWithSourcePageInformation(ListItem targetPage, BaseTransformationInformation baseTransformationInformation, string serverRelativePathForModernPage, bool crossSiteTransformation)
        {
            try
            {
                FieldUserValue pageAuthor = this.SourcePageAuthor;
                FieldUserValue pageEditor = this.SourcePageEditor;

                if (crossSiteTransformation && baseTransformationInformation.KeepPageCreationModificationInformation)
                {
                    // If transformtion is cross site collection we'll need to lookup users again
                    // Using a cloned context to not mess up with the pending list item updates
                    using (var clonedTargetContext = targetClientContext.Clone(targetClientContext.Web.GetUrl()))
                    {
                        var srcPageAuthor = this.userTransformator.RemapPrincipal(this.sourceClientContext, this.SourcePageAuthor);
                        var srcPageEditor = this.userTransformator.RemapPrincipal(this.sourceClientContext, this.SourcePageEditor);

                        var pageAuthorUser = clonedTargetContext.Web.EnsureUser(srcPageAuthor);
                        var pageEditorUser = clonedTargetContext.Web.EnsureUser(srcPageEditor);
                        clonedTargetContext.Load(pageAuthorUser);
                        clonedTargetContext.Load(pageEditorUser);
                        clonedTargetContext.ExecuteQueryRetry();

                        // Prep a new FieldUserValue object instance and update the list item
                        pageAuthor = new FieldUserValue()
                        {
                            LookupId = pageAuthorUser.Id
                        };

                        pageEditor = new FieldUserValue()
                        {
                            LookupId = pageEditorUser.Id
                        };
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
                        var targetPageFile = ((targetPage.Context) as ClientContext).Web.GetFileByServerRelativeUrl(serverRelativePathForModernPage);
                        targetPage.Context.Load(targetPageFile);
                        // Try to publish, if publish is not needed/possible (e.g. when no minor/major versioning set) then this will return an error that we'll be ignoring
                        targetPageFile.Publish(LogStrings.PublishMessage);
                    }
                }

                targetPage.Context.ExecuteQueryRetry();
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

        internal void SetAuthorInPageHeader(ClientContext targetContext, PnPCore.IPage targetClientSidePage)
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
            using (System.IO.Stream stream = typeof(BasePageTransformator).Assembly.GetManifestResourceStream(fileName))
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(stream))
                {
                    fileContent = reader.ReadToEnd();
                }
            }

            return fileContent;
        }

        internal static System.IO.Stream GenerateStreamFromString(string s)
        {
            var stream = new System.IO.MemoryStream();
            var writer = new System.IO.StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
        #endregion

    }
}

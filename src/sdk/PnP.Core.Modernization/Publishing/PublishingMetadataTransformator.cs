using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Taxonomy;
using PnP.Core.Modernization.Cache;
using PnP.Core.Modernization.Entities;
using PnP.Core.Modernization.Telemetry;
using PnP.Core.Modernization.Transform;
using PnP.Framework.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using PnPCore = PnP.Core.Model.SharePoint;

namespace PnP.Core.Modernization.Publishing
{
    /// <summary>
    /// Class responsible for handling the metadata copying of publishing pages
    /// </summary>
    public class PublishingMetadataTransformator : BaseTransform
    {
        private PublishingPageTransformationInformation publishingPageTransformationInformation;
        private ClientContext sourceClientContext;
        private ClientContext targetClientContext;
        private PnPCore.IPage page;
        private File savedTargetPage;
        private PageLayout pageLayoutMappingModel;
        private PublishingPageTransformation publishingPageTransformation;
        private PublishingFunctionProcessor functionProcessor;
        private UserTransformator userTransformator;
        private TermTransformator termTransformator;

        #region Construction
        public PublishingMetadataTransformator(PublishingPageTransformationInformation publishingPageTransformationInformation, ClientContext sourceClientContext,
            ClientContext targetClientContext, File savedTargetPage, PnPCore.IPage page, PageLayout publishingPageLayoutModel, PublishingPageTransformation publishingPageTransformation,
            UserTransformator userTransformator, IList<ILogObserver> logObservers = null)
        {
            // Register observers
            if (logObservers != null)
            {
                foreach (var observer in logObservers)
                {
                    base.RegisterObserver(observer);
                }
            }

            this.publishingPageTransformationInformation = publishingPageTransformationInformation;
            this.sourceClientContext = sourceClientContext;
            this.targetClientContext = targetClientContext;
            this.page = page;
            this.savedTargetPage = savedTargetPage;
            this.pageLayoutMappingModel = publishingPageLayoutModel;
            this.publishingPageTransformation = publishingPageTransformation;
            this.functionProcessor = new PublishingFunctionProcessor(publishingPageTransformationInformation.SourcePage, sourceClientContext, targetClientContext, this.publishingPageTransformation, publishingPageTransformationInformation as BaseTransformationInformation, base.RegisteredLogObservers);
            this.userTransformator = userTransformator;
            this.termTransformator = new TermTransformator(publishingPageTransformationInformation, sourceClientContext, targetClientContext, base.RegisteredLogObservers);
        }
        #endregion

        /// <summary>
        /// Process the metadata copying (as defined in the used page layout mapping)
        /// </summary>
        public void Transform()
        {
            if (this.pageLayoutMappingModel != null)
            {
                bool isDirty = false;
                bool listItemWasReloaded = false;
                string contentTypeId = null;
                var groupedEnterpriseWikiRatings = this.publishingPageTransformationInformation.EnableEnterpriseWikiCapabilities &&
                    this.publishingPageTransformationInformation.IsEnterpriseWikiPage &&
                    EnterpriseWikiRatings.UsesSameNameMapping(this.pageLayoutMappingModel);

                // Set content type
                if (!string.IsNullOrEmpty(this.pageLayoutMappingModel.AssociatedContentType))
                {
                    contentTypeId = CacheManager.Instance.GetContentTypeId(this.savedTargetPage.ListItemAllFields.ParentList, pageLayoutMappingModel.AssociatedContentType);
                    if (!string.IsNullOrEmpty(contentTypeId))
                    {
                        // Load the target page list item, needs to be loaded as it was previously saved and we need to avoid version conflicts
                        this.targetClientContext.Load(this.savedTargetPage.ListItemAllFields);
                        this.targetClientContext.ExecuteQueryRetry();
                        listItemWasReloaded = true;

                        this.savedTargetPage.ListItemAllFields[Constants.ContentTypeIdField] = contentTypeId;
                        //this.savedTargetPage.ListItemAllFields.UpdateOverwriteVersion();
                        this.savedTargetPage.ListItemAllFields.SystemUpdate();
                        isDirty = true;
                    }
                }

                // Determine content type to use
                if (string.IsNullOrEmpty(contentTypeId))
                {
                    // grab the default content type
                    contentTypeId = this.savedTargetPage.ListItemAllFields[Constants.ContentTypeIdField].ToString();
                }

                if (this.pageLayoutMappingModel.MetaData != null)
                {
                    // Handle the taxonomy fields
                    bool targetSitePagesLibraryLoaded = false;
                    bool sourceLibraryLoaded = false;
                    List targetSitePagesLibrary = null;
                    List sourceLibrary = null;

                    foreach (var fieldToProcess in this.pageLayoutMappingModel.MetaData.Field)
                    {
                        // Process only fields which have a target field set...
                        if (!string.IsNullOrEmpty(fieldToProcess.TargetFieldName))
                        {
                            if (!listItemWasReloaded)
                            {
                                // Load the target page list item, needs to be loaded as it was previously saved and we need to avoid version conflicts
                                this.targetClientContext.Load(this.savedTargetPage.ListItemAllFields);
                                this.targetClientContext.ExecuteQueryRetry();
                                listItemWasReloaded = true;
                            }

                            // Get information about this content type field
                            var targetFieldData = CacheManager.Instance.GetPublishingContentTypeField(this.savedTargetPage.ListItemAllFields.ParentList, contentTypeId, fieldToProcess.TargetFieldName);

                            if (targetFieldData == null)
                            {
                                LogWarning($"{LogStrings.TransformCopyingMetaDataFieldSkipped} {fieldToProcess.TargetFieldName}", LogStrings.Heading_CopyingPageMetadata);
                            }
                            else
                            {
                                // Taxonomy Field
                                if (targetFieldData.FieldType == "TaxonomyFieldTypeMulti" || targetFieldData.FieldType == "TaxonomyFieldType")
                                {

                                    #region Load Library Field Data

                                    if (!targetSitePagesLibraryLoaded)
                                    {
                                        var sitePagesServerRelativeUrl = UrlUtility.Combine(targetClientContext.Web.ServerRelativeUrl, "sitepages");
                                        targetSitePagesLibrary = this.targetClientContext.Web.GetList(sitePagesServerRelativeUrl);
                                        this.targetClientContext.Web.Context.Load(targetSitePagesLibrary, l => l.Fields.IncludeWithDefaultProperties(f => f.Id, f => f.Title, f => f.Hidden, f => f.InternalName, f => f.DefaultValue, f => f.Required));
                                        this.targetClientContext.ExecuteQueryRetry();

                                        targetSitePagesLibraryLoaded = true;
                                    }

                                    // Loads the source library
                                    if (!sourceLibraryLoaded)
                                    {
                                        sourceLibrary = this.publishingPageTransformationInformation.SourcePage.ParentList;
                                        this.sourceClientContext.Web.Context.Load(sourceLibrary, l => l.Fields.IncludeWithDefaultProperties(f => f.Id, f => f.Title, f => f.Hidden, f => f.InternalName, f => f.DefaultValue, f => f.Required));
                                        this.sourceClientContext.ExecuteQueryRetry();
                                        sourceLibraryLoaded = true;
                                    }

                                    #endregion

                                    var targetTaxFieldBeforeCast = targetSitePagesLibrary.Fields.Where(p => p.Id.Equals(targetFieldData.FieldId)).FirstOrDefault();
                                    if (targetTaxFieldBeforeCast != null)
                                    {

                                        var srcTaxFieldBeforeCast = sourceLibrary.Fields.Where(p => p.InternalName.Equals(fieldToProcess.Name)).FirstOrDefault();
                                        if (this.publishingPageTransformationInformation.SourcePage.FieldExists(fieldToProcess.Name) && srcTaxFieldBeforeCast != null)
                                        {
                                            var targetTaxField = this.targetClientContext.CastTo<TaxonomyField>(targetTaxFieldBeforeCast);
                                            var srcTaxField = this.sourceClientContext.CastTo<TaxonomyField>(srcTaxFieldBeforeCast);

                                            //Block if the source field is a multi-valued tax field and target is single-valued
                                            if (targetTaxField.AllowMultipleValues != srcTaxField.AllowMultipleValues && srcTaxField.AllowMultipleValues)
                                            {
                                                LogWarning($"{LogStrings.TransformCopyingMetaDataFieldSkipped} {fieldToProcess.TargetFieldName} {LogStrings.TransformCopyingMetaDataFieldMismatch}", LogStrings.Heading_CopyingPageMetadata);
                                            }
                                            else
                                            {
                                                try
                                                {
                                                 
                                                    // If source and target field point to the same termset then termmapping is not needed
                                                    var sourceTermSetId = Guid.Empty;
                                                    var sourceSsdId = Guid.Empty;
                                                    
                                                    var isSP2010 = publishingPageTransformationInformation.SourceVersion == SPVersion.SP2010;

                                                    if (isSP2010)
                                                    {
                                                        // 2010 doesnt appear to be able to cast this type via CSOM
                                                        var extractedTermSetId = TermTransformator.ExtractTermSetIdOrSspIdFromXmlSchema(srcTaxFieldBeforeCast.SchemaXml);
                                                        Guid.TryParse(extractedTermSetId, out sourceTermSetId);
                                                        var extractedSspId = TermTransformator.ExtractTermSetIdOrSspIdFromXmlSchema(srcTaxFieldBeforeCast.SchemaXml, true);
                                                        Guid.TryParse(extractedSspId, out sourceSsdId);
                                                    }
                                                    else
                                                    {
                                                        sourceTermSetId = srcTaxField.TermSetId;
                                                        sourceSsdId = srcTaxField.SspId;
                                                    }

                                                    bool skipTermMapping = ((sourceTermSetId == targetTaxField.TermSetId) && string.IsNullOrEmpty(this.publishingPageTransformationInformation.TermMappingFile));

                                                    if (!skipTermMapping)
                                                    {
                                                        //Gather terms from the term store
                                                        termTransformator.CacheTermsFromTermStore(sourceTermSetId, targetTaxField.TermSetId, sourceSsdId, isSP2010);   
                                                    }

                                                    object fieldValueToSet = null;

                                                    switch (targetFieldData.FieldType)
                                                    {
                                                        case "TaxonomyFieldTypeMulti":
                                                            {
                                                                #region Multiple Valued Taxonomy Field

                                                                if (!string.IsNullOrEmpty(fieldToProcess.Functions))
                                                                {
                                                                    // execute function
                                                                    var evaluatedField = this.functionProcessor.Process(fieldToProcess.Functions, fieldToProcess.Name, CastToPublishingFunctionProcessorFieldType(targetFieldData.FieldType));
                                                                    if (!string.IsNullOrEmpty(evaluatedField.Item1))
                                                                    {
                                                                        if (!string.IsNullOrEmpty(evaluatedField.Item2))
                                                                        {
                                                                            List<string> termInfoStrings = new List<string>();
                                                                            if (evaluatedField.Item2.Contains("§"))
                                                                            {
                                                                                string[] termInfoStringList = evaluatedField.Item2.Split(new string[] { "§" }, StringSplitOptions.RemoveEmptyEntries);
                                                                                termInfoStrings.AddRange(termInfoStringList);
                                                                            }
                                                                            else
                                                                            {
                                                                                termInfoStrings.Add(evaluatedField.Item2);
                                                                            }

                                                                            if (termInfoStrings.Count > 0)
                                                                            {
                                                                                fieldValueToSet = new Dictionary<string, object>();
                                                                                List<Dictionary<string, object>> termsToSetList = new List<Dictionary<string, object>>();

                                                                                foreach (var term in termInfoStrings)
                                                                                {
                                                                                    string[] termValueParts = term.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

                                                                                    if (termValueParts.Length == 2)
                                                                                    {
                                                                                        Dictionary<string, object> termsToSet = new Dictionary<string, object>();
                                                                                        (termsToSet as Dictionary<string, object>).Add("Label", termValueParts[0]);
                                                                                        (termsToSet as Dictionary<string, object>).Add("TermGuid", termValueParts[1]);
                                                                                        termsToSetList.Add(termsToSet);
                                                                                    }
                                                                                }

                                                                                (fieldValueToSet as Dictionary<string, object>).Add("_Child_Items_", termsToSetList.ToArray());
                                                                            }
                                                                        }
                                                                    }
                                                                }

                                                                // No value was set via the function processing, so let's stick with the default
                                                                if (fieldValueToSet == null)
                                                                {
                                                                    fieldValueToSet = this.publishingPageTransformationInformation.SourcePage[fieldToProcess.Name];
                                                                }

                                                                if (fieldValueToSet is TaxonomyFieldValueCollection)
                                                                {
                                                                    var valueCollectionToCopy = (fieldValueToSet as TaxonomyFieldValueCollection);

                                                                    IQueryable<string> taxonomyFieldValueArray = null;
                                                                    Tuple<TaxonomyFieldValueCollection, List<TaxonomyFieldValue>> resultTermTransform = null;
                                                                    if (!skipTermMapping)
                                                                    {
                                                                        //Term Transformator
                                                                        resultTermTransform = termTransformator.TransformCollection(valueCollectionToCopy);
                                                                        valueCollectionToCopy = resultTermTransform.Item1;
                                                                        taxonomyFieldValueArray = valueCollectionToCopy.Except(resultTermTransform.Item2).Select(taxonomyFieldValue => $"-1;#{taxonomyFieldValue.Label}|{taxonomyFieldValue.TermGuid}");
                                                                    }
                                                                    else
                                                                    {
                                                                        taxonomyFieldValueArray = valueCollectionToCopy.Select(taxonomyFieldValue => $"-1;#{taxonomyFieldValue.Label}|{taxonomyFieldValue.TermGuid}");
                                                                    }

                                                                    if (taxonomyFieldValueArray != null)
                                                                    {
                                                                        var valueCollection = new TaxonomyFieldValueCollection(this.targetClientContext, string.Join(";#", taxonomyFieldValueArray), targetTaxField);
                                                                        targetTaxField.SetFieldValueByValueCollection(this.savedTargetPage.ListItemAllFields, valueCollection);
                                                                        isDirty = true;
                                                                        LogInfo($"{LogStrings.TransformCopyingMetaDataField} {targetFieldData.FieldName}", LogStrings.Heading_CopyingPageMetadata);

                                                                        if (!skipTermMapping)
                                                                        {
                                                                            if (resultTermTransform.Item2.Any())
                                                                            {
                                                                                resultTermTransform.Item2.ForEach(field =>
                                                                                {
                                                                                    LogWarning(string.Format(LogStrings.TransformCopyingMetaDataTaxFieldValue, field.Label), LogStrings.Heading_CopyingPageMetadata);
                                                                                });
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                                else if (fieldValueToSet is Dictionary<string, object>)
                                                                {
                                                                    var taxDictionaryList = (fieldValueToSet as Dictionary<string, object>);
                                                                    List<string> taxonomyFieldValueArray = new List<string>();
                                                                    
                                                                    if (taxDictionaryList.ContainsKey("_Child_Items_"))
                                                                    {

                                                                        var valueCollectionToCopy = taxDictionaryList["_Child_Items_"] as Object[];

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
                                                                                    taxonomyFieldValueArray.Add($"-1;#{transformTerm.TermLabel}|{transformTerm.TermGuid}");
                                                                                }
                                                                                else
                                                                                {
                                                                                    LogWarning(string.Format(LogStrings.TransformCopyingMetaDataTaxFieldValue, label), LogStrings.Heading_CopyingPageMetadata);
                                                                                }
                                                                            }
                                                                            else
                                                                            {
                                                                                taxonomyFieldValueArray.Add($"-1;#{taxDictionary["Label"].ToString()}|{taxDictionary["TermGuid"].ToString()}");
                                                                            }
                                                                        }

                                                                    }
                                                                    else
                                                                    {
                                                                        // Single Value into Multiple Value Field Scenario
                                                                        if (taxDictionaryList.ContainsKey("Label") && taxDictionaryList.ContainsKey("TermGuid"))
                                                                        {
                                                                            var label = taxDictionaryList["Label"].ToString();
                                                                            var termGuid = new Guid(taxDictionaryList["TermGuid"].ToString());

                                                                            if (!skipTermMapping)
                                                                            {
                                                                                //Term Transformator
                                                                                var transformTerm = termTransformator.Transform(new TermData() { TermGuid = termGuid, TermLabel = label });

                                                                                if (transformTerm.IsTermResolved)
                                                                                {
                                                                                    taxonomyFieldValueArray.Add($"-1;#{transformTerm.TermLabel}|{transformTerm.TermGuid}");
                                                                                }
                                                                                else
                                                                                {
                                                                                    LogWarning(string.Format(LogStrings.TransformCopyingMetaDataTaxFieldValue, label), LogStrings.Heading_CopyingPageMetadata);
                                                                                }
                                                                            }
                                                                            else
                                                                            {
                                                                                taxonomyFieldValueArray.Add($"-1;#{taxDictionaryList["Label"].ToString()}|{taxDictionaryList["TermGuid"].ToString()}");
                                                                            }
                                                                        }
                                                                    }

                                                                    if (taxonomyFieldValueArray.Any())
                                                                    {
                                                                        var valueCollection = new TaxonomyFieldValueCollection(this.targetClientContext, string.Join(";#", taxonomyFieldValueArray), targetTaxField);
                                                                        targetTaxField.SetFieldValueByValueCollection(this.savedTargetPage.ListItemAllFields, valueCollection);
                                                                        isDirty = true;
                                                                        LogInfo($"{LogStrings.TransformCopyingMetaDataField} {targetFieldData.FieldName}", LogStrings.Heading_CopyingPageMetadata);
                                                                    }
                                                                    else
                                                                    {
                                                                        // Publishing field was empty, so let's skip the metadata copy
                                                                        LogInfo(string.Format(LogStrings.TransformCopyingMetaDataTaxFieldEmpty, targetFieldData.FieldName), LogStrings.Heading_CopyingPageMetadata);
                                                                    }
                                                                   
                                                                }
                                                                else if (fieldValueToSet is Array && isSP2010)
                                                                {
                                                                    var taxValueArray = (fieldValueToSet as Array);

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
                                                                        var valueCollection = new TaxonomyFieldValueCollection(this.targetClientContext, string.Join(";#", taxonomyFieldValueArray), targetTaxField);
                                                                        targetTaxField.SetFieldValueByValueCollection(this.savedTargetPage.ListItemAllFields, valueCollection);
                                                                        isDirty = true;
                                                                        LogInfo(string.Format(LogStrings.TransformCopyingMetaDataTaxFieldEmpty, targetFieldData.FieldName), LogStrings.Heading_CopyingPageMetadata);
                                                                    }
                                                                    else
                                                                    {
                                                                        // Field was empty, so let's skip the metadata copy
                                                                        LogInfo(string.Format(LogStrings.TransformCopyingMetaDataTaxFieldEmpty, targetFieldData.FieldName), LogStrings.Heading_CopyingPageMetadata);
                                                                    }
                                                                }
                                                                else if (fieldValueToSet is TaxonomyFieldValue)
                                                                {

                                                                    var labelToSet = (fieldValueToSet as TaxonomyFieldValue).Label;
                                                                    var termGuidToSet = (fieldValueToSet as TaxonomyFieldValue).TermGuid;
                                                                    TaxonomyFieldValue taxValue = new TaxonomyFieldValue();

                                                                    if (!skipTermMapping)
                                                                    {
                                                                        //Term Transformator
                                                                        var termTranform = termTransformator.Transform(new TermData() { TermGuid = new Guid(termGuidToSet), TermLabel = labelToSet });
                                                                        if (termTranform.IsTermResolved)
                                                                        {
                                                                            taxValue.Label = termTranform.TermLabel;
                                                                            taxValue.TermGuid = termTranform.TermGuid.ToString();
                                                                            taxValue.WssId = -1;
                                                                            targetTaxField.SetFieldValueByValue(this.savedTargetPage.ListItemAllFields, taxValue);
                                                                            isDirty = true;
                                                                            LogInfo($"{LogStrings.TransformCopyingMetaDataField} {targetFieldData.FieldName}", LogStrings.Heading_CopyingPageMetadata);
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
                                                                        targetTaxField.SetFieldValueByValue(this.savedTargetPage.ListItemAllFields, taxValue);
                                                                        isDirty = true;
                                                                        LogInfo($"{LogStrings.TransformCopyingMetaDataField} {targetFieldData.FieldName}", LogStrings.Heading_CopyingPageMetadata);
                                                                    }

                                                                }

                                                                else
                                                                {
                                                                    // Publishing field was empty, so let's skip the metadata copy
                                                                    LogInfo(string.Format(LogStrings.TransformCopyingMetaDataTaxFieldEmpty, targetFieldData.FieldName), LogStrings.Heading_CopyingPageMetadata);
                                                                }
                                                                #endregion

                                                            }
                                                            break;

                                                        case "TaxonomyFieldType":
                                                            {

                                                                #region Single Valued Taxonomy Field                                                                

                                                                if (!string.IsNullOrEmpty(fieldToProcess.Functions))
                                                                {
                                                                    // execute function
                                                                    var evaluatedField = this.functionProcessor.Process(fieldToProcess.Functions, fieldToProcess.Name, CastToPublishingFunctionProcessorFieldType(targetFieldData.FieldType));
                                                                    if (!string.IsNullOrEmpty(evaluatedField.Item1))
                                                                    {
                                                                        if (!string.IsNullOrEmpty(evaluatedField.Item2))
                                                                        {
                                                                            string[] termValueParts = evaluatedField.Item2.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

                                                                            if (termValueParts.Length == 2)
                                                                            {
                                                                                fieldValueToSet = new Dictionary<string, object>();

                                                                                (fieldValueToSet as Dictionary<string, object>).Add("Label", termValueParts[0]);
                                                                                (fieldValueToSet as Dictionary<string, object>).Add("TermGuid", termValueParts[1]);
                                                                            }
                                                                        }
                                                                    }
                                                                }

                                                                // No value was set via the function processing, so let's stick with the default
                                                                if (fieldValueToSet == null)
                                                                {
                                                                    fieldValueToSet = this.publishingPageTransformationInformation.SourcePage[fieldToProcess.Name];
                                                                }

                                                                TaxonomyFieldValue taxValue = new TaxonomyFieldValue();

                                                                if (fieldValueToSet is TaxonomyFieldValue)
                                                                {

                                                                    var labelToSet = (fieldValueToSet as TaxonomyFieldValue).Label;
                                                                    var termGuidToSet = (fieldValueToSet as TaxonomyFieldValue).TermGuid;

                                                                    if (!skipTermMapping)
                                                                    {
                                                                        //Term Transformator
                                                                        var termTranform = termTransformator.Transform(new TermData() { TermGuid = new Guid(termGuidToSet), TermLabel = labelToSet });
                                                                        if (termTranform.IsTermResolved)
                                                                        {
                                                                            taxValue.Label = termTranform.TermLabel;
                                                                            taxValue.TermGuid = termTranform.TermGuid.ToString();
                                                                            taxValue.WssId = -1;
                                                                            targetTaxField.SetFieldValueByValue(this.savedTargetPage.ListItemAllFields, taxValue);
                                                                            isDirty = true;
                                                                            LogInfo($"{LogStrings.TransformCopyingMetaDataField} {targetFieldData.FieldName}", LogStrings.Heading_CopyingPageMetadata);
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
                                                                        targetTaxField.SetFieldValueByValue(this.savedTargetPage.ListItemAllFields, taxValue);
                                                                        isDirty = true;
                                                                        LogInfo($"{LogStrings.TransformCopyingMetaDataField} {targetFieldData.FieldName}", LogStrings.Heading_CopyingPageMetadata);
                                                                    }

                                                                }
                                                                else if ((fieldValueToSet is Dictionary<string, object>))
                                                                {
                                                                    var taxDictionary = (fieldValueToSet as Dictionary<string, object>);

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
                                                                            targetTaxField.SetFieldValueByValue(this.savedTargetPage.ListItemAllFields, taxValue);
                                                                            isDirty = true;
                                                                            LogInfo($"{LogStrings.TransformCopyingMetaDataField} {targetFieldData.FieldName}", LogStrings.Heading_CopyingPageMetadata);
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
                                                                        targetTaxField.SetFieldValueByValue(this.savedTargetPage.ListItemAllFields, taxValue);
                                                                        isDirty = true;
                                                                        LogInfo($"{LogStrings.TransformCopyingMetaDataField} {targetFieldData.FieldName}", LogStrings.Heading_CopyingPageMetadata);

                                                                    }
                                                                }
                                                                else if((fieldValueToSet is string))
                                                                {

                                                                    string[] termValueParts = fieldValueToSet.ToString().Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

                                                                    if (termValueParts.Length == 2)
                                                                    {

                                                                        var labelToSet = termValueParts[0];
                                                                        var termGuidToSet = termValueParts[1];

                                                                        if (!skipTermMapping)
                                                                        {
                                                                            //Term Transformator
                                                                            var termTranform = termTransformator.Transform(new TermData() { TermGuid = new Guid(termGuidToSet), TermLabel = labelToSet });
                                                                            if (termTranform.IsTermResolved)
                                                                            {
                                                                                taxValue.Label = termTranform.TermLabel;
                                                                                taxValue.TermGuid = termTranform.TermGuid.ToString();
                                                                                taxValue.WssId = -1;
                                                                                targetTaxField.SetFieldValueByValue(this.savedTargetPage.ListItemAllFields, taxValue);
                                                                                isDirty = true;
                                                                                LogInfo($"{LogStrings.TransformCopyingMetaDataField} {targetFieldData.FieldName}", LogStrings.Heading_CopyingPageMetadata);
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
                                                                            targetTaxField.SetFieldValueByValue(this.savedTargetPage.ListItemAllFields, taxValue);
                                                                            isDirty = true;
                                                                            LogInfo($"{LogStrings.TransformCopyingMetaDataField} {targetFieldData.FieldName}", LogStrings.Heading_CopyingPageMetadata);
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        //Term not formatted correctly.
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    // Publishing field was empty, so let's skip the metadata copy
                                                                    LogInfo(string.Format(LogStrings.TransformCopyingMetaDataTaxFieldEmpty, targetFieldData.FieldName), LogStrings.Heading_CopyingPageMetadata);
                                                                }

                                                                #endregion
                                                                break;
                                                            }
                                                    }

                                                }
                                                catch (Exception ex)
                                                {
                                                    LogError(string.Format(LogStrings.Error_TransformingTaxonomyField, fieldToProcess.Name), LogStrings.Heading_CopyingPageMetadata, ex);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // Log that field in page layout mapping was not found
                                            LogWarning(string.Format(LogStrings.Warning_FieldNotFoundInSourcePage, fieldToProcess.Name), LogStrings.Heading_CopyingPageMetadata);
                                        }

                                    }
                                    else
                                    {
                                        LogWarning(string.Format(LogStrings.Warning_FieldNotFoundInTargetPage, fieldToProcess.Name), LogStrings.Heading_CopyingPageMetadata);
                                    }

                                }
                            }
                        }
                    }

                    // Persist changes
                    if (isDirty)
                    {
                        try
                        {
                            //this.savedTargetPage.ListItemAllFields.UpdateOverwriteVersion();
                            this.savedTargetPage.ListItemAllFields.SystemUpdate();
                            targetClientContext.Load(this.savedTargetPage.ListItemAllFields);
                            targetClientContext.ExecuteQueryRetry();

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

                    string bannerImageUrl = null;

                    // Copy the field metadata
                    foreach (var fieldToProcess in this.pageLayoutMappingModel.MetaData.Field)
                    {

                        // check if the source field name attribute contains a delimiter value
                        if (fieldToProcess.Name.Contains(";"))
                        {
                            // extract the array of field names to process, and trims each one
                            string[] sourceFieldNames = fieldToProcess.Name.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();

                            // sets the field name to the first "valid" entry
                            fieldToProcess.Name = this.publishingPageTransformationInformation.GetFirstNonEmptyFieldName(sourceFieldNames);
                        }

                        // Process only fields which have a target field set...
                        if (!string.IsNullOrEmpty(fieldToProcess.TargetFieldName))
                        {
                            if (!listItemWasReloaded)
                            {
                                // Load the target page list item, needs to be loaded as it was previously saved and we need to avoid version conflicts
                                this.targetClientContext.Load(this.savedTargetPage.ListItemAllFields);
                                this.targetClientContext.ExecuteQueryRetry();
                                listItemWasReloaded = true;
                            }

                            // Get information about this content type field
                            var targetFieldData = CacheManager.Instance.GetPublishingContentTypeField(this.savedTargetPage.ListItemAllFields.ParentList, contentTypeId, fieldToProcess.TargetFieldName);

                            if (targetFieldData == null)
                            {
                                LogWarning($"{LogStrings.TransformCopyingMetaDataFieldSkipped} {fieldToProcess.TargetFieldName}", LogStrings.Heading_CopyingPageMetadata);
                            }
                            else
                            {
                                if (targetFieldData.FieldType != "TaxonomyFieldTypeMulti" && targetFieldData.FieldType != "TaxonomyFieldType" &&
                                    !(groupedEnterpriseWikiRatings && EnterpriseWikiRatings.IsRatingField(fieldToProcess.Name) &&
                                      fieldToProcess.TargetFieldName == fieldToProcess.Name && string.IsNullOrEmpty(fieldToProcess.Functions)))
                                {
                                    if (this.publishingPageTransformationInformation.SourcePage.FieldExists(fieldToProcess.Name))
                                    {
                                        object fieldValueToSet = null;

                                        if (!string.IsNullOrEmpty(fieldToProcess.Functions))
                                        {
                                            // execute function
                                            var evaluatedField = this.functionProcessor.Process(fieldToProcess.Functions, fieldToProcess.Name, CastToPublishingFunctionProcessorFieldType(targetFieldData.FieldType));
                                            if (!string.IsNullOrEmpty(evaluatedField.Item1))
                                            {
                                                fieldValueToSet = evaluatedField.Item2;
                                            }
                                        }
                                        else
                                        {
                                            fieldValueToSet = this.publishingPageTransformationInformation.SourcePage[fieldToProcess.Name];
                                        }

                                        if (fieldValueToSet != null)
                                        {
                                            if (targetFieldData.FieldType == "User" || targetFieldData.FieldType == "UserMulti")
                                            {
                                                if (fieldValueToSet is FieldUserValue)
                                                {
                                                    // Publishing page transformation always goes cross site collection, so we'll need to lookup a user again
                                                    // Important to use a cloned context to not mess up with the pending list item updates
                                                    try
                                                    {
                                                        // Source User
                                                        var fieldUser = (fieldValueToSet as FieldUserValue).LookupValue;
                                                        // Mapped target user
                                                        fieldUser = this.userTransformator.RemapPrincipal(this.sourceClientContext, (fieldValueToSet as FieldUserValue));

                                                        // Ensure user exists on target site
                                                        var ensuredUserOnTarget = CacheManager.Instance.GetEnsuredUser(this.targetClientContext, fieldUser);
                                                        if (ensuredUserOnTarget != null)
                                                        {
                                                            // Prep a new FieldUserValue object instance and update the list item
                                                            var newUser = new FieldUserValue()
                                                            {
                                                                LookupId = ensuredUserOnTarget.Id
                                                            };
                                                            this.savedTargetPage.ListItemAllFields[targetFieldData.FieldName] = newUser;
                                                        }
                                                        else
                                                        {
                                                            // Clear target field - needed in overwrite scenarios
                                                            this.savedTargetPage.ListItemAllFields[targetFieldData.FieldName] = null;
                                                            LogWarning(string.Format(LogStrings.Warning_UserIsNotMappedOrResolving, (fieldValueToSet as FieldUserValue).LookupValue, targetFieldData.FieldName), LogStrings.Heading_CopyingPageMetadata);
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
                                                            fieldUser = this.userTransformator.RemapPrincipal(this.sourceClientContext, (currentUser as FieldUserValue));

                                                            // Ensure user exists on target site
                                                            var ensuredUserOnTarget = CacheManager.Instance.GetEnsuredUser(this.targetClientContext, fieldUser);
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
                                                                LogWarning(string.Format(LogStrings.Warning_UserIsNotMappedOrResolving, (currentUser as FieldUserValue).LookupValue, targetFieldData.FieldName), LogStrings.Heading_CopyingPageMetadata);
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            LogWarning(string.Format(LogStrings.Warning_UserIsNotResolving, (currentUser as FieldUserValue).LookupValue, ex.Message), LogStrings.Heading_CopyingPageMetadata);
                                                        }
                                                    }

                                                    if (userValues.Count > 0)
                                                    {
                                                        this.savedTargetPage.ListItemAllFields[targetFieldData.FieldName] = userValues.ToArray();
                                                    }
                                                    else
                                                    {
                                                        // Clear target field - needed in overwrite scenarios
                                                        this.savedTargetPage.ListItemAllFields[targetFieldData.FieldName] = null;
                                                    }

                                                }
                                            }
                                            else
                                            {
                                                this.savedTargetPage.ListItemAllFields[targetFieldData.FieldName] = fieldValueToSet;

                                                // If we set the BannerImageUrl we also need to update the page to ensure this updated page image "sticks"
                                                if (targetFieldData.FieldName == Constants.BannerImageUrlField)
                                                {
                                                    bannerImageUrl = fieldValueToSet.ToString();
                                                }
                                            }

                                            isDirty = true;

                                            LogInfo($"{LogStrings.TransformCopyingMetaDataField} {targetFieldData.FieldName}", LogStrings.Heading_CopyingPageMetadata);
                                        }
                                    }
                                    else
                                    {
                                        // Log that field in page layout mapping was not found
                                        LogWarning(string.Format(LogStrings.Warning_FieldNotFoundInSourcePage, fieldToProcess.Name), LogStrings.Heading_CopyingPageMetadata);
                                    }
                                }
                            }
                        }
                    }

                    // Persist changes
                    if (isDirty)
                    {
                        // If we've set a custom thumbnail value then we need to update the page html to mark the isDefaultThumbnail pageslicer property to false
                        if (!string.IsNullOrEmpty(bannerImageUrl))
                        {
                            this.savedTargetPage.ListItemAllFields[Constants.CanvasContentField] = SetIsDefaultThumbnail(this.savedTargetPage.ListItemAllFields[Constants.CanvasContentField].ToString());
                        }

                        //this.savedTargetPage.ListItemAllFields.UpdateOverwriteVersion();
                        this.savedTargetPage.ListItemAllFields.SystemUpdate();
                        targetClientContext.Load(this.savedTargetPage.ListItemAllFields);
                        targetClientContext.ExecuteQueryRetry();


                        isDirty = false;
                    }
                }
            }
            else
            {
                LogDebug("Page Layout mapping model not found", LogStrings.Heading_CopyingPageMetadata);
            }
        }

        #region Helper methods
        private PublishingFunctionProcessor.FieldType CastToPublishingFunctionProcessorFieldType(string fieldType)
        {
            if (fieldType.Equals("User", StringComparison.InvariantCultureIgnoreCase))
            {
                return PublishingFunctionProcessor.FieldType.User;
            }
            else
            {
                return PublishingFunctionProcessor.FieldType.String;
            }
        }

        private string SetIsDefaultThumbnail(string pageHtml)
        {
            return pageHtml.Replace("&quot;isDefaultThumbnail&quot;&#58;true", "&quot;isDefaultThumbnail&quot;&#58;false");
        }
        #endregion

    }
}

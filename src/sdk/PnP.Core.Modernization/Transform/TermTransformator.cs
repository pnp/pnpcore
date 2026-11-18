
using PnP.Core.Modernization.Cache;
using PnP.Core.Modernization.Entities;
using PnP.Core.Modernization.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace PnP.Core.Modernization.Transform
{
    public class TermTransformator : BaseTransform
    {
        private PnPContext _sourceContext;
        private PnPContext _targetContext;
        private List<TermMapping> termMappings;
        private bool skipTermStoreMapping;
        private BaseTransformationInformation _baseTransformationInformation;
        public const string TermNodeDelimiter = "|";
        public const string TermGroupUnknownName = "DEFAULT";

        #region Construction        

        /// <summary>
        /// Constructor for the Term Transformator class
        /// </summary>
        /// <param name="baseTransformationInformation"></param>
        /// <param name="sourceContext"></param>
        /// <param name="targetContext"></param>
        /// <param name="logObservers"></param>
        public TermTransformator(BaseTransformationInformation baseTransformationInformation, PnPContext sourceContext, PnPContext targetContext, IList<ILogObserver> logObservers = null)
        {
            // Hookup logging
            if (logObservers != null)
            {
                foreach (var observer in logObservers)
                {
                    base.RegisterObserver(observer);
                }
            }

            // Ensure source and target context are set
            if (sourceContext == null && targetContext != null)
            {
                sourceContext = targetContext;
            }

            if (targetContext == null && sourceContext != null)
            {
                targetContext = sourceContext;
            }

            this._sourceContext = sourceContext;
            this._targetContext = targetContext;
            if (sourceContext != null) SharePointOnlineSupport.Validate(sourceContext);
            if (targetContext != null) SharePointOnlineSupport.Validate(targetContext);

            // Load the Term mapping file
            if (!string.IsNullOrEmpty(baseTransformationInformation?.TermMappingFile))
            {
                this.termMappings = CacheManager.Instance.GetTermMapping(baseTransformationInformation.TermMappingFile, logObservers);
            }

            if (baseTransformationInformation != null)
            {
                this.skipTermStoreMapping = baseTransformationInformation.SkipTermStoreMapping;
                this._baseTransformationInformation = baseTransformationInformation;
            }
        }

        #endregion

        /// <summary>
        /// Transforms a collection of terms in a dictionary
        /// </summary>
        /// <returns>
        ///     Tuple&lt;TaxonomyFieldValueCollection,List&lt;TaxonomyFieldValue&gt;&gt; 
        ///     TaxonomyFieldValueCollection - Original Array
        ///     List&lt;TaxonomyFieldValue&gt; - Items to remove as they are not resolved
        /// </returns>
        public Tuple<IFieldValueCollection, List<IFieldTaxonomyValue>>  TransformCollection(IFieldValueCollection taxonomyFieldValueCollection)
        {
            List<IFieldTaxonomyValue> exceptFields = new List<IFieldTaxonomyValue>();
            
            foreach (IFieldTaxonomyValue fieldValue in taxonomyFieldValueCollection.Values)
            {
                var result = this.Transform(new TermData() { TermGuid = fieldValue.TermId, TermLabel = fieldValue.Label });
                if (result.IsTermResolved)
                {
                    fieldValue.Label = result.TermLabel;
                    fieldValue.TermId = result.TermGuid;
                }
                else
                {
                    exceptFields.Add(fieldValue);
                }
            }
            
            // Return fields to remove by calling code.
            return new Tuple<IFieldValueCollection, List<IFieldTaxonomyValue>>(taxonomyFieldValueCollection, exceptFields);
        }

        /// <summary>
        /// Main entry method for transforming terms
        /// </summary>
        public TermData Transform(TermData inputSourceTerm)
        {
            //Design:
            // This will have two modes:
            // Default mode to work out the terms from source to destination based on identical IDs or Term Paths
            // Mapping file to override default mode for specifically mapping a source term to designation term

            //Scenarios:
            // Term Ids or Term Names
            // Source or Target Term ID/Name may not be found
                       
            // Default Mode 
            if (!this.skipTermStoreMapping && !_baseTransformationInformation.IsCrossFarmTransformation)
            {
                var resolvedInputMapping = ResolveTermInCache(this._sourceContext, inputSourceTerm.TermGuid);

                if (resolvedInputMapping.IsTermResolved)
                {
                    //Check if the source term ID exists in target then map.
                    var resolvedInputMappingInTarget = ResolveTermInCache(this._targetContext, inputSourceTerm.TermGuid);
                    if (resolvedInputMappingInTarget.IsTermResolved && !resolvedInputMapping.IsSourceTerm)
                    {
                        inputSourceTerm.IsTermResolved = true; //Happy that term ID is the same as source
                        inputSourceTerm.TermLabel = resolvedInputMappingInTarget.TermLabel; //Just in case the ids are the same and labels are not
                        return inputSourceTerm;
                    }

                    //Check if the term labels are the same, ids maybe different - in this scenario, validate if the term paths are the same.
                    //if so, then auto-map.
                    resolvedInputMappingInTarget = ResolveTermInCache(this._targetContext, resolvedInputMapping.TermPath);
                    if (resolvedInputMappingInTarget.IsTermResolved && !resolvedInputMapping.IsSourceTerm)
                    {
                        inputSourceTerm.IsTermResolved = true; //Happy that term ID is the same as source
                        inputSourceTerm.TermGuid = resolvedInputMappingInTarget.TermGuid; //Just in case the ids are the same and labels are not
                        return inputSourceTerm;
                    }
                }

            }

            // Mapping Mode 
            if (termMappings != null)
            {
                var resolvedInputMapping = ResolveTermInCache(this._sourceContext, inputSourceTerm.TermGuid);

                //Check Source Mappings
                foreach (var mapping in termMappings)
                {

                    // Simple Check, if the delimiter is | lets check for that
                    if (mapping.SourceTerm.Contains("|"))
                    {
                        //Term Path
                        // If found validate against the term cache
                        if (resolvedInputMapping.TermPath == mapping.SourceTerm)
                        {
                            var resolvedTargetMapping = ResolveTermInCache(this._targetContext, mapping.TargetTerm);
                            if (resolvedTargetMapping != default)
                            {
                                return resolvedTargetMapping;
                            }
                            else
                            {
                                //Log Failure in resolving to target mapping
                                LogWarning(string.Format(LogStrings.Warning_TermMappingFailedResolveTarget, mapping.TargetTerm), LogStrings.Heading_TermMapping);
                            }
                        }
                    }
                    else
                    {
                        //Guid
                        if (Guid.TryParse(mapping.SourceTerm, out Guid mappingSourceTermId))
                        {
                            //Found 
                            if (resolvedInputMapping.TermGuid == mappingSourceTermId)
                            {
                                if (Guid.TryParse(mapping.TargetTerm, out Guid mappingTargetTermId))
                                {
                                    var resolvedTargetMapping = ResolveTermInCache(this._targetContext, mappingTargetTermId);
                                    if (resolvedTargetMapping != default)
                                    {
                                        return resolvedTargetMapping;
                                    }
                                    else
                                    {
                                        //Log Failure in resolving to target mapping
                                        LogWarning(string.Format(LogStrings.Warning_TermMappingFailedResolveTarget, mapping.TargetTerm), LogStrings.Heading_TermMapping);
                                    }
                                }
                                else
                                {
                                    var resolvedTargetMapping = ResolveTermInCache(this._targetContext, mapping.TargetTerm);
                                    if (resolvedTargetMapping != default)
                                    {
                                        return resolvedTargetMapping;
                                    }
                                    else
                                    {
                                        //Log Failure in resolving to target mapping
                                        LogWarning(string.Format(LogStrings.Warning_TermMappingFailedResolveTarget, mapping.TargetTerm), LogStrings.Heading_TermMapping);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Failure in parsing the Term ID

                        }
                    }
                }

                //Log Failure in mapping
                LogWarning(string.Format(LogStrings.Warning_TermMappingFailedMapping, inputSourceTerm.TermGuid, inputSourceTerm.TermLabel), LogStrings.Heading_TermMapping);
            }


            return inputSourceTerm; //Pass-Through
        }

        /// <summary>
        /// Sets the cache for contents of the term store to be used when getting terms for fields
        /// </summary>
        /// <param name="sourceTermSetId"></param>
        /// <param name="targetTermSetId"></param>
        /// <param name="sourceSspId"></param>
        /// <param name="isSP2010"></param>
        public void CacheTermsFromTermStore(Guid sourceTermSetId, Guid targetTermSetId, Guid sourceSspId, bool isSP2010)
        {
            // Collect source terms
            if (sourceTermSetId != Guid.Empty)
            {
                CacheManager.Instance.StoreTermSetTerms(this._sourceContext, sourceTermSetId, sourceSspId, isSP2010, true);
            }

            if (targetTermSetId != Guid.Empty)
            {
                CacheManager.Instance.StoreTermSetTerms(this._targetContext, targetTermSetId, sourceSspId, false, false);
            }

        }

        #region Called from Cache Manager

        /// <summary>
        /// Extract all the terms from a termset for caching and quicker processing
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Dictionary<Guid, TermData> GetAllTermsFromTermSet(Guid termSetId, PnPContext context)
        {
            SharePointOnlineSupport.Validate(context);
            //Use a source and target Dictionary<guid,string>
            //Key = Id, Value = Path(e.g.termgroup | termset | term)
            var termsCache = new Dictionary<Guid, TermData>();

            try
            {
                using (PnPContext clonedContext = context.Clone(new Uri(context.Web.GetUrl())))
                {
                    clonedContext.TermStore.EnsureProperties(store => store.DefaultLanguage);
                    var language = clonedContext.TermStore.DefaultLanguage;
                    var termSet = clonedContext.TermStore.GetTermSetById(termSetId.ToString(),
                        t => t.Terms.QueryProperties(x => x.Labels), t => t.LocalizedNames, t => t.Group);
                    var termGroup = termSet.Group;
                    termGroup.LoadBatch(g => g.Name);
                    clonedContext.Execute();

                    var termGroupName = termGroup.Name;
                    var setName = termSet.LocalizedNames.AsRequested().FirstOrDefault(n => n.LanguageTag == language)?.Name ??
                        termSet.LocalizedNames.AsRequested().First().Name;
                    var termSetPath = $"{termGroupName}{TermTransformator.TermNodeDelimiter}{setName}";
                    foreach (var term in termSet.Terms)
                    {
                        var termName = GetTermLabel(term, language);
                        var termPath = $"{termSetPath}{TermNodeDelimiter}{termName}";
                        termsCache.Add(Guid.Parse(term.Id),
                            new TermData() { TermGuid = Guid.Parse(term.Id), TermLabel = termName, TermPath = termPath, TermSetId = termSetId });

                        var subTerms = ParseSubTerms(termPath, term, termSetId, clonedContext);
                        foreach (var foundTerm in subTerms)
                        {
                            termsCache.Add(foundTerm.Key, foundTerm.Value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not read term set '{termSetId}' from '{context.Uri}'. No partial term mapping will be cached.", ex);
            }

            return termsCache;
        }

        /// <summary>
        /// Gets the term labels within a term recursively
        /// </summary>
        /// <param name="subTermPath"></param>
        /// <param name="term"></param>
        /// <param name="termSetId"></param>
        /// <param name="clientContext"></param>
        /// <returns></returns>
        /// Reference: https://github.com/SharePoint/PnP-Sites-Core/blob/master/Core/OfficeDevPnP.Core/Extensions/TaxonomyExtensions.cs
        public static Dictionary<Guid, TermData> ParseSubTerms(string subTermPath, ITerm term, Guid termSetId, PnPContext clientContext)
        {
            var items = new Dictionary<Guid, TermData>();
            clientContext.TermStore.EnsureProperties(store => store.DefaultLanguage);
            term.Load(t => t.Terms.QueryProperties(child => child.Labels));

            foreach (var subTerm in term.Terms)
            {
                var termName = GetTermLabel(subTerm, clientContext.TermStore.DefaultLanguage);
                var termPath = $"{subTermPath}{TermTransformator.TermNodeDelimiter}{termName}";

                items.Add(Guid.Parse(subTerm.Id), new TermData() { TermGuid = Guid.Parse(subTerm.Id), TermLabel = termName, TermPath = termPath, TermSetId = termSetId });

                var moreSubTerms = ParseSubTerms(termPath, subTerm, termSetId, clientContext);
                foreach (var foundTerm in moreSubTerms)
                {
                    items.Add(foundTerm.Key, foundTerm.Value);
                }

            }
            return items;
        }

        private static string GetTermLabel(ITerm term, string language) =>
            term.Labels.AsRequested().FirstOrDefault(l => l.IsDefault && l.LanguageTag == language)?.Name ??
            term.Labels.AsRequested().FirstOrDefault(l => l.IsDefault)?.Name ?? term.Labels.AsRequested().First().Name;

        #endregion

        /// <summary>
        /// Validate the source term contains the path and is recognised in the term store
        /// </summary>
        /// <param name="context"></param>
        /// <param name="termPath"></param>
        public TermData ResolveTermInCache(PnPContext context, string termPath)
        {
            //Use the cache
            var result = CacheManager.Instance.GetTransformTermCacheTermByName(context, termPath:termPath);
            if (result != default && result.Any())
            {
                var cachedTerm = result.First();
                cachedTerm.IsTermResolved = true;
                return cachedTerm; // First mapping
            }

            return default;
        }

        /// <summary>
        /// Validate the source term contains the GUID and is recognised in the term store
        /// </summary>
        /// <param name="context"></param>
        /// <param name="termId"></param>
        public TermData ResolveTermInCache(PnPContext context, Guid termId)
        {
            //Use the cache
            var cachedTerm = CacheManager.Instance.GetTransformTermCacheTermById(context, termId);
            if (cachedTerm != default)
            {
                cachedTerm.IsTermResolved = true;
            }
            return cachedTerm;
        }

        /// <summary>
        /// Extracts the term set id from the xml schema
        /// </summary>
        /// <param name="xmlfieldSchema">XML Schema</param>
        /// <param name="findSspId">If true the SspId will be returned, otherwise the TermSetId will be</param>
        /// <returns>TermSetId or SspId depending on <paramref name="findSspId"/> value</returns>
        public static string ExtractTermSetIdOrSspIdFromXmlSchema(string xmlfieldSchema, bool findSspId = false)
        {
            //Credit: https://sharepointfieldnotes.blogspot.com/2011/08/sharepoint-2010-code-tips-setting.html

            string termSetId = string.Empty;

            var schemaRoot = XElement.Parse(xmlfieldSchema);

            foreach (var property in schemaRoot.Descendants("Property"))
            {
                var name = property.Element("Name");
                var value = property.Element("Value");

                if (name != null && value != null)
                {
                    if (!findSspId)
                    {
                        if (name.Value == "TermSetId")
                        {
                            return value.Value;
                        }
                    }
                    else
                    {
                        if (name.Value == "SspId")
                        {
                            return value.Value;
                        }
                    }
                }
            }

            return string.Empty;
        }

    }

    /// <summary>
    /// Class containing details of terms returned from web services
    /// </summary>
    class XmlTermSetTerm
    {
        public string TermLabel { get; set; }
        public string TermGuid { get; set; }
        public bool HasChildren { get; set; }

    }
}

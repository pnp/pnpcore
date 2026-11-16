using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Taxonomy;
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
        private ClientContext _sourceContext;
        private ClientContext _targetContext;
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
        public TermTransformator(BaseTransformationInformation baseTransformationInformation, ClientContext sourceContext, ClientContext targetContext, IList<ILogObserver> logObservers = null)
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
        public Tuple<TaxonomyFieldValueCollection,List<TaxonomyFieldValue>>  TransformCollection(TaxonomyFieldValueCollection taxonomyFieldValueCollection)
        {
            List<TaxonomyFieldValue> exceptFields = new List<TaxonomyFieldValue>();
            
            foreach (var fieldValue in taxonomyFieldValueCollection)
            {
                var result = this.Transform(new TermData() { TermGuid = Guid.Parse(fieldValue.TermGuid), TermLabel = fieldValue.Label });
                if (result.IsTermResolved)
                {
                    fieldValue.Label = result.TermLabel;
                    fieldValue.TermGuid = result.TermGuid.ToString();
                }
                else
                {
                    exceptFields.Add(fieldValue);
                }
            }
            
            // Return fields to remove by calling code.
            return new Tuple<TaxonomyFieldValueCollection, List<TaxonomyFieldValue>>(taxonomyFieldValueCollection, exceptFields);
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
                Cache.CacheManager.Instance.StoreTermSetTerms(this._sourceContext, sourceTermSetId, sourceSspId, isSP2010, true);
            }

            if (targetTermSetId != Guid.Empty)
            {
                Cache.CacheManager.Instance.StoreTermSetTerms(this._targetContext, targetTermSetId, sourceSspId, false, false);
            }

        }

        #region Called from Cache Manager

        /// <summary>
        /// Extract all the terms from a termset for caching and quicker processing
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Dictionary<Guid, TermData> GetAllTermsFromTermSet(Guid termSetId, ClientContext context)
        {
            //Use a source and target Dictionary<guid,string>
            //Key = Id, Value = Path(e.g.termgroup | termset | term)
            var termsCache = new Dictionary<Guid, TermData>();

            try
            {
                using (var clonedContext = context.Clone(context.Web.GetUrl()))
                {
                    TaxonomySession session = TaxonomySession.GetTaxonomySession(clonedContext);
                    TermStore termStore = session.GetDefaultSiteCollectionTermStore();
                    var termSet = termStore.GetTermSet(termSetId);
                    var termGroup = termSet.Group;
                    clonedContext.Load(termSet, t => t.Terms, t => t.Name);
                    clonedContext.Load(termGroup, g => g.Name);
                    clonedContext.ExecuteQueryRetry();

                    var termGroupName = termGroup.Name;
                    var setName = termSet.Name;
                    var termSetPath = $"{termGroupName}{TermTransformator.TermNodeDelimiter}{setName}";
                    foreach (var term in termSet.Terms)
                    {
                        var termName = term.Name;
                        var termPath = $"{termSetPath}{TermNodeDelimiter}{termName}";
                        termsCache.Add(term.Id,
                            new TermData() { TermGuid = term.Id, TermLabel = termName, TermPath = termPath, TermSetId = termSetId });

                        if (term.TermsCount > 0)
                        {
                            var subTerms = ParseSubTerms(termPath, term, termSetId, clonedContext);
                            //termsCache
                            foreach (var foundTerm in subTerms)
                            {
                                termsCache.Add(foundTerm.Key, foundTerm.Value);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                //TODO: Record any failure
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
        public static Dictionary<Guid, TermData> ParseSubTerms(string subTermPath, Term term, Guid termSetId, ClientRuntimeContext clientContext)
        {
            var items = new Dictionary<Guid, TermData>();
            if (term.ServerObjectIsNull == null || term.ServerObjectIsNull == false)
            {
                clientContext.Load(term.Terms);
                clientContext.ExecuteQueryRetry();
            }

            foreach (var subTerm in term.Terms)
            {
                var termName = subTerm.Name;
                var termPath = $"{subTermPath}{TermTransformator.TermNodeDelimiter}{termName}";

                items.Add(subTerm.Id, new TermData() { TermGuid = subTerm.Id, TermLabel = termName, TermPath = termPath, TermSetId = termSetId });

                if (term.TermsCount > 0)
                {
                    var moreSubTerms = ParseSubTerms(termPath, subTerm, termSetId, clientContext);
                    foreach (var foundTerm in moreSubTerms)
                    {
                        items.Add(foundTerm.Key, foundTerm.Value);
                    }
                }

            }
            return items;
        }

        #endregion

        /// <summary>
        /// Validate the source term contains the path and is recognised in the term store
        /// </summary>
        /// <param name="context"></param>
        /// <param name="termPath"></param>
        public TermData ResolveTermInCache(ClientContext context, string termPath)
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
        public TermData ResolveTermInCache(ClientContext context, Guid termId)
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

        /// <summary>
        /// Calls the web services to get the termset details
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sspId"></param>
        /// <param name="termSetId"></param>
        /// <returns></returns>
        public static Dictionary<Guid, TermData> CallTaxonomyWebServiceFindTermSetId(ClientContext context, Guid sspId, Guid termSetId)
        {
            var termsCache = new Dictionary<Guid, TermData>();

            try
            {
                //LogInfo("", LogStrings.Heading_ContentTransform);

                #region Web Service Call

                string webUrl = context.Web.GetUrl();
                string webServiceUrl = webUrl + "/_vti_bin/taxonomyclientservice.asmx";

                StringBuilder soapEnvelope = new StringBuilder();

                soapEnvelope.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                soapEnvelope.Append("<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
                soapEnvelope.Append(String.Format(
                 "<soap:Body>" +
                     "<GetChildTermsInTermSet xmlns=\"http://schemas.microsoft.com/sharepoint/taxonomy/soap/\">" +
                       "<sspId>{0}</sspId>" +
                       "<termSetId>{1}</termSetId> "+
                       "<lcid>{2}</lcid>" +
                     "</GetChildTermsInTermSet>" +
                 "</soap:Body>", sspId.ToString(), termSetId.ToString(),1033));

                soapEnvelope.Append("</soap:Envelope>");

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(webServiceUrl);
                request.AddAuthenticationData(context);
                request.Method = "POST";
                request.ContentType = "text/xml; charset=\"utf-8\"";
                request.Accept = "text/xml";
                request.Headers.Add("SOAPAction", "\"http://schemas.microsoft.com/sharepoint/taxonomy/soap/GetChildTermsInTermSet\"");

                using (System.IO.Stream stream = request.GetRequestStream())
                {
                    using (System.IO.StreamWriter writer = new System.IO.StreamWriter(stream))
                    {
                        writer.Write(soapEnvelope.ToString());
                    }
                }

                #endregion

                #region Web Service Response

                var response = request.GetResponse();
                using (var dataStream = response.GetResponseStream())
                {
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.Load(dataStream);

                    if (xDoc.DocumentElement != null && xDoc.DocumentElement.InnerText.Length > 0)
                    {
                        #region Process Xml

                        /*
                            Response Example from WS Key:                            

                            T = Term
                            TMS = TermSet
                            TM = Term
                            TL = Term Label

                            a31 = default label
                            a9 / a45 = term id
                            a32 = label
                            a12 = term set label
                            a24 = term set id
                            a1000/a69 = hasChildren ?

                       */

                        XElement queryXml = XElement.Parse(xDoc.DocumentElement.InnerText);
                        var xmlTermSetId = termSetId.ToString().Trim('{','}');
                        var xmlTermSetLabel = "";
                        var foundTermSetName = false;
                        var listOfTerms = new List<XmlTermSetTerm>();

                        //Term Details
                        foreach (XElement property in queryXml.Descendants("T"))
                        {
                            var term = new XmlTermSetTerm();

                            var queryTermId = property.Attribute("a9");
                            term.TermGuid = queryTermId?.Value;

                            var queryDefaultLabel = property.Descendants("TL").FirstOrDefault(desc => desc.Attribute("a31").Value == "true"); //Default Labels
                            term.TermLabel = queryDefaultLabel.Attribute("a32").Value;

                            var queryTermSetName = property.Descendants("TM").FirstOrDefault(tsn => tsn.Attribute("a24").Value == xmlTermSetId);
                            if (queryTermSetName != null)
                            {
                                term.HasChildren = (queryTermSetName.Attribute("a69")?.Value == "true");

                                if (!foundTermSetName)
                                {
                                    xmlTermSetLabel = queryTermSetName.Attribute("a12")?.Value;
                                    foundTermSetName = true;
                                }
                            }

                            listOfTerms.Add(term);

                            
                        }

                        //Term Set Details

                        #endregion

                        #region Process Data

                        var termSetPath = $"{TermTransformator.TermGroupUnknownName}{TermTransformator.TermNodeDelimiter}{xmlTermSetLabel}";
                        foreach (var term in listOfTerms)
                        {
                            var termName = term.TermLabel;
                            var termPath = $"{termSetPath}{TermNodeDelimiter}{termName}";
                            termsCache.Add(new Guid(term.TermGuid),
                                new TermData() { TermGuid = new Guid(term.TermGuid), TermLabel = termName, TermPath = termPath, TermSetId = termSetId });

                            if (term.HasChildren)
                            {
                                var moreSubTerms = TermTransformator.CallTaxonomyWebServiceFindChildTerms(context, sspId, termSetId, Guid.Parse(term.TermGuid), termPath);
                                foreach (var foundTerm in moreSubTerms)
                                {
                                    termsCache.Add(foundTerm.Key, foundTerm.Value);
                                }
                            }
                        }

                        #endregion
                    }
                }

                #endregion
            }
            catch (WebException)
            {
                //LogError("An error occurred calling the web services", LogStrings.Heading_ContentTransform, ex);

            }

            return termsCache;
        }

        /// <summary>
        /// Finds the child terms using the fall back web services
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sspId"></param>
        /// <param name="termSetId"></param>
        /// <param name="termId"></param>
        /// <param name="subTermPath"></param>
        /// <returns></returns>
        public static Dictionary<Guid, TermData> CallTaxonomyWebServiceFindChildTerms(ClientContext context, Guid sspId, Guid termSetId, Guid termId, string subTermPath)
        {
            var termsCache = new Dictionary<Guid, TermData>();

            try
            {
                //LogInfo("", LogStrings.Heading_ContentTransform);

                #region Web Service Call

                string webUrl = context.Web.GetUrl();
                string webServiceUrl = webUrl + "/_vti_bin/taxonomyclientservice.asmx";

                StringBuilder soapEnvelope = new StringBuilder();

                soapEnvelope.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                soapEnvelope.Append("<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
                soapEnvelope.Append(String.Format(
                 "<soap:Body>" +
                     "<GetChildTermsInTerm xmlns=\"http://schemas.microsoft.com/sharepoint/taxonomy/soap/\">" +
                       "<sspId>{0}</sspId>" +
                       "<termSetId>{1}</termSetId> " +
                       "<lcid>{2}</lcid>" +
                       "<termId>{3}</termId>" +
                     "</GetChildTermsInTerm>" +
                 "</soap:Body>", sspId.ToString(), termSetId.ToString(), 1033, termId.ToString()));

                soapEnvelope.Append("</soap:Envelope>");

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(webServiceUrl);
                request.AddAuthenticationData(context);
                request.Method = "POST";
                request.ContentType = "text/xml; charset=\"utf-8\"";
                request.Accept = "text/xml";
                request.Headers.Add("SOAPAction", "\"http://schemas.microsoft.com/sharepoint/taxonomy/soap/GetChildTermsInTerm\"");

                using (System.IO.Stream stream = request.GetRequestStream())
                {
                    using (System.IO.StreamWriter writer = new System.IO.StreamWriter(stream))
                    {
                        writer.Write(soapEnvelope.ToString());
                    }
                }

                #endregion

                #region Web Service Response

                var response = request.GetResponse();
                using (var dataStream = response.GetResponseStream())
                {
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.Load(dataStream);

                    if (xDoc.DocumentElement != null && xDoc.DocumentElement.InnerText.Length > 0)
                    {
                        #region Process Xml

                        /*
                            Response Example from WS Key:                            

                            T = Term
                            TMS = TermSet
                            TM = Term
                            TL = Term Label

                            a31 = default label
                            a9  = term id
                            a45 = term path ids
                            a32 = label
                            a12 = term set label
                            a24 = term set id
                            a25 = parent term id
                            a40 = parent term/term set
                            a1000/a69 = hasChildren ?

                       */

                        XElement queryXml = XElement.Parse(xDoc.DocumentElement.InnerText);
                        var xmlTermSetId = termSetId.ToString().Trim('{', '}');
                        var listOfTerms = new List<XmlTermSetTerm>();

                        ////Term Details
                        foreach (XElement property in queryXml.Descendants("T"))
                        {
                            var term = new XmlTermSetTerm();

                            var queryTermId = property.Attribute("a9");
                            term.TermGuid = queryTermId?.Value;

                            var queryDefaultLabel = property.Descendants("TL").FirstOrDefault(desc => desc.Attribute("a31").Value == "true"); //Default Labels
                            term.TermLabel = queryDefaultLabel.Attribute("a32").Value;

                            var queryTermSetName = property.Descendants("TM").FirstOrDefault(tsn => tsn.Attribute("a24").Value == xmlTermSetId);
                            if (queryTermSetName != null)
                            {
                                term.HasChildren = (queryTermSetName.Attribute("a69")?.Value == "true");
                            }

                            listOfTerms.Add(term);
                        }

                        #endregion

                        #region Process Data

                        foreach (var term in listOfTerms)
                        {
                            var termName = term.TermLabel;
                            var termPath = $"{subTermPath}{TermTransformator.TermNodeDelimiter}{termName}";

                            termsCache.Add(new Guid(term.TermGuid),
                                new TermData() { TermGuid = new Guid(term.TermGuid), TermLabel = termName, TermPath = termPath, TermSetId = termSetId });

                            if (term.HasChildren)
                            {
                                var moreSubTerms = CallTaxonomyWebServiceFindChildTerms(context, sspId, termSetId, Guid.Parse(term.TermGuid), termPath);
                                foreach (var foundTerm in moreSubTerms)
                                {
                                    termsCache.Add(foundTerm.Key, foundTerm.Value);
                                }
                            }
                        }

                        #endregion
                    }
                }

                #endregion
            }
            catch (WebException)
            {
                //LogError("An error occurred calling the web services", LogStrings.Heading_ContentTransform, ex);

            }

            return termsCache;
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

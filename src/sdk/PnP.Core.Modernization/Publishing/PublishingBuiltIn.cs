using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using PnP.Core.Modernization.Functions;
using PnP.Core.Modernization.Telemetry;
using PnP.Core.Modernization.Transform;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Modernization.Publishing
{
    public class PublishingBuiltIn: FunctionsBase
    {
        private ClientContext sourceClientContext;
        private ClientContext targetClientContext;
        private HtmlParser parser;
        private BuiltIn builtIn;
        private BaseTransformationInformation baseTransformationInformation;
        private UserTransformator userTransformator;

        #region Construction
        /// <summary>
        /// Instantiates the base builtin function library
        /// </summary>
        /// <param name="baseTransformationInformation">Page transformation information</param>
        /// <param name="sourceClientContext">The ClientContext for the source </param>
        /// <param name="targetClientContext"></param>
        /// <param name="logObservers"></param>
        public PublishingBuiltIn(BaseTransformationInformation baseTransformationInformation, ClientContext sourceClientContext, ClientContext targetClientContext, IList<ILogObserver> logObservers = null) : base(sourceClientContext)
        {
            if (logObservers != null)
            {
                foreach (var observer in logObservers)
                {
                    base.RegisterObserver(observer);
                }
            }

            this.sourceClientContext = sourceClientContext;
            this.targetClientContext = targetClientContext;
            this.baseTransformationInformation = baseTransformationInformation;
            this.parser = new HtmlParser();
            this.builtIn = new BuiltIn(this.baseTransformationInformation, targetClientContext, sourceClientContext, logObservers: logObservers);
            this.userTransformator = new UserTransformator(baseTransformationInformation, this.sourceClientContext, this.targetClientContext, base.RegisteredLogObservers);
        }
        #endregion

        #region Text functions
        /// <summary>
        /// Returns an empty string
        /// </summary>
        /// <returns>Empty string</returns>
        [FunctionDocumentation(Description = "Returns an empty string",
                               Example = "EmptyString()")]
        [OutputDocumentation(Name = "return value", Description = "Empty string")]
        public string EmptyString()
        {
            return "";
        }


        /// <summary>
        /// Returns an the (static) string provided as input
        /// </summary>
        /// <param name="staticString">Static string that will be returned</param>
        /// <returns>String provided as input</returns>
        [FunctionDocumentation(Description = "Returns an the (static) string provided as input",
                               Example = "StaticString('static string')")]
        [InputDocumentation(Name = "'static string'", Description = "Static input string")]
        [OutputDocumentation(Name = "return value", Description = "String provided as input")]
        public string StaticString(string staticString)
        {
            return staticString;
        }

        /// <summary>
        /// Prefixes a string
        /// </summary>
        /// <param name="prefix">Prefix to be applied</param>
        /// <param name="content">Value to apply the prefix to</param>
        /// <param name="applyIfContentIsEmpty">Apply prefix also when the content field is empty</param>
        /// <returns>Prefixed string</returns>
        [FunctionDocumentation(Description = "Prefixes the input text with another text. The applyIfContentIsEmpty parameter controls if the prefix also needs to happen when the actual content is empty",
                               Example = "Prefix('&lt;H1&gt;Prefix some extra text&lt;/H1&gt;', {PublishingPageContent}, 'false')")]
        [InputDocumentation(Name = "'prefix string'", Description = "Static input string which will be used as prefix")]
        [InputDocumentation(Name = "{PublishingPageContent}", Description = "The actual publishing page HTML field content to prefix")]
        [InputDocumentation(Name = "'static boolean value'", Description = "Static bool ('true', 'false') to indicate if the prefixing still needs to happen when the {PublishingPageContent} field content is emty")]
        [OutputDocumentation(Name = "return value", Description = "Value of {PublishingPageContent} prefixed with the provided prefix value")]
        public string Prefix(string prefix, string content, string applyIfContentIsEmpty)
        {
            if (!string.IsNullOrEmpty(prefix) && !string.IsNullOrEmpty(content))
            {
                return prefix + content;
            }
            else if (string.IsNullOrEmpty(prefix) && !string.IsNullOrEmpty(content))
            {
                return content;
            }
            else if (!string.IsNullOrEmpty(prefix) && string.IsNullOrEmpty(content))
            {
                if (!bool.TryParse(applyIfContentIsEmpty, out bool applyIfEmpty))
                {
                    applyIfEmpty = true;
                }

                if (applyIfEmpty)
                {
                    return prefix;
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Suffixes a string
        /// </summary>
        /// <param name="suffix">Suffix to be applied</param>
        /// <param name="content">Value to apply the suffix to</param>
        /// <param name="applyIfContentIsEmpty">Apply suffix also when the content field is empty</param>
        /// <returns>Prefixed string</returns>
        [FunctionDocumentation(Description = "Suffixes the input text with another text. The applyIfContentIsEmpty parameter controls if the suffix also needs to happen when the actual content is empty",
                               Example = "Suffix('&lt;H1&gt;Suffix some extra text&lt;/H1&gt;', {PublishingPageContent}, 'false')")]
        [InputDocumentation(Name = "'suffix string'", Description = "Static input string which will be used as suffix")]
        [InputDocumentation(Name = "{PublishingPageContent}", Description = "The actual publishing page HTML field content to suffix")]
        [InputDocumentation(Name = "'static boolean value'", Description = "Static bool ('true', 'false') to indicate if the suffixing still needs to happen when the {PublishingPageContent} field content is emty")]
        [OutputDocumentation(Name = "return value", Description = "Value of {PublishingPageContent} suffixed with the provided suffix value")]
        public string Suffix(string suffix, string content, string applyIfContentIsEmpty)
        {
            if (!string.IsNullOrEmpty(suffix) && !string.IsNullOrEmpty(content))
            {
                return content + suffix;
            }
            else if (string.IsNullOrEmpty(suffix) && !string.IsNullOrEmpty(content))
            {
                return content;
            }
            else if (!string.IsNullOrEmpty(suffix) && string.IsNullOrEmpty(content))
            {
                if (!bool.TryParse(applyIfContentIsEmpty, out bool applyIfEmpty))
                {
                    applyIfEmpty = true;
                }

                if (applyIfEmpty)
                {
                    return suffix;
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Prefixes and suffixes a string
        /// </summary>
        /// <param name="prefix">Prefix to be applied</param>
        /// <param name="suffix">Suffix to be applied</param>
        /// <param name="content">Value to apply the prefix/suffix to</param>
        /// <param name="applyIfContentIsEmpty">Apply prefix/suffix also when the content field is empty</param>
        /// <returns>Prefixed string</returns>
        [FunctionDocumentation(Description = "Prefixes and suffixes the input text with another text. The applyIfContentIsEmpty parameter controls if the prefix/suffix also needs to happen when the actual content is empty",
                               Example = "PrefixAndSuffix('&lt;H1&gt;Prefix some extra text&lt;/H1&gt;','&lt;H1&gt;Suffix some extra text&lt;/H1&gt;',{PublishingPageContent},'false')")]
        [InputDocumentation(Name = "'prefix string'", Description = "Static input string which will be used as prefix")]
        [InputDocumentation(Name = "'suffix string'", Description = "Static input string which will be used as suffix")]
        [InputDocumentation(Name = "{PublishingPageContent}", Description = "The actual publishing page HTML field content to prefix/suffix")]
        [InputDocumentation(Name = "'static boolean value'", Description = "Static bool ('true', 'false') to indicate if the prefixing/suffixing still needs to happen when the {PublishingPageContent} field content is emty")]
        [OutputDocumentation(Name = "return value", Description = "Value of {PublishingPageContent} prefixed/suffixed with the provided values")]
        public string PrefixAndSuffix(string prefix, string suffix, string content, string applyIfContentIsEmpty)
        {
            if (!string.IsNullOrEmpty(prefix) && !string.IsNullOrEmpty(suffix) && !string.IsNullOrEmpty(content))
            {
                return prefix + content + suffix;
            }
            if (!string.IsNullOrEmpty(prefix) && string.IsNullOrEmpty(suffix) && !string.IsNullOrEmpty(content))
            {
                return prefix + content;
            }
            if (string.IsNullOrEmpty(prefix) && !string.IsNullOrEmpty(suffix) && !string.IsNullOrEmpty(content))
            {
                return content + suffix;
            }
            else if (string.IsNullOrEmpty(suffix) && string.IsNullOrEmpty(prefix) && !string.IsNullOrEmpty(content))
            {
                return content;
            }
            else if (string.IsNullOrEmpty(content))
            {
                if (!bool.TryParse(applyIfContentIsEmpty, out bool applyIfEmpty))
                {
                    applyIfEmpty = true;
                }

                if (applyIfEmpty)
                {
                    if (!string.IsNullOrEmpty(prefix) && !string.IsNullOrEmpty(suffix))
                    {
                        return prefix + suffix;
                    }
                    else if (string.IsNullOrEmpty(prefix) && !string.IsNullOrEmpty(suffix))
                    {
                        return suffix;
                    }
                    else
                    {
                        return prefix;
                    }
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }
        #endregion

        #region Image functions
        /// <summary>
        /// Returns the server relative image url of a Publishing Image field value
        /// </summary>
        /// <param name="htmlImage">Publishing Image field value</param>
        /// <returns>Server relative image url</returns>
        [FunctionDocumentation(Description = "Returns the server relative image url of a Publishing Image field value.",
                       Example = "ToImageUrl({PublishingPageImage})")]
        [InputDocumentation(Name = "{PublishingPageImage}", Description = "Publishing Image field value")]
        [OutputDocumentation(Name = "return value", Description = "Server relative image url")]
        public string ToImageUrl(string htmlImage)
        {
            // If the image string is not a html image representation then simply return the trimmed value. If an image has a link it's wrapped in an anchor tag
            if (string.IsNullOrEmpty(htmlImage) || !(htmlImage.Trim().StartsWith("<img", System.StringComparison.InvariantCultureIgnoreCase) || htmlImage.Trim().StartsWith("<a", System.StringComparison.InvariantCultureIgnoreCase)))
            {
                return htmlImage;
            }

            // Sample input: <img alt="" src="/sites/devportal/PublishingImages/page-travel-instructions.jpg?RenditionID=2" style="BORDER: 0px solid; ">
            var htmlDoc = parser.ParseDocument(htmlImage);
            var imgElement = htmlDoc.QuerySelectorAll("img").FirstOrDefault();

            string imageUrl = "";

            if (imgElement != null && imgElement != default(IElement) && imgElement.HasAttribute("src"))
            {
                imageUrl = imgElement.GetAttribute("src");

                // drop of url params (if any)
                if (imageUrl.Contains("?"))
                {
                    imageUrl = imageUrl.Substring(0, imageUrl.IndexOf("?"));
                }
            }

            return imageUrl;
        }

        /// <summary>
        /// Returns the image alternate text of a Publishing Image field value.
        /// </summary>
        /// <param name="htmlImage">PublishingPageImage</param>
        /// <returns>Image alternate text</returns>
        [FunctionDocumentation(Description = "Returns the image alternate text of a Publishing Image field value.",
                       Example = "ToImageAltText({PublishingPageImage})")]
        [InputDocumentation(Name = "{PublishingPageImage}", Description = "Publishing Image field value")]
        [OutputDocumentation(Name = "return value", Description = "Image alternate text")]
        public string ToImageAltText(string htmlImage)
        {
            // If the image string is not a html image representation then simply return the trimmed value. If an image has a link it's wrapped in an anchor tag
            if (string.IsNullOrEmpty(htmlImage) || !(htmlImage.Trim().StartsWith("<img", System.StringComparison.InvariantCultureIgnoreCase) || htmlImage.Trim().StartsWith("<a", System.StringComparison.InvariantCultureIgnoreCase)))
            {
                return htmlImage;
            }

            // Sample input: <img alt="bla" src="/sites/devportal/PublishingImages/page-travel-instructions.jpg?RenditionID=2" style="BORDER: 0px solid; ">
            var htmlDoc = parser.ParseDocument(htmlImage);
            var imgElement = htmlDoc.QuerySelectorAll("img").FirstOrDefault();

            string imageAltText = "";

            if (imgElement != null && imgElement != default(IElement) && imgElement.HasAttribute("alt"))
            {
                imageAltText = imgElement.GetAttribute("alt");
            }

            return imageAltText;
        }

        /// <summary>
        /// Returns the image anchor url of a Publishing Image field value
        /// </summary>
        /// <param name="htmlImage">Publishing Image field value</param>
        /// <returns>Image anchor url</returns>
        [FunctionDocumentation(Description = "Returns the image anchor url of a Publishing Image field value.",
                       Example = "ToImageAnchor({PublishingPageImage})")]
        [InputDocumentation(Name = "{PublishingPageImage}", Description = "Publishing Image field value")]
        [OutputDocumentation(Name = "return value", Description = "Image anchor url")]
        public string ToImageAnchor(string htmlImage)
        {
            // If the image string is not a html image representation then simply return the trimmed value. If an image has a link it's wrapped in an anchor tag
            if (string.IsNullOrEmpty(htmlImage) || !(htmlImage.Trim().StartsWith("<img", System.StringComparison.InvariantCultureIgnoreCase) || htmlImage.Trim().StartsWith("<a", System.StringComparison.InvariantCultureIgnoreCase)))
            {
                return htmlImage;
            }

            // Sample input: <img alt="" src="/sites/devportal/PublishingImages/page-travel-instructions.jpg?RenditionID=2" style="BORDER: 0px solid; ">
            var htmlDoc = parser.ParseDocument(htmlImage);
            var anchorElement = htmlDoc.QuerySelectorAll("a").FirstOrDefault();

            string imageAnchor = "";

            if (anchorElement != null && anchorElement != default(IElement) && anchorElement.HasAttribute("href"))
            {
                imageAnchor = anchorElement.GetAttribute("href");

                // drop of url params (if any)
                if (imageAnchor.Contains("?"))
                {
                    imageAnchor = imageAnchor.Substring(0, imageAnchor.IndexOf("?"));
                }
            }

            return imageAnchor;
        }

        /// <summary>
        /// Returns the image caption of a Publishing Html image caption field
        /// </summary>
        /// <param name="htmlField">Publishing Html image caption field value</param>
        /// <returns>Image caption</returns>
        [FunctionDocumentation(Description = "Returns the image caption of a Publishing Html image caption field",
                       Example = "ToImageCaption({PublishingImageCaption})")]
        [InputDocumentation(Name = "{PublishingImageCaption}", Description = "Publishing Html image caption field value")]
        [OutputDocumentation(Name = "return value", Description = "Image caption")]
        public string ToImageCaption(string htmlField)
        {
            // If the image string is not a html image representation then simply return the trimmed value. If an image has a link it's wrapped in an anchor tag
            if (string.IsNullOrEmpty(htmlField))
            {
                return "";
            }

            // Sample input: <p>Some caption<BR></p> 
            try
            {
                var htmlDoc = parser.ParseDocument(htmlField);

                string imageCaption = null;

                if (htmlDoc.FirstElementChild != null)
                {
                    imageCaption = htmlDoc.FirstElementChild.TextContent;
                }

                if (!string.IsNullOrEmpty(imageCaption))
                {
                    return imageCaption;
                }
            }
            catch
            {
                // No need to fail for this reason...
            }

            return "";
        }

        /// <summary>
        /// Returns a page preview image url
        /// </summary>
        /// <param name="image">A publishing image field value or a string containing a server relative image path</param>
        /// <returns>A formatted preview image url</returns>
        [FunctionDocumentation(Description = "Returns a page preview image url.",
                                   Example = "ToPreviewImageUrl({PreviewImage})")]
        [InputDocumentation(Name = "{PreviewImage}", Description = "A publishing image field value or a string containing a server relative image path")]
        [OutputDocumentation(Name = "return value", Description = "A formatted preview image url")]
        public string ToPreviewImageUrl(string image)
        {
            if (string.IsNullOrEmpty(image))
            {
                return "";
            }

            // If the image string is a html image representation
            if (image.Trim().StartsWith("<img", System.StringComparison.InvariantCultureIgnoreCase) || image.Trim().StartsWith("<a", System.StringComparison.InvariantCultureIgnoreCase))
            {
                image = ToImageUrl(image);
            }

            // The image string should now be a server relative path...trigger asset transfer if needed by calling the builtin function ReturnCrossSiteRelativePath
            var previewServerRelativeUrl = this.builtIn.ReturnCrossSiteRelativePath(image);

            // Lookup the image properties by calling the builtin function ImageLookup
            var imageProperties = this.builtIn.ImageLookup(previewServerRelativeUrl);

            // Construct preview image url
            string siteIdString = this.targetClientContext.Site.EnsureProperty(p => p.Id).ToString().Replace("-", "");
            string webIdString = this.targetClientContext.Web.EnsureProperty(p => p.Id).ToString().Replace("-", "");
            if (imageProperties.TryGetValue("ImageUniqueId", out string uniqueIdString))
            {
                uniqueIdString = uniqueIdString.Replace("-", "");
                string extension = System.IO.Path.GetExtension(previewServerRelativeUrl);
                if (!string.IsNullOrEmpty(extension))
                {
                    extension = extension.Replace(".", "");
                }

                if (!string.IsNullOrEmpty(siteIdString) && !string.IsNullOrEmpty(webIdString) && !string.IsNullOrEmpty(uniqueIdString) && !string.IsNullOrEmpty(extension))
                {
                    return $"{this.targetClientContext.Web.GetUrl()}/_layouts/15/getpreview.ashx?guidSite={siteIdString}&guidWeb={webIdString}&guidFile={uniqueIdString}&ext={extension}";
                }
            }

            // Something went wrong...leave preview image url blank so that the default logic during page save can still pick up a nice preview image
            return "";
        }
        #endregion

        #region Person functions
        /// <summary>
        /// Looks up user information for passed user id
        /// </summary>
        /// <param name="userId">The id (int) of a user</param>
        /// <returns>A formatted json blob describing the user's details</returns>
        [FunctionDocumentation(Description = "Looks up user information for passed user id",
                                   Example = "ToAuthors({PublishingContact})")]
        [InputDocumentation(Name = "{userId}", Description = "The id (int) of a user")]
        [OutputDocumentation(Name = "return value", Description = "A formatted json blob describing the user's details")]

        public string ToAuthors(string userId)
        {
            if (int.TryParse(userId, out int userIdInt))
            {
                // Get the user information from the source site
                var author = Cache.CacheManager.Instance.GetUserFromUserList(this.sourceClientContext, userIdInt);

                // If the provided ID is a group then no point in continuing...
                if (author != null && !author.IsGroup)
                {
                    // Will this user be mapped to another user?
                    var newUpn = this.userTransformator.RemapPrincipal(author.LoginName);

                    // Drop online prefix to avoid second unneeded lookup via upn later on
                    if (newUpn.StartsWith("i:0#.f|membership|"))
                    {
                        newUpn = newUpn.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries)[2];
                    }

                    if (!string.IsNullOrEmpty(newUpn) && !newUpn.Equals(author.Upn, StringComparison.InvariantCultureIgnoreCase))
                    {
                        // We'll need to retrieve the info from this user again as we've mapped to another user account in the target site
                        author = Cache.CacheManager.Instance.GetUserFromUserList(this.targetClientContext, newUpn);

                        if (author == null)
                        {
                            // The principal returned from the user mapping is not available on the target site, so return empty
                            return "";
                        }
                    }

                    // Don't serialize null values
                    var jsonSerializerSettings = new JsonSerializerSettings()
                    {
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore
                    };

                    var json = JsonConvert.SerializeObject(author, jsonSerializerSettings);
                    return json;
                }
            }

            return "";
        }
        #endregion

        #region Taxonomy functions
        /// <summary>
        /// Populate a taxonomy field based upon provided term id's. You can configure to optionally overwrite existing values
        /// </summary>
        /// <param name="fieldValue">List of term id's to set, multiple values can also be used when the taxonomy field is configured to accept multiple terms</param>
        /// <param name="termIdString">Static bool ('true', 'false') to indicate if the default term values have to be set in case the fiels already contains terms</param>
        /// <param name="overwriteString">String with term information needed to set the taxonomy field</param>
        /// <returns></returns>
        [FunctionDocumentation(Description = "Populate a taxonomy field based upon provided term id's. You can configure to optionally overwrite existing values",
                               Example = "DefaultTaxonomyFieldValue({TaxField2},'a65537e8-aa27-4b3a-bad6-f0f61f84b9f7|69524923-a5a0-44d1-b5ec-7f7c6d0ec160','true')")]
        [InputDocumentation(Name = "{Taxonomy Field}", Description = "The taxonomy field to update")]
        [InputDocumentation(Name = "'term ids split by |'", Description = "List of term id's to set, multiple values can also be used when the taxonomy field is configured to accept multiple terms")]
        [InputDocumentation(Name = "'static boolean value'", Description = "Static bool ('true', 'false') to indicate if the default term values have to be set in case the fiels already contains terms")]
        [OutputDocumentation(Name = "return value", Description = "String with term information needed to set the taxonomy field")]
        public string DefaultTaxonomyFieldValue(string fieldValue, string termIdString, string overwriteString)
        {
            List<string> termIds = new List<string>();
            if (termIdString.Contains("|"))
            {
                string[] termIdParts = termIdString.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                termIds.AddRange(termIdParts);
            }
            else
            {
                termIds.Add(termIdString);
            }

            if (!bool.TryParse(overwriteString, out bool overwrite))
            {
                overwrite = false;
            }

            if (!string.IsNullOrEmpty(fieldValue) && !overwrite)
            {
                return null;
            }

            string resultingTermInfo = "";
            foreach (var term in termIds)
            {
                if (Guid.TryParse(term, out Guid termId))
                {
                    var termInfo = Cache.CacheManager.Instance.GetTermFromId(this.targetClientContext, termId);

                    if (termInfo != null)
                    {
                        if (string.IsNullOrEmpty(resultingTermInfo))
                        {
                            resultingTermInfo = $"{termInfo}|{termId}";
                        }
                        else
                        {
                            resultingTermInfo += $"§{termInfo}|{termId}";
                        }
                    }                    
                }
            }

            return resultingTermInfo;
        }
        #endregion

    }
}

using System;
using System.Collections.Generic;


namespace PnP.Core.Modernization
{
    /// <summary>
    /// SharePoint Online modernization extensions for Core sites.
    /// </summary>
    public static class SiteExtensionsModernization
    {
        /// <summary>
        /// Gets all sub sites for a given site
        /// </summary>
        /// <param name="site">Site to find all sub site for</param>
        /// <returns>IEnumerable of strings holding the sub site urls</returns>
        public static IEnumerable<string> GetAllSubSites(this ISite site)
        {
            PnPContext siteContext = site.PnPContext;
            SharePointOnlineSupport.Validate(siteContext);
            site.LoadBatch(s => s.Url);
            siteContext.Execute();
            var queue = new Queue<string>();
            queue.Enqueue(site.Url.AbsoluteUri);
            while (queue.Count > 0)
            {
                var currentUrl = queue.Dequeue();
                try
                {
                    using (PnPContext webContext = siteContext.Clone(new Uri(currentUrl)))
                    {
                        IWebCollection webs = webContext.Web.Webs;
                        webs.LoadBatch();
                        webContext.Execute();
                        foreach(IWeb subWeb in webs)
                        {
                            // Ensure these props are loaded...sometimes the initial load did not handle this
                            subWeb.EnsureProperties(s => s.Url, s => s.WebTemplate);
                            
                            // Don't dive into App webs and Access Services web apps
                            if (!subWeb.WebTemplate.Equals("App", StringComparison.InvariantCultureIgnoreCase) &&
                                !subWeb.WebTemplate.Equals("ACCSVC", StringComparison.InvariantCultureIgnoreCase))
                            {
                                queue.Enqueue(subWeb.Url.AbsoluteUri);
                            }
                        }
                    }
                }
                catch(Exception)
                {
                    // Eat exceptions when certain subsites aren't accessible, better this then breaking the complete flow
                }

                yield return currentUrl;
            }
        }
    }
}

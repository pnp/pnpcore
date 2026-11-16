using System;
using System.Collections.Generic;

using Microsoft.SharePoint.Client;

namespace PnP.Core.Modernization
{
    /// <summary>
    /// Class holding extension methods for the Microsoft.SharePoint.Client.Site object
    /// </summary>
    public static class SiteExtensionsModernization
    {
        /// <summary>
        /// Gets all sub sites for a given site
        /// </summary>
        /// <param name="site">Site to find all sub site for</param>
        /// <returns>IEnumerable of strings holding the sub site urls</returns>
        public static IEnumerable<string> GetAllSubSites(this Site site)
        {
            var siteContext = site.Context;
            siteContext.Load(site, s => s.Url);
            siteContext.ExecuteQueryRetry();
            var queue = new Queue<string>();
            queue.Enqueue(site.Url);
            while (queue.Count > 0)
            {
                var currentUrl = queue.Dequeue();
                try
                {
                    using (var webContext = siteContext.Clone(currentUrl))
                    {
                        var webs = webContext.Web.GetSubwebsForCurrentUser(null);
                        webContext.Load(webs);
                        webContext.ExecuteQueryRetry();
                        foreach(var subWeb in webs)
                        {
                            // Ensure these props are loaded...sometimes the initial load did not handle this
                            subWeb.EnsureProperties(s => s.Url, s => s.WebTemplate);
                            
                            // Don't dive into App webs and Access Services web apps
                            if (!subWeb.WebTemplate.Equals("App", StringComparison.InvariantCultureIgnoreCase) &&
                                !subWeb.WebTemplate.Equals("ACCSVC", StringComparison.InvariantCultureIgnoreCase))
                            {
                                queue.Enqueue(subWeb.Url);
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

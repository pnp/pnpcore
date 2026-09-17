using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// Strips artefacts from a template that a NoScript site will not accept, so provisioning
    /// degrades rather than failing outright.
    /// </summary>
    internal class NoScriptTemplateCleaner
    {
        private readonly PnPContext _context;

        public ProvisioningMessagesDelegate MessagesDelegate { get; set; }

        /// <summary>
        /// Creates a cleaner for the given site.
        /// </summary>
        /// <param name="context">The context of the site the template will be applied to</param>
        public NoScriptTemplateCleaner(PnPContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Removes what a NoScript site cannot accept from the template.
        /// </summary>
        /// <param name="template">The template to clean</param>
        /// <returns>The same template, with unsupported artefacts removed</returns>
        public async Task<ProvisioningTemplate> CleanUpBeforeProvisioningAsync(ProvisioningTemplate template)
        {
            bool isNoScriptSite = await _context.Web.IsNoScriptSiteAsync().ConfigureAwait(false);

            if (!isNoScriptSite)
            {
                return template;
            }

            var listsToRemove = new List<ListInstance>();

            foreach (ListInstance templateList in template.Lists)
            {
                if (templateList.Url == "Style Library")
                {
                    listsToRemove.Add(templateList);
                    WriteMessage(
                        string.Format(System.Globalization.CultureInfo.CurrentCulture,
                            PnPCoreProvisioningResources.Provisioning_ObjectHandlers_ListInstances_List__0__is_Style_Library_of_NoScript_will_Skip,
                            templateList.Title),
                        ProvisioningMessageType.Warning);
                }
            }

            foreach (ListInstance listToRemove in listsToRemove)
            {
                template.Lists.Remove(listToRemove);
            }

            return template;
        }

        internal void WriteMessage(string message, ProvisioningMessageType messageType)
        {
            MessagesDelegate?.Invoke(message, messageType);
        }
    }
}

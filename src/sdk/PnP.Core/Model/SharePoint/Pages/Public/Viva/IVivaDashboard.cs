using PnP.Core.Model.SharePoint.Pages.Public;
using PnP.Core.Model.SharePoint.Pages.Public.Viva;
using PnP.Core.Model.SharePoint.Pages.Public.Viva.AdaptiveCardExtensions.ACEFactory;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint.Viva
{
    /// <summary>
    /// Represents Viva Dashboard page
    /// </summary>
    public interface IVivaDashboard
    {
        /// <summary>
        /// Returns list of ACEs added to the Dashboard
        /// </summary>
        public List<AdaptiveCardExtension> ACEs { get; }
        /// <summary>
        /// Saves changes made to ACEs
        /// </summary>
        public void Save();
        /// <summary>
        /// Saves changes made to ACEs
        /// </summary>
        public Task SaveAsync();
        /// <summary>
        /// Adds new ACE
        /// </summary>
        public void AddACE(AdaptiveCardExtension ace);
        /// <summary>
        /// Removes ACE with provided Guid from the Dashboard
        /// </summary>
        /// <param name="instanceId"></param>
        public void RemoveACE(Guid instanceId);
        /// <summary>
        /// Loads manifest of provided ACE. Useful to set up a default configuration of an ACE
        /// </summary>
        /// <typeparam name="T">Type of parameters of the ACE</typeparam>
        /// <param name="aceId">Id of the ACE</param>
        /// <returns></returns>
        public PageComponentManifest<T> LoadACEManifest<T>(Guid aceId);
        /// <summary>
        /// Allows Dashboard to read custom Adaptive Card Extension as a custom ACE with custom properties.
        /// Once used You can access custom ACEs by dashboard.ACEs.OfType<![CDATA[T]]>();
        /// </summary>
        /// <param name="aceFactory">An implementation of ACEFactory for your custom ACE</param>
        public void RegisterCustomACEFactory(ACEFactory aceFactory);
    }
}

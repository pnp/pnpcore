using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
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
        /// Saves changes made to ACEs by persisting the dashboard
        /// </summary>
        public void Save();

        /// <summary>
        /// Saves changes made to ACEs by persisting the dashboard
        /// </summary>
        public Task SaveAsync();

        /// <summary>
        /// Creates a new ACE for adding on the dashboard based upon the default settings of the 
        /// </summary>
        /// <param name="defaultACE">OOB ace to instantiate</param>
        /// <param name="cardSize">Card to use when the ACE is added to the dashboard</param>
        /// <returns>Configured ACE, ready for customization and adding</returns>
        public AdaptiveCardExtension NewACE(DefaultACE defaultACE, CardSize cardSize = CardSize.Medium);

        /// <summary>
        /// Creates a new ACE for adding on the dashboard based upon the default settings of the 
        /// </summary>
        /// <param name="aceId">ID of the ace to instantiate</param>
        /// <param name="cardSize">Card to use when the ACE is added to the dashboard</param>
        /// <returns>Configured ACE, ready for customization and adding</returns>
        public AdaptiveCardExtension NewACE(Guid aceId, CardSize cardSize = CardSize.Medium);

        /// <summary>
        /// Adds new ACE
        /// </summary>
        /// <param name="ace">ACE to add</param>
        public void AddACE(AdaptiveCardExtension ace);

        /// <summary>
        /// Adds new ACE
        /// </summary>
        /// <param name="ace">ACE to add</param>
        /// <param name="order">Order of the ACE in the dashboard</param>
        public void AddACE(AdaptiveCardExtension ace, int order);

        /// <summary>
        /// Updates an existing dashboard ACE
        /// </summary>
        /// <param name="ace">ACE to update</param>
        public void UpdateACE(AdaptiveCardExtension ace);

        /// <summary>
        /// Updates an existing dashboard ACE
        /// </summary>
        /// <param name="ace">ACE to update</param>
        /// <param name="order">Order of the ACE in the dashboard</param>
        public void UpdateACE(AdaptiveCardExtension ace, int order);

        /// <summary>
        /// Removes ACE with provided Guid from the Dashboard
        /// </summary>
        /// <param name="instanceId"></param>
        public void RemoveACE(Guid instanceId);               
        
        /// <summary>
        /// Allows Dashboard to read custom Adaptive Card Extension as a custom ACE with custom properties.
        /// Once used You can access custom ACEs by dashboard.ACEs.OfType<![CDATA[T]]>();
        /// </summary>
        /// <param name="aceFactory">An implementation of ACEFactory for your custom ACE</param>
        public void RegisterCustomACEFactory(ACEFactory aceFactory);
    }
}

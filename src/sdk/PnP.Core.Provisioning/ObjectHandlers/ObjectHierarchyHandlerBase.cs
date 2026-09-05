using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// Decides whether a sequence within a hierarchy should be provisioned.
    /// </summary>
    /// <param name="context">A tenant admin scoped context</param>
    /// <param name="hierarchy">The hierarchy being applied</param>
    internal delegate bool ShouldProvisionSequenceTest(PnPContext context, ProvisioningHierarchy hierarchy);

    /// <summary>
    /// Base class for the tenant scoped object handlers - the ones that operate above a single
    /// site, creating site collections, teams, term groups and tenant settings.
    /// </summary>
    internal abstract class ObjectHierarchyHandlerBase
    {
        internal bool? _willExtract;
        internal bool? _willProvision;

        private bool _reportProgress = true;

        /// <summary>
        /// The handler's display name, shown in progress reporting.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Whether this handler counts towards the progress total.
        /// </summary>
        public bool ReportProgress
        {
            get { return _reportProgress; }
            set { _reportProgress = value; }
        }

        /// <summary>
        /// Callback the handler reports messages and sub-progress through.
        /// </summary>
        public ProvisioningMessagesDelegate MessagesDelegate { get; set; }

        /// <summary>
        /// Whether this handler has anything to do for the given hierarchy and sequence.
        /// </summary>
        public abstract bool WillProvision(PnPContext context, ProvisioningHierarchy hierarchy, string sequenceId, ApplyConfiguration configuration);

        /// <summary>
        /// Whether this handler has anything to extract for the given hierarchy and sequence.
        /// </summary>
        public abstract bool WillExtract(PnPContext context, ProvisioningHierarchy hierarchy, string sequenceId, ExtractConfiguration configuration);

        /// <summary>
        /// Writes this handler's part of the hierarchy to the tenant.
        /// </summary>
        /// <returns>The token parser, with any tokens this handler registered</returns>
        public abstract Task<TokenParser> ProvisionObjectsAsync(PnPContext context, ProvisioningHierarchy hierarchy, string sequenceId, TokenParser parser, ApplyConfiguration configuration);

        /// <summary>
        /// Reads this handler's part of the tenant into the hierarchy.
        /// </summary>
        /// <returns>The hierarchy, with this handler's contribution added</returns>
        public abstract Task<ProvisioningHierarchy> ExtractObjectsAsync(PnPContext context, ProvisioningHierarchy hierarchy, ExtractConfiguration configuration);

        internal void WriteMessage(string message, ProvisioningMessageType messageType)
        {
            MessagesDelegate?.Invoke(message, messageType);
        }

        internal void WriteSubProgress(string title, string message, int step, int total)
        {
            MessagesDelegate?.Invoke($"{title}|{message}|{step}|{total}", ProvisioningMessageType.Progress);
        }
    }
}

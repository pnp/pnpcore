using Microsoft.Extensions.Logging;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using TermGroupModel = PnP.Core.Provisioning.Model.TermGroup;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// Creates the term groups a tenant template's sequence declares, before its sites are created.
    /// </summary>
    internal class ObjectHierarchySequenceTermGroups : ObjectHierarchyHandlerBase
    {
        public override string Name => "Term Groups";

        public override bool WillProvision(PnPContext context, ProvisioningHierarchy hierarchy, string sequenceId,
            ApplyConfiguration configuration)
        {
            _willProvision ??= hierarchy?.Sequences?
                .Any(s => s.TermStore?.TermGroups?.Count > 0) == true;

            return _willProvision.Value;
        }

        public override bool WillExtract(PnPContext context, ProvisioningHierarchy hierarchy, string sequenceId,
            ExtractConfiguration configuration)
        {
            _willExtract ??= false;
            return _willExtract.Value;
        }

        public override Task<ProvisioningHierarchy> ExtractObjectsAsync(PnPContext context, ProvisioningHierarchy hierarchy,
            ExtractConfiguration configuration)
        {
            return Task.FromResult(hierarchy);
        }

        public override async Task<TokenParser> ProvisionObjectsAsync(PnPContext context, ProvisioningHierarchy hierarchy,
            string sequenceId, TokenParser parser, ApplyConfiguration configuration)
        {
            if (hierarchy?.Sequences == null)
            {
                return parser;
            }

            var provisioner = new TermGroupProvisioner(context, parser,
                m => WriteMessage(m, ProvisioningMessageType.Warning));

            foreach (ProvisioningSequence sequence in hierarchy.Sequences)
            {
                if (!(sequence.TermStore?.TermGroups?.Count > 0))
                {
                    continue;
                }

                int index = 0;

                foreach (TermGroupModel modelGroup in sequence.TermStore.TermGroups)
                {
                    index++;
                    WriteSubProgress("Term group", modelGroup.Name, index, sequence.TermStore.TermGroups.Count);

                    try
                    {
                        await provisioner.ProcessGroupAsync(modelGroup).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        string warning = $"The term group '{modelGroup.Name}' could not be provisioned: " +
                            ErrorText.Describe(ex);
                        context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                        WriteMessage(warning, ProvisioningMessageType.Warning);
                    }
                }
            }

            await provisioner.ApplyDeferredReusesAsync().ConfigureAwait(false);

            WriteMessage("Done processing term groups", ProvisioningMessageType.Completed);

            return parser;
        }
    }
}

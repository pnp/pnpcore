using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers;
using PnP.Core.Services;
using System;
using System.Threading.Tasks;
using WorkflowsModel = PnP.Core.Provisioning.Model.Workflows;
using WorkflowDefinitionModel = PnP.Core.Provisioning.Model.WorkflowDefinition;

namespace PnP.Core.Provisioning.Test.Live.Handlers
{
    /// <summary>
    /// Live coverage for <c>ObjectWorkflows</c>
    /// </summary>
    [TestClass]
    public class ObjectWorkflowsLiveTests : LiveTestBase
    {
        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Workflows_ReportOnceAndCarryOnWhenTheServiceIsRetired()
        {
            using (PnPContext context = await GetClassicContextAsync().ConfigureAwait(false))
            {
                int warnings = 0;
                string lastWarning = null;

                var template = new ProvisioningTemplate
                {
                    Workflows = new WorkflowsModel(),
                };

                for (int i = 1; i <= 3; i++)
                {
                    template.Workflows.WorkflowDefinitions.Add(new WorkflowDefinitionModel
                    {
                        Id = Guid.NewGuid(),
                        DisplayName = $"{TestPrefix}Workflow{i}",
                        XamlPath = $"{TestPrefix}Workflow{i}.xaml",
                        Published = true,
                    });
                }

                var configuration = new ApplyConfiguration
                {
                    MessagesDelegate = (message, type) =>
                    {
                        Console.WriteLine($"[{type}] {message}");

                        if (type == ProvisioningMessageType.Warning)
                        {
                            warnings++;
                            lastWarning = message;
                        }
                    },
                };

                await context.GetProvisioningManager().ApplyTemplateAsync(template, configuration).ConfigureAwait(false);

                Console.WriteLine($"{warnings} warning(s)");

                if (lastWarning == null)
                {
                    Assert.Inconclusive("The workflow service appears to be available on this tenant, " +
                        "so the retired-platform path could not be exercised.");
                }

                Assert.AreEqual(1, warnings,
                    "The handler reported the missing workflow service more than once.");

                StringAssert.Contains(lastWarning, "retired",
                    "The warning does not tell the reader that the platform is retired.");
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Workflows_ExtractIsSilentWhenTheServiceIsRetired()
        {
            using (PnPContext context = await GetClassicContextAsync().ConfigureAwait(false))
            {
                var configuration = new ExtractConfiguration();
                configuration.Handlers.Add(ConfigurationHandler.Workflows);

                ProvisioningTemplate extracted = await context.GetProvisioningManager()
                    .GetTemplateAsync(configuration).ConfigureAwait(false);

                Console.WriteLine($"Workflows element: {(extracted.Workflows == null ? "(none)" : "present")}");

                if (extracted.Workflows != null)
                {
                    Assert.AreEqual(0, extracted.Workflows.WorkflowDefinitions.Count,
                        "Workflow definitions were extracted from a tenant with no workflow service.");
                }
            }
        }
    }
}

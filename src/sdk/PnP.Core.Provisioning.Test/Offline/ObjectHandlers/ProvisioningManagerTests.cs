using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers;
using PnP.Core.Services;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.Test.Offline.ObjectHandlers
{
    /// <summary>
    /// Covers the engine's public entry point and the handler ordering it is built around.
    /// </summary>
    [TestClass]
    public class ProvisioningManagerTests
    {
        private sealed class FakeProvisioningManager : IProvisioningManager
        {
            public Task ApplyTemplateAsync(ProvisioningTemplate template, ApplyConfiguration configuration = null) => Task.CompletedTask;
            public void ApplyTemplate(ProvisioningTemplate template, ApplyConfiguration configuration = null) { }
            public Task<ProvisioningTemplate> GetTemplateAsync(ExtractConfiguration configuration = null) => Task.FromResult(new ProvisioningTemplate());
            public ProvisioningTemplate GetTemplate(ExtractConfiguration configuration = null) => new ProvisioningTemplate();
            public Task ApplyTenantTemplateAsync(ProvisioningHierarchy hierarchy, string sequenceId, ApplyConfiguration configuration = null) => Task.CompletedTask;
            public void ApplyTenantTemplate(ProvisioningHierarchy hierarchy, string sequenceId, ApplyConfiguration configuration = null) { }
            public Task<ProvisioningHierarchy> GetTenantTemplateAsync(ExtractConfiguration configuration = null) => Task.FromResult(new ProvisioningHierarchy());
            public ProvisioningHierarchy GetTenantTemplate(ExtractConfiguration configuration = null) => new ProvisioningHierarchy();
        }

        private sealed class FakeExtensions : IPnPContextProvisioningExtensions
        {
            public IProvisioningManager GetProvisioningManager(IPnPContext context) => new FakeProvisioningManager();
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void GetProvisioningManager_ImplementationCanBeSubstitutedAndReverted()
        {
            try
            {
                PnPContextProvisioningExtensions.Implementation = new FakeExtensions();

                IProvisioningManager manager = ((IPnPContext)null).GetProvisioningManager();

                Assert.IsInstanceOfType(manager, typeof(FakeProvisioningManager));
            }
            finally
            {
                PnPContextProvisioningExtensions.RevertToDefaultImplementation();
            }
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void ProvisioningManager_RejectsANullContext()
        {
            Type managerType = typeof(IProvisioningManager).Assembly
                .GetType("PnP.Core.Provisioning.ObjectHandlers.ProvisioningManager");
            Assert.IsNotNull(managerType, "ProvisioningManager was renamed or removed.");

            TargetInvocationException ex = Assert.ThrowsException<TargetInvocationException>(() =>
                Activator.CreateInstance(managerType,
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null, new object[] { null }, null));

            Assert.IsInstanceOfType(ex.InnerException, typeof(ArgumentNullException));
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void IProvisioningManager_ExposesTheFourOperationsInBothSyncAndAsyncForms()
        {
            string[] expected =
            {
                nameof(IProvisioningManager.ApplyTemplateAsync),
                nameof(IProvisioningManager.ApplyTemplate),
                nameof(IProvisioningManager.GetTemplateAsync),
                nameof(IProvisioningManager.GetTemplate),
                nameof(IProvisioningManager.ApplyTenantTemplateAsync),
                nameof(IProvisioningManager.ApplyTenantTemplate),
                nameof(IProvisioningManager.GetTenantTemplateAsync),
                nameof(IProvisioningManager.GetTenantTemplate),
            };

            string[] actual = typeof(IProvisioningManager).GetMethods().Select(m => m.Name).ToArray();

            CollectionAssert.AreEquivalent(expected, actual);
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void BuildApplyHandlers_StillDocumentsTheThreePassOrdering()
        {
            Type managerType = typeof(IProvisioningManager).Assembly
                .GetType("PnP.Core.Provisioning.ObjectHandlers.ProvisioningManager");

            MethodInfo buildApplyHandlers = managerType.GetMethod("BuildApplyHandlers",
                BindingFlags.NonPublic | BindingFlags.Static);

            Assert.IsNotNull(buildApplyHandlers,
                "ProvisioningManager.BuildApplyHandlers is where the three-pass lookup-field ordering lives. " +
                "If it was renamed, move the ordering comment and this test with it.");

            Assert.AreEqual(typeof(System.Collections.Generic.List<>).Name,
                buildApplyHandlers.ReturnType.Name);
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void FieldAndListProvisioningStep_HasTheFourStepsTheOrderingDependsOn()
        {
            CollectionAssert.AreEqual(
                new[] { "ListAndStandardFields", "LookupFields", "ListSettings", "Export" },
                Enum.GetNames(typeof(FieldAndListProvisioningStepHelper.Step)),
                "The provisioning step order is load-bearing - see ProvisioningManager.BuildApplyHandlers.");
        }
    }
}

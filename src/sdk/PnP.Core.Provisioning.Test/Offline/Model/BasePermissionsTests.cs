using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using System.Collections.Generic;

using BasePermissions = PnP.Core.Provisioning.Model.BasePermissions;

namespace PnP.Core.Provisioning.Test.Offline.Model
{
    /// <summary>
    /// Guards <see cref="BasePermissions"/>, which replaces <c>Microsoft.SharePoint.Client.BasePermissions</c>
    /// in the ported provisioning model.
    /// </summary>
    [TestClass]
    public class BasePermissionsTests
    {
        /// <summary>
        /// The (high, low) mask produced by calling Set() with each PermissionKind on a fresh instance,
        /// as observed from the CSOM implementation.
        /// </summary>
        public static IEnumerable<object[]> SetExpectations()
        {
            yield return new object[] { PermissionKind.EmptyMask, 0u, 0u };
            yield return new object[] { PermissionKind.ViewListItems, 0u, 1u };
            yield return new object[] { PermissionKind.AddListItems, 0u, 2u };
            yield return new object[] { PermissionKind.EditListItems, 0u, 4u };
            yield return new object[] { PermissionKind.DeleteListItems, 0u, 8u };
            yield return new object[] { PermissionKind.ApproveItems, 0u, 16u };
            yield return new object[] { PermissionKind.OpenItems, 0u, 32u };
            yield return new object[] { PermissionKind.ViewVersions, 0u, 64u };
            yield return new object[] { PermissionKind.DeleteVersions, 0u, 128u };
            yield return new object[] { PermissionKind.CancelCheckout, 0u, 256u };
            yield return new object[] { PermissionKind.ManagePersonalViews, 0u, 512u };
            yield return new object[] { PermissionKind.ManageLists, 0u, 2048u };
            yield return new object[] { PermissionKind.ViewFormPages, 0u, 4096u };
            yield return new object[] { PermissionKind.AnonymousSearchAccessList, 0u, 8192u };
            yield return new object[] { PermissionKind.Open, 0u, 65536u };
            yield return new object[] { PermissionKind.ViewPages, 0u, 131072u };
            yield return new object[] { PermissionKind.AddAndCustomizePages, 0u, 262144u };
            yield return new object[] { PermissionKind.ApplyThemeAndBorder, 0u, 524288u };
            yield return new object[] { PermissionKind.ApplyStyleSheets, 0u, 1048576u };
            yield return new object[] { PermissionKind.ViewUsageData, 0u, 2097152u };
            yield return new object[] { PermissionKind.CreateSSCSite, 0u, 4194304u };
            yield return new object[] { PermissionKind.ManageSubwebs, 0u, 8388608u };
            yield return new object[] { PermissionKind.CreateGroups, 0u, 16777216u };
            yield return new object[] { PermissionKind.ManagePermissions, 0u, 33554432u };
            yield return new object[] { PermissionKind.BrowseDirectories, 0u, 67108864u };
            yield return new object[] { PermissionKind.BrowseUserInfo, 0u, 134217728u };
            yield return new object[] { PermissionKind.AddDelPrivateWebParts, 0u, 268435456u };
            yield return new object[] { PermissionKind.UpdatePersonalWebParts, 0u, 536870912u };
            yield return new object[] { PermissionKind.ManageWeb, 0u, 1073741824u };
            yield return new object[] { PermissionKind.AnonymousSearchAccessWebLists, 0u, 2147483648u };
            yield return new object[] { PermissionKind.UseClientIntegration, 16u, 0u };
            yield return new object[] { PermissionKind.UseRemoteAPIs, 32u, 0u };
            yield return new object[] { PermissionKind.ManageAlerts, 64u, 0u };
            yield return new object[] { PermissionKind.CreateAlerts, 128u, 0u };
            yield return new object[] { PermissionKind.EditMyUserInfo, 256u, 0u };
            yield return new object[] { PermissionKind.EnumeratePermissions, 1073741824u, 0u };
            yield return new object[] { PermissionKind.FullMask, 32767u, 65535u };
        }

        [TestMethod]
        [TestCategory("Offline")]
        [DynamicData(nameof(SetExpectations), DynamicDataSourceType.Method)]
        public void Set_ProducesTheSameMaskAsCsom(PermissionKind permission, uint expectedHigh, uint expectedLow)
        {
            BasePermissions permissions = new BasePermissions();

            permissions.Set(permission);

            Assert.AreEqual(expectedHigh, permissions.High, $"High word mismatch for {permission}");
            Assert.AreEqual(expectedLow, permissions.Low, $"Low word mismatch for {permission}");
        }

        [TestMethod]
        [TestCategory("Offline")]
        [DynamicData(nameof(SetExpectations), DynamicDataSourceType.Method)]
        public void Set_ThenHas_ReturnsTrueForThatPermission(PermissionKind permission, uint expectedHigh, uint expectedLow)
        {
            _ = expectedHigh;
            _ = expectedLow;

            BasePermissions permissions = new BasePermissions();
            permissions.Set(permission);

            Assert.IsTrue(permissions.Has(permission), $"Has({permission}) should be true after Set({permission})");
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void Has_EmptyMask_IsAlwaysTrue()
        {
            Assert.IsTrue(new BasePermissions().Has(PermissionKind.EmptyMask));

            BasePermissions permissions = new BasePermissions();
            permissions.Set(PermissionKind.ManageWeb);
            Assert.IsTrue(permissions.Has(PermissionKind.EmptyMask));
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void Set_FullMask_DoesNotGrantEveryPermission()
        {
            BasePermissions permissions = new BasePermissions();
            permissions.Set(PermissionKind.FullMask);

            Assert.IsTrue(permissions.Has(PermissionKind.FullMask));
            Assert.IsTrue(permissions.Has(PermissionKind.ViewListItems), "low bit 0 is within the mask");
            Assert.IsTrue(permissions.Has(PermissionKind.UseClientIntegration), "high bit 4 is within the mask");

            Assert.IsFalse(permissions.Has(PermissionKind.ManageWeb), "low bit 30 is outside the FullMask value");
            Assert.IsFalse(permissions.Has(PermissionKind.AnonymousSearchAccessWebLists), "low bit 31 is outside the FullMask value");
            Assert.IsFalse(permissions.Has(PermissionKind.EnumeratePermissions), "high bit 30 is outside the FullMask value");
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void Has_FullMask_RequiresTheExactFullMaskValue()
        {
            BasePermissions permissions = new BasePermissions();
            permissions.Set(PermissionKind.ViewListItems);

            Assert.IsFalse(permissions.Has(PermissionKind.FullMask));
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void Clear_RemovesOnlyTheGivenPermission()
        {
            BasePermissions permissions = new BasePermissions();
            permissions.Set(PermissionKind.ViewListItems);
            permissions.Set(PermissionKind.ManageWeb);
            permissions.Set(PermissionKind.UseClientIntegration);

            permissions.Clear(PermissionKind.ViewListItems);

            Assert.IsFalse(permissions.Has(PermissionKind.ViewListItems));
            Assert.IsTrue(permissions.Has(PermissionKind.ManageWeb));
            Assert.IsTrue(permissions.Has(PermissionKind.UseClientIntegration));
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void Set_EmptyMask_ResetsAPopulatedMask()
        {
            BasePermissions permissions = new BasePermissions();
            permissions.Set(PermissionKind.ViewListItems);
            permissions.Set(PermissionKind.ManageWeb);
            permissions.Set(PermissionKind.UseClientIntegration);

            permissions.Set(PermissionKind.EmptyMask);

            Assert.AreEqual(0u, permissions.High);
            Assert.AreEqual(0u, permissions.Low);
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void Set_FullMask_OverwritesRatherThanAccumulates()
        {
            BasePermissions permissions = new BasePermissions();
            permissions.Set(PermissionKind.EnumeratePermissions);

            permissions.Set(PermissionKind.FullMask);

            Assert.AreEqual(32767u, permissions.High);
            Assert.AreEqual(65535u, permissions.Low);
            Assert.IsFalse(permissions.Has(PermissionKind.EnumeratePermissions));
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void Clear_FullMaskAndEmptyMask_AreNoOps()
        {
            BasePermissions permissions = new BasePermissions();
            permissions.Set(PermissionKind.ViewListItems);
            permissions.Set(PermissionKind.ManageWeb);
            permissions.Set(PermissionKind.UseClientIntegration);

            uint highBefore = permissions.High;
            uint lowBefore = permissions.Low;

            permissions.Clear(PermissionKind.FullMask);
            permissions.Clear(PermissionKind.EmptyMask);

            Assert.AreEqual(highBefore, permissions.High);
            Assert.AreEqual(lowBefore, permissions.Low);
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void ClearAll_EmptiesTheMask()
        {
            BasePermissions permissions = new BasePermissions();
            permissions.Set(PermissionKind.FullMask);

            permissions.ClearAll();

            Assert.AreEqual(0u, permissions.High);
            Assert.AreEqual(0u, permissions.Low);
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void Equality_IsByValue()
        {
            BasePermissions first = new BasePermissions();
            first.Set(PermissionKind.ManageWeb);

            BasePermissions second = new BasePermissions();
            second.Set(PermissionKind.ManageWeb);

            BasePermissions different = new BasePermissions();
            different.Set(PermissionKind.ViewListItems);

            Assert.IsTrue(first.Equals(second));
            Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
            Assert.IsFalse(first.Equals(different));
            Assert.IsFalse(first.Equals(null));
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void HasPermissions_TestsEveryBitOfTheGivenMask()
        {
            BasePermissions permissions = new BasePermissions();
            permissions.Set(PermissionKind.FullMask);

            Assert.IsTrue(permissions.HasPermissions(0u, 0u));
            Assert.IsTrue(permissions.HasPermissions(32767u, 65535u));
            Assert.IsFalse(permissions.HasPermissions(0u, 65536u));
            Assert.IsFalse(permissions.HasPermissions(1073741824u, 0u));
        }
    }
}

using System;

namespace PnP.Core.Modernization.Publishing
{
    internal enum PublishingPageTransformationTarget
    {
        CrossSiteCollection,
        SameWeb,
        SameSiteCollectionDifferentWeb,
        SameWebRequiresWritableSitePages,
    }

    /// <summary>
    /// Pure validation rules for choosing the publishing page transformation target mode.
    /// </summary>
    internal static class PublishingPageTransformationValidator
    {
        internal static PublishingPageTransformationTarget ValidateTarget(
            Guid sourceSiteId,
            Guid targetSiteId,
            Guid sourceWebId,
            Guid targetWebId,
            bool hasWritableSitePages)
        {
            if (sourceSiteId != targetSiteId)
            {
                return PublishingPageTransformationTarget.CrossSiteCollection;
            }

            if (sourceWebId != targetWebId)
            {
                return PublishingPageTransformationTarget.SameSiteCollectionDifferentWeb;
            }

            if (!hasWritableSitePages)
            {
                return PublishingPageTransformationTarget.SameWebRequiresWritableSitePages;
            }

            return PublishingPageTransformationTarget.SameWeb;
        }

        internal static bool CanOverwriteTarget(PublishingPageTransformationTarget target, bool overwriteRequested)
        {
            return target == PublishingPageTransformationTarget.CrossSiteCollection && overwriteRequested;
        }

        internal static bool UsesCrossSitePermissionSemantics(PublishingPageTransformationTarget target)
        {
            return target == PublishingPageTransformationTarget.CrossSiteCollection;
        }
    }
}

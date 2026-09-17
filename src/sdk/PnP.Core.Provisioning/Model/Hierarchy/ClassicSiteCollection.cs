using System;

namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// Concrete type defining a classic Site Collection
    /// </summary>
    public partial class ClassicSiteCollection : SiteCollection
    {
        /// <summary>
        /// The URL of the target Site
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Owner of the target Site
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// The TimeZone of the target Site
        /// </summary>
        public int TimeZoneId { get; set; }

        /// <summary>
        /// WebTemplate to use for creating this site collection
        /// </summary>
        public string WebTemplate { get; set; }

        /// <summary>
        /// The Classification of the target Site
        /// </summary>
        public string Classification { get; set; }

        /// <summary>
        /// Language of the target Site
        /// </summary>
        public int Language { get; set; }

        protected override bool EqualsInherited(SiteCollection other)
        {
            if (!(other is ClassicSiteCollection otherTyped))
            {
                return (false);
            }

            return (this.Url == otherTyped.Url &&
                this.Owner == otherTyped.Owner &&
                this.TimeZoneId == otherTyped.TimeZoneId &&
                this.WebTemplate == otherTyped.WebTemplate &&
                this.Classification == otherTyped.Classification &&
                this.Language == otherTyped.Language
                );
        }

        protected override int GetInheritedHashCode()
        {
            return (String.Format("{0}|{1}|{2}|{3}|{4}|{5}|",
                this.Url?.GetHashCode() ?? 0,
                this.Owner?.GetHashCode() ?? 0,
                this.TimeZoneId,
                this.WebTemplate?.GetHashCode(),
                this.Classification?.GetHashCode() ?? 0,
                this.Language.GetHashCode()
            ).GetHashCode());
        }
    }
}

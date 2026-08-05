using PnP.Core.Provisioning.Utilities;
using System;
using System.Linq;

namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// Defines the Footer settings for the target site
    /// </summary>
    public partial class SiteFooter : BaseModel, IEquatable<SiteFooter>
    {
        #region Public Members

        /// <summary>
        /// Defines whether the site Footer is enabled or not
        /// </summary>
        public Boolean Enabled { get; set; }

        /// <summary>
        /// Defines the Logo to render in the Footer
        /// </summary>
        public String Logo { get; set; }

        /// <summary>
        /// Defines the name of the footer. Only visible if "NameVisiblity" has been set to true.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Defines whether the existing site Footer links should be removed
        /// </summary>
        public Boolean RemoveExistingNodes { get; set; }

        /// <summary>
        /// Defines the Layout of the Footer
        /// </summary>
        public SiteFooterLayout Layout { get; set; }

        /// <summary>
        /// Defines the Display Name for the footer, optional attribute
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Defines the Background Emphasis of the Header
        /// </summary>
        public Emphasis BackgroundEmphasis { get; set; }

        /// <summary>
        /// Defines the Footer Links for the target site
        /// </summary>
        public SiteFooterLinkCollection FooterLinks { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for SiteFooter
        /// </summary>
        public SiteFooter()
        {
            this.FooterLinks = new SiteFooterLinkCollection(this.ParentTemplate);
        }

        #endregion

        #region Comparison code

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Returns HashCode</returns>
        public override int GetHashCode()
        {
            return (String.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|",
                Enabled.GetHashCode(),
                Logo?.GetHashCode() ?? 0,
                Name?.GetHashCode(),
                RemoveExistingNodes.GetHashCode(),
                FooterLinks.Aggregate(0, (acc, next) => acc += (next != null ? next.GetHashCode() : 0)),
                Layout.GetHashCode(),
                DisplayName.GetHashCode(),
                BackgroundEmphasis.GetHashCode()
            ).GetHashCode());
        }

        /// <summary>
        /// Compares object with SiteFooter class
        /// </summary>
        /// <param name="obj">Object that represents SiteFooter</param>
        /// <returns>Checks whether object is SiteFooter class</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is SiteFooter))
            {
                return (false);
            }
            return (Equals((SiteFooter)obj));
        }

        /// <summary>
        /// Compares SiteFooter object based on Logo, Name, RemoveExistingNodes, FooterLinks, Layout, and DisplayName 
        /// </summary>
        /// <param name="other">SiteFooter Class object</param>
        /// <returns>true if the SiteFooter object is equal to the current object; otherwise, false.</returns>
        public bool Equals(SiteFooter other)
        {
            if (other == null)
            {
                return (false);
            }

            return (this.Enabled == other.Enabled &&
                this.Logo == other.Logo &&
                this.Name == other.Name &&
                this.RemoveExistingNodes == other.RemoveExistingNodes &&
                this.FooterLinks.DeepEquals(other.FooterLinks) &&
                this.Layout == other.Layout &&
                this.DisplayName == other.DisplayName && 
                this.BackgroundEmphasis == other.BackgroundEmphasis
                );
        }

        #endregion
    }

    /// <summary>
    /// Defines the Layouts available for the Footer of a Site
    /// </summary>
    public enum SiteFooterLayout
    {
        /// <summary>
        /// Defines the Simple Layout for the Site Footer
        /// </summary>
        Simple,
        /// <summary>
        /// Defines the Extended Layout for the Site Footer
        /// </summary>
        Extended,
    }
}

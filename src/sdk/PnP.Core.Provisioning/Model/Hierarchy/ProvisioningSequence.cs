using PnP.Core.Provisioning.Utilities;
using System;
using System.Linq;

namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// Defines a sequence of activities to execute with the engine
    /// </summary>
    public partial class ProvisioningSequence : BaseHierarchyModel, IEquatable<ProvisioningSequence>
    {
        #region Private Members

        private ProvisioningTermStore _termStore;

        #endregion

        #region Constructors

        public ProvisioningSequence()
        {
            this.SiteCollections = new SiteCollectionCollection(this.ParentHierarchy);
        }

        #endregion

        #region Public Members

        /// <summary>
        /// A unique identifier of the Sequence, required attribute.
        /// </summary>
        public string ID { get; set; }

        public SiteCollectionCollection SiteCollections { get; private set; }

        /// <summary>
        /// Defines the TermStore to provision, if any
        /// </summary>
        public ProvisioningTermStore TermStore
        {
            get { return this._termStore; }
            set { this._termStore = value; }
        }

        public override string ToString()
        {
            return ID;
        }
        #endregion

        #region Comparison code
        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Returns HashCode</returns>
        public override int GetHashCode()
        {
            return (String.Format("{0}|{1}|{2}|",
                this.ID?.GetHashCode() ?? 0,
                this.TermStore?.GetHashCode() ?? 0,
                this.SiteCollections.Aggregate(0, (acc, next) => acc += (next != null ? next.GetHashCode() : 0))
            ).GetHashCode());
        }

        /// <summary>
        /// Compares object with ProvisioningSequence
        /// </summary>
        /// <param name="obj">Object that represents ProvisioningSequence</param>
        /// <returns>true if the current object is equal to the ProvisioningSequence</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is ProvisioningSequence))
            {
                return (false);
            }
            return (Equals((ProvisioningSequence)obj));
        }

        /// <summary>
        /// Compares ProvisioningSequence object based on its properties
        /// </summary>
        /// <param name="other">ProvisioningSequence object</param>
        /// <returns>true if the ProvisioningSequence object is equal to the current object; otherwise, false.</returns>
        public bool Equals(ProvisioningSequence other)
        {
            if (other == null)
            {
                return (false);
            }

            return 
                this.ID == other.ID &&
                (this.TermStore != null ? this.TermStore.Equals(other.TermStore) : this.TermStore == null && other.TermStore == null ? true : false) &&
                this.SiteCollections.DeepEquals(other.SiteCollections);
        }

        #endregion
    }
}

using System.Text.Json.Serialization;

namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// Base type for any Domain Model object in the Provisioning Hierarchy (from the ProvisioningTemplate type and above)
    /// </summary>
    public abstract class BaseHierarchyModel : IProvisioningHierarchyDescendant
    {
        private ProvisioningHierarchy _parentHierarchy;

        /// <summary>
        /// Represents a reference to the parent Provisioning Hierarchy object, if any
        /// </summary>
        [JsonIgnore]
        public ProvisioningHierarchy ParentHierarchy
        {
            get
            {
                return (this._parentHierarchy);
            }
            internal set
            {
                this._parentHierarchy = value;
            }
        }
    }
}

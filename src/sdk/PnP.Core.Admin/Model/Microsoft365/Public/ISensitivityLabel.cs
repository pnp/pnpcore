using System;
using System.Collections.Generic;

namespace PnP.Core.Admin.Model.Microsoft365
{
    /// <summary>
    /// A Microsoft 365 sensitivity label
    /// </summary>
    public interface ISensitivityLabel
    {
        /// <summary>
        /// Id of the sensitivity label
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Name of the sensitivity label
        /// </summary>
        public string Name {  get; }

        /// <summary>
        /// Display name of the sensitivity label
        /// </summary>
        public string DisplayName {  get; }

        /// <summary>
        /// Description of the sensitivity label
        /// </summary>
        public string Description {  get; }

        /// <summary>
        /// Is this the default sensitivity label?
        /// </summary>
        public bool IsDefault { get; }

        /// <summary>
        /// To what services is this label applicable (email, site, unifiedgroup)
        /// </summary>
        public List<string> ApplicableTo {  get; }
        
    }
}

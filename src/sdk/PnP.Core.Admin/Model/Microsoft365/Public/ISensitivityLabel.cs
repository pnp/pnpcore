using System;

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
        public string Name { get; }

        /// <summary>
        /// Description of the sensitivity label
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Is this the sensitivity label active?
        /// </summary>
        public bool IsActive { get; }

        /// <summary>
        /// Tooltip to use if this label is used in a user interface
        /// </summary>
        public string Tooltip { get; }

        /// <summary>
        /// The sensitivity setting for this label
        /// </summary>
        public int Sensitivity { get; }

    }
}

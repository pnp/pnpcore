using System;

namespace PnP.Core.Services
{
    /// <summary>
    /// Generic module that can be used to plug in custom logic to the request pipeline
    /// </summary>
    internal class GenericRequestModule : RequestModuleBase
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        internal GenericRequestModule()
        {
        }

        /// <summary>
        /// Unique ID of this request module
        /// </summary>
        public override Guid Id { get => PnPConstants.GenericRequestModuleId; }
    }
}

using System;

namespace PnP.Core.Provisioning.Services.Core.CSOM.Requests.Lists
{
    /// <summary>
    /// What <see cref="CreateListRequest"/> reads back about the list it created.
    /// </summary>
    internal sealed class CreatedListInfo
    {
        /// <summary>The id of the newly created list.</summary>
        internal Guid Id { get; set; }
    }
}

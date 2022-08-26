using PnP.Core.Model.Me;
using PnP.Core.Model.Security;
using PnP.Core.Model.SharePoint;
using PnP.Core.Model.Teams;

namespace PnP.Core.Services
{
    /// <summary>
    /// PnPContext interface to support mocking
    /// </summary>
    public interface IPnPContext
    {
        /// <summary>
        /// Entry point for the Web Object
        /// </summary>
        IWeb Web { get; }

        /// <summary>
        /// Entry point for the Site Object
        /// </summary>
        ISite Site { get; }

        /// <summary>
        /// Entry point for the Team Object
        /// </summary>
        ITeam Team { get; }

        /// <summary>
        /// Entry point for the Group Object
        /// </summary>
        IGraphGroup Group { get; }

        /// <summary>
        /// Entry point for the TermStore Object
        /// </summary>
        ITermStore TermStore { get; }

        /// <summary>
        /// Entry point for the Social Object
        /// </summary>
        ISocial Social { get; }

        /// <summary>
        /// Entry point for the Me Object
        /// </summary>
        IMe Me { get; }

        /// <summary>
        /// Entry point for the ContentTypeHub Object
        /// </summary>
        IContentTypeHub ContentTypeHub { get; }
    }
}

using System.Collections.Generic;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// A metadata for a person
    /// </summary>
    [ConcreteType(typeof(PersonProperties))]
    public interface IPersonProperties
    {
        /// <summary>
        /// Person's account name in a form of "i:0#.f|membership|admin@m365x790252.onmicrosoft.com"
        /// </summary>
        string AccountName { get; set; }

        /// <summary>
        /// Persons' display name
        /// </summary>
        string DisplayName { get; set; }

        /// <summary>
        /// Person's email
        /// </summary>
        string Email { get; set; }

        /// <summary>
        /// Whether the current user is followed or not
        /// </summary>
        bool IsFollowed { get; set; }

        /// <summary>
        /// Person's latest post url
        /// </summary>
        string LatestPost { get; set; }

        /// <summary>
        /// Person's site host url
        /// </summary>
        string PersonalSiteHostUrl { get; set; }

        /// <summary>
        /// Person's personal url
        /// </summary>
        string PersonalUrl { get; set; }

        /// <summary>
        /// Profile photo url
        /// </summary>
        string PictureUrl { get; set; }

        /// <summary>
        /// Person's title
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// User url
        /// </summary>
        string UserUrl { get; set; }

        /// <summary>
        /// Direct reports for the current user
        /// </summary>
        List<string> DirectReports { get; set;}

        /// <summary>
        /// Extended managers for the current user
        /// </summary>
        List<string> ExtendedManagers { get; set; }

        /// <summary>
        /// Extended reports for the current user
        /// </summary>
        List<string> ExtendedReports { get; set; }

        /// <summary>
        /// Current user's peers
        /// </summary>
        List<string> Peers { get; set; }

        /// <summary>
        /// User profile properties key-value collection
        /// </summary>
        Dictionary<string, object> UserProfileProperties { get; set; }
    }
}

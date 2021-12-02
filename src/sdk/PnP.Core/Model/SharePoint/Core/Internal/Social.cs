using PnP.Core.Services;
using System;

namespace PnP.Core.Model.SharePoint
{
    internal class Social : ISocial
    {
        private readonly Lazy<IUserProfile> userProfile = new Lazy<IUserProfile>(() =>
        {
            return new UserProfile();
        }, true);

        private readonly Lazy<IFollowing> following = new Lazy<IFollowing>(() =>
        {
            return new Following();
        }, true);

        public IUserProfile UserProfile
        {
            get
            {
                userProfile.Value.PnPContext = PnPContext;
                return userProfile.Value;
            }
        }

        public IFollowing Following
        {
            get
            {
                following.Value.PnPContext = PnPContext;
                return following.Value;
            }
        }

        [SystemProperty]
        public PnPContext PnPContext { get; set; }
    }
}

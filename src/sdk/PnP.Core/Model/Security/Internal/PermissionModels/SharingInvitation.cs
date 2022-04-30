namespace PnP.Core.Model.Security
{
    [GraphType]
    internal sealed class SharingInvitation : ISharingInvitation
    {
        public string Email { get; set; }
        public IIdentitySet InvitedBy { get; set; }
        public bool SignInRequired { get; set; }
    }
}

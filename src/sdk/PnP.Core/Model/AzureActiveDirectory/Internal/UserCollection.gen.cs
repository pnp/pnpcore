using System.ComponentModel;

namespace PnP.Core.Model.AzureActiveDirectory
{
    internal partial class UserCollection : BaseDataModelCollection<IUser>, IUserCollection
    {
        public override IUser CreateNew()
        {
            return NewUser();
        }

        private User NewUser()
        {
            var newUser = new User
            {
                PnPContext = this.PnPContext,
                Parent = this,
            };
            return newUser;
        }
    }
}

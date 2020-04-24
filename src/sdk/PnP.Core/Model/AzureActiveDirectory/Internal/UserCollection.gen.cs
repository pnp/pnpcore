using System.ComponentModel;

namespace PnP.Core.Model.AzureActiveDirectory
{
    internal partial class UserCollection : BaseDataModelCollection<IUser>, IUserCollection
    {
        public override IUser CreateNew()
        {
            return NewUser();
        }

        // PAOLO: It looks like we can remove this method
        //private User AddNewUser()
        //{
        //    var newUser = NewUser();
        //    this.items.Add(newUser);
        //    return newUser;
        //}

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

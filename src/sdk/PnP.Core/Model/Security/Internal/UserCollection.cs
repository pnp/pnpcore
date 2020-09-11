using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model.Security
{
    internal partial class UserCollection
    {

        public async Task<IUser> GetByUserPrincipalNameAsync(string userPrincipalName, params Expression<Func<IUser, object>>[] expressions)
        {
            // Was this list previously loaded?
            if (!(items.FirstOrDefault(u => u.IsPropertyAvailable(p => p.UserPrincipalName) 
            && u.UserPrincipalName.Equals(userPrincipalName, StringComparison.InvariantCultureIgnoreCase)) is User userToLoad))
            {
                // List was not loaded before, so add it the current set of loaded lists
                userToLoad = CreateNewAndAdd() as User;
                userToLoad.UserPrincipalName = userPrincipalName;
            }

            // TODO Shouldn't it be added elsewhere more appropriate ?
            userToLoad.AddMetadata(PnPConstants.MetaDataGraphId, userPrincipalName);
            await userToLoad.GetAsync(expressions).ConfigureAwait(false);

            return userToLoad;
        }
    }
}

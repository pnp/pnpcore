using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Model.AzureActiveDirectory
{
    /// <summary>
    /// Public interface to define a collection of Users of Azure Active Directory
    /// </summary>
    public interface IUserCollection : IDataModelCollection<IUser>, ISupportPaging
    {
    }
}

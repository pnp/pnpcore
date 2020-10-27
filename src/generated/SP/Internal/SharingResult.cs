using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// SharingResult class, write your custom code here
    /// </summary>
    [SharePointType("SP.SharingResult", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class SharingResult : BaseDataModel<ISharingResult>, ISharingResult
    {
        #region Construction
        public SharingResult()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public string ErrorMessage { get => GetValue<string>(); set => SetValue(value); }

        public string IconUrl { get => GetValue<string>(); set => SetValue(value); }

        public string Name { get => GetValue<string>(); set => SetValue(value); }

        public string PermissionsPageRelativeUrl { get => GetValue<string>(); set => SetValue(value); }

        public int StatusCode { get => GetValue<int>(); set => SetValue(value); }

        public string Url { get => GetValue<string>(); set => SetValue(value); }

        public IGroupCollection GroupsSharedWith { get => GetModelCollectionValue<IGroupCollection>(); }


        public IGroup GroupUsersAddedTo { get => GetModelValue<IGroup>(); }


        public IUserCollection UsersWithAccessRequests { get => GetModelCollectionValue<IUserCollection>(); }


        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}

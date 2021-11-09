using PnP.Core.Model.Security;
using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ListItemVersion class, write your custom code here
    /// </summary>
    [SharePointType("SP.ListItemVersion", Target = typeof(IListItem), Uri = "_api/web/lists/getbyid(guid'{List.Id}')/items({Parent.Id})/versions/getbyid({Id})", LinqGet = "_api/web/lists(guid'{List.Id}')/items({Parent.Id})/versions")]
    internal sealed class ListItemVersion : ExpandoBaseDataModel<IListItemVersion>, IListItemVersion
    {
        #region Properties
        public DateTime Created { get => GetValue<DateTime>(); set => SetValue(value); }

        [SharePointProperty("VersionId")]
        public int Id { get => GetValue<int>(); set => SetValue(value); }

        public bool IsCurrentVersion { get => GetValue<bool>(); set => SetValue(value); }

        public string VersionLabel { get => GetValue<string>(); set => SetValue(value); }

        public ISharePointUser CreatedBy { get => GetModelValue<ISharePointUser>(); }

        public IFieldCollection Fields { get => GetModelCollectionValue<IFieldCollection>(); }

        public IFileVersion FileVersion { get => GetModelValue<IFileVersion>(); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = (int)value; }

        [SharePointProperty("*")]
        public object All { get => null; }
        #endregion

        #region Methods

        // TODO:
        //#region DeleteObject
        //public async Task DeleteObjectAsync()
        //{

        //}

        //public void DeleteObject()
        //{
        //    DeleteObjectAsync().GetAwaiter().GetResult();
        //}
        //#endregion

        #endregion
    }
}

using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a FileVersion object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class FileVersion : BaseDataModel<IFileVersion>, IFileVersion
    {
        public string CheckInComment { get => GetValue<string>(); set => SetValue(value); }

        public DateTime Created { get => GetValue<DateTime>(); set => SetValue(value); }

        [SharePointProperty("ID")]
        public int Id { get => GetValue<int>(); set => SetValue(value); }

        public bool IsCurrentVersion { get => GetValue<bool>(); set => SetValue(value); }

        public int Size { get => GetValue<int>(); set => SetValue(value); }

        public string Url { get => GetValue<string>(); set => SetValue(value); }

        public string VersionLabel { get => GetValue<string>(); set => SetValue(value); }

        //public IUser CreatedBy
        //{
        //    get
        //    {
        //        if (!NavigationPropertyInstantiated())
        //        {
        //            var propertyValue = new User
        //            {
        //                PnPContext = this.PnPContext,
        //                Parent = this,
        //            };
        //            SetValue(propertyValue);
        //            InstantiateNavigationProperty();
        //        }
        //        return GetValue<IUser>();
        //    }
        //    set
        //    {
        //        InstantiateNavigationProperty();
        //        SetValue(value);                
        //    }
        //}

        [KeyProperty(nameof(Created))]
        public override object Key { get => this.Created; set => this.Created = DateTime.Parse(value.ToString()); }

    }
}

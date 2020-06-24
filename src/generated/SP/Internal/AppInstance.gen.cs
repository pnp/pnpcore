using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a AppInstance object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class AppInstance : BaseDataModel<IAppInstance>, IAppInstance
    {

        #region New properties

        public string AppPrincipalId { get => GetValue<string>(); set => SetValue(value); }

        public string AppWebFullUrl { get => GetValue<string>(); set => SetValue(value); }

        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        public string ImageFallbackUrl { get => GetValue<string>(); set => SetValue(value); }

        public string ImageUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool InError { get => GetValue<bool>(); set => SetValue(value); }

        public string StartPage { get => GetValue<string>(); set => SetValue(value); }

        public Guid ProductId { get => GetValue<Guid>(); set => SetValue(value); }

        public string RemoteAppUrl { get => GetValue<string>(); set => SetValue(value); }

        public string SettingsPageUrl { get => GetValue<string>(); set => SetValue(value); }

        public Guid SiteId { get => GetValue<Guid>(); set => SetValue(value); }

        public int Status { get => GetValue<int>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public Guid WebId { get => GetValue<Guid>(); set => SetValue(value); }

        #endregion

        [KeyProperty("Id")]
        public override object Key { get => this.Id; set => this.Id = Guid.Parse(value.ToString()); }


    }
}

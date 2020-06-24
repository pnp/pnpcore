using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a WebTemplate object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class WebTemplate : BaseDataModel<IWebTemplate>, IWebTemplate
    {

        #region New properties

        public string Description { get => GetValue<string>(); set => SetValue(value); }

        public string DisplayCategory { get => GetValue<string>(); set => SetValue(value); }

        public int Id { get => GetValue<int>(); set => SetValue(value); }

        public string ImageUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool IsHidden { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsRootWebOnly { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsSubWebOnly { get => GetValue<bool>(); set => SetValue(value); }

        public int Lcid { get => GetValue<int>(); set => SetValue(value); }

        public string Name { get => GetValue<string>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        #endregion

        [KeyProperty("Id")]
        public override object Key { get => this.Id; set => this.Id = (int)value; }


    }
}

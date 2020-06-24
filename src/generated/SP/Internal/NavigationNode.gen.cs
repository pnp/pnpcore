using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a NavigationNode object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class NavigationNode : BaseDataModel<INavigationNode>, INavigationNode
    {

        #region New properties

        public int Id { get => GetValue<int>(); set => SetValue(value); }

        public bool IsDocLib { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsExternal { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsVisible { get => GetValue<bool>(); set => SetValue(value); }

        public int ListTemplateType { get => GetValue<int>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public string Url { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("Children", Expandable = true)]
        public INavigationNodeCollection Children
        {
            get
            {
                if (!HasValue(nameof(Children)))
                {
                    var collection = new NavigationNodeCollection(this.PnPContext, this, nameof(Children));
                    SetValue(collection);
                }
                return GetValue<INavigationNodeCollection>();
            }
        }

        #endregion

        [KeyProperty("Id")]
        public override object Key { get => this.Id; set => this.Id = (int)value; }


    }
}

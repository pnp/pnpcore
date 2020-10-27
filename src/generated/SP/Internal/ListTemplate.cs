using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ListTemplate class, write your custom code here
    /// </summary>
    [SharePointType("SP.ListTemplate", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class ListTemplate : BaseDataModel<IListTemplate>, IListTemplate
    {
        #region Construction
        public ListTemplate()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public bool AllowsFolderCreation { get => GetValue<bool>(); set => SetValue(value); }

        public int BaseType { get => GetValue<int>(); set => SetValue(value); }

        public string Description { get => GetValue<string>(); set => SetValue(value); }

        public Guid FeatureId { get => GetValue<Guid>(); set => SetValue(value); }

        public bool Hidden { get => GetValue<bool>(); set => SetValue(value); }

        public string ImageUrl { get => GetValue<string>(); set => SetValue(value); }

        public string InternalName { get => GetValue<string>(); set => SetValue(value); }

        public bool IsCustomTemplate { get => GetValue<bool>(); set => SetValue(value); }

        public string Name { get => GetValue<string>(); set => SetValue(value); }

        public bool OnQuickLaunch { get => GetValue<bool>(); set => SetValue(value); }

        public int ListTemplateTypeKind { get => GetValue<int>(); set => SetValue(value); }

        public bool Unique { get => GetValue<bool>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}

using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// FieldLink class, write your custom code here
    /// </summary>
    [SharePointType("SP.FieldLink", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class FieldLink : BaseDataModel<IFieldLink>, IFieldLink
    {
        #region Construction
        public FieldLink()
        {
        }
        #endregion

        #region Properties
        #region Existing properties

        public string DisplayName { get => GetValue<string>(); set => SetValue(value); }

        public string FieldInternalName { get => GetValue<string>(); set => SetValue(value); }

        public bool Hidden { get => GetValue<bool>(); set => SetValue(value); }

        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        public string Name { get => GetValue<string>(); set => SetValue(value); }

        public bool ReadOnly { get => GetValue<bool>(); set => SetValue(value); }

        public bool Required { get => GetValue<bool>(); set => SetValue(value); }

        public bool ShowInDisplayForm { get => GetValue<bool>(); set => SetValue(value); }

        #endregion

        #region New properties

        #endregion

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }


        #endregion

        #region Extension methods
        #endregion
    }
}

using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// PropertyValues class
    /// </summary>
    [SharePointType("SP.PropertyValues")]
    internal partial class PropertyValues : ExpandoBaseDataModel<IPropertyValues>, IPropertyValues
    {
        #region Construction
        public PropertyValues() { }
        #endregion

        #region Properties
        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }
        #endregion
    }
}

using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Field string values class
    /// </summary>
    [SharePointType("SP.PropertyValues")]
    internal partial class FieldStringValues : ExpandoBaseDataModel<IFieldStringValues>, IFieldStringValues
    {
        #region Construction
        public FieldStringValues()
        {
        }
        #endregion

        #region Properties
        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }
        #endregion
    }
}

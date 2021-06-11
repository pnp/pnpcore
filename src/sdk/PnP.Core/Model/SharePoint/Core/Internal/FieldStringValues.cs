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
        public int Id { get => GetValue<int>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = int.Parse(value.ToString()); }
        #endregion
    }
}

using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a PropertyValues object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    [SharePointType("SP.PropertyValues")]
    internal partial class PropertyValues : ExpandoBaseDataModel<IPropertyValues>, IPropertyValues
    {
        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        [KeyProperty("Id")]
        public override object Key { get => this.Id; set => this.Id = Guid.Parse(value.ToString()); }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// 
    /// </summary>
    internal partial class Feature : BaseDataModel<IFeature>, IFeature
    {
        // Property that uniquely identifies this model instance is a guid named Id

        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }


        // Implement they Key property to use the guid ID:
        [KeyProperty("Id")]
        public override object Key { get => this.Id; set => this.Id = Guid.Parse(value.ToString()); }
    }
}

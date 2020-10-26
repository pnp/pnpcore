using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a FieldLink object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class FieldLink : BaseDataModel<IFieldLink>, IFieldLink
    {
        public string DisplayName { get => GetValue<string>(); set => SetValue(value); }

        public string FieldInternalName { get => GetValue<string>(); set => SetValue(value); }

        public bool Hidden { get => GetValue<bool>(); set => SetValue(value); }

        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        public string Name { get => GetValue<string>(); set => SetValue(value); }

        public bool ReadOnly { get => GetValue<bool>(); set => SetValue(value); }

        public bool Required { get => GetValue<bool>(); set => SetValue(value); }

        public bool ShowInDisplayForm { get => GetValue<bool>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => this.Id; set => this.Id = Guid.Parse(value.ToString()); }
    }
}

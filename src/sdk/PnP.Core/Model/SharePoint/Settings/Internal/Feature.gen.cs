using System;

namespace PnP.Core.Model.SharePoint
{
    internal partial class Feature : BaseDataModel<IFeature>, IFeature
    {
        public Guid DefinitionId { get => GetValue<Guid>(); set => SetValue(value); }

        //TODO: To get the displayname, needs to explicitly use this in select clase
        public string DisplayName { get => GetValue<string>(); set => SetValue(value); }

        [KeyProperty(nameof(DefinitionId))]
        public override object Key { get => this.DefinitionId; set => this.DefinitionId = Guid.Parse(value.ToString()); }
    }
}

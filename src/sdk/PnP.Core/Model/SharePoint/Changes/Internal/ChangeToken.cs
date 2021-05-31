using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    [SharePointType("SP.ChangeToken")]
    internal partial class ChangeToken : TransientObject, IChangeToken, IMetadataExtensible
    {
        public ChangeToken()
        {
        }

        public ChangeToken(string stringValue)
        {
            this.StringValue = stringValue;
        }

        public string StringValue { get => GetValue<string>(); set => SetValue(value); }

        [SystemProperty]
        public Dictionary<string, string> Metadata { get; internal set; } = new Dictionary<string, string>();

        Task IMetadataExtensible.SetGraphToRestMetadataAsync()
        {
            return Task.CompletedTask;
        }

        Task IMetadataExtensible.SetRestToGraphMetadataAsync()
        {
            return Task.CompletedTask;
        }

        public override string ToString()
        {
            return this.StringValue;
        }
    }
}
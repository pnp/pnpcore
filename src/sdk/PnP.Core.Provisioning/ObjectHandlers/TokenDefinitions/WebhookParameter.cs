using System.Text.RegularExpressions;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{

    internal class WebhookParameter : SimpleTokenDefinition
    {
        private readonly string _value = null;

        public WebhookParameter(string name, string value)
            : base($"{{webhookparam:{Regex.Escape(name)}}}", $"{{webhookparameter:{Regex.Escape(name)}}}")
        {
            _value = value;
        }

        public override string GetReplaceValue()
        {
            if (string.IsNullOrEmpty(CacheValue))
            {
                CacheValue = _value;
            }
            return CacheValue;
        }
    }
}

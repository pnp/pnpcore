using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace PnP.Core.Provisioning.Providers.Json
{
    public class JsonPnPFormatter : ITemplateFormatterWithValidation
    {
        private TemplateProviderBase _provider;

        public void Initialize(TemplateProviderBase provider)
        {
            this._provider = provider;
        }

        public bool IsValid(Stream template)
        {
            return GetValidationResults(template).IsValid;
        }

        public ValidationResult GetValidationResults(System.IO.Stream template)
        {
            return new ValidationResult { IsValid = true, Exceptions = null };
        }

        /// <summary>
        /// Serializer options shared by both directions.
        /// </summary>
        private static readonly JsonSerializerOptions serializerOptions = CreateSerializerOptions();

        private static JsonSerializerOptions CreateSerializerOptions()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new BasePermissionsConverter());
            return options;
        }

        public System.IO.Stream ToFormattedTemplate(Model.ProvisioningTemplate template)
        {
            String jsonString = JsonSerializer.Serialize(template, serializerOptions);
            Byte[] jsonBytes = System.Text.Encoding.Unicode.GetBytes(jsonString);
            MemoryStream jsonStream = new MemoryStream(jsonBytes)
            {
                Position = 0
            };

            return (jsonStream);
        }

        public Model.ProvisioningTemplate ToProvisioningTemplate(System.IO.Stream template)
        {
            return (this.ToProvisioningTemplate(template, null));
        }

        public Model.ProvisioningTemplate ToProvisioningTemplate(System.IO.Stream template, string identifier)
        {
            StreamReader sr = new StreamReader(template, Encoding.Unicode);
            String jsonString = sr.ReadToEnd();
            Model.ProvisioningTemplate result = JsonSerializer.Deserialize<Model.ProvisioningTemplate>(jsonString, serializerOptions);
            return (result);
        }
    }
}

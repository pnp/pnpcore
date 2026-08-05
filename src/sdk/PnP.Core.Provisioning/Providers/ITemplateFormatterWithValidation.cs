using System.IO;

namespace PnP.Core.Provisioning.Providers
{
    interface ITemplateFormatterWithValidation : ITemplateFormatter
    {
        /// <summary>
        /// Method to validate the content of a formatted template instance
        /// </summary>
        /// <param name="template">The formatted template instance as a Stream</param>
        /// <returns>A validation result of the validation</returns>
        ValidationResult GetValidationResults(Stream template);
    }
}

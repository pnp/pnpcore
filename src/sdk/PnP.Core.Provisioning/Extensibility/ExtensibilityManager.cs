using Microsoft.Extensions.Logging;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers;
using PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.Extensibility
{
    /// <summary>
    /// Loads and invokes the custom handlers a template declares.
    /// </summary>
    public partial class ExtensibilityManager
    {
        private readonly Dictionary<ExtensibilityHandler, object> handlerCache = new Dictionary<ExtensibilityHandler, object>();

        /// <summary>
        /// Invokes a custom token provider and returns the token definitions it contributes.
        /// </summary>
        /// <param name="context">Context of the site being provisioned. Do not dispose it inside the provider.</param>
        /// <param name="provider">The handler declaration from the template</param>
        /// <param name="template">The template being processed</param>
        /// <exception cref="ExtensiblityPipelineException">The provider threw</exception>
        /// <exception cref="ArgumentException"><paramref name="provider"/> is missing its assembly or type name</exception>
        /// <exception cref="ArgumentNullException"><paramref name="context"/> is null</exception>
        public async Task<IEnumerable<TokenDefinition>> ExecuteTokenProviderCallOutAsync(PnPContext context, ExtensibilityHandler provider, ProvisioningTemplate template)
        {
            ValidateHandler(context, provider, nameof(provider));

            try
            {
                object providerInstance = GetProviderInstance(provider);

                if (providerInstance is IProvisioningExtensibilityTokenProvider tokenProvider)
                {
                    LogBeforeInvocation(context, provider);
                    IEnumerable<TokenDefinition> tokens = await tokenProvider.GetTokensAsync(context, template, provider.Configuration).ConfigureAwait(false);
                    LogSuccess(context, provider);
                    return tokens;
                }

                if (providerInstance != null)
                {
                    throw new ArgumentOutOfRangeException(nameof(provider), InvalidImplementationMessage(provider));
                }

                return new List<TokenDefinition>();
            }
            catch (Exception ex)
            {
                throw Fail(context, provider, ex);
            }
        }

        /// <summary>
        /// Invokes a custom handler during template application.
        /// </summary>
        /// <param name="context">Context of the site being provisioned. Do not dispose it inside the handler.</param>
        /// <param name="handler">The handler declaration from the template</param>
        /// <param name="template">The template being applied</param>
        /// <param name="configuration">The apply configuration in force</param>
        /// <param name="tokenParser">The parser threaded through the run</param>
        /// <param name="logger">Logger for the current pipeline step</param>
        /// <exception cref="ExtensiblityPipelineException">The handler threw</exception>
        /// <exception cref="ArgumentException"><paramref name="handler"/> is missing its assembly or type name</exception>
        /// <exception cref="ArgumentNullException"><paramref name="context"/> is null</exception>
        public async Task ExecuteExtensibilityProvisionCallOutAsync(PnPContext context, ExtensibilityHandler handler,
            ProvisioningTemplate template, ApplyConfiguration configuration, TokenParser tokenParser, ILogger logger)
        {
            ValidateHandler(context, handler, nameof(handler));

            try
            {
                object instance = GetProviderInstance(handler);

                if (instance is IProvisioningExtensibilityHandler extensibilityHandler)
                {
                    LogBeforeInvocation(context, handler);
                    await extensibilityHandler.ProvisionAsync(context, template, configuration, tokenParser, logger, handler.Configuration).ConfigureAwait(false);
                    LogSuccess(context, handler);
                }
                else if (instance != null && !(instance is IProvisioningExtensibilityTokenProvider))
                {
                    throw new ArgumentOutOfRangeException(nameof(handler), InvalidImplementationMessage(handler));
                }
            }
            catch (Exception ex)
            {
                throw Fail(context, handler, ex);
            }
        }

        /// <summary>
        /// Invokes a custom handler during template extraction.
        /// </summary>
        /// <param name="context">Context of the site being extracted. Do not dispose it inside the handler.</param>
        /// <param name="handler">The handler declaration from the template</param>
        /// <param name="template">The template built so far</param>
        /// <param name="configuration">The extract configuration in force</param>
        /// <param name="logger">Logger for the current pipeline step</param>
        /// <returns>The template, enriched by the handler</returns>
        /// <exception cref="ExtensiblityPipelineException">The handler threw</exception>
        /// <exception cref="ArgumentException"><paramref name="handler"/> is missing its assembly or type name</exception>
        /// <exception cref="ArgumentNullException"><paramref name="context"/> is null</exception>
        public async Task<ProvisioningTemplate> ExecuteExtensibilityExtractionCallOutAsync(PnPContext context, ExtensibilityHandler handler,
            ProvisioningTemplate template, ExtractConfiguration configuration, ILogger logger)
        {
            ValidateHandler(context, handler, nameof(handler));

            try
            {
                object instance = GetProviderInstance(handler);

                if (instance is IProvisioningExtensibilityHandler extensibilityHandler)
                {
                    LogBeforeInvocation(context, handler);
                    ProvisioningTemplate parsedTemplate = await extensibilityHandler
                        .ExtractAsync(context, template, configuration, logger, handler.Configuration).ConfigureAwait(false);
                    LogSuccess(context, handler);
                    return parsedTemplate;
                }

                if (instance != null && !(instance is IProvisioningExtensibilityTokenProvider))
                {
                    throw new ArgumentOutOfRangeException(nameof(handler), InvalidImplementationMessage(handler));
                }

                return template;
            }
            catch (Exception ex)
            {
                throw Fail(context, handler, ex);
            }
        }

        private static void ValidateHandler(PnPContext context, ExtensibilityHandler handler, string parameterName)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context), PnPCoreProvisioningResources.Provisioning_Extensibility_Pipeline_ClientCtxNull);
            }

            if (handler == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (string.IsNullOrWhiteSpace(handler.Assembly))
            {
                throw new ArgumentException(PnPCoreProvisioningResources.Provisioning_Extensibility_Pipeline_Missing_AssemblyName,
                    $"{parameterName}.{nameof(handler.Assembly)}");
            }

            if (string.IsNullOrWhiteSpace(handler.Type))
            {
                throw new ArgumentException(PnPCoreProvisioningResources.Provisioning_Extensibility_Pipeline_Missing_TypeName,
                    $"{parameterName}.{nameof(handler.Type)}");
            }
        }

        private static void LogBeforeInvocation(PnPContext context, ExtensibilityHandler handler)
        {
            context.Logger?.LogInformation(PnPCoreProvisioningResources.Provisioning_Extensibility_Pipeline_BeforeInvocation,
                handler.Assembly, handler.Type);
        }

        private static void LogSuccess(PnPContext context, ExtensibilityHandler handler)
        {
            context.Logger?.LogInformation(PnPCoreProvisioningResources.Provisioning_Extensibility_Pipeline_Success,
                handler.Assembly, handler.Type);
        }

        private string InvalidImplementationMessage(ExtensibilityHandler handler)
        {
            return string.Format(CultureInfo.CurrentCulture,
                PnPCoreProvisioningResources.Provisioning_Extensibility_Invalid_Handler_Implementation,
                GetType().Assembly.GetName().Version.ToString(), handler.Assembly, handler.Type);
        }

        private static ExtensiblityPipelineException Fail(PnPContext context, ExtensibilityHandler handler, Exception ex)
        {
            string message = string.Format(CultureInfo.CurrentCulture,
                PnPCoreProvisioningResources.Provisioning_Extensibility_Pipeline_Exception,
                handler.Assembly, handler.Type, ex);

            context.Logger?.LogError(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, message);

            return new ExtensiblityPipelineException(message, ex);
        }

        private object GetProviderInstance(ExtensibilityHandler handler)
        {
            if (!handlerCache.ContainsKey(handler))
            {
                Type handlerType = Type.GetType($"{handler.Type}, {handler.Assembly}", true);
                handlerCache.Add(handler, Activator.CreateInstance(handlerType));
            }

            return handlerCache[handler];
        }
    }
}
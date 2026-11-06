using Microsoft.Extensions.Logging;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// Delivers the webhook notifications a template asks for as provisioning progresses.
    /// </summary>
    public static class WebhookSender
    {
        /// <summary>
        /// Sends a webhook notification.
        /// </summary>
        /// <param name="webhook">The webhook to notify</param>
        /// <param name="httpClient">The client to send the notification with</param>
        /// <param name="kind">Which provisioning event is being reported</param>
        /// <param name="parser">Parser used to resolve tokens in the url and parameters, optional</param>
        /// <param name="objectHandler">Name of the handler the event relates to, optional</param>
        /// <param name="exception">The exception being reported, optional</param>
        /// <param name="logger">Logger for delivery failures, optional</param>
        public static async Task InvokeWebhookAsync(ProvisioningWebhookBase webhook,
            HttpClient httpClient,
            ProvisioningTemplateWebhookKind kind,
            TokenParser parser = null,
            string objectHandler = null,
            Exception exception = null,
            ILogger logger = null)
        {
            if (webhook == null)
            {
                throw new ArgumentNullException(nameof(webhook));
            }

            if (httpClient == null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }

            var requestParameters = new Dictionary<string, string>();

            if (exception != null)
            {
                requestParameters["__exception"] =
                    webhook.Method == ProvisioningTemplateWebhookMethod.GET
                        ? exception.Message
                        : exception.ToString();
            }

            var internalParser = new SimpleTokenParser();
            if (webhook.Parameters != null)
            {
                foreach (KeyValuePair<string, string> webhookParam in webhook.Parameters)
                {
                    requestParameters.Add(webhookParam.Key, parser != null ? parser.ParseString(webhookParam.Value) : webhookParam.Value);
                    internalParser.AddToken(new WebhookParameter(webhookParam.Key, requestParameters[webhookParam.Key]));
                }
            }

            string url = parser != null ? parser.ParseString(webhook.Url) : webhook.Url; // parse for template scoped parameters
            url = internalParser.ParseString(url); // parse for webhook scoped parameters

            if (!url.Contains("?"))
            {
                url += "?";
            }

            bool reportsHandler = kind == ProvisioningTemplateWebhookKind.ObjectHandlerProvisioningStarted
                || kind == ProvisioningTemplateWebhookKind.ObjectHandlerProvisioningCompleted
                || kind == ProvisioningTemplateWebhookKind.ExceptionOccurred;

            try
            {
                switch (webhook.Method)
                {
                    case ProvisioningTemplateWebhookMethod.GET:
                        {
                            url += $"&__webhookKind={kind}"; // add the webhook kind to the REST request URL

                            foreach (string key in requestParameters.Keys)
                            {
                                url += $"&{Uri.EscapeDataString(key)}={Uri.EscapeDataString(requestParameters[key])}";
                            }

                            if (reportsHandler && objectHandler != null)
                            {
                                url += $"&__handler={Uri.EscapeDataString(objectHandler)}"; // add the handler name to the REST request URL
                            }

                            Task get = httpClient.GetAsync(url);
                            if (!webhook.Async)
                            {
                                await get.ConfigureAwait(false);
                            }
                            break;
                        }

                    case ProvisioningTemplateWebhookMethod.POST:
                        {
                            requestParameters.Add("__webhookKind", kind.ToString()); // add the webhook kind to the parameters of the request body

                            if (reportsHandler && objectHandler != null)
                            {
                                requestParameters.Add("__handler", objectHandler); // add the handler name to the parameters of the request body
                            }

                            Task post = PostAsync(httpClient, url, webhook.BodyFormat, requestParameters);
                            if (!webhook.Async)
                            {
                                await post.ConfigureAwait(false);
                            }
                            break;
                        }
                }
            }
            catch (HttpRequestException ex)
            {
                logger?.LogError(ex, "{Source}: Error calling provisioning template webhook", Constants.LOGGING_SOURCE);
            }
        }

        private static async Task PostAsync(HttpClient httpClient, string url, ProvisioningTemplateWebhookBodyFormat bodyFormat, Dictionary<string, string> requestParameters)
        {
            switch (bodyFormat)
            {
                case ProvisioningTemplateWebhookBodyFormat.Json:
                    using (var stringContent = new StringContent(JsonSerializer.Serialize(requestParameters), Encoding.UTF8, "application/json"))
                    {
                        await httpClient.PostAsync(url, stringContent).ConfigureAwait(false);
                    }
                    break;

                case ProvisioningTemplateWebhookBodyFormat.Xml:
                    using (var stringContent = new StringContent(SerializeXml(requestParameters), Encoding.UTF8, "application/xml"))
                    {
                        await httpClient.PostAsync(url, stringContent).ConfigureAwait(false);
                    }
                    break;

                case ProvisioningTemplateWebhookBodyFormat.FormUrlEncoded:
                    using (var content = new FormUrlEncodedContent(requestParameters))
                    {
                        await httpClient.PostAsync(url, content).ConfigureAwait(false);
                    }
                    break;
            }
        }

        /// <summary>
        /// Renders the webhook parameters as XML.
        /// </summary>
        private static string SerializeXml(Dictionary<string, string> requestParameters)
        {
            var root = new XElement("parameters",
                requestParameters.Select(p => new XElement("parameter",
                    new XAttribute("key", p.Key),
                    p.Value ?? string.Empty)));

            return new XDocument(new XDeclaration("1.0", "utf-8", null), root).ToString();
        }
    }
}

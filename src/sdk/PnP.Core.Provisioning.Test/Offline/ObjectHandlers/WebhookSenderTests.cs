using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.ObjectHandlers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.Test.Offline.ObjectHandlers
{
    /// <summary>
    /// Covers webhook delivery: the url and body a template's webhook declaration turns into.
    /// </summary>
    [TestClass]
    public class WebhookSenderTests
    {
        private sealed class RecordingHandler : HttpMessageHandler
        {
            public HttpRequestMessage Request { get; private set; }
            public string Body { get; private set; }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Request = request;
                if (request.Content != null)
                {
                    Body = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        }

        private static ProvisioningTemplateWebhook Webhook(ProvisioningTemplateWebhookMethod method, string url = "https://contoso.example/hook")
        {
            return new ProvisioningTemplateWebhook
            {
                Url = url,
                Method = method,
                Kind = ProvisioningTemplateWebhookKind.ProvisioningTemplateStarted,
                Async = false,
            };
        }

        [TestMethod]
        [TestCategory("Offline")]
        public async Task Get_PutsTheWebhookKindAndParametersInTheQueryString()
        {
            var handler = new RecordingHandler();
            using (var client = new HttpClient(handler))
            {
                ProvisioningTemplateWebhook webhook = Webhook(ProvisioningTemplateWebhookMethod.GET);
                webhook.Parameters.Add("Environment", "Production");

                await WebhookSender.InvokeWebhookAsync(webhook, client,
                    ProvisioningTemplateWebhookKind.ProvisioningTemplateStarted).ConfigureAwait(false);

                string url = handler.Request.RequestUri.ToString();

                Assert.AreEqual(HttpMethod.Get, handler.Request.Method);
                StringAssert.Contains(url, "__webhookKind=ProvisioningTemplateStarted");
                StringAssert.Contains(url, "Environment=Production");
            }
        }

        [TestMethod]
        [TestCategory("Offline")]
        public async Task Get_UrlEncodesParameterNamesAndValues()
        {
            var handler = new RecordingHandler();
            using (var client = new HttpClient(handler))
            {
                ProvisioningTemplateWebhook webhook = Webhook(ProvisioningTemplateWebhookMethod.GET);
                webhook.Parameters.Add("Site Name", "Contoso & Fabrikam");

                await WebhookSender.InvokeWebhookAsync(webhook, client,
                    ProvisioningTemplateWebhookKind.ProvisioningTemplateStarted).ConfigureAwait(false);

                string url = handler.Request.RequestUri.ToString();

                StringAssert.Contains(url, "%26",
                    "'&' inside a parameter value must stay encoded, or it splits the query string.");
                Assert.IsFalse(url.Contains("Contoso & Fabrikam"),
                    "The raw '&' must not reach the query string.");
            }
        }

        [TestMethod]
        [TestCategory("Offline")]
        public async Task Get_AppendsAQuestionMarkWhenTheUrlHasNoQueryString()
        {
            var handler = new RecordingHandler();
            using (var client = new HttpClient(handler))
            {
                await WebhookSender.InvokeWebhookAsync(Webhook(ProvisioningTemplateWebhookMethod.GET), client,
                    ProvisioningTemplateWebhookKind.ProvisioningTemplateStarted).ConfigureAwait(false);

                StringAssert.Contains(handler.Request.RequestUri.ToString(), "?");
            }
        }

        [TestMethod]
        [TestCategory("Offline")]
        public async Task Get_ReportsTheHandlerNameForHandlerScopedEvents()
        {
            var handler = new RecordingHandler();
            using (var client = new HttpClient(handler))
            {
                ProvisioningTemplateWebhook webhook = Webhook(ProvisioningTemplateWebhookMethod.GET);

                await WebhookSender.InvokeWebhookAsync(webhook, client,
                    ProvisioningTemplateWebhookKind.ObjectHandlerProvisioningStarted,
                    objectHandler: "ObjectListInstance").ConfigureAwait(false);

                StringAssert.Contains(handler.Request.RequestUri.ToString(), "__handler=ObjectListInstance");
            }
        }

        [TestMethod]
        [TestCategory("Offline")]
        public async Task Get_TruncatesAnExceptionToItsMessage()
        {
            var handler = new RecordingHandler();
            using (var client = new HttpClient(handler))
            {
                await WebhookSender.InvokeWebhookAsync(Webhook(ProvisioningTemplateWebhookMethod.GET), client,
                    ProvisioningTemplateWebhookKind.ExceptionOccurred,
                    exception: new InvalidOperationException("something went wrong")).ConfigureAwait(false);

                string url = Uri.UnescapeDataString(handler.Request.RequestUri.ToString());

                StringAssert.Contains(url, "something went wrong");
                Assert.IsFalse(url.Contains("   at "), "A GET webhook must not carry the stack trace.");
            }
        }

        [TestMethod]
        [TestCategory("Offline")]
        public async Task Post_SendsAJsonBodyWithTheWebhookKind()
        {
            var handler = new RecordingHandler();
            using (var client = new HttpClient(handler))
            {
                ProvisioningTemplateWebhook webhook = Webhook(ProvisioningTemplateWebhookMethod.POST);
                webhook.BodyFormat = ProvisioningTemplateWebhookBodyFormat.Json;
                webhook.Parameters.Add("Environment", "Production");

                await WebhookSender.InvokeWebhookAsync(webhook, client,
                    ProvisioningTemplateWebhookKind.ProvisioningTemplateStarted).ConfigureAwait(false);

                Assert.AreEqual(HttpMethod.Post, handler.Request.Method);
                Assert.AreEqual("application/json", handler.Request.Content.Headers.ContentType.MediaType);
                StringAssert.Contains(handler.Body, "\"__webhookKind\":\"ProvisioningTemplateStarted\"");
                StringAssert.Contains(handler.Body, "\"Environment\":\"Production\"");
            }
        }

        [TestMethod]
        [TestCategory("Offline")]
        public async Task Post_SendsAFormUrlEncodedBodyWhenAsked()
        {
            var handler = new RecordingHandler();
            using (var client = new HttpClient(handler))
            {
                ProvisioningTemplateWebhook webhook = Webhook(ProvisioningTemplateWebhookMethod.POST);
                webhook.BodyFormat = ProvisioningTemplateWebhookBodyFormat.FormUrlEncoded;
                webhook.Parameters.Add("Environment", "Production");

                await WebhookSender.InvokeWebhookAsync(webhook, client,
                    ProvisioningTemplateWebhookKind.ProvisioningTemplateStarted).ConfigureAwait(false);

                Assert.AreEqual("application/x-www-form-urlencoded", handler.Request.Content.Headers.ContentType.MediaType);
                StringAssert.Contains(handler.Body, "Environment=Production");
            }
        }

        [TestMethod]
        [TestCategory("Offline")]
        public async Task Post_SendsAnXmlBodyWhenAsked()
        {
            var handler = new RecordingHandler();
            using (var client = new HttpClient(handler))
            {
                ProvisioningTemplateWebhook webhook = Webhook(ProvisioningTemplateWebhookMethod.POST);
                webhook.BodyFormat = ProvisioningTemplateWebhookBodyFormat.Xml;
                webhook.Parameters.Add("Environment", "Production");

                await WebhookSender.InvokeWebhookAsync(webhook, client,
                    ProvisioningTemplateWebhookKind.ProvisioningTemplateStarted).ConfigureAwait(false);

                Assert.AreEqual("application/xml", handler.Request.Content.Headers.ContentType.MediaType);
                StringAssert.Contains(handler.Body, "<parameter key=\"Environment\">Production</parameter>");
                StringAssert.Contains(handler.Body, "<parameter key=\"__webhookKind\">ProvisioningTemplateStarted</parameter>");
            }
        }

        [TestMethod]
        [TestCategory("Offline")]
        public async Task Post_CarriesTheFullExceptionDetail()
        {
            var handler = new RecordingHandler();
            using (var client = new HttpClient(handler))
            {
                ProvisioningTemplateWebhook webhook = Webhook(ProvisioningTemplateWebhookMethod.POST);
                webhook.BodyFormat = ProvisioningTemplateWebhookBodyFormat.Json;

                Exception thrown;
                try
                {
                    throw new InvalidOperationException("something went wrong");
                }
                catch (InvalidOperationException ex)
                {
                    thrown = ex;
                }

                await WebhookSender.InvokeWebhookAsync(webhook, client,
                    ProvisioningTemplateWebhookKind.ExceptionOccurred, exception: thrown).ConfigureAwait(false);

                StringAssert.Contains(handler.Body, "InvalidOperationException");
            }
        }

        [TestMethod]
        [TestCategory("Offline")]
        public async Task Parameters_AreResolvedAgainstEachOther()
        {
            var handler = new RecordingHandler();
            using (var client = new HttpClient(handler))
            {
                ProvisioningTemplateWebhook webhook = Webhook(ProvisioningTemplateWebhookMethod.GET,
                    "https://contoso.example/hook/{webhookparam:Environment}");
                webhook.Parameters.Add("Environment", "Production");

                await WebhookSender.InvokeWebhookAsync(webhook, client,
                    ProvisioningTemplateWebhookKind.ProvisioningTemplateStarted).ConfigureAwait(false);

                StringAssert.Contains(handler.Request.RequestUri.AbsolutePath, "/hook/Production");
            }
        }

        [TestMethod]
        [TestCategory("Offline")]
        public async Task NullArguments_AreRejected()
        {
            using (var client = new HttpClient(new RecordingHandler()))
            {
                await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
                    WebhookSender.InvokeWebhookAsync(null, client,
                        ProvisioningTemplateWebhookKind.ProvisioningTemplateStarted)).ConfigureAwait(false);
            }

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
                WebhookSender.InvokeWebhookAsync(Webhook(ProvisioningTemplateWebhookMethod.GET), null,
                    ProvisioningTemplateWebhookKind.ProvisioningTemplateStarted)).ConfigureAwait(false);
        }
    }
}

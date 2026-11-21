using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Modernization.Tests
{
    internal static class CoreTestContext
    {
        private static readonly ConditionalWeakTable<PnPContext, ResponseHandler> handlers = new();
        private static readonly ConcurrentBag<IDisposable> resources = new();

        internal static PnPContext Create(string siteUrl = "https://contoso.sharepoint.com/sites/test",
            Func<HttpRequestMessage, string, HttpResponseMessage> respond = null,
            Dictionary<string, object> webProperties = null, Dictionary<string, object> siteProperties = null)
        {
            var handler = new ResponseHandler(siteUrl, respond, webProperties, siteProperties);
            var restHttp = new HttpClient(handler);
            var graphHttp = new HttpClient(new ResponseHandler(siteUrl, null, null, null));
            resources.Add(restHttp);
            resources.Add(graphHttp);
            var global = Options.Create(new PnPGlobalSettingsOptions());
            var factory = new PnPContextFactory(NullLogger<PnPContext>.Instance,
                new SharePointRestClient(restHttp, NullLogger<SharePointRestClient>.Instance, global),
                new MicrosoftGraphClient(graphHttp, NullLogger<MicrosoftGraphClient>.Instance, global),
                Options.Create(new PnPContextFactoryOptions { GraphFirst = false }), global, new EventHub());
            var context = factory.Create(new Uri(siteUrl), new OfflineAuthenticationProvider());
            handlers.Add(context, handler);
            return context;
        }

        internal static void SetResponse(PnPContext context, Func<HttpRequestMessage, string, HttpResponseMessage> response)
        {
            handlers.GetValue(context, _ => throw new InvalidOperationException("The context is not an offline test context.")).Respond = response;
        }

        internal static PnPContext WithFile(string siteUrl, object file,
            Dictionary<string, object> webProperties = null, Dictionary<string, object> siteProperties = null) =>
            Create(siteUrl, (request, _) =>
            {
                if (!request.RequestUri.AbsolutePath.Contains("/GetFileBy", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"Unexpected file fixture request: {request.Method} {request.RequestUri}");
                return Json(file);
            }, webProperties, siteProperties);

        internal static IListItem Item(PnPContext context, int id = 1)
        {
            var handler = handlers.GetValue(context, _ => throw new InvalidOperationException("Expected an offline context."));
            var original = handler.Respond;
            var listId = Id(context.Uri.AbsoluteUri + "/Pages");
            handler.Respond = (request, body) =>
            {
                var path = request.RequestUri.AbsolutePath;
                var item = new Dictionary<string, object>
                    {
                        ["odata.type"] = "SP.Data.PagesItem",
                        ["odata.id"] = request.RequestUri.GetLeftPart(UriPartial.Path),
                        ["Id"] = id,
                        ["ParentList"] = new { Id = listId }
                    };
                if (path.EndsWith($"/items({id})", StringComparison.OrdinalIgnoreCase))
                    return Json(item);
                if (path.EndsWith("/items", StringComparison.OrdinalIgnoreCase))
                    return Json(new { value = new[] { item } });
                if (path.EndsWith("/lists", StringComparison.OrdinalIgnoreCase))
                    return Json(new { value = new[] { new { Id = listId, Title = "Pages", ListItemEntityTypeFullName = "SP.Data.PagesItem" } } });
                if (path.Contains("/lists", StringComparison.OrdinalIgnoreCase))
                    return Json(new { Id = listId, Title = "Pages", ListItemEntityTypeFullName = "SP.Data.PagesItem" });
                return original?.Invoke(request, body)
                    ?? throw new InvalidOperationException($"Unexpected item fixture request: {request.RequestUri}");
            };
            try
            {
                return context.Web.Lists.GetById(listId).Items.GetById(id);
            }
            finally
            {
                handler.Respond = original;
            }
        }

        internal static HttpResponseMessage Json(object value, HttpStatusCode status = HttpStatusCode.OK) =>
            new HttpResponseMessage(status)
            {
                Content = new StringContent(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json")
            };

        internal static HttpResponseMessage Error(string type, string message, int code)
        {
            var status = type == "System.IO.FileNotFoundException" ? HttpStatusCode.NotFound :
                type == "System.UnauthorizedAccessException" ? HttpStatusCode.Forbidden : HttpStatusCode.InternalServerError;
            return Json(new Dictionary<string, object>
            {
                ["odata.error"] = new { code = $"{code}, {type}", message = new { lang = "en-US", value = message } }
            }, status);
        }

        internal static void DisposeResources()
        {
            while (resources.TryTake(out var resource))
            {
                resource.Dispose();
            }
        }

        private static Guid Id(string value) => new Guid(SHA256.HashData(Encoding.UTF8.GetBytes(value)).AsSpan(0, 16));

        private sealed class ResponseHandler : HttpMessageHandler
        {
            private readonly Uri site;
            private readonly Dictionary<string, object> web;
            private readonly Dictionary<string, object> siteData;
            internal Func<HttpRequestMessage, string, HttpResponseMessage> Respond { get; set; }

            internal ResponseHandler(string url, Func<HttpRequestMessage, string, HttpResponseMessage> respond,
                Dictionary<string, object> webProperties, Dictionary<string, object> siteProperties)
            {
                site = new Uri(url.TrimEnd('/'));
                Respond = respond;
                web = new Dictionary<string, object>
                {
                    ["Id"] = Id(url + "/web"),
                    ["Url"] = site.AbsoluteUri,
                    ["ServerRelativeUrl"] = site.AbsolutePath,
                    ["WebTemplate"] = "STS",
                    ["Language"] = 1033,
                    ["RegionalSettings"] = new
                    {
                        LocaleId = 1033,
                        TimeZone = new { Id = 0, Description = "UTC", Information = new { Bias = 0, StandardBias = 0, DaylightBias = 0 } }
                    }
                };
                siteData = new Dictionary<string, object>
                {
                    ["Id"] = Id(url + "/site"),
                    ["GroupId"] = Guid.Empty,
                    ["Url"] = site.AbsoluteUri,
                    ["ServerRelativeUrl"] = site.AbsolutePath
                };
                if (webProperties != null)
                    foreach (var pair in webProperties) web[pair.Key] = pair.Value;
                if (siteProperties != null)
                    foreach (var pair in siteProperties) siteData[pair.Key] = pair.Value;
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var body = request.Content == null ? "" : await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                if (request.RequestUri.AbsolutePath.EndsWith("/_api/$batch", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = Regex.Matches(body,
                        @"^(?<method>GET|POST|PATCH|DELETE|PUT|MERGE) (?<url>\S+) HTTP/1\.1\r?\n(?<headers>(?:[^\r\n]+\r?\n)*)\r?\n(?<body>.*?)(?=^--|\r?\n--|\z)",
                        RegexOptions.Multiline | RegexOptions.Singleline);
                    if (parts.Count == 0) throw new InvalidOperationException("The offline fixture could not parse the REST batch.");
                    const string boundary = "batchresponse_offline_fixture";
                    var responseBody = new StringBuilder();
                    foreach (Match part in parts)
                    {
                        using var inner = new HttpRequestMessage(new HttpMethod(part.Groups["method"].Value), part.Groups["url"].Value);
                        using var result = RespondTo(inner, part.Groups["body"].Value);
                        var content = result.Content == null ? "" :
                            await result.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                        responseBody.Append($"--{boundary}\r\nContent-Type: application/http\r\nContent-Transfer-Encoding: binary\r\n\r\n")
                            .Append($"HTTP/1.1 {(int)result.StatusCode} {result.ReasonPhrase}\r\nContent-Type: application/json;odata=nometadata\r\n\r\n")
                            .Append(content).Append("\r\n");
                    }
                    responseBody.Append($"--{boundary}--\r\n");
                    var batchContent = new StringContent(responseBody.ToString(), Encoding.UTF8);
                    batchContent.Headers.ContentType = new MediaTypeHeaderValue("multipart/mixed");
                    batchContent.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", boundary));
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = batchContent };
                }
                return RespondTo(request, body);
            }

            private HttpResponseMessage RespondTo(HttpRequestMessage request, string body)
            {
                var path = request.RequestUri.AbsolutePath;
                if (request.RequestUri.Host == site.Host && request.Method == HttpMethod.Get)
                {
                    if (path.Equals(site.AbsolutePath + "/_api/web", StringComparison.OrdinalIgnoreCase))
                    {
                        var query = Uri.UnescapeDataString(request.RequestUri.Query);
                        var fields = Regex.Matches(query, @"(?:\?|&)\$(?:select|expand)=([^&]+)")
                            .SelectMany(match => match.Groups[1].Value.Split(','))
                            .Select(field => field.Split('/')[0]);
                        var bootstrap = new[] { "Id", "Url", "ServerRelativeUrl", "WebTemplate", "Language", "Description", "RegionalSettings" };
                        if (fields.All(field => bootstrap.Contains(field, StringComparer.OrdinalIgnoreCase))) return Json(web);
                    }
                    if (path.Equals(site.AbsolutePath + "/_api/site", StringComparison.OrdinalIgnoreCase)) return Json(siteData);
                }
                if (request.RequestUri.Host == site.Host && request.Method == HttpMethod.Post &&
                    path.Equals(site.AbsolutePath + "/_api/contextinfo", StringComparison.OrdinalIgnoreCase))
                    return Json(new { LibraryVersion = "16.0.0.26000", FormDigestValue = "offline", FormDigestTimeoutSeconds = 1800 });
                return Respond?.Invoke(request, body)
                    ?? throw new InvalidOperationException($"Unexpected offline request: {request.Method} {request.RequestUri}");
            }
        }

        private sealed class OfflineAuthenticationProvider : IAuthenticationProvider
        {
            public Task AuthenticateRequestAsync(Uri resource, HttpRequestMessage request) => Task.CompletedTask;
            public Task<string> GetAccessTokenAsync(Uri resource, string[] scopes) =>
                throw new InvalidOperationException("Offline tests must not request access tokens.");
            public Task<string> GetAccessTokenAsync(Uri resource) =>
                throw new InvalidOperationException("Offline tests must not request access tokens.");
        }
    }

}

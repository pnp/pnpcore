using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Services
{
    internal static class HttpRequestMessageExtensions
    {
        /// <summary>
        /// Create a new HTTP request by copying previous HTTP request's headers and properties from response's request message.
        /// Copied from: https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/blob/dev/src/Microsoft.Graph.Core/Extensions/HttpRequestMessageExtensions.cs
        /// </summary>
        /// <param name="originalRequest">The previous <see cref="HttpRequestMessage"/> needs to be copy.</param>
        /// <returns>The <see cref="HttpRequestMessage"/>.</returns>
        /// <remarks>
        /// Re-issue a new HTTP request with the previous request's headers and properities
        /// </remarks>
        internal static async Task<HttpRequestMessage> CloneAsync(this HttpRequestMessage originalRequest)
        {
            var newRequest = new HttpRequestMessage(originalRequest.Method, originalRequest.RequestUri);

            // Copy request headers.
            foreach (var header in originalRequest.Headers)
                newRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);

            // Copy request properties.
#pragma warning disable CS0618 // Type or member is obsolete
            foreach (var property in originalRequest.Properties)
            {
                newRequest.Properties.Add(property);
            }
#pragma warning restore CS0618 // Type or member is obsolete

            // Set Content if previous request had one.
            if (originalRequest.Content != null)
            {
                // HttpClient doesn't rewind streams and we have to explicitly do so.
#pragma warning disable CA2008 // Do not create tasks without passing a TaskScheduler
                await originalRequest.Content.ReadAsStreamAsync().ContinueWith(t =>
                {
                    if (t.Result.CanSeek)
                    {
                        t.Result.Seek(0, SeekOrigin.Begin);
                    }

                    newRequest.Content = new StreamContent(t.Result);
                }).ConfigureAwait(false);
#pragma warning restore CA2008 // Do not create tasks without passing a TaskScheduler

                // Copy content headers.
                if (originalRequest.Content.Headers != null)
                    foreach (var contentHeader in originalRequest.Content.Headers)
                        newRequest.Content.Headers.TryAddWithoutValidation(contentHeader.Key, contentHeader.Value);
            }

            return newRequest;
        }
    }
}

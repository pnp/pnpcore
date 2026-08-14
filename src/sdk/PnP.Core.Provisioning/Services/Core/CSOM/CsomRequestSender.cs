using Microsoft.Extensions.Logging;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using PnP.Core.Services.Core;
using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.Services.Core.CSOM
{
    /// <summary>
    /// Sends CSOM requests through PnP Core's batching pipeline.
    /// </summary>
    internal static class CsomRequestSender
    {
        /// <summary>
        /// Sends a single CSOM request and returns its typed result.
        /// </summary>
        /// <typeparam name="T">
        /// The request's result type. <b>Must be a reference type</b> - see the remarks.
        /// </typeparam>
        /// <param name="context">The context to send against</param>
        /// <param name="request">The request to send</param>
        /// <returns>The request's <c>Result</c> after the response has been processed</returns>
        internal static async Task<T> SendAsync<T>(PnPContext context, IRequest<T> request)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            await SendManyAsync(context, new List<IRequest<object>> { (IRequest<object>)request }).ConfigureAwait(false);

            return request.Result;
        }

        /// <summary>
        /// Sends several CSOM requests in a single round trip.
        /// </summary>
        /// <param name="context">The context to send against</param>
        /// <param name="requests">The requests to send, in order</param>
        internal static async Task SendManyAsync(PnPContext context, List<IRequest<object>> requests)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (requests == null || requests.Count == 0)
            {
                throw new ArgumentException("At least one request is required.", nameof(requests));
            }

            var apiCall = new ApiCall(new List<IRequest<object>> { new CompositeRequest(requests) });

            await SendWithSaveConflictRetryAsync(context, apiCall).ConfigureAwait(false);
        }

        /// <summary>
        /// How many times a save conflict is retried before giving up.
        /// </summary>
        private const int SaveConflictAttempts = 4;

        /// <summary>
        /// Sends the call, retrying the one error that is meant to be retried.
        /// </summary>
        private static async Task SendWithSaveConflictRetryAsync(PnPContext context, ApiCall apiCall)
        {
            for (int attempt = 1; ; attempt++)
            {
                try
                {
                    await (context.Web as Web).RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
                    return;
                }
                catch (Exception ex) when (attempt < SaveConflictAttempts && IsSaveConflict(ex))
                {
                    int delay = 500 * attempt;

                    context.Logger?.LogInformation(
                        "{Source}: the term store reported a save conflict; retrying in {Delay}ms (attempt {Attempt} of {Total}).",
                        Constants.LOGGING_SOURCE, delay, attempt, SaveConflictAttempts);

                    await Task.Delay(delay).ConfigureAwait(false);
                }
            }
        }

        private static bool IsSaveConflict(Exception ex)
        {
            for (Exception current = ex; current != null; current = current.InnerException)
            {
                if (current is ServiceException serviceException
                    && serviceException.Error?.ToString()?.IndexOf("save conflict", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Presents several CSOM requests to PnP Core as one, so all of them see the response.
        /// </summary>
        private sealed class CompositeRequest : IRequest<object>
        {
            private readonly List<IRequest<object>> requests;

            internal CompositeRequest(List<IRequest<object>> requests)
            {
                this.requests = requests;
            }

            public object Result => null;

            public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
            {
                var paths = new List<ActionObjectPath>();

                foreach (IRequest<object> request in requests)
                {
                    paths.AddRange(request.GetRequest(idProvider));
                }

                return paths;
            }

            public void ProcessResponse(string response)
            {
                foreach (IRequest<object> request in requests)
                {
                    request.ProcessResponse(response);
                }
            }
        }

        /// <summary>
        /// Loads the site and web ids, which almost every CSOM identity string needs.
        /// </summary>
        internal static async Task<(Guid SiteId, Guid WebId)> GetSiteAndWebIdAsync(PnPContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            await context.Site.EnsurePropertiesAsync(s => s.Id).ConfigureAwait(false);
            await context.Web.EnsurePropertiesAsync(w => w.Id).ConfigureAwait(false);

            return (context.Site.Id, context.Web.Id);
        }
    }
}

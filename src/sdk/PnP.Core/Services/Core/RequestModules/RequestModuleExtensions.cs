using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Model
{
    /// <summary>
    /// Set of extension methods used to bring request module support
    /// </summary>
    public static class RequestModuleExtensions
    {
        /// <summary>
        /// Adds request modules to be executed when the request for this model will be executed
        /// </summary>
        /// <typeparam name="TModel">Model type</typeparam>
        /// <param name="dataModel">Model instance to operate on</param>
        /// <param name="module">Request module to add</param>
        /// <returns>The passed model instance</returns>
        internal static TModel WithModule<TModel>(this ISupportModules<TModel> dataModel, IRequestModule module)
        {
            var context = (dataModel as IDataModelWithContext).PnPContext;

            if (context.RequestModules == null)
            {
                context.RequestModules = new List<IRequestModule>();
            }

            if (context.RequestModules.FirstOrDefault(p => p.Id == module.Id) == null)
            {
                context.RequestModules.Add(module);
            }

            return (TModel)dataModel;
        }

        #region Module shorthands
        /// <summary>
        /// Executes this request with additional request headers
        /// </summary>
        /// <typeparam name="TModel">Model type</typeparam>
        /// <param name="dataModel">Model instance to operate on</param>
        /// <param name="headers">Collection of headers to add to add to the request</param>
        /// <param name="responseHeaders">Delegate that can be invoked to pass along the response headers</param>
        /// <returns>The passed model instance</returns>
        public static TModel WithHeaders<TModel>(this ISupportModules<TModel> dataModel, Dictionary<string, string> headers, Action<Dictionary<string, string>> responseHeaders = null)
        {
            return dataModel.WithModule(new CustomHeadersRequestModule(headers, responseHeaders));
        }

        /// <summary>
        /// Returns the response headers for Microsoft Graph, SharePoint REST and CSOM requests after this request has been executed
        /// </summary>
        /// <typeparam name="TModel">Model type</typeparam>
        /// <param name="dataModel">Model instance to operate on</param>
        /// <param name="responseHeaders">Delegate that can be invoked to receive the response headers</param>
        /// <returns>The passed model instance</returns>
        public static TModel WithResponseHeaders<TModel>(this ISupportModules<TModel> dataModel, Action<Dictionary<string, string>> responseHeaders = null)
        {
            return dataModel.WithModule(new ResponseHeadersModule(responseHeaders));
        }
        #endregion

    }
}

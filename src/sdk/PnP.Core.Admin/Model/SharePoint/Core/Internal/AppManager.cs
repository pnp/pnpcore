using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.SharePoint
{
    internal abstract class AppManager<T> : IAppManager<T> where T : IApp
    {
        protected readonly PnPContext context;
        protected abstract string Scope { get; }
        protected enum AppManagerAction
        {
            Install,
            Retract,
            Remove,
            Deploy,
            Upgrade,
            Uninstall,
            Approve
        }

        internal AppManager(PnPContext pnpContext)
        {
            context = pnpContext;
        }

        public async Task<Uri> GetTenantAppCatalogUriAsync()
        {
            var result = await (context.Web as Web).RawRequestAsync(new ApiCall("_api/SP_TenantSettings_Current", ApiType.SPORest), HttpMethod.Get).ConfigureAwait(false);

            var root = JsonDocument.Parse(result.Json).RootElement.GetProperty("CorporateCatalogUrl");

            if (root.ValueKind == JsonValueKind.String)
            {
                return new Uri(root.GetString());
            }

            return null;
        }

        public Uri GetTenantAppCatalogUri()
        {
            return GetTenantAppCatalogUriAsync().GetAwaiter().GetResult();
        }

        public async Task<bool> EnsureTenantAppCatalogAsync()
        {
            var result = await (context.Web as Web).RawRequestAsync(new ApiCall("_api/web/EnsureTenantAppCatalog(callerId='pnpcoresdk')", ApiType.SPORest), HttpMethod.Post).ConfigureAwait(false);
            var root = JsonDocument.Parse(result.Json).RootElement.GetProperty("value");
            return root.GetBoolean();
        }

        public bool EnsureTenantAppCatalog()
        {
            return EnsureTenantAppCatalogAsync().GetAwaiter().GetResult();
        }

        public T GetAvailable(Guid id)
        {
            return GetAvailableAsync(id).GetAwaiter().GetResult();
        }

        public async Task<T> GetAvailableAsync(Guid id)
        {
            var apiCall = new ApiCall($"_api/web/{Scope}appcatalog/AvailableApps/GetById('{id}')", ApiType.SPORest);

            var response = await (context.Web as Web).RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);

            return GetModelFromJson<T>(response.Json);
        }

        public T GetAvailable(string title)
        {
            return GetAvailableAsync(title).GetAwaiter().GetResult();
        }

        public async Task<T> GetAvailableAsync(string title)
        {
            if (title == null)
            {
                throw new ArgumentException(nameof(title));
            }

            var allApps = await GetAvailableAsync().ConfigureAwait(false);

            if (allApps == null) return default(T);

            return allApps.FirstOrDefault(a => a.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
        }

        public IList<T> GetAvailable()
        {
            return GetAvailableAsync().GetAwaiter().GetResult();
        }

        public async Task<IList<T>> GetAvailableAsync()
        {
            var apiCall = new ApiCall($"_api/web/{Scope}appcatalog/AvailableApps", ApiType.SPORest);

            var response = await (context.Web as Web).RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);

            return GetModelListFromJson<T>(response.Json);
        }

        public bool Deploy(Guid id, bool skipFeatureDeployment = true)
        {
            return DeployAsync(id, skipFeatureDeployment).GetAwaiter().GetResult();
        }

        public async Task<bool> DeployAsync(Guid id, bool skipFeatureDeployment = true)
        {
            var postObj = new Dictionary<string, object>
            {
                { "skipFeatureDeployment", skipFeatureDeployment }
            };

            return await ExecuteAppActionAsync(id, AppManagerAction.Deploy, postObj).ConfigureAwait(false);
        }

        public bool Retract(Guid id)
        {
            return RetractAsync(id).GetAwaiter().GetResult();
        }

        public async Task<bool> RetractAsync(Guid id)
        {
            return await ExecuteAppActionAsync(id, AppManagerAction.Retract).ConfigureAwait(false);
        }

        public bool Remove(Guid id)
        {
            return RemoveAsync(id).GetAwaiter().GetResult();
        }

        public async Task<bool> RemoveAsync(Guid id)
        {
            return await ExecuteAppActionAsync(id, AppManagerAction.Remove).ConfigureAwait(false);
        }

        public bool Install(Guid id)
        {
            return InstallAsync(id).GetAwaiter().GetResult();
        }

        public async Task<bool> InstallAsync(Guid id)
        {
            return await ExecuteAppActionAsync(id, AppManagerAction.Install).ConfigureAwait(false);
        }

        public bool Upgrade(Guid id)
        {
            return UpgradeAsync(id).GetAwaiter().GetResult();
        }

        public async Task<bool> UpgradeAsync(Guid id)
        {
            return await ExecuteAppActionAsync(id, AppManagerAction.Upgrade).ConfigureAwait(false);
        }

        public bool Uninstall(Guid id)
        {
            return UninstallAsync(id).GetAwaiter().GetResult();
        }

        public async Task<bool> UninstallAsync(Guid id)
        {
            return await ExecuteAppActionAsync(id, AppManagerAction.Uninstall).ConfigureAwait(false);
        }

        public T Add(byte[] file, string fileName, bool overwrite = false)
        {
            return AddAsync(file, fileName, overwrite).GetAwaiter().GetResult();
        }

        public async Task<T> AddAsync(byte[] fileBytes, string fileName, bool overwrite = false)
        {
            if (fileBytes == null || fileBytes.Length == 0)
            {
                throw new ArgumentException(nameof(fileBytes));
            }

            if (fileName == null)
            {
                throw new ArgumentException(nameof(fileName));
            }

            var apiCall = new ApiCall($"_api/web/{Scope}appcatalog/Add(overwrite={overwrite.ToString().ToLower()},url='{fileName}')", ApiType.SPORest)
            {
                Interactive = true,
                Content = new ByteArrayContent(fileBytes),
                Headers = new Dictionary<string, string>()
            };

            apiCall.Headers.Add("binaryStringRequestBody", "true");
            var pnpContext = this is TenantAppManager ?
                    await GetTenantAppCatalogContextAsync().ConfigureAwait(false)
                    : context;

            return await ExecuteWithDisposeAsync(pnpContext, async () =>
            {
                var response = await (pnpContext.Web as Web).RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
                var document = JsonSerializer.Deserialize<JsonElement>(response.Json);

                var uniqueId = document.Get("UniqueId")?.GetString();

                if (string.IsNullOrEmpty(uniqueId))
                {
                    throw new ClientException(PnPCoreAdminResources.Exception_UnexpectedJson);
                }

                return await GetByIdWithRetryAsync(new Guid(uniqueId)).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        public T Add(string path, bool overwrite = false)
        {
            return AddAsync(path, overwrite).GetAwaiter().GetResult();
        }

        public async Task<T> AddAsync(string path, bool overwrite = false)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException(nameof(path));
            }

            if (!System.IO.File.Exists(path))
            {
                throw new IOException(PnPCoreAdminResources.Exception_AppManager_FileNotFound);
            }

            var bytes = System.IO.File.ReadAllBytes(path);
            var fileInfo = new FileInfo(path);

            return await AddAsync(bytes, fileInfo.Name, overwrite).ConfigureAwait(false);
        }

        public async Task<IPermissionGrant2[]> ApproveAsync(string aadPermissions)
        {
            var result = new List<IPermissionGrant2>();
            foreach (var appPermissionRequest in ParsePermissionRequests(aadPermissions))
            {
                result.Add(await this.ServicePrincipal
                    .AddGrant2Async(
                        appPermissionRequest.Key, 
                        appPermissionRequest.Value).ConfigureAwait(false));
            }

            return result.ToArray();
        }

        internal static Dictionary<string, string> ParsePermissionRequests(string aadPermissions)
        {
            var result = new Dictionary<string, string>();

            if (aadPermissions == null) return result;

            foreach (string permission in aadPermissions.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries))
            {
                var appAndScope = permission.Split(',');
                if (appAndScope.Length != 2)
                {
                    continue;
                }

                var appName = appAndScope[0].Trim();
                var scope = appAndScope[1].Trim();
                if (result.TryGetValue(appName, out string value))
                {
                    result[appName] = $"{value} {scope}"; // append
                }
                else
                {
                    result[appName] = scope; // add
                }
            }

            return result;
        }
        
        public IServicePrincipal ServicePrincipal
        {
            get
            {
                return new ServicePrincipal(context);
            }
        }

        /// <summary>
        /// Executes an action and disposes <see cref="PnPContext" /> if it's not the current context
        /// </summary>
        /// <typeparam name="P">The type param is resolved automatically based on the action return type.</typeparam>
        /// <param name="pnpContext">A <see cref="PnPContext" /> instance, which will be disposed.</param>
        /// <param name="action">An action delegate.</param>
        /// <returns>A value, which is returned from action delegate.</returns>
        protected async Task<P> ExecuteWithDisposeAsync<P>(PnPContext pnpContext, Func<Task<P>> action)
        {
            try
            {
                return await action().ConfigureAwait(false);
            }
            finally
            {
                if (pnpContext != null && pnpContext != context)
                {
                    pnpContext.Dispose();
                }
            }
        }

        /// <summary>
        /// Gets an instance of the app by it's unique id with retry mechanism (if the app is not yet available in the app catalog).
        /// </summary>
        /// <param name="id">A unique id of the app.</param>
        /// <param name="waitSeconds">Seconds to wait betwen retries.</param>
        /// <param name="retryCount">Retry count.</param>
        /// <returns>An instance of the app.</returns>
        protected async Task<T> GetByIdWithRetryAsync(Guid id, int waitSeconds = 5, int retryCount = 10)
        {
            T app;

            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    app = await GetAvailableAsync(id).ConfigureAwait(false);

                    if (app != null)
                    {
                        return app;
                    }
                }
                catch (SharePointRestServiceException ex)
                {
                    if (ex.Error is SharePointRestError && ((SharePointRestError)ex.Error).HttpResponseCode == (int)HttpStatusCode.NotFound)
                    {
                        await context.WaitAsync(TimeSpan.FromMilliseconds(1000 * waitSeconds)).ConfigureAwait(false);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            throw new ClientException(PnPCoreAdminResources.Exception_AppManager_GetAppTimeout);
        }

        /// <summary>
        /// Creates a new instance of tenant app catalog context for some operations (Deploy, Retract, Remove).
        /// </summary>
        /// <param name="action">Current action.</param>
        /// <returns>A <see cref="PnPContext" /> instance.</returns>
        protected async Task<PnPContext> GetTenantAppCatalogContextIfNeededAsync(AppManagerAction action)
        {
            if (this is TenantAppManager &&
                (action == AppManagerAction.Deploy ||
                action == AppManagerAction.Retract ||
                action == AppManagerAction.Remove ||
                action == AppManagerAction.Approve 
                ))
            {
                return await GetTenantAppCatalogContextAsync().ConfigureAwait(false);
            }
            else
            {
                return context;
            }
        }

        /// <summary>
        /// Gets tenant app catalog context. If the current context is already a tenant app catalog context, then returns the current context.
        /// </summary>
        /// <returns>A <see cref="PnPContext" /> instance.</returns>
        protected async Task<PnPContext> GetTenantAppCatalogContextAsync()
        {
            var currentContextUrl = context.Uri.AbsoluteUri.TrimEnd('/');
            var appCatalogUri = await GetTenantAppCatalogUriAsync().ConfigureAwait(false);

            if (appCatalogUri == null)
            {
                throw new ClientException(PnPCoreAdminResources.Exception_AppManager_AppCatalogNotFound);
            }

            var appCatalogUrl = appCatalogUri.AbsoluteUri.TrimEnd('/');

            if (currentContextUrl.Equals(appCatalogUrl, StringComparison.OrdinalIgnoreCase))
            {
                return context;
            }

            return await context.CloneAsync(appCatalogUri).ConfigureAwait(false);
        }

        /// <summary>
        /// Generic method to execute an action like Deploy, Install, etc.
        /// </summary>
        /// <param name="id">A unique app id.</param>
        /// <param name="action">Action of type <see cref="AppManagerAction"/> </param>
        /// <param name="postObject">Optional dictionary with HTTP POST parameters.</param>
        /// <returns></returns>
        protected async Task<bool> ExecuteAppActionAsync(Guid id, AppManagerAction action, Dictionary<string, object> postObject = null)
        {
            var apiCall = new ApiCall($"_api/web/{Scope}appcatalog/AvailableApps/GetById('{id}')/{action}", ApiType.SPORest);

            if (postObject != null)
            {
                var jsonBody = JsonSerializer.Serialize(postObject);
                apiCall.JsonBody = jsonBody;
            }

            var pnpContext = await GetTenantAppCatalogContextIfNeededAsync(action).ConfigureAwait(false);

            return await ExecuteWithDisposeAsync(pnpContext, async () =>
            {
                await (pnpContext.Web as Web).RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

                return true;
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Extracts model from json.
        /// </summary>
        /// <typeparam name="TModel">TModel is an interface with the <see cref="ConcreteTypeAttribute"/> attribute.</typeparam>
        /// <param name="json">JSON string.</param>
        /// <returns>An instance of the object, which implements TModel interface.</returns>
        protected TModel GetModelFromJson<TModel>(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new ClientException(PnPCoreAdminResources.Exception_UnexpectedJson);
            }

            var document = JsonSerializer.Deserialize<JsonElement>(json);

            var type = EntityManager.GetEntityConcreteInstance(typeof(TModel)).GetType();

            var model = (TModel)document.ToObject(type, PnPConstants.JsonSerializer_PropertyNameCaseInsensitiveTrue);

            if (model is IDataModelWithContext)
            {
                ((IDataModelWithContext)model).PnPContext = context;
            }

            return model;
        }

        /// <summary>
        /// Extracts model list from json.
        /// </summary>
        /// <typeparam name="TModel">TModel is an interface with the <see cref="ConcreteTypeAttribute"/> attribute.</typeparam>
        /// <param name="json">JSON string.</param>
        /// <returns>A list of the objects, which implements TModel interface.</returns>
        protected IList<TModel> GetModelListFromJson<TModel>(string json)
        {
            var document = JsonSerializer.Deserialize<JsonElement>(json);

            var modelJson = document.Get("value");

            if (modelJson == null)
            {
                throw new ClientException(PnPCoreAdminResources.Exception_UnexpectedJson);
            }

            var type = EntityManager.GetEntityConcreteInstance(typeof(TModel)).GetType();
            var genericListType = typeof(List<>).MakeGenericType(type);

            var models = ((System.Collections.IList)(modelJson.Value).ToObject(genericListType, PnPConstants.JsonSerializer_PropertyNameCaseInsensitiveTrue)).Cast<TModel>().ToList();

            foreach (var model in models)
            {
                if (model is IDataModelWithContext)
                {
                    ((IDataModelWithContext)model).PnPContext = context;
                }
            }

            return models;
        }
    }
}

using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Utils;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;

namespace PnP.Core.Provisioning.Services.Core.CSOM.Requests.Tenant
{
    /// <summary>
    /// Which CDN a request is about.
    /// </summary>
    internal enum TenantCdnType
    {
        Public = 0,
        Private = 1,
    }

    /// <summary>
    /// Which CDN policy a request is about.
    /// </summary>
    internal enum TenantCdnPolicyType
    {
        IncludeFileExtensions = 0,
        ExcludeRestrictedSiteClassifications = 1,
        ExcludeIfNoScriptDisabled = 2,
    }

    /// <summary>
    /// Shared plumbing for the tenant CDN requests.
    /// </summary>
    internal abstract class TenantCdnRequestBase
    {
        protected TenantCdnRequestBase(TenantCdnType cdnType)
        {
            CdnType = cdnType;
        }

        protected TenantCdnType CdnType { get; }

        /// <summary>
        /// Builds <c>new Tenant(context)</c> and the action that realises it.
        /// </summary>
        protected static int AddTenantConstructor(List<ActionObjectPath> result, IIdProvider idProvider)
        {
            int constructorId = idProvider.GetActionId();

            result.Add(new ActionObjectPath
            {
                ObjectPath = new ConstructorPath
                {
                    Id = constructorId,
                    TypeId = CsomTypeIds.Tenant,
                },
            });

            result.Add(new ActionObjectPath
            {
                Action = new BaseAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = constructorId.ToString(CultureInfo.InvariantCulture),
                },
            });

            return constructorId;
        }

        protected Parameter CdnTypeParameter()
        {
            return new Parameter
            {
                Type = "Enum",
                Value = ((int)CdnType).ToString(CultureInfo.InvariantCulture),
            };
        }

        /// <summary>
        /// Adds a method call on the tenant and the action that invokes it.
        /// </summary>
        protected static int AddMethod(List<ActionObjectPath> result, IIdProvider idProvider,
            int tenantId, string name, List<Parameter> parameters)
        {
            int methodId = idProvider.GetActionId();

            result.Add(new ActionObjectPath
            {
                ObjectPath = new ObjectPathMethod
                {
                    Id = methodId,
                    ParentId = tenantId,
                    Name = name,
                    Parameters = new MethodParameter { Properties = parameters },
                },
            });

            result.Add(new ActionObjectPath
            {
                Action = new BaseAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = methodId.ToString(CultureInfo.InvariantCulture),
                },
            });

            return methodId;
        }
    }

    /// <summary>
    /// Reads whether a CDN is switched on.
    /// </summary>
    internal sealed class GetTenantCdnEnabledRequest : TenantCdnRequestBase, IRequest<CdnEnabledInfo>
    {
        internal GetTenantCdnEnabledRequest(TenantCdnType cdnType) : base(cdnType)
        {
        }

        public CdnEnabledInfo Result { get; private set; } = new CdnEnabledInfo();

        internal CSOMResponseHelper ResponseHelper { get; set; } = new CSOMResponseHelper();

        private int resultId;

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            var result = new List<ActionObjectPath>();

            int tenantId = AddTenantConstructor(result, idProvider);

            int methodId = idProvider.GetActionId();
            result.Add(new ActionObjectPath
            {
                ObjectPath = new ObjectPathMethod
                {
                    Id = methodId,
                    ParentId = tenantId,
                    Name = "GetTenantCdnEnabled",
                    Parameters = new MethodParameter
                    {
                        Properties = new List<Parameter> { CdnTypeParameter() },
                    },
                },
            });

            // A scalar method result comes back through an ObjectPath action, not a query - there
            // are no properties to select on a bool.
            resultId = idProvider.GetActionId();
            result.Add(new ActionObjectPath
            {
                Action = new BaseAction
                {
                    Id = resultId,
                    ObjectPathId = methodId.ToString(CultureInfo.InvariantCulture),
                },
            });

            return result;
        }

        public void ProcessResponse(string response)
        {
            Result = new CdnEnabledInfo
            {
                Enabled = ResponseHelper.ProcessResponse<bool>(response, resultId),
            };
        }
    }

    /// <summary>
    /// Whether a CDN is enabled. A reference type, for the reason given on
    /// <see cref="GetTenantCdnEnabledRequest"/>.
    /// </summary>
    internal sealed class CdnEnabledInfo
    {
        internal bool Enabled { get; set; }
    }

    /// <summary>
    /// Switches a CDN on or off.
    /// </summary>
    internal sealed class SetTenantCdnEnabledRequest : TenantCdnRequestBase, IRequest<object>
    {
        private readonly bool enabled;

        internal SetTenantCdnEnabledRequest(TenantCdnType cdnType, bool enabled) : base(cdnType)
        {
            this.enabled = enabled;
        }

        public object Result { get; set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            var result = new List<ActionObjectPath>();

            int tenantId = AddTenantConstructor(result, idProvider);

            AddMethod(result, idProvider, tenantId, "SetTenantCdnEnabled", new List<Parameter>
            {
                CdnTypeParameter(),
                new Parameter { Type = "Boolean", Value = enabled },
            });

            return result;
        }

        public void ProcessResponse(string response)
        {
        }
    }

    /// <summary>
    /// What to do to a CDN's origin list.
    /// </summary>
    internal enum CdnOriginAction
    {
        Add,
        Remove,
        CreateDefaults,
    }

    /// <summary>
    /// Adds an origin, removes one, or restores the defaults.
    /// </summary>
    internal sealed class TenantCdnOriginRequest : TenantCdnRequestBase, IRequest<object>
    {
        private readonly CdnOriginAction action;
        private readonly string origin;

        internal TenantCdnOriginRequest(TenantCdnType cdnType, CdnOriginAction action, string origin = null)
            : base(cdnType)
        {
            this.action = action;
            this.origin = origin;
        }

        public object Result { get; set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            var result = new List<ActionObjectPath>();

            int tenantId = AddTenantConstructor(result, idProvider);

            var parameters = new List<Parameter> { CdnTypeParameter() };

            if (action != CdnOriginAction.CreateDefaults)
            {
                parameters.Add(new Parameter { Type = "String", Value = origin });
            }

            AddMethod(result, idProvider, tenantId, MethodName(), parameters);

            return result;
        }

        private string MethodName()
        {
            switch (action)
            {
                case CdnOriginAction.Add:
                    return "AddTenantCdnOrigin";
                case CdnOriginAction.Remove:
                    return "RemoveTenantCdnOrigin";
                default:
                    return "CreateTenantCdnDefaultOrigins";
            }
        }

        public void ProcessResponse(string response)
        {
        }
    }

    /// <summary>
    /// Reads a CDN's origins, or its policies - both come back as a list of strings.
    /// </summary>
    internal sealed class GetTenantCdnStringsRequest : TenantCdnRequestBase, IRequest<List<string>>
    {
        private readonly string methodName;
        private int resultId;

        private GetTenantCdnStringsRequest(TenantCdnType cdnType, string methodName) : base(cdnType)
        {
            this.methodName = methodName;
        }

        internal static GetTenantCdnStringsRequest Origins(TenantCdnType cdnType)
        {
            return new GetTenantCdnStringsRequest(cdnType, "GetTenantCdnOrigins");
        }

        internal static GetTenantCdnStringsRequest Policies(TenantCdnType cdnType)
        {
            return new GetTenantCdnStringsRequest(cdnType, "GetTenantCdnPolicies");
        }

        public List<string> Result { get; private set; } = new List<string>();

        internal CSOMResponseHelper ResponseHelper { get; set; } = new CSOMResponseHelper();

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            var result = new List<ActionObjectPath>();

            int tenantId = AddTenantConstructor(result, idProvider);

            int methodId = idProvider.GetActionId();
            result.Add(new ActionObjectPath
            {
                ObjectPath = new ObjectPathMethod
                {
                    Id = methodId,
                    ParentId = tenantId,
                    Name = methodName,
                    Parameters = new MethodParameter
                    {
                        Properties = new List<Parameter> { CdnTypeParameter() },
                    },
                },
            });

            resultId = idProvider.GetActionId();
            result.Add(new ActionObjectPath
            {
                Action = new BaseAction
                {
                    Id = resultId,
                    ObjectPathId = methodId.ToString(CultureInfo.InvariantCulture),
                },
            });

            return result;
        }

        public void ProcessResponse(string response)
        {
            JsonElement value = ResponseHelper.ProcessResponse<JsonElement>(response, resultId);

            var parsed = new List<string>();

            if (value.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement item in value.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String)
                    {
                        parsed.Add(item.GetString());
                    }
                }
            }

            Result = parsed;
        }
    }

    /// <summary>
    /// Sets one CDN policy.
    /// </summary>
    internal sealed class SetTenantCdnPolicyRequest : TenantCdnRequestBase, IRequest<object>
    {
        private readonly TenantCdnPolicyType policyType;
        private readonly string value;

        internal SetTenantCdnPolicyRequest(TenantCdnType cdnType, TenantCdnPolicyType policyType, string value)
            : base(cdnType)
        {
            this.policyType = policyType;
            this.value = value;
        }

        public object Result { get; set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            var result = new List<ActionObjectPath>();

            int tenantId = AddTenantConstructor(result, idProvider);

            AddMethod(result, idProvider, tenantId, "SetTenantCdnPolicy", new List<Parameter>
            {
                CdnTypeParameter(),
                new Parameter
                {
                    Type = "Enum",
                    Value = ((int)policyType).ToString(CultureInfo.InvariantCulture),
                },
                new Parameter { Type = "String", Value = value },
            });

            return result;
        }

        public void ProcessResponse(string response)
        {
        }
    }

    /// <summary>
    /// Reads the <c>Name;Value</c> strings <c>GetTenantCdnPolicies</c> answers with.
    /// </summary>
    internal static class TenantCdnPolicies
    {
        /// <summary>
        /// Turns the raw entries into a lookup, skipping anything unrecognised.
        /// </summary>
        internal static Dictionary<TenantCdnPolicyType, string> Parse(IEnumerable<string> entries)
        {
            var parsed = new Dictionary<TenantCdnPolicyType, string>();

            if (entries == null)
            {
                return parsed;
            }

            foreach (string entry in entries)
            {
                if (string.IsNullOrEmpty(entry))
                {
                    continue;
                }

                int separator = entry.IndexOf(';');

                if (separator < 0)
                {
                    continue;
                }

                if (System.Enum.TryParse(entry.Substring(0, separator), out TenantCdnPolicyType type))
                {
                    parsed[type] = entry.Substring(separator + 1);
                }
            }

            return parsed;
        }
    }
}

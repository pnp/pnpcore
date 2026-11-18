using PnP.Core.Modernization.Cache;
using PnP.Core.Modernization.Entities;
using PnP.Core.Modernization.Telemetry;

using System;
using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Modernization.Transform
{
    /// <summary>Maps SharePoint Online principals using an optional user mapping file.</summary>
    public class UserTransformator : BaseTransform
    {
        private readonly PnPContext targetContext;
        private readonly List<UserMappingEntity> userMappings;
        private readonly bool skipUserMapping;

        public bool IsUserMappingSpecified => userMappings != null;
        public bool ShouldMapUsers => !skipUserMapping;

        public UserTransformator(BaseTransformationInformation baseTransformationInformation, PnPContext sourceContext,
            PnPContext targetContext, IList<ILogObserver> logObservers = null)
        {
            if (logObservers != null)
            {
                foreach (var observer in logObservers) RegisterObserver(observer);
            }
            if (sourceContext != null) SharePointOnlineSupport.Validate(sourceContext);
            if (targetContext != null) SharePointOnlineSupport.Validate(targetContext);
            if (!string.IsNullOrEmpty(baseTransformationInformation?.LDAPConnectionString))
            {
                throw new NotSupportedException("LDAP/on-premises user resolution is not supported. Use an explicit SharePoint Online user mapping file.");
            }
            this.targetContext = targetContext ?? sourceContext;
            skipUserMapping = baseTransformationInformation?.SkipUserMapping ?? false;
            if (!string.IsNullOrEmpty(baseTransformationInformation?.UserMappingFile))
            {
                userMappings = CacheManager.Instance.GetUserMapping(baseTransformationInformation.UserMappingFile, logObservers);
            }
        }

        public string RemapPrincipal(string principalInput)
        {
            if (string.IsNullOrEmpty(principalInput) || !ShouldMapUsers || !IsUserMappingSpecified) return principalInput;
            var matches = userMappings.Where(m => m.SourceUser.Equals(principalInput, StringComparison.OrdinalIgnoreCase)).ToList();
            if (matches.Count == 0 && principalInput.Contains("|"))
            {
                matches = userMappings.Where(m => m.SourceUser.Equals(principalInput.GetUserName(), StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (matches.Count == 0)
            {
                LogInfo(string.Format(LogStrings.UserTransformMappingNotFound, principalInput.GetUserName()), LogStrings.Heading_UserTransform);
                return principalInput;
            }
            var result = matches[0].TargetUser;
            result = EnsureValidUserExists(result) ?? result;
            if (matches.Count > 1)
            {
                LogWarning(string.Format(LogStrings.Warning_MultipleMatchFound, result.GetUserName()), LogStrings.Heading_UserTransform);
            }
            else
            {
                LogInfo(string.Format(LogStrings.UserTransformSuccess, principalInput.GetUserName(), result), LogStrings.Heading_UserTransform);
            }
            return result;
        }

        public string RemapPrincipal(PnPContext context, IFieldUserValue userField)
        {
            if (userField == null) throw new ArgumentNullException(nameof(userField));
            // LookupValue is a display name, not a login name. Resolve by the source user ID.
            var user = context.Web.SiteUsers.GetById(userField.LookupId);
            return RemapPrincipal(user.LoginName);
        }

        internal string EnsureValidUserExists(string principal) =>
            CacheManager.Instance.GetEnsuredUser(targetContext, principal) != null ? principal : null;

        public string StripUserPrefixTokenAndDomain(string principal)
        {
            var value = principal.Split('|').Last();
            return value.Contains('\\') ? value.Split('\\')[1] : value;
        }
    }
}

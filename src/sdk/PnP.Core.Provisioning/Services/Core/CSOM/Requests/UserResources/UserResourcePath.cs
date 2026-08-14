using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Utils;
using System;
using System.Collections.Generic;

namespace PnP.Core.Provisioning.Services.Core.CSOM.Requests.UserResources
{
    /// <summary>
    /// Identifies one localizable text property on one SharePoint object, and knows how to persist
    /// a change to it.
    /// </summary>
    internal sealed class UserResourcePath
    {
        /// <summary>
        /// The CSOM identity of the object that owns the resource - see <see cref="CsomIdentity"/>.
        /// </summary>
        internal string ParentIdentity { get; }

        /// <summary>
        /// The name of the resource property on that object.
        /// </summary>
        internal string PropertyName { get; }

        /// <summary>
        /// The method that persists a staged change on the parent - <c>Update</c> or
        /// <c>UpdateAndPushChanges</c>.
        /// </summary>
        internal string ParentUpdateMethod { get; }

        /// <summary>
        /// Whether <see cref="ParentUpdateMethod"/> takes a boolean argument.
        /// </summary>
        internal bool ParentUpdateTakesFlag { get; }

        /// <summary>
        /// Emits the parent object path when it cannot be written as a plain identity.
        /// </summary>
        private readonly Func<IIdProvider, List<ActionObjectPath>, int> parentPathBuilder;

        internal UserResourcePath(string parentIdentity, string propertyName,
            string parentUpdateMethod = "Update", bool parentUpdateTakesFlag = false)
        {
            if (string.IsNullOrEmpty(parentIdentity))
            {
                throw new ArgumentException("A parent identity is required.", nameof(parentIdentity));
            }

            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("A resource property name is required.", nameof(propertyName));
            }

            ParentIdentity = parentIdentity;
            PropertyName = propertyName;
            ParentUpdateMethod = parentUpdateMethod;
            ParentUpdateTakesFlag = parentUpdateTakesFlag;
        }

        private UserResourcePath(Func<IIdProvider, List<ActionObjectPath>, int> parentPath, string propertyName,
            string parentUpdateMethod = "Update", bool parentUpdateTakesFlag = false)
        {
            parentPathBuilder = parentPath ?? throw new ArgumentNullException(nameof(parentPath));

            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("A resource property name is required.", nameof(propertyName));
            }

            PropertyName = propertyName;
            ParentUpdateMethod = parentUpdateMethod;
            ParentUpdateTakesFlag = parentUpdateTakesFlag;
        }

        /// <summary>
        /// Adds the object paths that reach the parent, and returns the id of the last one.
        /// </summary>
        internal int AppendParentPath(IIdProvider idProvider, List<ActionObjectPath> paths)
        {
            if (parentPathBuilder != null)
            {
                return parentPathBuilder(idProvider, paths);
            }

            int parentIdentityId = idProvider.GetActionId();
            paths.Add(new ActionObjectPath
            {
                ObjectPath = new Identity
                {
                    Id = parentIdentityId,
                    Name = ParentIdentity,
                },
            });

            return parentIdentityId;
        }

        /// <summary>A localizable property on a web. Persisted with <c>Web.Update()</c>.</summary>
        internal static UserResourcePath ForWeb(Guid siteId, Guid webId, string propertyName)
        {
            return new UserResourcePath(CsomIdentity.Web(siteId, webId), propertyName);
        }

        /// <summary>A localizable property on a list. Persisted with <c>List.Update()</c>.</summary>
        internal static UserResourcePath ForList(Guid siteId, Guid webId, Guid listId, string propertyName)
        {
            return new UserResourcePath(CsomIdentity.List(siteId, webId, listId), propertyName);
        }

        /// <summary>
        /// A localizable property on a site column. Persisted with
        /// <c>Field.UpdateAndPushChanges(true)</c> so list columns derived from it follow.
        /// </summary>
        internal static UserResourcePath ForField(Guid siteId, Guid webId, Guid fieldId, string propertyName)
        {
            return new UserResourcePath(CsomIdentity.Field(siteId, webId, fieldId), propertyName,
                "UpdateAndPushChanges", parentUpdateTakesFlag: true);
        }

        /// <summary>
        /// A localizable property on a <b>list</b> column. Persisted with
        /// <c>Field.UpdateAndPushChanges(true)</c>.
        /// </summary>
        internal static UserResourcePath ForListField(Guid siteId, Guid webId, Guid listId, Guid fieldId, string propertyName)
        {
            return new UserResourcePath(CsomIdentity.ListField(siteId, webId, listId, fieldId), propertyName,
                "UpdateAndPushChanges", parentUpdateTakesFlag: true);
        }

        /// <summary>
        /// A localizable property on a content type. Persisted with <c>ContentType.Update(true)</c>
        /// so child content types follow.
        /// </summary>
        internal static UserResourcePath ForContentType(Guid siteId, Guid webId, string contentTypeId, string propertyName)
        {
            return new UserResourcePath(CsomIdentity.ContentType(siteId, webId, contentTypeId), propertyName,
                "Update", parentUpdateTakesFlag: true);
        }

        /// <summary>
        /// A localizable property on a user custom action. Persisted with
        /// <c>UserCustomAction.Update()</c>.
        /// </summary>
        internal static UserResourcePath ForUserCustomAction(Guid siteId, Guid webId, Guid customActionId, string propertyName)
        {
            return ForCustomActionOn(CsomIdentity.Web(siteId, webId), customActionId, propertyName);
        }

        /// <summary>
        /// A localizable property on a <b>site collection</b> scoped user custom action. Persisted
        /// with <c>UserCustomAction.Update()</c>.
        /// </summary>
        internal static UserResourcePath ForSiteUserCustomAction(Guid siteId, Guid webId, Guid customActionId, string propertyName)
        {
            return ForCustomActionOn(CsomIdentity.Site(siteId, webId), customActionId, propertyName);
        }

        /// <summary>
        /// A localizable property on a <b>list</b> scoped user custom action. Persisted with
        /// <c>UserCustomAction.Update()</c>.
        /// </summary>
        internal static UserResourcePath ForListUserCustomAction(Guid siteId, Guid webId, Guid listId,
            Guid customActionId, string propertyName)
        {
            return ForCustomActionOn(CsomIdentity.List(siteId, webId, listId), customActionId, propertyName);
        }

        /// <summary>
        /// Walks <c>&lt;owner&gt;.UserCustomActions.GetById(id)</c>.
        /// </summary>
        private static UserResourcePath ForCustomActionOn(string ownerIdentity, Guid customActionId, string propertyName)
        {
            return new UserResourcePath((idProvider, paths) =>
            {
                int ownerId = idProvider.GetActionId();
                paths.Add(new ActionObjectPath
                {
                    ObjectPath = new Identity { Id = ownerId, Name = ownerIdentity },
                });

                int collectionId = idProvider.GetActionId();
                paths.Add(new ActionObjectPath
                {
                    ObjectPath = new Property { Id = collectionId, ParentId = ownerId, Name = "UserCustomActions" },
                });

                int actionId = idProvider.GetActionId();
                paths.Add(new ActionObjectPath
                {
                    ObjectPath = new ObjectPathMethod
                    {
                        Id = actionId,
                        ParentId = collectionId,
                        Name = "GetById",
                        Parameters = new MethodParameter
                        {
                            Properties = new List<Parameter>
                            {
                                new Parameter { Type = "Guid", Value = customActionId },
                            },
                        },
                    },
                });

                return actionId;
            }, propertyName);
        }
    }

    /// <summary>
    /// The names of the localizable resource properties, per object type.
    /// </summary>
    internal static class ResourceProperty
    {
        /// <summary>The title of a web, list, field, user custom action or navigation node.</summary>
        internal const string Title = "TitleResource";

        /// <summary>The description of a web, list, field, content type or user custom action.</summary>
        internal const string Description = "DescriptionResource";

        /// <summary>A content type's display name. Not <c>TitleResource</c>.</summary>
        internal const string ContentTypeName = "NameResource";
    }
}

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    [SharePointType("SP.Change", Target = typeof(Site), Get = "_api/site/getchanges")]
    [SharePointType("SP.Change", Target = typeof(Web), Get = "_api/web/getchanges")]
    [SharePointType("SP.Change", Target = typeof(List), Get = "_api/web/lists/getbyid(guid'{Id}')/getchanges")]
    [SharePointType("SP.Change", Target = typeof(Folder), Get = "_api/web/getFolderById('{Id}')/getchanges")]
    [SharePointType("SP.Change", Target = typeof(ListItem), Get = "_api/web/lists/getbyid(guid'{List.Id}')/items({Id})/getchanges")]
    internal class Change : TransientObject, IChange, IMetadataExtensible
    {
        public IChangeToken ChangeToken { get => GetValue<IChangeToken>(); set => SetValue(value); }

        public ChangeType ChangeType { get => GetValue<ChangeType>(); set => SetValue(value); }

        public Guid SiteId { get => GetValue<Guid>(); set => SetValue(value); }

        public DateTime Time { get => GetValue<DateTime>(); set => SetValue(value); }

        [SystemProperty]
        public Dictionary<string, string> Metadata { get; internal set; } = new Dictionary<string, string>();

        Task IMetadataExtensible.SetGraphToRestMetadataAsync()
        {
            return Task.CompletedTask;
        }

        Task IMetadataExtensible.SetRestToGraphMetadataAsync()
        {
            return Task.CompletedTask;
        }

        public bool IsPropertyAvailable<TModel>(Expression<Func<TModel, object>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var body = expression.Body as MemberExpression ?? ((UnaryExpression)expression.Body).Operand as MemberExpression;

            if (body.Expression is MemberExpression)
            {
                throw new ClientException(ErrorType.Unsupported, PnPCoreResources.Exception_PropertyNotLoaded_NestedProperties);
            }

            if (HasValue(body.Member.Name))
            {
                if (GetValue(body.Member.Name) is IRequestable)
                {
                    if ((GetValue(body.Member.Name) as IRequestable).Requested)
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }

            return false;
        }
    }
}

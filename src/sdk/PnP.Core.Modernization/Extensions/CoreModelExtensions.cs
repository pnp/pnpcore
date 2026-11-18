


using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace PnP.Core.Modernization.Extensions
{
    internal static class CoreModelExtensions
    {
        internal static TValue EnsureProperty<TModel, TValue>(this TModel model, Expression<Func<TModel, TValue>> property)
            where TModel : class, IDataModel<TModel>
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }
            var selector = Expression.Lambda<Func<TModel, object>>(
                Expression.Convert(property.Body, typeof(object)), property.Parameters);
            model.EnsureProperties(selector);
            return property.Compile()(model);
        }

        internal static IList GetListById(this IWeb web, Guid id, params Expression<Func<IList, object>>[] selectors) => web.Lists.GetById(id, selectors);

        internal static IList GetListByTitle(this IWeb web, string title) => web.Lists.GetByTitle(title);

        internal static IList GetList(this IWeb web, string serverRelativeUrl) =>
            web.Lists.GetByServerRelativeUrl(serverRelativeUrl);

        internal static IList GetListByUrl(this IWeb web, string serverRelativeUrl) => web.GetList(serverRelativeUrl);

        internal static IList GetSiteUserInfoList(this IWeb web)
        {
            web.EnsureProperties(w => w.SiteUserInfoList.QueryProperties(list => list.Id));
            return web.SiteUserInfoList;
        }

        internal static IField GetFieldByInternalName(this IWeb web, string name, bool searchAvailableFields = false) =>
            (searchAvailableFields ? web.AvailableFields : web.Fields).GetByInternalNameOrTitle(name);

        internal static IField GetFieldByInternalName(this IFieldCollection fields, string name) => fields.GetByInternalNameOrTitle(name);

        internal static IField GetByInternalNameOrTitle(this IFieldCollection fields, string name) =>
            fields.AsRequested().FirstOrDefault(f =>
                f.IsPropertyAvailable(p => p.InternalName) && f.InternalName == name ||
                f.IsPropertyAvailable(p => p.Title) && f.Title == name) ??
            fields.AsQueryable().FirstOrDefault(f => f.InternalName == name || f.Title == name);

        internal static object CopyFieldValue(this IField field, object value)
        {
            if (value is IFieldValueCollection collection)
            {
                return field.NewFieldValueCollection(collection.Values.Select(v => (IFieldValue)field.CopyFieldValue(v)));
            }
            if (value is IFieldUserValue user) return field.NewFieldUserValue(user.LookupId);
            if (value is IFieldLookupValue lookup) return field.NewFieldLookupValue(lookup.LookupId);
            if (value is IFieldTaxonomyValue term) return field.NewFieldTaxonomyValue(term.TermId, term.Label);
            if (value is IFieldUrlValue url) return field.NewFieldUrlValue(url.Url, url.Description);
            return value;
        }

        internal static ISharePointUser GetById(this ISharePointUserCollection users, int id) =>
            users.AsQueryable().FirstOrDefault(u => u.Id == id);

        internal static IContentType GetById(this IContentTypeCollection types, string id) =>
            types.AsQueryable().FirstOrDefault(t => t.Id == id);

        internal static string GetPropertyBagValueString(this IWeb web, string key, string defaultValue)
        {
            web.EnsureProperties(w => w.AllProperties);
            return web.AllProperties.Values.TryGetValue(key, out var value) ? value?.ToString() : defaultValue;
        }

        internal static bool PropertyBagContainsKey(this IWeb web, string key)
        {
            web.EnsureProperties(w => w.AllProperties);
            return web.AllProperties.Values.ContainsKey(key);
        }

        internal static string GetFileAsString(this IWeb web, string url)
        {
            using (var stream = web.GetFileByServerRelativeUrl(url).GetContent())
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        internal static IListItemCollection GetItems(this IList list, CamlQueryOptions query)
        {
            // Core collections are cumulative; keep this query's result isolated from prior CAML loads.
            list.Items.Clear();
            list.LoadItemsByCamlQuery(query);
            return list.Items;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    internal abstract class FieldValue : IFieldValue
    {
        private readonly Dictionary<string, object> current = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        //private readonly Dictionary<string, object> initial = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        //private readonly HashSet<string> changes = new HashSet<string>();

        internal FieldValue(string propertyName, TransientDictionary parent)
        {
            Parent = parent;
            PropertyName = propertyName;
        }

        internal string PropertyName { get; set; }

        internal TransientDictionary Parent { get; set; }

        //internal IEnumerable<string> ChangedProperties => changes;

        //internal bool HasChanges => changes.Count > 0;

        internal abstract string SharePointRestType { get; }

        internal abstract Guid CsomType { get; }

        internal bool IsArray { get; set; }

        public IField Field { get; set; }

        internal abstract IFieldValue FromJson(JsonElement json);

        internal abstract IFieldValue FromListDataAsStream(Dictionary<string, string> properties);

        internal abstract object ToJson();

        internal abstract string ToCsomXml();

        //internal virtual void Commit()
        //{
        //    Swap(initial, current);
        //}

        //private void Swap(IDictionary<string, object> a, IDictionary<string, object> b)
        //{
        //    a.Clear();
        //    foreach (var pair in b)
        //    {
        //        a[pair.Key] = pair.Value;
        //    }

        //    //changes.Clear();
        //}

        internal virtual T GetValue<T>([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            if (current.TryGetValue(propertyName, out object value))
            {
                return (T)value;
            }

            throw new ClientException(ErrorType.PropertyNotLoaded,
                string.Format(PnPCoreResources.Exception_PropertyNotLoaded, propertyName));
        }

        internal virtual bool SetValue<T>(T value, [System.Runtime.CompilerServices.CallerMemberName] string property = "")
        {
            if (current.TryGetValue(property, out object oldValue))
            {
                if (object.Equals(oldValue, value))
                {
                    return false;
                }
            }

            if (current.ContainsKey(property))
            {
                // We're changing this property
                //if (!initial.ContainsKey(property))
                //{
                //    initial[property] = value;
                //}
                current[property] = value;
                //changes.Add(property);
                Parent.MarkAsChanged(PropertyName);
            }
            else
            {
                // initial load
                current[property] = value;
            }

            return true;
        }

        internal bool HasValue([System.Runtime.CompilerServices.CallerMemberName] string property = "")
        {
            return current.ContainsKey(property);
        }

        ///// <summary>
        ///// Sets a property value without marking it as "changed"
        ///// </summary>
        ///// <typeparam name="T">Type of the property to set</typeparam>
        ///// <param name="value">Value to set</param>
        ///// <param name="property">Name of the property</param>
        //internal void SetSystemValue<T>(T value, [System.Runtime.CompilerServices.CallerMemberName] string property = "")
        //{
        //    if (current.TryGetValue(property, out object oldValue))
        //    {
        //        if (object.Equals(oldValue, value))
        //        {
        //            return;
        //        }
        //    }

        //    current[property] = value;
        //}
    }
}

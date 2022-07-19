using PnP.Core.Model.SharePoint.Core.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Base class for all field value classes
    /// </summary>
    public abstract class FieldValue : IFieldValue, ICSOMField
    {
        private readonly Dictionary<string, object> current = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> changes = new HashSet<string>();
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public FieldValue()
        {
            // Mark newly created FieldValue as changed to ensure it's picked up
            MarkAsChanged();
        }

        internal bool HasChanges => changes.Count > 0;

        internal abstract string SharePointRestType { get; }

        internal abstract Guid CsomType { get; }

        internal bool IsArray { get; set; }

        /// <summary>
        /// Field linked to this field value
        /// </summary>
        public IField Field { get; set; }

        Guid ICSOMField.CsomType => CsomType;

        internal abstract IFieldValue FromJson(JsonElement json);

        internal abstract IFieldValue FromListDataAsStream(Dictionary<string, string> properties);

        internal abstract object ToJson();

        internal abstract object ToValidateUpdateItemJson();

        internal abstract string ToCsomXml();

        internal virtual void Commit()
        {
            changes.Clear();
        }

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
                current[property] = value;
                changes.Add(property);
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

        internal void MarkAsChanged()
        {
            if (!changes.Any())
            {
                changes.Add("changed");
            }
        }

        string ICSOMField.ToCsomXml()
        {
            return ToCsomXml();
        }
    }
}

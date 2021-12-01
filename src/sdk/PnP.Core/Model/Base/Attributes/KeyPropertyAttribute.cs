using System;

namespace PnP.Core.Model
{
    /// <summary>
    /// Indicates that this property is a key property, specifying the keyfield name is required
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    internal sealed class KeyPropertyAttribute : Attribute
    {
        internal KeyPropertyAttribute(string keyPopertyName)
        {
            KeyPropertyName = keyPopertyName;
        }

        internal string KeyPropertyName { get; set; }
    }
}

using System;

namespace PnP.Core.Model
{
    /// <summary>
    /// Indicates that a property is not a model property but a system property
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    internal class SystemPropertyAttribute : Attribute
    {
    }
}

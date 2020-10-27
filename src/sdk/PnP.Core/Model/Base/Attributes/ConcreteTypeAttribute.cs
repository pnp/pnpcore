using System;

namespace PnP.Core.Model
{
    /// <summary>
    /// Attribute to declare the concrete type for this interface
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    internal class ConcreteTypeAttribute : Attribute
    {
        public ConcreteTypeAttribute(Type type)
        {
            Type = type;
        }

        /// <summary>
        /// The actual concrete type implementing the interface
        /// </summary>
        public Type Type { get; private set; }
    }
}

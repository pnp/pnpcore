using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Model.Base
{
    /// <summary>
    /// Attribute to declare the concrete type for this interface
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    internal class ConcreteTypeAttribute : Attribute
    {
        public ConcreteTypeAttribute(Type type)
        {
            this.Type = type;
        }

        /// <summary>
        /// The actual concrete type implementing the interface
        /// </summary>
        public Type Type { get; private set; }
    }
}

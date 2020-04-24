using System;
using System.Reflection;

namespace PnP.Core.Model
{
    /// <summary>
    /// Extensions to the Type class
    /// </summary>
    public static class TypeExtensions
    {

        /// <summary>
        /// Verify if a generic interface was implemented
        /// </summary>
        /// <param name="propertyType">Property to check on </param>
        /// <param name="interfaceType">Interface to check for</param>
        /// <returns>True if implemented, false otherwise</returns>
        public static bool ImplementsInterface(this Type propertyType, Type interfaceType)
        {
            if (propertyType == null)
            {
                throw new ArgumentNullException(nameof(propertyType));
            }

            if (interfaceType == null)
            {
                throw new ArgumentNullException(nameof(interfaceType));
            }

            TypeFilter myFilter = new TypeFilter(StartsWithInterfaceFilter);

            Type[] foundInterfaces = propertyType.FindInterfaces(myFilter, interfaceType.FullName);

            return foundInterfaces.Length > 0;
        }

        private static bool StartsWithInterfaceFilter(Type typeObj, Object criteriaObj)
        {
            return typeObj.FullName.StartsWith(criteriaObj.ToString());
        }
    }
}

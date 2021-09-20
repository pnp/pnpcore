using System;
using System.Linq;

namespace PnP.Core.Model
{
    /// <summary>
    /// Extensions to the Type class
    /// </summary>
    internal static class TypeExtensions
    {

        /// <summary>
        /// Verify if a generic interface was implemented
        /// </summary>
        /// <param name="propertyType">Property to check on </param>
        /// <param name="interfaceType">Interface to check for</param>
        /// <returns>True if implemented, false otherwise</returns>
        internal static bool ImplementsInterface(this Type propertyType, Type interfaceType)
        {
            if (propertyType == null)
            {
                throw new ArgumentNullException(nameof(propertyType));
            }

            if (interfaceType == null)
            {
                throw new ArgumentNullException(nameof(interfaceType));
            }

            if (interfaceType.IsGenericType && !interfaceType.IsConstructedGenericType)
            {
                return propertyType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType);
            }
            else
            {
                return interfaceType.IsAssignableFrom(propertyType);
            }
        }

        #region Old implementation replaced by above for performance. See PnP.Core.Perf for more context

        //internal static bool ImplementsInterface(this Type propertyType, Type interfaceType)
        //{
        //    if (propertyType == null)
        //    {
        //        throw new ArgumentNullException(nameof(propertyType));
        //    }

        //    if (interfaceType == null)
        //    {
        //        throw new ArgumentNullException(nameof(interfaceType));
        //    }

        //    TypeFilter myFilter = new TypeFilter(StartsWithInterfaceFilter);

        //    Type[] foundInterfaces = propertyType.FindInterfaces(myFilter, interfaceType.FullName);

        //    return foundInterfaces.Length > 0;
        //}

        //private static bool StartsWithInterfaceFilter(Type typeObj, Object criteriaObj)
        //{
        //    return typeObj.FullName.StartsWith(criteriaObj.ToString());
        //}
        #endregion
    }
}

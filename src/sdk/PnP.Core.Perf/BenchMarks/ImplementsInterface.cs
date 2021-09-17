using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Perf.BenchMarks
{
    public static class ImplementsInterface
    {
        /// <summary>
        /// New approach, way faster than the previous one + less memory pressure!!
        /// |                  Method |        Mean |     Error |    StdDev | Ratio |  Gen 0 | Allocated |
        /// |------------------------ |------------:|----------:|----------:|------:|-------:|----------:|
        /// | BaseImplementsInterface | 40,035.4 ns | 314.50 ns | 294.19 ns | 1.000 | 0.1221 |     913 B |
        /// |  NewImplementsInterface |    173.1 ns |   1.01 ns |   0.95 ns | 0.004 | 0.0598 |     376 B |
        /// </summary>
        internal static bool NewImplementsInterface(this Type propertyType, Type interfaceType)
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

        /// <summary>
        /// Combined approach test
        /// </summary>
        /// <param name="propertyType"></param>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        internal static bool ImplementsInterfaceViaQuery(this Type propertyType, Type interfaceType)
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

        /// <summary>
        /// Fastest approach!
        /// </summary>
        /// <param name="propertyType"></param>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        internal static bool ImplementsInterfaceViaIsAssignableFrom(this Type propertyType, Type interfaceType)
        {
            if (propertyType == null)
            {
                throw new ArgumentNullException(nameof(propertyType));
            }

            if (interfaceType == null)
            {
                throw new ArgumentNullException(nameof(interfaceType));
            }

            return interfaceType.IsAssignableFrom(propertyType);
        }

        /// <summary>
        /// Fast but fails on generic interfaceType
        /// </summary>
        /// <param name="propertyType"></param>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        internal static bool ImplementsInterfaceViaGetInterfaces(this Type propertyType, Type interfaceType)
        {
            if (propertyType == null)
            {
                throw new ArgumentNullException(nameof(propertyType));
            }

            if (interfaceType == null)
            {
                throw new ArgumentNullException(nameof(interfaceType));
            }

            return propertyType.GetInterfaces().Contains(interfaceType);
        }

        /// <summary>
        /// Slowest!
        /// </summary>
        /// <param name="propertyType"></param>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        internal static bool BaseImplementsInterface(this Type propertyType, Type interfaceType)
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

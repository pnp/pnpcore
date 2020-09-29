using PnP.Core;

namespace System.Reflection
{
    internal static class ReflectionExtensions
    {
        public static object GetValue(this MemberInfo member, object source)
        {
            switch (member)
            {
                case PropertyInfo pi:
                    return pi.GetValue(source);
                case FieldInfo fi:
                    return fi.GetValue(source);
            }

            throw new NotSupportedException(string.Format(PnPCoreResources.Exception_Unsupported_MemberType, member.GetType()));
        }
    }
}

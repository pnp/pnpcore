using PnP.Core;

namespace System.Reflection
{
    internal static class ReflectionExtensions
    {
        public static object GetValue(this MemberInfo member, object source)
        {
            return member switch
            {
                PropertyInfo pi => pi.GetValue(source),
                FieldInfo fi => fi.GetValue(source),
                _ => throw new NotSupportedException(string.Format(PnPCoreResources.Exception_Unsupported_MemberType, member.GetType())),
            };
        }
    }
}

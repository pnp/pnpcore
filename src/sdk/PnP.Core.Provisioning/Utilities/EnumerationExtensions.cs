namespace System
{
    /// <summary>
    /// Extension methods to make working with Enum values easier. Copied from http://hugoware.net/blog/enumeration-extensions-2-0.
    /// </summary>
    public static class EnumerationExtensions
    {

        #region Extension Methods

        /// <summary>
        /// Includes an enumerated type and returns the new value
        /// </summary>
        /// <typeparam name="T">Generic type</typeparam>
        /// <param name="value">Enum value</param>
        /// <param name="append">Generic type parameter</param>
        /// <returns>Returns new value</returns>
        public static T Include<T>(this Enum value, T append)
        {
            Type type = value.GetType();

            object result = value;
            _Value parsed = new _Value(append, type);
            if (parsed.Signed is long)
            {
                result = Convert.ToInt64(value) | (long)parsed.Signed;
            }
            else if (parsed.Unsigned is ulong)
            {
                result = Convert.ToUInt64(value) | (ulong)parsed.Unsigned;
            }

            return (T)Enum.Parse(type, result.ToString());
        }

        /// <summary>
        /// Removes an enumerated type and returns the new value
        /// </summary>
        /// <typeparam name="T">Generic type</typeparam>
        /// <param name="value">Enum value</param>
        /// <param name="remove">Generic type parameter</param>
        /// <returns>Returns new value</returns>
        public static T Remove<T>(this Enum value, T remove)
        {
            Type type = value.GetType();

            object result = value;
            _Value parsed = new _Value(remove, type);
            if (parsed.Signed is long)
            {
                result = Convert.ToInt64(value) & ~(long)parsed.Signed;
            }
            else if (parsed.Unsigned is ulong)
            {
                result = Convert.ToUInt64(value) & ~(ulong)parsed.Unsigned;
            }

            return (T)Enum.Parse(type, result.ToString());
        }

        /// <summary>
        /// Checks if an enumerated type contains a value
        /// </summary>
        /// <typeparam name="T">Generic type</typeparam>
        /// <param name="value">Enum value</param>
        /// <param name="check">Generic type parameter</param>
        /// <returns>Returns true if condition matches and enumerated type contains value</returns>
        public static bool Has<T>(this Enum value, T check)
        {
            Type type = value.GetType();

            object result = value;
            _Value parsed = new _Value(check, type);
            if (parsed.Signed is long)
            {
                return (Convert.ToInt64(value) & (long)parsed.Signed) == (long)parsed.Signed;
            }
            else if (parsed.Unsigned is ulong)
            {
                return (Convert.ToUInt64(value) & (ulong)parsed.Unsigned) == (ulong)parsed.Unsigned;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if an enumerated type is missing a value
        /// </summary>
        /// <typeparam name="T">Generic type</typeparam>
        /// <param name="obj">Enum object</param>
        /// <param name="value">Generic type parameter</param>
        /// <returns>Returns true if enumerated type is missing a value</returns>
        public static bool Missing<T>(this Enum obj, T value)
        {
            return !Has<T>(obj, value);
        }

        #endregion

        #region Helper Classes

        private class _Value
        {

            private static readonly Type _UInt64 = typeof(ulong);
            private static readonly Type _UInt32 = typeof(long);

            public readonly long? Signed;
            public readonly ulong? Unsigned;

            public _Value(object value, Type type)
            {

                if (!type.IsEnum)
                {
                    throw new ArgumentException("Value provided is not an enumerated type!");
                }

                Type compare = Enum.GetUnderlyingType(type);

                if (compare.Equals(_UInt32) || compare.Equals(_UInt64))
                {
                    Unsigned = Convert.ToUInt64(value);
                }
                else
                {
                    Signed = Convert.ToInt64(value);
                }

            }

        }

        #endregion

    }

}

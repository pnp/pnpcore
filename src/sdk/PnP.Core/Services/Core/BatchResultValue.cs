namespace PnP.Core.Services
{
    /// <summary>
    /// Wraps a reference or value type in an object
    /// </summary>
    /// <typeparam name="T">Type of the value/reference type to wrap</typeparam>
    public class BatchResultValue<T>
    {
        internal BatchResultValue(T value)
        {
            Value = value;
        }

        /// <summary>
        /// The wrapped reference/value type
        /// </summary>
        public T Value { get; internal set; }
    }
}

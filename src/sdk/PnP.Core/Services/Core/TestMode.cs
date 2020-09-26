#if DEBUG

namespace PnP.Core.Services
{
    /// <summary>
    /// Mode in which we operate
    /// </summary>
    public enum TestMode
    {
        /// <summary>
        /// We're not running in a test modus
        /// </summary>
        Default = 0,

        /// <summary>
        /// We're in test modus and want to record server responses
        /// </summary>
        Record = 1,

        /// <summary>
        /// We're in test modus and want to playback server responses
        /// </summary>
        Mock = 2
    }
}

#endif
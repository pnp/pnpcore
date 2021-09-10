using PnP.Core.Test.Common.Utilities;
using System;

namespace PnP.Core.Admin.Test.Utilities
{
    public sealed class TestCommon : TestCommonBase
    {
        private static readonly Lazy<TestCommon> _lazyInstance = new Lazy<TestCommon>(() => new TestCommon(), true);

        /// <summary>
        /// Gets the single TestCommon instance, singleton pattern
        /// </summary>
        internal static TestCommon Instance
        {
            get
            {
                return _lazyInstance.Value;
            }
        }

        /// <summary>
        /// Private constructor since this is a singleton
        /// </summary>
        private TestCommon()
        {

        }
    }
}

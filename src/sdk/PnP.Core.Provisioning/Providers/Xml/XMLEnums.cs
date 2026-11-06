using System;

namespace PnP.Core.Provisioning.Providers.Xml
{
    public enum XMLPnPSchemaVersion
    {
        LATEST = 0,

        [Obsolete("The PnP Provisioning Schema v201503 is obsolete and deprecated, please use the latest version available at https://github.com/pnp/PnP-Provisioning-Schema")]
        V201503 = 1,
        [Obsolete("The PnP Provisioning Schema v201505 is obsolete and deprecated, please use the latest version available at https://github.com/pnp/PnP-Provisioning-Schema")]
        V201505 = 2,
        [Obsolete("The PnP Provisioning Schema v201508 is obsolete and deprecated, please use the latest version available at https://github.com/pnp/PnP-Provisioning-Schema")]
        V201508 = 3,
        [Obsolete("The PnP Provisioning Schema v201512 is obsolete and deprecated, please use the latest version available at https://github.com/pnp/PnP-Provisioning-Schema")]
        V201512 = 4,
        [Obsolete("The PnP Provisioning Schema v201605 is obsolete and deprecated, please use the latest version available at https://github.com/pnp/PnP-Provisioning-Schema")]
        V201605 = 5,
        [Obsolete("The PnP Provisioning Schema v201705 is obsolete and deprecated, please use the latest version available at https://github.com/pnp/PnP-Provisioning-Schema")]
        V201705 = 6,
        [Obsolete("The PnP Provisioning Schema v201801 is obsolete and deprecated, please use the latest version available at https://github.com/pnp/PnP-Provisioning-Schema")]
        V201801 = 7,
        [Obsolete("The PnP Provisioning Schema v201805 is obsolete and deprecated, please use the latest version available at https://github.com/pnp/PnP-Provisioning-Schema")]
        V201805 = 8,
        [Obsolete("The PnP Provisioning Schema v201807 is obsolete and deprecated, please use the latest version available at https://github.com/pnp/PnP-Provisioning-Schema")]
        V201807 = 9,
        [Obsolete("The PnP Provisioning Schema v201903 is obsolete and deprecated, please use the latest version available at https://github.com/pnp/PnP-Provisioning-Schema")]
        V201903 = 10,

        V201909 = 11,
        V202002 = 12,
        V202103 = 13,
        V202209 = 14,
    }
}

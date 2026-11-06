using System;

namespace PnP.Core.Provisioning.Providers.Xml
{
    public static class XMLConstants
    {
        public const String PROVISIONING_SCHEMA_PREFIX = "pnp";

        [Obsolete("The PnP Provisioning Schema v201903 is obsolete and deprecated, please use the latest version available at https://github.com/pnp/PnP-Provisioning-Schema")] 
        public const String PROVISIONING_SCHEMA_NAMESPACE_2019_03 = "http://schemas.dev.office.com/PnP/2019/03/ProvisioningSchema";
        
        public const String PROVISIONING_SCHEMA_NAMESPACE_2019_09 = "http://schemas.dev.office.com/PnP/2019/09/ProvisioningSchema";
        public const String PROVISIONING_SCHEMA_NAMESPACE_2020_02 = "http://schemas.dev.office.com/PnP/2020/02/ProvisioningSchema";
        public const String PROVISIONING_SCHEMA_NAMESPACE_2021_03 = "http://schemas.dev.office.com/PnP/2021/03/ProvisioningSchema";
        public const String PROVISIONING_SCHEMA_NAMESPACE_2022_09 = "http://schemas.dev.office.com/PnP/2022/09/ProvisioningSchema";
    }
}


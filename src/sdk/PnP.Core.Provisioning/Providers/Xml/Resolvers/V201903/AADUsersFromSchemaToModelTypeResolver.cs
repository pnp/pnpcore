using PnP.Core.Provisioning.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using AAD = PnP.Core.Provisioning.Model.AzureActiveDirectory;

namespace PnP.Core.Provisioning.Providers.Xml.Resolvers
{
    /// <summary>
    /// Resolves the AAD Users from the Schema to the Model
    /// </summary>
    internal class AADUsersFromSchemaToModelTypeResolver : ITypeResolver
    {
        public string Name => this.GetType().Name;

        public bool CustomCollectionResolver => false;

        public object Resolve(object source, Dictionary<String, IResolver> resolvers = null, Boolean recursive = false)
        {
            var result = new List<AAD.User>();

            var users = source.GetPublicInstancePropertyValue("Users");

            if (null != users)
            {
                foreach (var u in ((IEnumerable)users))
                {
                    var targetItem = new AAD.User();
                    PnPObjectsMapper.MapProperties(u, targetItem, resolvers, recursive);
                    result.Add(targetItem);
                }
            }

            return (result);
        }
    }
}

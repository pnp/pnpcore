using System;
using System.Collections.Generic;

namespace PnP.Core.Model
{
    /// <summary>
    /// Holds information that describes how a fields is expanded. Is populated when .Include() is used 
    /// </summary>
    internal class EntityFieldExpandInfo
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public bool Expandable { get; set; }
        public List<EntityFieldExpandInfo> Fields { get; } = new List<EntityFieldExpandInfo>();

    }
}

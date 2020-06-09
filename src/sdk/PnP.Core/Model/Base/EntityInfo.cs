using System;
using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Model
{
    /// <summary>
    /// Class describing the underlying data store, used to map data store to model. This class contains the dynamic and the static part
    /// </summary>
    internal class EntityInfo
    {
        /// <summary>
        /// Default constructor        
        /// </summary>
        internal EntityInfo()
        {
        }

        /// <summary>
        /// Parameterized constructor        
        /// </summary>
        /// <param name="entityInfo">An instance of EntityInfo to construct the new EntityInfo from</param>
        internal EntityInfo(EntityInfo entityInfo)
        {
            entityInfo.SharePointTargets.ForEach(f => SharePointTargets.Add((EntitySharePointTypeInfo)f.Clone()));
            entityInfo.GraphTargets.ForEach(f => GraphTargets.Add((EntityGraphTypeInfo)f.Clone()));
            entityInfo.Fields.ForEach(f => Fields.Add((EntityFieldInfo)f.Clone()));

            ActualKeyFieldName = entityInfo.ActualKeyFieldName;
            UseOverflowField = entityInfo.UseOverflowField;
        }


        internal List<EntitySharePointTypeInfo> SharePointTargets { get; private set; } = new List<EntitySharePointTypeInfo>();

        internal List<EntityGraphTypeInfo> GraphTargets { get; private set; } = new List<EntityGraphTypeInfo>();

        /// <summary>
        /// Field mapping information
        /// </summary>
        internal List<EntityFieldInfo> Fields { get; private set; } = new List<EntityFieldInfo>();

        /// <summary>
        /// Name of the actual key field
        /// </summary>
        internal string ActualKeyFieldName;

        /// <summary>
        /// Data store type when using REST
        /// </summary>
        internal string SharePointType
        {
            get
            {
                return SharePoint?.Type;
            }
        }
        /// <summary>
        /// The actual target type for which this attribute is valid
        /// </summary>
        internal Type SharePointTarget
        {
            get
            {
                return SharePoint?.Target;
            }
        }

        /// <summary>
        /// The actual target type for which this attribute is valid
        /// </summary>
        internal Type GraphTarget
        {
            get
            {
                return Graph?.Target;
            }
        }

        /// <summary>
        /// Uri that uniquely identifies this item via REST
        /// </summary>
        internal string SharePointUri
        {
            get
            {
                return SharePoint?.Uri;
            }
        }

        /// <summary>
        /// Value of the id field used to load graph relationships (e.g. load lists from a given site)
        /// </summary>
        internal string GraphId
        {
            get
            {
                return Graph?.Id;
            }
        }

        /// <summary>
        /// Specifies if this class requires the Microsoft Graph beta endpoint
        /// </summary>
        internal bool GraphBeta
        {
            get
            {
                return Graph.Beta;
            }
        }

        /// <summary>
        /// API call for a Graph get
        /// </summary>
        internal string GraphGet
        {
            get
            {
                return Graph?.Get;
            }
        }

        /// <summary>
        /// API call for a REST get
        /// </summary>
        internal string SharePointGet
        {
            get
            {
                return SharePoint?.Get;
            }
        }

        /// <summary>
        /// API call for a Graph LINQ get
        /// </summary>
        internal string GraphLinqGet
        {
            get
            {
                return Graph?.LinqGet;
            }
        }

        /// <summary>
        /// API call for a REST LINQ get
        /// </summary>
        internal string SharePointLinqGet
        {
            get
            {
                return SharePoint?.LinqGet;
            }
        }


        /// <summary>
        /// Indicates if this class must be handled as a generic dictionary by populating data in the provided field
        /// </summary>
        internal bool UseOverflowField { get; set; }

        /// <summary>
        /// Indicates the property used for the overflow field when a REST query is used
        /// </summary>
        internal string SharePointOverflowProperty
        {
            get
            {
                return SharePoint?.OverflowProperty;
            }
        }

        /// <summary>
        /// Indicates the property used for the overflow field when a Graph query is used
        /// </summary>
        internal string GraphOverflowProperty
        {
            get
            {
                return Graph?.OverflowProperty;
            }
        }

        /// <summary>
        /// API call for a Graph update
        /// </summary>
        internal string GraphUpdate
        {
            get
            {
                return Graph?.Update;
            }
        }

        /// <summary>
        /// API call for a REST update
        /// </summary>
        internal string SharePointUpdate
        {
            get
            {
                return SharePoint?.Update;
            }
        }

        /// <summary>
        /// API call for a Graph delete
        /// </summary>
        internal string GraphDelete
        {
            get
            {
                return Graph?.Delete;
            }
        }

        /// <summary>
        /// API call for a REST delete
        /// </summary>
        internal string SharePointDelete
        {
            get
            {
                return SharePoint?.Delete;
            }
        }

        /// <summary>
        /// Target type for which the SharePoint or Graph entity type information is needed
        /// </summary>
        internal Type Target { get; set; }

        /// <summary>
        /// Indicates for what operation the identifier URI of the REST resource can be resolved from metadata when possible
        /// Default is All
        /// </summary>
        internal ResolveUriFromMetadataFor ResolveSharePointUriFromMetadataFor
        {
            get
            {
                return SharePoint.ResolveUriFromMetadataFor;
            }
        }

        private EntitySharePointTypeInfo SharePoint
        {
            get
            {
                if (SharePointTargets.Count == 1)
                {
                    return SharePointTargets.First();
                }
                else if (SharePointTargets.Count > 1 && Target != null)
                {
                    var target = SharePointTargets.FirstOrDefault(p => p.Target == Target);
                    if (target != null)
                    {
                        return target;
                    }
                }

                return null;
            }
        }

        private EntityGraphTypeInfo Graph
        {
            get
            {
                if (GraphTargets.Count == 1)
                {
                    return GraphTargets.First();
                }
                else if (GraphTargets.Count > 1 && Target != null)
                {
                    var target = GraphTargets.FirstOrDefault(p => p.Target == Target);
                    if (target != null)
                    {
                        return target;
                    }
                }

                return null;
            }
        }

        private EntityFieldInfo _sharePointKeyField;

        /// <summary>
        /// Gets the first field marked as IsKey
        /// </summary>
        internal EntityFieldInfo SharePointKeyField
        {
            get
            {
                if (_sharePointKeyField == null)
                {
                    _sharePointKeyField = Fields.FirstOrDefault(f => f.IsSharePointKey);
                }
                return _sharePointKeyField;
            }
        }

        private EntityFieldInfo _graphKeyField;

        /// <summary>
        /// Gets the first field marked as IsKey
        /// </summary>
        internal EntityFieldInfo GraphKeyField
        {
            get
            {
                if (_graphKeyField == null)
                {
                    _graphKeyField = Fields.FirstOrDefault(f => f.IsGraphKey);
                }
                return _graphKeyField;
            }
        }

        private List<EntityFieldInfo> _graphNonExpandableCollections;

        /// <summary>
        /// Returns a list of fields in this entity which do require a separate query (they can't be expanded)
        /// </summary>
        internal List<EntityFieldInfo> GraphNonExpandableCollections
        {
            get
            {
                if (_graphNonExpandableCollections == null)
                {
                    _graphNonExpandableCollections =
                        Fields.Where(f => !string.IsNullOrEmpty(f.GraphGet)).ToList();
                }
                return _graphNonExpandableCollections;
            }
        }

        /// <summary>
        /// Was there an expression provided to build up the fields lists of this entity
        /// </summary>
        internal bool SharePointFieldsLoadedViaExpression { get; set; } = false;

        /// <summary>
        /// Was there an expression provided to build up the fields lists of this entity
        /// </summary>
        internal bool GraphFieldsLoadedViaExpression { get; set; } = false;

        /// <summary>
        /// Check if Microsoft Graph can be used based upon the requested fields to load
        /// </summary>
        internal bool CanUseGraphGet
        {
            get
            {
                // in case there was no expression used and the entity allows to be fetched via Graph than favor graph
                if (!GraphFieldsLoadedViaExpression && (GraphTargets.Any() && !string.IsNullOrEmpty(GraphTargets.First().Get)))
                {
                    return true;
                }

                // get collection of fields that need to be loaded
                return Fields
                    .Where(f => f.Load)
                    .All(f => !string.IsNullOrEmpty(f.GraphName));
            }
        }        
    }
}
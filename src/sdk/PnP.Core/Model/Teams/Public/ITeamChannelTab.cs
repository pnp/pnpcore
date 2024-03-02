using System;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Defines the tab in a Team channel
    /// </summary>
    [ConcreteType(typeof(TeamChannelTab))]
    public interface ITeamChannelTab : IDataModel<ITeamChannelTab>, IDataModelGet<ITeamChannelTab>, IDataModelLoad<ITeamChannelTab>, IDataModelDelete, IDataModelUpdate, IQueryableDataModel
    {
        /// <summary>
        /// Identifier that uniquely identifies a specific instance of a channel tab. Read only.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Name of the tab.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Index of the order used for sorting tabs.
        /// </summary>
        public string SortOrderIndex { get; set; }

        /// <summary>
        /// Deep link url of the tab instance. Read only.
        /// </summary>
        public Uri WebUrl { get; }

        /// <summary>
        /// Container for custom settings applied to a tab. The tab is considered configured only once this property is set.
        /// </summary>
        public ITeamChannelTabConfiguration Configuration { get; }

        /// <summary>
        /// The application that is linked to the tab. This cannot be changed after tab creation.
        /// </summary>
        public ITeamApp TeamsApp { get; }

    }
}

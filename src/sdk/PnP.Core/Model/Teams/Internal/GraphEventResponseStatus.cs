using System;

namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal sealed class GraphEventResponseStatus : BaseDataModel<IGraphEventResponseStatus>, IGraphEventResponseStatus
    {
        #region Construction
        public GraphEventResponseStatus()
        {
        }
        #endregion

        #region Properties
        public EventResponse Response { get => GetValue<EventResponse>(); set => SetValue(value); }

        public DateTimeOffset Time { get => GetValue<DateTimeOffset>(); set => SetValue(value); }        
        #endregion
    }
}

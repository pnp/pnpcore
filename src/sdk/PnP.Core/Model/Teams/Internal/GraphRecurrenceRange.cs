using System;

namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal sealed class GraphRecurrenceRange : BaseDataModel<IGraphRecurrenceRange>, IGraphRecurrenceRange
    {
        #region Construction
        public GraphRecurrenceRange()
        {
        }
        #endregion

        #region Properties

        public DateTime StartDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public DateTime EndDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public int NumberOfOccurrences { get => GetValue<int>(); set => SetValue(value); }

        public string RecurrenceTimeZone { get => GetValue<string>(); set => SetValue(value); }

        public EventRecurrenceRangeType Type { get => GetValue<EventRecurrenceRangeType>(); set => SetValue(value); }

        #endregion
    }
}

using System;
using System.Collections.Generic;

namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal sealed class GraphRecurrencePattern : BaseDataModel<IGraphRecurrencePattern>, IGraphRecurrencePattern
    {
        #region Construction
        public GraphRecurrencePattern()
        {
        }
        #endregion

        #region Properties

        public int DayOfMonth { get => GetValue<int>(); set => SetValue(value); }

        public List<string> DaysOfWeek { get => GetValue<List<string>>(); set => SetValue(value); }

        public string FirstDayOfWeek { get => GetValue<string>(); set => SetValue(value); }

        public EventWeekIndex Index { get => GetValue<EventWeekIndex>(); set => SetValue(value); }

        public int Interval { get => GetValue<int>(); set => SetValue(value); }

        public int Month { get => GetValue<int>(); set => SetValue(value); }

        public EventRecurrenceType Type { get => GetValue<EventRecurrenceType>(); set => SetValue(value); }
        #endregion
    }
}

namespace PnP.Core.Auth.ACS.OAuth
{
    [System.Diagnostics.DebuggerNonUserCode]
    public static class DateTimeUtil
    {
        public static System.DateTime Add(System.DateTime time, System.TimeSpan timespan)
        {
            if (timespan == System.TimeSpan.Zero)
            {
                return time;
            }
            if (timespan > System.TimeSpan.Zero && System.DateTime.MaxValue - time <= timespan)
            {
                return DateTimeUtil.GetMaxValue(time.Kind);
            }
            if (timespan < System.TimeSpan.Zero && System.DateTime.MinValue - time >= timespan)
            {
                return DateTimeUtil.GetMinValue(time.Kind);
            }
            return time + timespan;
        }

        public static System.DateTime AddNonNegative(System.DateTime time, System.TimeSpan timeSpan)
        {
            if (timeSpan < System.TimeSpan.Zero)
            {
                throw new System.ArgumentException("TimeSpan must be greater than or equal to TimeSpan.Zero.", "timeSpan");
            }
            return DateTimeUtil.Add(time, timeSpan);
        }

        public static System.DateTime GetMaxValue(System.DateTimeKind kind)
        {
            return new System.DateTime(System.DateTime.MaxValue.Ticks, kind);
        }

        public static System.DateTime GetMinValue(System.DateTimeKind kind)
        {
            return new System.DateTime(System.DateTime.MinValue.Ticks, kind);
        }
    }
}

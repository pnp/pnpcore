namespace PnP.Core.Auth.ACS.OAuth
{
    public class EpochTime
    {
        public static readonly System.DateTime UnixEpoch =
            new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);

        private readonly long _secondsSinceUnixEpoch;

        public long SecondsSinceUnixEpoch
        {
            get
            {
                return this._secondsSinceUnixEpoch;
            }
        }

        public System.DateTime DateTime
        {
            get
            {
                System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(_secondsSinceUnixEpoch);
                return DateTimeUtil.AddNonNegative(EpochTime.UnixEpoch, timeSpan);
            }
        }

        public EpochTime(string secondsSinceUnixEpochString)
        {
            long secondsSinceUnixEpoch;
            if (!long.TryParse(secondsSinceUnixEpochString, out secondsSinceUnixEpoch))
            {
                throw new System.ArgumentException(nameof(secondsSinceUnixEpochString),"Invalid date time string format.");
            }
            this._secondsSinceUnixEpoch = secondsSinceUnixEpoch;
        }


        public EpochTime(System.DateTime dateTime)
        {
            if (dateTime < EpochTime.UnixEpoch)
            {
                string message = string.Format(System.Globalization.CultureInfo.InvariantCulture, "DateTime must be greater than or equal to {0}", new object[]
                {
                    EpochTime.UnixEpoch.ToString()
                });
                throw new System.ArgumentOutOfRangeException(nameof(dateTime),message);
            }
            this._secondsSinceUnixEpoch = (long)(dateTime - EpochTime.UnixEpoch).TotalSeconds;
        }
    }
}

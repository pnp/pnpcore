using System;

namespace PnP.Core.Services.Core.CSOM.Utils.DateHelpers
{
    internal sealed class FromMilisecondsConversionStrategy : IDateConversionStrategy
    {
        /// <summary>
        /// JS reference date is 1/1/1970, however .NET reference date is 1/1/0001
        /// </summary>
        internal DateTime ReferenceDate { get; set; } = new DateTime(1970, 1, 1);

        public DateTime? ConverDate(string dateValue)
        {
            DateTime? result = null;

            if (long.TryParse(dateValue.Replace("/Date(", "").Replace(")/", ""), out long numberOfMiliseconds))
            {
                TimeSpan timeSpanToAdd = TimeSpan.FromMilliseconds(numberOfMiliseconds);
                return ReferenceDate + timeSpanToAdd;
            }
            return result;
        }
    }
}

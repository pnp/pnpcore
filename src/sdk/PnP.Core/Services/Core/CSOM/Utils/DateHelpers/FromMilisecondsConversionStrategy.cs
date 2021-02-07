using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Services.Core.CSOM.Utils.DateHelpers
{
    class FromMilisecondsConversionStrategy : IDateConversionStrategy
    {
        /// <summary>
        /// JS reference date is 1/1/1970, however .NET reference date is 1/1/0001
        /// </summary>
        public DateTime ReferenceDate { get; set; } = new DateTime(1970, 1, 1);
        public DateTime? ConverDate(string dateValue)
        {
            DateTime? result = null;
            Int64 numberOfMiliseconds;
            if(Int64.TryParse(dateValue.Replace("/Date(", "").Replace(")/", ""), out numberOfMiliseconds))
            {
                TimeSpan timeSpanToAdd = TimeSpan.FromMilliseconds(numberOfMiliseconds);
                return ReferenceDate + timeSpanToAdd;
            }
            return result;
        }
    }
}

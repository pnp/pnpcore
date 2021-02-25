using System;

namespace PnP.Core.Services.Core.CSOM.Utils.DateHelpers
{
    internal class DateConstuctorStrategy : IDateConversionStrategy
    {
        public DateTime? ConverDate(string dateValue)
        {
            DateTime? result = null;
            string[] constructorValues = dateValue.Replace("/Date(", "").Replace(")/", "").Split(',');
            if (constructorValues.Length == 7)
            {
                result = new DateTime(
                    Convert.ToInt32(constructorValues[0]),
                    Convert.ToInt32(constructorValues[1]),
                    Convert.ToInt32(constructorValues[2]),
                    Convert.ToInt32(constructorValues[3]),
                    Convert.ToInt32(constructorValues[4]),
                    Convert.ToInt32(constructorValues[5]),
                    Convert.ToInt32(constructorValues[6]));
            }
            return result;
        }
    }
}

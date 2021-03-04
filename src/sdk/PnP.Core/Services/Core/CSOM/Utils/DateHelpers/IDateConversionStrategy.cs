using System;

namespace PnP.Core.Services.Core.CSOM.Utils.DateHelpers
{
    internal interface IDateConversionStrategy
    {
        DateTime? ConverDate(string dateValue);
    }
}

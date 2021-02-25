using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Services.Core.CSOM.Utils.DateHelpers
{
    interface IDateConversionStrategy
    {
        DateTime? ConverDate(string dateValue);
    }
}

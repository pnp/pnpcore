using System;

namespace PnP.Core.Services.Core.CSOM.Utils.DateHelpers
{
    internal sealed class FromDateTimeStrategy : IDateConversionStrategy
    {
        internal static readonly long MinDateTimeTicks = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;

        /// <summary>
        /// CSOM Date format: https://docs.microsoft.com/en-us/openspecs/sharepoint_protocols/ms-csom/2edd9d2a-2706-4703-9d27-81d1d7aca699
        /// Implementation is aligned with what is used inside the CSOM code base
        /// </summary>
        /// <param name="dateValue">CSOM string date format to parse</param>
        /// <returns></returns>
        public DateTime? ConverDate(string dateValue)
        {
            DateTime? result = null;

            long[] m_dateTimeComponents = new long[7];
            char[] m_dateTimeSigns = new char[7];
            char[] m_dateTimeHhmm = new char[4];

            if (string.IsNullOrEmpty(dateValue))
            {
                return result;
            }

            dateValue = dateValue.Replace("/Date(", "").Replace(")/", "");

            int part;

            // the + or - of for the HHMM
            char hhmmSign = '\0';
            // whether the current position is in the HHMM
            bool inHHMM = false;

            // whether the current position is in the number
            bool inNumber = false;

            // zero the buffer
            for (part = 0; part < m_dateTimeComponents.Length; part++)
            {
                m_dateTimeComponents[part] = 0;
                m_dateTimeSigns[part] = '\0';
            }
            
            // the index in the HHMM
            int hhmmIndex;
            for (hhmmIndex = 0; hhmmIndex < m_dateTimeHhmm.Length; hhmmIndex++)
            {
                m_dateTimeHhmm[hhmmIndex] = '\0';
            }

            // set initial value
            part = 0;
            hhmmIndex = 0;

            for (int i = 0; i < dateValue.Length; i++)
            {
                switch (dateValue[i])
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        if (inHHMM)
                        {
                            if (hhmmIndex < m_dateTimeHhmm.Length)
                            {
                                m_dateTimeHhmm[hhmmIndex] = dateValue[i];
                                hhmmIndex++;
                            }
                            else
                            {
                                throw new FormatException(string.Format(PnPCoreResources.Exception_CSOM_MalformedDateTime, dateValue));
                            }
                        }
                        else if (!inNumber)
                        {
                            inNumber = true;
                        }
                        if (inNumber)
                        {
                            m_dateTimeComponents[part] = m_dateTimeComponents[part] * 10 + (dateValue[i] - '0');
                        }
                        break;
                    case '-':
                    case '+':
                        if (inNumber)
                        {
                            if (part == 0)
                            {
                                hhmmSign = dateValue[i];
                                inHHMM = true;
                                inNumber = false;
                            }
                            else
                            {
                                throw new FormatException(string.Format(PnPCoreResources.Exception_CSOM_MalformedDateTime, dateValue));
                            }
                        }
                        else
                        {
                            // it's the begining of the number
                            if (m_dateTimeSigns[part] == '\0')
                            {
                                m_dateTimeSigns[part] = dateValue[i];
                            }
                            else
                            {
                                throw new FormatException(string.Format(PnPCoreResources.Exception_CSOM_MalformedDateTime, dateValue));
                            }
                        }
                        break;
                    case ',':
                        inNumber = false;
                        inHHMM = false;
                        part++;
                        if (part >= m_dateTimeComponents.Length)
                        {
                            throw new FormatException(string.Format(PnPCoreResources.Exception_CSOM_MalformedDateTime, dateValue));
                        }
                        break;
                    case ' ':
                        inNumber = false;
                        inHHMM = false;
                        break;                    
                    default:
                        throw new FormatException(string.Format(PnPCoreResources.Exception_CSOM_MalformedDateTime, dateValue));

                }
            }

            for (int index = 0; index < m_dateTimeComponents.Length; index++)
            {
                if (m_dateTimeSigns[index] == '-')
                {
                    m_dateTimeComponents[index] = -m_dateTimeComponents[index];
                }
            }
            if (part == 0)
            {
                long ticks = m_dateTimeComponents[0];

                result = new DateTime(ticks * 10000 + MinDateTimeTicks, DateTimeKind.Utc);

                if (hhmmSign != '\0')
                {
                    // convert it to local time.
                    result = result.Value.ToLocalTime();
                }
            }
            else
            {
                result = new DateTime(
                    (int)m_dateTimeComponents[0] /*year*/,
                    (int)m_dateTimeComponents[1] + 1 /*month*/,
                    (int)m_dateTimeComponents[2] /*day*/,
                    (int)m_dateTimeComponents[3] /*hour*/,
                    (int)m_dateTimeComponents[4] /*minute*/,
                    (int)m_dateTimeComponents[5] /*second*/,
                    (int)m_dateTimeComponents[6] /*milliseconds*/,
                    DateTimeKind.Unspecified);
            }

            return result;
        }
    }
}

using System;
using System.Collections.Generic;

namespace PnP.Core.Transformation.SharePoint.KQL
{
    /// <summary>
    /// Class to parse KQL queries
    /// </summary>
    public class KQLParser
    {
        /// <summary>
        /// Parses a KQL query and returns a list of tokens
        /// </summary>
        /// <param name="searchQuery">Query to parse</param>
        /// <returns>List of <see cref="KQLElement"/> tokens</returns>
        public List<KQLElement> Parse(string searchQuery)
        {
            // See https://docs.microsoft.com/en-us/sharepoint/dev/general-development/keyword-query-language-kql-syntax-reference
            List<KQLElement> parsedQuery = new List<KQLElement>(10);

            try
            {
                if (string.IsNullOrEmpty(searchQuery))
                {
                    return parsedQuery;
                }

                var parts = searchQuery.Trim().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                int group = 0;
                bool groupOpen = false;
                bool groupToClose = false;
                for (int i = 0; i < parts.Length; i++)
                {
                    var part = parts[i];

                    if (!groupOpen && part.StartsWith("("))
                    {
                        group = group + 1;
                        groupOpen = true;
                        // Strip the (
                        part = part.Substring(1);
                    }

                    if (groupOpen && part.EndsWith(")"))
                    {
                        groupToClose = true;

                        // Strip the )
                        part = part.TrimEnd(new char[] { ')' });
                    }

                    KQLPropertyOperator op = DetectOperator(part);

                    if (op != KQLPropertyOperator.Undefined)
                    {
                        string opAsString = OperatorToString(op);
                        KQLElement k = new KQLElement()
                        {
                            Type = KQLFilterType.PropertyFilter,
                            Operator = op,
                            Group = groupOpen ? group : 0,
                            Filter = part.Substring(0, part.IndexOf(opAsString)),
                            Value = part.Substring(part.IndexOf(opAsString) + opAsString.Length)
                        };
                        parsedQuery.Add(k);
                    }
                    else if (part.StartsWith("{") && part.EndsWith("}"))
                    {
                        KQLElement k = new KQLElement()
                        {
                            Type = KQLFilterType.KeywordFilter,
                            Operator = KQLPropertyOperator.Undefined,
                            Group = groupOpen ? group : 0,
                            Filter = part.Replace("{", "").Replace("}", ""),
                            Value = "",
                        };
                        parsedQuery.Add(k);
                    }
                    else
                    {
                        if (part.Equals("OR", StringComparison.InvariantCultureIgnoreCase) ||
                            part.Equals("AND", StringComparison.InvariantCultureIgnoreCase) ||
                            part.Equals("NEAR", StringComparison.InvariantCultureIgnoreCase) ||
                            part.StartsWith("NEAR(", StringComparison.InvariantCultureIgnoreCase) ||
                            part.Equals("ONENEAR", StringComparison.InvariantCultureIgnoreCase) ||
                            part.StartsWith("ONENEAR(", StringComparison.InvariantCultureIgnoreCase) ||
                            part.StartsWith("XRANK(", StringComparison.InvariantCultureIgnoreCase) ||
                            part.Equals("NOT", StringComparison.InvariantCultureIgnoreCase))
                        {
                            // Don't add operators as KQL elements
                        }
                        else
                        {
                            KQLElement k = new KQLElement()
                            {
                                Type = KQLFilterType.Text,
                                Operator = KQLPropertyOperator.Undefined,
                                Group = groupOpen ? group : 0,
                                Filter = "",
                                Value = part,
                            };
                            parsedQuery.Add(k);
                        }
                    }

                    if (groupToClose)
                    {
                        groupOpen = false;
                        groupToClose = false;
                    }

                }
            }
            catch(Exception)
            {
                // Eat exceptions for now, KQL query parsing will be obsolete when the AdvancedQuery option becomes available
            }

            return parsedQuery;
        }

        #region Helper methods
        private string OperatorToString(KQLPropertyOperator ops)
        {
            switch (ops)
            {
                case KQLPropertyOperator.DoesNoEqual: return "<>";
                case KQLPropertyOperator.EqualTo: return "=";
                case KQLPropertyOperator.GreaterThanOrEqualTo: return ">=";
                case KQLPropertyOperator.GreaterThan: return ">";
                case KQLPropertyOperator.LesserThan: return "<";
                case KQLPropertyOperator.LesserThanOrEqualTo: return "<=";
                case KQLPropertyOperator.Matches: return ":";
                case KQLPropertyOperator.Restriction: return "..";
            }

            return "";
        }

        private KQLPropertyOperator DetectOperator(string part)
        {
            if (part.Contains(">="))
            {
                return KQLPropertyOperator.GreaterThanOrEqualTo;
            }
            else if (part.Contains("<="))
            {
                return KQLPropertyOperator.LesserThanOrEqualTo;
            }
            else if (part.Contains("<>"))
            {
                return KQLPropertyOperator.DoesNoEqual;
            }
            else if (part.Contains(".."))
            {
                return KQLPropertyOperator.Restriction;
            }
            else if (part.Contains(">"))
            {
                return KQLPropertyOperator.GreaterThan;
            }
            else if (part.Contains("<"))
            {
                return KQLPropertyOperator.LesserThan;
            }
            else if (part.Contains("="))
            {
                return KQLPropertyOperator.EqualTo;
            }
            else if (part.Contains(":"))
            {
                return KQLPropertyOperator.Matches;
            }
            else
            {
                return KQLPropertyOperator.Undefined;
            }
        }
        #endregion
    }
}

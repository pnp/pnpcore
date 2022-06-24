using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace PnP.Core.Model.SharePoint
{
    internal static class SearchConfigurationHandler
    {
        internal static List<IManagedProperty> GetManagedPropertiesFromConfigurationXml(string configXml)
        {
            using (StringReader sr = new StringReader(configXml))
            {
                var doc = XDocument.Load(sr);
                var mps = GetCustomManagedProperties(doc);

                foreach (ManagedProperty mp in mps)
                {
                    mp.Aliases = new List<string>();
                    mp.Mappings = new List<string>();

                    var mappings = GetCpMappingsFromPid(doc, mp.Pid);
                    mp.Mappings = mappings;
                    var aliases = GetAliasesFromPid(doc, mp.Pid);
                    mp.Aliases = aliases;
                }

                return mps;
            }
        }

        private static string PidToName(string pid)
        {
            /*
            RefinableString00 1000000000
            Int00             1000000100
            Date00            1000000200
            Decimal00         1000000300
            Double00          1000000400
            RefinableInt00    1000000500
            RefinableDate00   1000000600
            RefinableDateSingle00 1000000650
            RefinableDateInvariant00  1000000660
            RefinableDecimal00    1000000700
            RefinableDouble00     1000000800
            RefinableString100  1000000900                
            */

            if (!int.TryParse(pid, out int p))
            {
                return pid;
            }

            if (p < 1000000000)
            {
                return pid;
            }

            var autoMpNum = pid.Substring(pid.Length - 2);
            var mpName = pid;

            if (p < 1000000100) mpName = "RefinableString";
            else if (p < 1000000200) mpName = "Int";
            else if (p < 1000000300) mpName = "Date";
            else if (p < 1000000400) mpName = "Decimal";
            else if (p < 1000000500) mpName = "Double";
            else if (p < 1000000600) mpName = "RefinableInt";
            else if (p < 1000000650) mpName = "RefinableDate";
            else if (p < 1000000660) mpName = "RefinableDateSingle";
            else if (p < 1000000700) mpName = "RefinableDateInvariant";
            else if (p < 1000000800) mpName = "RefinableDecimal";
            else if (p < 1000000900) mpName = "RefinableDouble";
            else if (p < 1000001000) mpName = "RefinableString1";

            return mpName + autoMpNum;
        }

        private static List<IManagedProperty> GetCustomManagedProperties(XDocument doc)
        {
            var mpList = new List<IManagedProperty>();
            var mps = doc.Descendants().Where(n => n.Name.LocalName.StartsWith("KeyValueOfstringManagedPropertyInfo"));

            foreach (var mpNode in mps)
            {
                var name = mpNode.Descendants().Single(n => n.Name.LocalName == "Name").Value;
                var pid = mpNode.Descendants().Single(n => n.Name.LocalName == "Pid").Value;
                var type = mpNode.Descendants().Single(n => n.Name.LocalName == "ManagedType").Value;
                var mp = new ManagedProperty
                {
                    Name = PidToName(name),
                    Pid = pid,
                    Type = type
                };
                mpList.Add(mp);
            }

            var overrides = doc.Descendants().Where(n => n.Name.LocalName.StartsWith("KeyValueOfstringOverrideInfo"));

            foreach (var o in overrides)
            {
                var name = o.Descendants().Single(n => n.Name.LocalName == "Name").Value;
                var pid = o.Descendants().Single(n => n.Name.LocalName == "ManagedPid").Value;
                var mp = new ManagedProperty
                {
                    Name = PidToName(name),
                    Pid = pid
                };
                if (mp.Name.Contains("String")) mp.Type = "Text";
                else if (mp.Name.Contains("Date")) mp.Type = "Date";
                else if (mp.Name.Contains("Int")) mp.Type = "Integer";
                else if (mp.Name.Contains("Double")) mp.Type = "Double";
                else if (mp.Name.Contains("Decimal")) mp.Type = "Decimal";
                mpList.Add(mp);
            }

            return mpList;
        }

        private static List<string> GetAliasesFromPid(XDocument doc, string pid)
        {
            var aliasList = new List<string>();
            var aliases = doc.Descendants().Where(n => n.Name.LocalName.StartsWith("KeyValueOfstringAliasInfo"));

            foreach (var alias in aliases)
            {
                if (alias.Descendants().Single(n => n.Name.LocalName == "ManagedPid").Value == pid)
                {
                    var aliasName = alias.Descendants().Single(n => n.Name.LocalName == "Name").Value;
                    aliasList.Add(aliasName);
                }
            }

            return aliasList;
        }

        private static List<string> GetCpMappingsFromPid(XDocument doc, string pid)
        {
            var mappingList = new List<string>();
            var cps = doc.Descendants().Where(n => n.Name.LocalName.StartsWith("KeyValueOfstringMappingInfo"));

            foreach (var cp in cps)
            {
                if (cp.Descendants().Single(n => n.Name.LocalName == "ManagedPid").Value == pid)
                {
                    var cpName = cp.Descendants().Single(n => n.Name.LocalName == "CrawledPropertyName").Value;
                    mappingList.Add(cpName);
                }
            }
            
            return mappingList;
        }
    }
}

using PnP.Core.Model.SharePoint;
using System;
using System.Collections.Generic;
using PnP.Core.Provisioning.Model;

namespace PnP.Core.Provisioning.ObjectHandlers.Utilities
{
    /// <summary>
    /// Translates between the template's <see cref="WebPartType"/> and PnP Core's
    /// <see cref="DefaultWebPart"/>.
    /// </summary>
    internal static class DefaultWebPartMap
    {
        private static readonly Dictionary<WebPartType, DefaultWebPart> ToDefaultWebPart = BuildToDefaultWebPart();

        private static readonly Dictionary<DefaultWebPart, WebPartType> ToWebPartType = BuildToWebPartType();

        private static Dictionary<WebPartType, DefaultWebPart> BuildToDefaultWebPart()
        {
            var map = new Dictionary<WebPartType, DefaultWebPart>();

            foreach (WebPartType webPartType in Enum.GetValues(typeof(WebPartType)))
            {
                // Text is a text control, not a web part - it has no DefaultWebPart counterpart.
                // Custom means "a third party web part identified by name or id", which the handler
                // resolves against the site's installed components rather than through this map.
                if (webPartType == WebPartType.Text || webPartType == WebPartType.Custom)
                {
                    continue;
                }

                if (Enum.TryParse(webPartType.ToString(), out DefaultWebPart defaultWebPart))
                {
                    map.Add(webPartType, defaultWebPart);
                }
            }

            return map;
        }

        private static Dictionary<DefaultWebPart, WebPartType> BuildToWebPartType()
        {
            var map = new Dictionary<DefaultWebPart, WebPartType>();

            foreach (DefaultWebPart defaultWebPart in Enum.GetValues(typeof(DefaultWebPart)))
            {
                if (Enum.TryParse(defaultWebPart.ToString(), out WebPartType webPartType))
                {
                    map.Add(defaultWebPart, webPartType);
                }
            }

            // Extraction reports a third party web part as Custom, not ThirdParty. Both names exist
            // in the template enum, but only Custom makes the apply side look the component up by
            // name or id - which is the only way a third party part can be re-created.
            map[DefaultWebPart.ThirdParty] = WebPartType.Custom;

            return map;
        }

        /// <summary>
        /// The default web part a template control names, if it names one.
        /// </summary>
        /// <returns><c>false</c> for text controls and third party web parts</returns>
        internal static bool TryGetDefaultWebPart(WebPartType webPartType, out DefaultWebPart defaultWebPart)
        {
            return ToDefaultWebPart.TryGetValue(webPartType, out defaultWebPart);
        }

        /// <summary>
        /// The template web part type for a default web part, falling back to
        /// <see cref="WebPartType.Custom"/> for anything the schema cannot name.
        /// </summary>
        internal static WebPartType GetWebPartType(DefaultWebPart defaultWebPart)
        {
            return ToWebPartType.TryGetValue(defaultWebPart, out WebPartType webPartType)
                ? webPartType
                : WebPartType.Custom;
        }
    }
}

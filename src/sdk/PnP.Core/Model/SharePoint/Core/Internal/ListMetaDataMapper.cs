using System;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    internal static class ListMetaDataMapper
    {
        internal static string MicrosoftGraphNameToRestEntityTypeName(string microsoftGraphListName, ListTemplateType listTemplateType)
        {
            if (listTemplateType == ListTemplateType.UserInformation)
            {
                return "UserInfo";
            }

            (bool isList, _, bool isCatalog) = DetectListType(listTemplateType);

            string entityName = microsoftGraphListName.Replace("_", "_x005f_").Replace(" ", "_x0020_");

            if (isCatalog)
            {
                entityName = $"OData__x005f_catalogs_x002f_{entityName}";
            }

            // Build the name
            string entityNameToUse = $"{entityName.Replace(" ", "")}{(isList ? "List" : "")}";
            // Ensure first character is upper case
            entityNameToUse = entityNameToUse.First().ToString().ToUpper() + entityNameToUse.Substring(1);

            return entityNameToUse;
        }

        //internal static string RestEntityTypeNameToUrl(Uri pnpContextUri, string restEntityTypeName, ListTemplateType listTemplateType)
        //{
        //    var contextUrl = pnpContextUri.ToString().TrimEnd('/');
        //    (bool isList, _, bool isCatalog) = DetectListType(listTemplateType);

        //    // Translate special chars back to their regular values
        //    if (isCatalog)
        //    {
        //        restEntityTypeName = restEntityTypeName.Replace("OData__x005f_catalogs_x002f_", "");
        //    }
        //    var listUrl = restEntityTypeName.Replace("_x005f_", "_").Replace("_x0020_", " ");

        //    if (listTemplateType == ListTemplateType.UserInformation)
        //    {
        //        listUrl = "users";
        //    }

        //    if (isList)
        //    {
        //        // Regular list

        //        // Drop List suffix
        //        listUrl = listUrl.Substring(0, listUrl.Length - 4);
        //        return $"{contextUrl}/lists/{listUrl}";
        //    }
        //    else if (isCatalog)
        //    {
        //        // catalog
        //        return $"{contextUrl}/_catalogs/{listUrl}";
        //    }
        //    else
        //    {
        //        // library
        //        return $"{contextUrl}/{listUrl}";
        //    }
        //}

        //internal static bool IsList(ListTemplateType listTemplateType)
        //{
        //    (bool isList, _, _) = DetectListType(listTemplateType);
        //    return isList;
        //}

        //internal static bool IsLibrary(ListTemplateType listTemplateType)
        //{
        //    (_, bool isLibrary, _) = DetectListType(listTemplateType);
        //    return isLibrary;
        //}
        //internal static bool IsCatalog(ListTemplateType listTemplateType)
        //{
        //    (_, _, bool isCatalog) = DetectListType(listTemplateType);
        //    return isCatalog;
        //}

        internal static (bool isList, bool isLibrary, bool isCatalog) DetectListType(ListTemplateType listTemplateType)
        {
            bool isList = true;
            bool isCatalog = false;
            bool isLibrary = false;

            if (listTemplateType == ListTemplateType.DocumentLibrary ||
                listTemplateType == ListTemplateType.XMLForm ||
                listTemplateType == ListTemplateType.PictureLibrary ||
                listTemplateType == ListTemplateType.WebPageLibrary ||
                listTemplateType == ListTemplateType.DataConnectionLibrary ||
                listTemplateType == ListTemplateType.HelpLibrary ||
                listTemplateType == ListTemplateType.HomePageLibrary ||
                listTemplateType == ListTemplateType.MySiteDocumentLibrary ||
                listTemplateType == ListTemplateType.SharingLinks ||
                // IWConvertedForms
                listTemplateType == (ListTemplateType)10102 ||
                // Sharing Links
                listTemplateType == (ListTemplateType)3300)
            {
                isList = false;
                isLibrary = true;
            }

            if (listTemplateType.ToString().EndsWith("Catalog") ||
                listTemplateType == ListTemplateType.MaintenanceLogs ||
                listTemplateType == ListTemplateType.NoCodePublic ||
                listTemplateType == ListTemplateType.UserInformation)
            {
                isList = false;
                isCatalog = true;
            }

            return (isList, isLibrary, isCatalog);
        }
    }
}

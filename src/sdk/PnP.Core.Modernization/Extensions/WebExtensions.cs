using PnP.Framework.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using PnP.Framework.Utilities;
using PnP.Core.Modernization.Transform;
using PnP.Core.Modernization;

using Microsoft.SharePoint.Client;

namespace PnP.Core.Modernization
{
    /// <summary>
    /// Class holding extension methods for the Microsoft.SharePoint.Client.Web object
    /// </summary>
    public static partial class WebExtensions
    {
        /// <summary>
        /// Returns the site pages from a web, optionally filtered on pagename
        /// </summary>
        /// <param name="web">Web to get the pages from</param>
        /// <param name="pageNameStartsWith">Filter to get all pages starting with</param>
        /// <param name="folder"></param>
        /// <returns>A list of pages (ListItem instances)</returns>
        public static ListItemCollection GetPages(this Web web, string pageNameStartsWith = null, string folder = null)
        {
            // Get pages library
            ListCollection listCollection = web.Lists;
            listCollection.EnsureProperties(coll => coll.Include(li => li.BaseTemplate, li => li.RootFolder, li => li.ListItemEntityTypeFullName));
            var sitePagesLibrary = listCollection
                .Where(p => p.BaseTemplate == (int)ListTemplateType.WebPageLibrary)
                // Verify this is the "real" pages library, sites supporting Viva Connections have a second pages library (named Announcements) used to
                // store Viva Connections announcements
                .Where(p => p.IsPropertyAvailable(p => p.ListItemEntityTypeFullName) && p.ListItemEntityTypeFullName == "SP.Data.SitePagesItem")
                .FirstOrDefault();
            if (sitePagesLibrary != null)
            {
                CamlQuery query = null;
                if (!string.IsNullOrEmpty(pageNameStartsWith))
                {
                    query = new CamlQuery
                    {
                        ViewXml = string.Format(Constants.CAMLQueryByExtensionAndName, pageNameStartsWith)
                    };
                }
                else
                {
                    query = new CamlQuery
                    {
                        ViewXml = Constants.CAMLQueryByExtension
                    };
                }

                if (!string.IsNullOrEmpty(folder))
                {
                    web.EnsureProperty(p => p.ServerRelativeUrl);
                    query.FolderServerRelativeUrl = $"{web.ServerRelativeUrl}/SitePages/{folder}";
                }

                var pages = sitePagesLibrary.GetItems(query);
                web.Context.Load(pages);
                web.Context.ExecuteQueryRetry();

                return pages;
            }

            return null;
        }

        /// <summary>
        /// Returns the site pages from a web, optionally filtered on pagename
        /// </summary>
        /// <param name="web">Web to get the pages from</param>
        /// <param name="webRelativeListName">Web relative URL of the list (e.g. SiteAssets)</param>
        /// <param name="pageNameStartsWith">Filter to get all pages starting with</param>
        /// <param name="folder">Folder to search in</param>
        /// <returns>A list of pages (ListItem instances)</returns>
        public static ListItemCollection GetPagesFromList(this Web web, string webRelativeListName, string pageNameStartsWith = null, string folder = null)
        {
            // Load selected list
            List listHoldingPages = null;
            try
            {
                web.EnsureProperty(p => p.ServerRelativeUrl);
                string listRelativeUrl = $"{web.ServerRelativeUrl.TrimEnd(new[] { '/' })}/{webRelativeListName}";
                
                // ByPass for 2010 onwards support
                listHoldingPages = web.Lists.GetByTitle(webRelativeListName);

                web.Context.Load(listHoldingPages, p => p.RootFolder);
                web.Context.ExecuteQueryRetry();
            }
            catch (ServerException ex)
            {
                if (ex.ServerErrorTypeName == "System.IO.FileNotFoundException")
                {
                    listHoldingPages = null;
                }
                else
                {
                    throw;
                }
            }

            if (listHoldingPages != null)
            {
                CamlQuery query = null;
                if (!string.IsNullOrEmpty(pageNameStartsWith))
                {
                    query = new CamlQuery
                    {
                        ViewXml = string.Format(Constants.CAMLQueryByExtensionAndName, pageNameStartsWith)
                    };
                }
                else
                {
                    query = new CamlQuery
                    {
                        ViewXml = Constants.CAMLQueryByExtension
                    };
                }

                if (!string.IsNullOrEmpty(folder))
                {
                    web.EnsureProperty(p => p.ServerRelativeUrl);
                    query.FolderServerRelativeUrl = $"{listHoldingPages.RootFolder.ServerRelativeUrl}/{folder}";
                }

                var pages = listHoldingPages.GetItems(query);
                web.Context.Load(pages);
                web.Context.ExecuteQueryRetry();

                return pages;
            }

            return null;
        }

        /// <summary>
        /// Returns the blogs from a web, optionally filtered on blog name
        /// </summary>
        /// <param name="web">Web to get the blogs from</param>
        /// <param name="webRelativeListName">Web relative URL of the blog list (e.g. Posts)</param>
        /// <param name="pageNameStartsWith">Filter to get all blogs starting with</param>
        /// <param name="folder">Folder to search in</param>
        /// <returns>A list of pages (ListItem instances)</returns>
        public static ListItemCollection GetBlogsFromList(this Web web, string webRelativeListName, string pageNameStartsWith = null, string folder = null)
        {
            // Load selected list
            List listHoldingPages = null;
            try
            {
                web.EnsureProperty(p => p.ServerRelativeUrl);
                string listRelativeUrl = $"{web.ServerRelativeUrl.TrimEnd(new[] { '/' })}/{webRelativeListName}";

                // ByPass for 2010 onwards support
                listHoldingPages = web.Lists.GetByTitle(webRelativeListName);

                web.Context.Load(listHoldingPages, p => p.RootFolder);
                web.Context.ExecuteQueryRetry();
            }
            catch (ServerException ex)
            {
                if (ex.ServerErrorTypeName == "System.IO.FileNotFoundException")
                {
                    listHoldingPages = null;
                }
                else
                {
                    throw;
                }
            }

            if (listHoldingPages != null)
            {
                CamlQuery query = null;
                if (!string.IsNullOrEmpty(pageNameStartsWith))
                {
                    query = new CamlQuery
                    {
                        ViewXml = string.Format(Constants.CAMLQueryByNameForBlog, pageNameStartsWith)
                    };
                }
                else
                {
                    query = CamlQuery.CreateAllItemsQuery();
                }

                if (!string.IsNullOrEmpty(folder))
                {
                    web.EnsureProperty(p => p.ServerRelativeUrl);
                    query.FolderServerRelativeUrl = $"{listHoldingPages.RootFolder.ServerRelativeUrl}/{folder}";
                }

                var pages = listHoldingPages.GetItems(query);
                web.Context.Load(pages);
                web.Context.ExecuteQueryRetry();

                return pages;
            }

            return null;
        }

        /// <summary>
        /// Returns the admins of this site
        /// </summary>
        /// <param name="web">Site to scan</param>
        /// <returns>List of admins</returns>
        public static List<UserEntity> GetAdmins(this Web web)
        {
            List<UserEntity> adminList = new List<UserEntity>(2);
            web.EnsureProperty(p => p.SiteUsers);

            var admins = web.SiteUsers.Where(p => p.IsSiteAdmin);
            if (admins != null && admins.Any())
            {
                foreach (var admin in admins)
                {
                    adminList.Add(new UserEntity() { LoginName = admin.LoginName, Email = admin.Email, Title = admin.Title });
                }
            }

            return adminList;
        }

        /// <summary>
        /// Returns owners of this web
        /// </summary>
        /// <param name="web">Web to scan</param>
        /// <returns>List of owners</returns>
        public static List<UserEntity> GetOwners(this Web web)
        {
            List<UserEntity> ownerList = new List<UserEntity>();
            web.EnsureProperty(p => p.AssociatedOwnerGroup);

            if (web.AssociatedOwnerGroup != null && !(web.AssociatedOwnerGroup.ServerObjectIsNull == null) && !web.AssociatedOwnerGroup.ServerObjectIsNull.Value)
            {
                web.AssociatedOwnerGroup.EnsureProperty(p => p.Users);
                foreach (var owner in web.AssociatedOwnerGroup.Users)
                {
                    ownerList.Add(new UserEntity() { LoginName = owner.LoginName, Email = owner.Email, Title = owner.Title });
                }
            }
            return ownerList;
        }

        /// <summary>
        /// Returns members of this site
        /// </summary>
        /// <param name="web">Web to scan</param>
        /// <returns>Members of this web</returns>
        public static List<UserEntity> GetMembers(this Web web)
        {
            List<UserEntity> memberList = new List<UserEntity>();
            web.EnsureProperty(p => p.AssociatedMemberGroup);

            if (web.AssociatedMemberGroup != null && !(web.AssociatedMemberGroup.ServerObjectIsNull == null) && !web.AssociatedMemberGroup.ServerObjectIsNull.Value)
            {
                web.AssociatedMemberGroup.EnsureProperty(p => p.Users);
                foreach (var member in web.AssociatedMemberGroup.Users)
                {
                    memberList.Add(new UserEntity() { LoginName = member.LoginName, Email = member.Email, Title = member.Title });
                }
            }
            return memberList;
        }

        /// <summary>
        /// Returns visitors of this site
        /// </summary>
        /// <param name="web">Web to scan</param>
        /// <returns>Visitors of this web</returns>
        public static List<UserEntity> GetVisitors(this Web web)
        {
            List<UserEntity> visitorList = new List<UserEntity>();
            web.EnsureProperty(p => p.AssociatedVisitorGroup);

            if (web.AssociatedVisitorGroup != null && !(web.AssociatedVisitorGroup.ServerObjectIsNull == null) && !web.AssociatedVisitorGroup.ServerObjectIsNull.Value)
            {
                web.AssociatedVisitorGroup.EnsureProperty(p => p.Users);
                foreach (var visitor in web.AssociatedVisitorGroup.Users)
                {
                    visitorList.Add(new UserEntity() { LoginName = visitor.LoginName, Email = visitor.Email, Title = visitor.Title });
                }
            }
            return visitorList;
        }

        /// <summary>
        /// Checks if the passed claims are assigned a role
        /// </summary>
        /// <param name="web">Web to check</param>
        /// <param name="claim1">Claim to check</param>
        /// <param name="claim2">Claim to check</param>
        /// <returns>True if claim1 or claim2 has a role</returns>
        public static bool ClaimsHaveRoleAssignment(this Web web, string claim1, string claim2)
        {
            web.EnsureProperties(p => p.SiteUsers, p => p.SiteGroups);

            bool usersAreInstantiated = true;
            foreach (var group in web.SiteGroups)
            {
                if (!group.IsObjectPropertyInstantiated("Users"))
                {
                    usersAreInstantiated = false;
                    break;
                }
            }

            if (!usersAreInstantiated)
            {
                web.SiteGroups.EnsureProperty(p => p.Include(s => s.Users));
            }

            // Check grants via SharePoint groups
            foreach (var group in web.SiteGroups)
            {
                var everyoneClaimsInGroups = group.Users.Where(p => p.LoginName.Equals(claim1, StringComparison.InvariantCultureIgnoreCase) || p.LoginName.Equals(claim2, StringComparison.InvariantCultureIgnoreCase));
                if (everyoneClaimsInGroups != null && everyoneClaimsInGroups.Any())
                {
                    return true;
                }
            }

            // Check direct grants
            var everyoneClaims = web.SiteUsers.Where(p => p.LoginName.Equals(claim1, StringComparison.InvariantCultureIgnoreCase) || p.LoginName.Equals(claim2, StringComparison.InvariantCultureIgnoreCase));
            if (everyoneClaims != null && everyoneClaims.Any())
            {
                web.EnsureProperty(p => p.RoleAssignments);

                foreach (var claim in everyoneClaims)
                {
                    var found = web.RoleAssignments.Where(p => p.PrincipalId == claim.Id).Any();
                    if (found)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Resolves the Everyone Except External Users claim
        /// </summary>
        /// <param name="web">web to use for the resolving</param>
        /// <returns>Loginname for the "Everyone Except External Users" claim</returns>
        public static string GetEveryoneExceptExternalUsersClaim(this Web web)
        {
            User spReader = null;
            try
            {
                // New tenant
                string userIdentity = $"c:0-.f|rolemanager|spo-grid-all-users/{web.GetAuthenticationRealm()}";
                spReader = web.EnsureUser(userIdentity);
                web.Context.Load(spReader);
                web.Context.ExecuteQueryRetry();
                return spReader.LoginName;
            }
            catch (ServerException)
            {
                // old tenant?
                string userIdentity = string.Empty;

                web.Context.Load(web, w => w.Language);
                web.Context.ExecuteQueryRetry();

                switch (web.Language)
                {
                    case 1025: // Arabic
                        userIdentity = "الجميع باستثناء المستخدمين الخارجيين";
                        break;
                    case 1069: // Basque
                        userIdentity = "Guztiak kanpoko erabiltzaileak izan ezik";
                        break;
                    case 1026: // Bulgarian
                        userIdentity = "Всички освен външни потребители";
                        break;
                    case 1027: // Catalan
                        userIdentity = "Tothom excepte els usuaris externs";
                        break;
                    case 2052: // Chinese (Simplified)
                        userIdentity = "除外部用户外的任何人";
                        break;
                    case 1028: // Chinese (Traditional)
                        userIdentity = "外部使用者以外的所有人";
                        break;
                    case 1050: // Croatian
                        userIdentity = "Svi osim vanjskih korisnika";
                        break;
                    case 1029: // Czech
                        userIdentity = "Všichni kromě externích uživatelů";
                        break;
                    case 1030: // Danish
                        userIdentity = "Alle undtagen eksterne brugere";
                        break;
                    case 1043: // Dutch
                        userIdentity = "Iedereen behalve externe gebruikers";
                        break;
                    case 1033: // English
                        userIdentity = "Everyone except external users";
                        break;
                    case 1061: // Estonian
                        userIdentity = "Kõik peale väliskasutajate";
                        break;
                    case 1035: // Finnish
                        userIdentity = "Kaikki paitsi ulkoiset käyttäjät";
                        break;
                    case 1036: // French
                        userIdentity = "Tout le monde sauf les utilisateurs externes";
                        break;
                    case 1110: // Galician
                        userIdentity = "Todo o mundo excepto os usuarios externos";
                        break;
                    case 1031: // German
                        userIdentity = "Jeder, außer externen Benutzern";
                        break;
                    case 1032: // Greek
                        userIdentity = "Όλοι εκτός από εξωτερικούς χρήστες";
                        break;
                    case 1037: // Hebrew
                        userIdentity = "כולם פרט למשתמשים חיצוניים";
                        break;
                    case 1081: // Hindi
                        userIdentity = "बाह्य उपयोगकर्ताओं को छोड़कर सभी";
                        break;
                    case 1038: // Hungarian
                        userIdentity = "Mindenki, kivéve külső felhasználók";
                        break;
                    case 1057: // Indonesian
                        userIdentity = "Semua orang kecuali pengguna eksternal";
                        break;
                    case 1040: // Italian
                        userIdentity = "Tutti tranne gli utenti esterni";
                        break;
                    case 1041: // Japanese
                        userIdentity = "外部ユーザー以外のすべてのユーザー";
                        break;
                    case 1087: // Kazakh
                        userIdentity = "Сыртқы пайдаланушылардан басқасының барлығы";
                        break;
                    case 1042: // Korean
                        userIdentity = "외부 사용자를 제외한 모든 사람";
                        break;
                    case 1062: // Latvian
                        userIdentity = "Visi, izņemot ārējos lietotājus";
                        break;
                    case 1063: // Lithuanian
                        userIdentity = "Visi, išskyrus išorinius vartotojus";
                        break;
                    case 1086: // Malay
                        userIdentity = "Semua orang kecuali pengguna luaran";
                        break;
                    case 1044: // Norwegian (Bokmål)
                        userIdentity = "Alle bortsett fra eksterne brukere";
                        break;
                    case 1045: // Polish
                        userIdentity = "Wszyscy oprócz użytkowników zewnętrznych";
                        break;
                    case 1046: // Portuguese (Brazil)
                        userIdentity = "Todos exceto os usuários externos";
                        break;
                    case 2070: // Portuguese (Portugal)
                        userIdentity = "Todos exceto os utilizadores externos";
                        break;
                    case 1048: // Romanian
                        userIdentity = "Toată lumea, cu excepția utilizatorilor externi";
                        break;
                    case 1049: // Russian
                        userIdentity = "Все, кроме внешних пользователей";
                        break;
                    case 10266: // Serbian (Cyrillic, Serbia)
                        userIdentity = "Сви осим спољних корисника";
                        break;
                    case 2074:// Serbian (Latin)
                        userIdentity = "Svi osim spoljnih korisnika";
                        break;
                    case 1051:// Slovak
                        userIdentity = "Všetci okrem externých používateľov";
                        break;
                    case 1060: // Slovenian
                        userIdentity = "Vsi razen zunanji uporabniki";
                        break;
                    case 3082: // Spanish
                        userIdentity = "Todos excepto los usuarios externos";
                        break;
                    case 1053: // Swedish
                        userIdentity = "Alla utom externa användare";
                        break;
                    case 1054: // Thai
                        userIdentity = "ทุกคนยกเว้นผู้ใช้ภายนอก";
                        break;
                    case 1055: // Turkish
                        userIdentity = "Dış kullanıcılar hariç herkes";
                        break;
                    case 1058: // Ukranian
                        userIdentity = "Усі, крім зовнішніх користувачів";
                        break;
                    case 1066: // Vietnamese
                        userIdentity = "Tất cả mọi người trừ người dùng bên ngoài";
                        break;
                }
                if (!string.IsNullOrEmpty(userIdentity))
                {
                    spReader = web.EnsureUser(userIdentity);
                    web.Context.Load(spReader);
                    web.Context.ExecuteQueryRetry();
                    return spReader.LoginName;
                }
                else
                {
                    throw new Exception("Language currently not supported, could not resolve everyone except external users claim");
                }
            }
        }

        /// <summary>
        /// Method to bypass missing property in SharePoint 2010
        /// </summary>
        /// <param name="web">Web to operate on</param>
        /// <remarks>Only required on source contexts</remarks>
        /// <returns>web url</returns>
        public static string GetUrl(this Web web)
        {
            if (BaseTransform.GetVersion(web.Context) == SPVersion.SP2010)
            {
                var siteCtx = web.Context.GetSiteCollectionContext();
                siteCtx.Site.EnsureProperties(p => p.ServerRelativeUrl, p => p.Url);
                web.EnsureProperty(p => p.ServerRelativeUrl);

                var siteUri = new Uri(siteCtx.Site.Url);
                string host = $"{siteUri.Scheme}://{siteUri.DnsSafeHost}";

                var serverRelativeUrl = web.ServerRelativeUrl;

                return UrlUtility.Combine(host, serverRelativeUrl);
            }
            else
            {
                return web.EnsureProperty(p => p.Url);
            }
        }

        /// <summary>
        /// Get Pages Library ID for the Web
        /// </summary>
        /// <param name="web">Web to operate on</param>
        /// <returns>ID of the pages library</returns>
        public static string GetPagesLibraryId(this Web web)
        {
            if (web.PropertyBagContainsKey(Constants.WebPropertyKeyPagesListId))
            {
                var keyVal = web.GetPropertyBagValueString(Constants.WebPropertyKeyPagesListId, string.Empty);
                if (!string.IsNullOrEmpty(keyVal))
                {
                    return keyVal;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets site pages library from web
        /// </summary>
        /// <param name="web">Web to operate on</param>
        /// <returns>Site pages library instance</returns>
        public static List GetSitePagesLibrary(this Web web)
        {
            //TemplateFeatureId - 00bfea71-c796-4402-9f2f-0eb9a6e71b18
            var lists = web.Lists;
            web.Context.Load(lists, list => list.Where(l => l.RootFolder.Name == "SitePages").Include(l => l.Id));
            web.Context.ExecuteQueryRetry();

            return lists.SingleOrDefault();
        }

        /// <summary>
        /// Gets blog post library in an SP2010 compatible fashion
        /// </summary>
        /// <param name="web">Web to operate on</param>
        /// <returns>Blog post library</returns>
        public static List GetPostsLibrary(this Web web)
        {
            var lists = web.Lists;
            web.Context.Load(lists, list => list.Where(l => l.BaseTemplate == (int)ListTemplateType.Posts).Include(l => l.Id, l => l.Fields));
            web.Context.ExecuteQueryRetry();

            return lists.SingleOrDefault();
        }

        /// <summary>
        /// Gets list from web in an SP2010 compatible fashion
        /// </summary>
        /// <param name="web">Web to operate on</param>
        /// <param name="listName">List to lookup</param>
        /// <returns>List instance</returns>
        public static List GetListByName(this Web web, string listName)
        {
            var lists = web.Lists;
            web.Context.Load(web.Lists, list => list.Include(l => l.Id, o => o.RootFolder.Name));
            web.Context.ExecuteQueryRetry();

            //Need to load all lists, server cannot process "Equals" or "ToLower" statements - generates CSOM error.
            //Its not ideal, but passes unit tests

            return lists.SingleOrDefault(o => o.RootFolder.Name.Equals(listName,StringComparison.InvariantCultureIgnoreCase));
        }
    }
}

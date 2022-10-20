using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;


namespace PnP.Core.Model.SharePoint
{
    internal sealed class BrandingManager : IBrandingManager
    {
        private readonly PnPContext context;

        internal BrandingManager(PnPContext pnpContext)
        {
            context = pnpContext;
        }

        #region Enumerating themes and setting them on the site

        public async Task<List<ITheme>> GetAvailableThemesAsync()
        {
            List<ITheme> availableThemes = new List<ITheme>();
            ApiCall apiCall = BuildGetAvailableThemesApiCall();

            var result = await (context.Web as Web).RawRequestAsync(apiCall, HttpMethod.Get, "GetAvailableThemes").ConfigureAwait(false);

            ProcessGetTenantThemingOptionsResponse(result.Json, availableThemes);

            return availableThemes;
        }

        private static ApiCall BuildGetAvailableThemesApiCall()
        {
            return new ApiCall("_api/ThemeManager/GetTenantThemingOptions", ApiType.SPORest);
        }

        private static void ProcessGetTenantThemingOptionsResponse(string jsonString, List<ITheme> availableThemes)
        {
            var json = JsonSerializer.Deserialize<JsonElement>(jsonString);

            if (json.TryGetProperty("themePreviews", out JsonElement themePreviews) && themePreviews.ValueKind == JsonValueKind.Array)
            {
                foreach (var theme in themePreviews.EnumerateArray())
                {
                    string name = "Unknown";
                    if (theme.TryGetProperty("name", out JsonElement nameElement))
                    {
                        if (!string.IsNullOrEmpty(nameElement.GetString()))
                        {
                            name = nameElement.GetString();
                        }
                    }

                    availableThemes.Add(new Theme
                    {
                        Name = name,
                        IsCustomTheme = true,
                        
                        // The JSON theme information received from the GetTenantThemingOptions is not in a format that can be applied, so rewrite to the right model
                        // This avoids the need for the tenant CSOM API to apply a custom theme!
                        ThemeJson = NormalizeThemeJson(theme.GetProperty("themeJson").GetString())
                    });
                }
            }

            // Add out of the box themes
            AddOutOfTheBoxTheme(availableThemes, SharePointTheme.Teal);
            AddOutOfTheBoxTheme(availableThemes, SharePointTheme.Blue);
            AddOutOfTheBoxTheme(availableThemes, SharePointTheme.Orange);
            AddOutOfTheBoxTheme(availableThemes, SharePointTheme.Red);
            AddOutOfTheBoxTheme(availableThemes, SharePointTheme.Purple);
            AddOutOfTheBoxTheme(availableThemes, SharePointTheme.Green);
            AddOutOfTheBoxTheme(availableThemes, SharePointTheme.Gray);
            AddOutOfTheBoxTheme(availableThemes, SharePointTheme.DarkYellow);
            AddOutOfTheBoxTheme(availableThemes, SharePointTheme.DarkBlue);

        }

        private static string NormalizeThemeJson(string themeJson)
        {
            var customThemeJson = JsonSerializer.Deserialize<JsonElement>(themeJson);

            var palette = customThemeJson.GetProperty("palette");

            dynamic paletteValues = new ExpandoObject();

            if (palette.TryGetProperty("themeLight", out JsonElement themeLight))
            {
                paletteValues.themeLight = ToJsonColor(themeLight.ToString());                
            }
            if (palette.TryGetProperty("themeTertiary", out JsonElement themeTertiary))
            {
                paletteValues.themeTertiary = ToJsonColor(themeTertiary.ToString());
            }
            if (palette.TryGetProperty("black", out JsonElement black))
            {
                paletteValues.black = ToJsonColor(black.ToString());
            }
            if (palette.TryGetProperty("neutralSecondary", out JsonElement neutralSecondary))
            {
                paletteValues.neutralSecondary = ToJsonColor(neutralSecondary.ToString());
            }
            if (palette.TryGetProperty("neutralTertiaryAlt", out JsonElement neutralTertiaryAlt))
            {
                paletteValues.neutralTertiaryAlt = ToJsonColor(neutralTertiaryAlt.ToString());
            }
            if (palette.TryGetProperty("themeSecondary", out JsonElement themeSecondary))
            {
                paletteValues.themeSecondary = ToJsonColor(themeSecondary.ToString());
            }
            if (palette.TryGetProperty("themeDarker", out JsonElement themeDarker))
            {
                paletteValues.themeDarker = ToJsonColor(themeDarker.ToString());
            }
            if (palette.TryGetProperty("primaryBackground", out JsonElement primaryBackground))
            {
                paletteValues.primaryBackground = ToJsonColor(primaryBackground.ToString());
            }
            if (palette.TryGetProperty("neutralQuaternary", out JsonElement neutralQuaternary))
            {
                paletteValues.neutralQuaternary = ToJsonColor(neutralQuaternary.ToString());
            }
            if (palette.TryGetProperty("neutralPrimaryAlt", out JsonElement neutralPrimaryAlt))
            {
                paletteValues.neutralPrimaryAlt = ToJsonColor(neutralPrimaryAlt.ToString());
            }
            if (palette.TryGetProperty("neutralPrimary", out JsonElement neutralPrimary))
            {
                paletteValues.neutralPrimary = ToJsonColor(neutralPrimary.ToString());
            }
            if (palette.TryGetProperty("themeDark", out JsonElement themeDark))
            {
                paletteValues.themeDark = ToJsonColor(themeDark.ToString());
            }
            if (palette.TryGetProperty("themeLighter", out JsonElement themeLighter))
            {
                paletteValues.themeLighter = ToJsonColor(themeLighter.ToString());
            }
            if (palette.TryGetProperty("neutralTertiary", out JsonElement neutralTertiary))
            {
                paletteValues.neutralTertiary = ToJsonColor(neutralTertiary.ToString());
            }
            if (palette.TryGetProperty("neutralQuaternaryAlt", out JsonElement neutralQuaternaryAlt))
            {
                paletteValues.neutralQuaternaryAlt = ToJsonColor(neutralQuaternaryAlt.ToString());
            }
            if (palette.TryGetProperty("themeLighterAlt", out JsonElement themeLighterAlt))
            {
                paletteValues.themeLighterAlt = ToJsonColor(themeLighterAlt.ToString());
            }
            if (palette.TryGetProperty("white", out JsonElement white))
            {
                paletteValues.white = ToJsonColor(white.ToString());
            }
            if (palette.TryGetProperty("neutralSecondaryAlt", out JsonElement neutralSecondaryAlt))
            {
                paletteValues.neutralSecondaryAlt = ToJsonColor(neutralSecondaryAlt.ToString());
            }
            if (palette.TryGetProperty("neutralLighter", out JsonElement neutralLighter))
            {
                paletteValues.neutralLighter = ToJsonColor(neutralLighter.ToString());
            }
            if (palette.TryGetProperty("neutralLight", out JsonElement neutralLight))
            {
                paletteValues.neutralLight = ToJsonColor(neutralLight.ToString());
            }
            if (palette.TryGetProperty("neutralDark", out JsonElement neutralDark))
            {
                paletteValues.neutralDark = ToJsonColor(neutralDark.ToString());
            }
            if (palette.TryGetProperty("themeDarkAlt", out JsonElement themeDarkAlt))
            {
                paletteValues.themeDarkAlt = ToJsonColor(themeDarkAlt.ToString());
            }
            if (palette.TryGetProperty("neutralLighterAlt", out JsonElement neutralLighterAlt))
            {
                paletteValues.neutralLighterAlt = ToJsonColor(neutralLighterAlt.ToString());
            }
            if (palette.TryGetProperty("primaryText", out JsonElement primaryText))
            {
                paletteValues.primaryText = ToJsonColor(primaryText.ToString());
            }
            if (palette.TryGetProperty("themePrimary", out JsonElement themePrimary))
            {
                paletteValues.themePrimary = ToJsonColor(themePrimary.ToString());
            }

            bool isInverted = false;
            if (customThemeJson.TryGetProperty("isInverted", out JsonElement isInvertedElement))
            {
                isInverted = isInvertedElement.GetBoolean();
            }

            var body = new
            {
                backgroundImageUri = "",
                palette = paletteValues,
                cacheToken = "",
                isDefault = true,
                isInverted,
                version = ""
            };

            string name = "Unknown";
            if (customThemeJson.TryGetProperty("name", out JsonElement nameElement))
            {
                if (!string.IsNullOrEmpty(nameElement.GetString()))
                {
                    name = nameElement.GetString();
                }
            }

            var final = new
            {
                name,
                themeJson = JsonSerializer.Serialize(body),
            };        

            return JsonSerializer.Serialize(final);
        }

        private static object ToJsonColor(string hexString)
        {
            var color = HexToColor(hexString);

            return new 
            {
                R = color.Item1,
                G = color.Item2,
                B = color.Item3,
                A = color.Item4,
            };
        }

        private static Tuple<int,int,int,int> HexToColor(string hexString)
        {
            //replace # occurences
            if (hexString.IndexOf('#') != -1)
            {
                hexString = hexString.Replace("#", "");
            }

            int r;
            int g;
            int b;

            if (hexString.Length == 6)
            {
                //#RRGGBB
                r = int.Parse(hexString.Substring(0, 2), NumberStyles.AllowHexSpecifier);
                g = int.Parse(hexString.Substring(2, 2), NumberStyles.AllowHexSpecifier);
                b = int.Parse(hexString.Substring(4, 2), NumberStyles.AllowHexSpecifier);
            }
            else
            {
                //#RGB
                r = int.Parse($"{hexString[0]}{hexString[0]}", NumberStyles.AllowHexSpecifier);
                g = int.Parse($"{hexString[1]}{hexString[1]}", NumberStyles.AllowHexSpecifier);
                b = int.Parse($"{hexString[2]}{hexString[2]}", NumberStyles.AllowHexSpecifier);
            }

            return Tuple.Create(r, g, b, 255);
        }

        public List<ITheme> GetAvailableThemes()
        {
            return GetAvailableThemesAsync().GetAwaiter().GetResult();
        }

        public async Task<IEnumerableBatchResult<ITheme>> GetAvailableThemesBatchAsync(Batch batch)
        {
            ApiCall apiCall = BuildGetAvailableThemesApiCall();

            // Since we're doing a raw batch request the processing of the batch response needs be implemented
            apiCall.RawEnumerableResult = new List<ITheme>();
            apiCall.RawResultsHandler = (json, apiCall) =>
            {
                ProcessGetTenantThemingOptionsResponse(json, (List<ITheme>)apiCall.RawEnumerableResult);
            };

            // Add the request to the batch
            var batchRequest = await (context.Web as Web).RawRequestBatchAsync(batch, apiCall, HttpMethod.Get).ConfigureAwait(false);

            // Return the batch result as Enumerable
            return new BatchEnumerableBatchResult<ITheme>(batch, batchRequest.Id, (IReadOnlyList<ITheme>)apiCall.RawEnumerableResult);
        }

        public IEnumerableBatchResult<ITheme> GetAvailableThemesBatch(Batch batch)
        { 
            return GetAvailableThemesBatchAsync(batch).GetAwaiter().GetResult();
        }

        public async Task<IEnumerableBatchResult<ITheme>> GetAvailableThemesBatchAsync()
        {
            return await GetAvailableThemesBatchAsync(context.CurrentBatch).ConfigureAwait(false);
        }

        public IEnumerableBatchResult<ITheme> GetAvailableThemesBatch()
        { 
            return GetAvailableThemesBatchAsync().GetAwaiter().GetResult();
        }

        private static void AddOutOfTheBoxTheme(List<ITheme> availableThemes, SharePointTheme theme)
        {
            availableThemes.Add(new Theme
            {
                Name = theme.ToString(),
                IsCustomTheme = false,
                ThemeJson = GetThemeJsonAsString(theme)
            });
        }

        private static string GetThemeJsonAsString(SharePointTheme theme)
        {
            return theme switch
            {
                SharePointTheme.Blue => "{'name':'Blue','themeJson':'{\"backgroundImageUri\":\"\",\"palette\":{\"themePrimary\":{\"R\":0,\"G\":120,\"B\":212,\"A\":255},\"themeLighterAlt\":{\"R\":239,\"G\":246,\"B\":252,\"A\":255},\"themeLighter\":{\"R\":222,\"G\":236,\"B\":249,\"A\":255},\"themeLight\":{\"R\":199,\"G\":224,\"B\":244,\"A\":255},\"themeTertiary\":{\"R\":113,\"G\":175,\"B\":229,\"A\":255},\"themeSecondary\":{\"R\":43,\"G\":136,\"B\":216,\"A\":255},\"themeDarkAlt\":{\"R\":16,\"G\":110,\"B\":190,\"A\":255},\"themeDark\":{\"R\":0,\"G\":90,\"B\":158,\"A\":255},\"themeDarker\":{\"R\":0,\"G\":69,\"B\":120,\"A\":255},\"accent\":{\"R\":135,\"G\":100,\"B\":184,\"A\":255},\"neutralLighterAlt\":{\"R\":248,\"G\":248,\"B\":248,\"A\":255},\"neutralLighter\":{\"R\":244,\"G\":244,\"B\":244,\"A\":255},\"neutralLight\":{\"R\":234,\"G\":234,\"B\":234,\"A\":255},\"neutralQuaternaryAlt\":{\"R\":218,\"G\":218,\"B\":218,\"A\":255},\"neutralQuaternary\":{\"R\":208,\"G\":208,\"B\":208,\"A\":255},\"neutralTertiaryAlt\":{\"R\":200,\"G\":200,\"B\":200,\"A\":255},\"neutralTertiary\":{\"R\":166,\"G\":166,\"B\":166,\"A\":255},\"neutralSecondary\":{\"R\":102,\"G\":102,\"B\":102,\"A\":255},\"neutralPrimaryAlt\":{\"R\":60,\"G\":60,\"B\":60,\"A\":255},\"neutralPrimary\":{\"R\":51,\"G\":51,\"B\":51,\"A\":255},\"neutralDark\":{\"R\":33,\"G\":33,\"B\":33,\"A\":255},\"black\":{\"R\":0,\"G\":0,\"B\":0,\"A\":255},\"white\":{\"R\":255,\"G\":255,\"B\":255,\"A\":255},\"primaryBackground\":{\"R\":255,\"G\":255,\"B\":255,\"A\":255},\"primaryText\":{\"R\":51,\"G\":51,\"B\":51,\"A\":255}},\"cacheToken\":\"\",\"isDefault\":true,\"version\":\"\"}'}",
                SharePointTheme.Orange => "{'name':'Orange','themeJson':'{\"backgroundImageUri\":\"\",\"palette\":{\"themePrimary\":{\"R\":202,\"G\":80,\"B\":16,\"A\":255},\"themeLighterAlt\":{\"R\":253,\"G\":247,\"B\":244,\"A\":255},\"themeLighter\":{\"R\":246,\"G\":223,\"B\":210,\"A\":255},\"themeLight\":{\"R\":239,\"G\":196,\"B\":173,\"A\":255},\"themeTertiary\":{\"R\":223,\"G\":143,\"B\":100,\"A\":255},\"themeSecondary\":{\"R\":208,\"G\":98,\"B\":40,\"A\":255},\"themeDarkAlt\":{\"R\":181,\"G\":73,\"B\":15,\"A\":255},\"themeDark\":{\"R\":153,\"G\":62,\"B\":12,\"A\":255},\"themeDarker\":{\"R\":113,\"G\":45,\"B\":9,\"A\":255},\"accent\":{\"R\":152,\"G\":111,\"B\":11,\"A\":255},\"neutralLighterAlt\":{\"R\":248,\"G\":248,\"B\":248,\"A\":255},\"neutralLighter\":{\"R\":244,\"G\":244,\"B\":244,\"A\":255},\"neutralLight\":{\"R\":234,\"G\":234,\"B\":234,\"A\":255},\"neutralQuaternaryAlt\":{\"R\":218,\"G\":218,\"B\":218,\"A\":255},\"neutralQuaternary\":{\"R\":208,\"G\":208,\"B\":208,\"A\":255},\"neutralTertiaryAlt\":{\"R\":200,\"G\":200,\"B\":200,\"A\":255},\"neutralTertiary\":{\"R\":166,\"G\":166,\"B\":166,\"A\":255},\"neutralSecondary\":{\"R\":102,\"G\":102,\"B\":102,\"A\":255},\"neutralPrimaryAlt\":{\"R\":60,\"G\":60,\"B\":60,\"A\":255},\"neutralPrimary\":{\"R\":51,\"G\":51,\"B\":51,\"A\":255},\"neutralDark\":{\"R\":33,\"G\":33,\"B\":33,\"A\":255},\"black\":{\"R\":0,\"G\":0,\"B\":0,\"A\":255},\"white\":{\"R\":255,\"G\":255,\"B\":255,\"A\":255},\"primaryBackground\":{\"R\":255,\"G\":255,\"B\":255,\"A\":255},\"primaryText\":{\"R\":51,\"G\":51,\"B\":51,\"A\":255}},\"cacheToken\":\"\",\"isDefault\":true,\"version\":\"\"}'}",
                SharePointTheme.Red => "{'name':'Red','themeJson':'{\"backgroundImageUri\":\"\",\"palette\":{\"themePrimary\":{\"R\":164,\"G\":38,\"B\":44,\"A\":255},\"themeLighterAlt\":{\"R\":251,\"G\":244,\"B\":244,\"A\":255},\"themeLighter\":{\"R\":240,\"G\":211,\"B\":212,\"A\":255},\"themeLight\":{\"R\":227,\"G\":175,\"B\":178,\"A\":255},\"themeTertiary\":{\"R\":200,\"G\":108,\"B\":112,\"A\":255},\"themeSecondary\":{\"R\":174,\"G\":56,\"B\":62,\"A\":255},\"themeDarkAlt\":{\"R\":147,\"G\":34,\"B\":39,\"A\":255},\"themeDark\":{\"R\":124,\"G\":29,\"B\":33,\"A\":255},\"themeDarker\":{\"R\":91,\"G\":21,\"B\":25,\"A\":255},\"accent\":{\"R\":202,\"G\":80,\"B\":16,\"A\":255},\"neutralLighterAlt\":{\"R\":248,\"G\":248,\"B\":248,\"A\":255},\"neutralLighter\":{\"R\":244,\"G\":244,\"B\":244,\"A\":255},\"neutralLight\":{\"R\":234,\"G\":234,\"B\":234,\"A\":255},\"neutralQuaternaryAlt\":{\"R\":218,\"G\":218,\"B\":218,\"A\":255},\"neutralQuaternary\":{\"R\":208,\"G\":208,\"B\":208,\"A\":255},\"neutralTertiaryAlt\":{\"R\":200,\"G\":200,\"B\":200,\"A\":255},\"neutralTertiary\":{\"R\":166,\"G\":166,\"B\":166,\"A\":255},\"neutralSecondary\":{\"R\":102,\"G\":102,\"B\":102,\"A\":255},\"neutralPrimaryAlt\":{\"R\":60,\"G\":60,\"B\":60,\"A\":255},\"neutralPrimary\":{\"R\":51,\"G\":51,\"B\":51,\"A\":255},\"neutralDark\":{\"R\":33,\"G\":33,\"B\":33,\"A\":255},\"black\":{\"R\":0,\"G\":0,\"B\":0,\"A\":255},\"white\":{\"R\":255,\"G\":255,\"B\":255,\"A\":255},\"primaryBackground\":{\"R\":255,\"G\":255,\"B\":255,\"A\":255},\"primaryText\":{\"R\":51,\"G\":51,\"B\":51,\"A\":255}},\"cacheToken\":\"\",\"isDefault\":true,\"version\":\"\"}'}",
                SharePointTheme.Purple => "{'name':'Purple','themeJson':'{\"backgroundImageUri\":\"\",\"palette\":{\"themePrimary\":{\"R\":135,\"G\":100,\"B\":184,\"A\":255},\"themeLighterAlt\":{\"R\":249,\"G\":248,\"B\":252,\"A\":255},\"themeLighter\":{\"R\":233,\"G\":226,\"B\":244,\"A\":255},\"themeLight\":{\"R\":215,\"G\":201,\"B\":234,\"A\":255},\"themeTertiary\":{\"R\":178,\"G\":154,\"B\":212,\"A\":255},\"themeSecondary\":{\"R\":147,\"G\":114,\"B\":192,\"A\":255},\"themeDarkAlt\":{\"R\":121,\"G\":89,\"B\":165,\"A\":255},\"themeDark\":{\"R\":102,\"G\":75,\"B\":140,\"A\":255},\"themeDarker\":{\"R\":75,\"G\":56,\"B\":103,\"A\":255},\"accent\":{\"R\":3,\"G\":131,\"B\":135,\"A\":255},\"neutralLighterAlt\":{\"R\":248,\"G\":248,\"B\":248,\"A\":255},\"neutralLighter\":{\"R\":244,\"G\":244,\"B\":244,\"A\":255},\"neutralLight\":{\"R\":234,\"G\":234,\"B\":234,\"A\":255},\"neutralQuaternaryAlt\":{\"R\":218,\"G\":218,\"B\":218,\"A\":255},\"neutralQuaternary\":{\"R\":208,\"G\":208,\"B\":208,\"A\":255},\"neutralTertiaryAlt\":{\"R\":200,\"G\":200,\"B\":200,\"A\":255},\"neutralTertiary\":{\"R\":166,\"G\":166,\"B\":166,\"A\":255},\"neutralSecondary\":{\"R\":102,\"G\":102,\"B\":102,\"A\":255},\"neutralPrimaryAlt\":{\"R\":60,\"G\":60,\"B\":60,\"A\":255},\"neutralPrimary\":{\"R\":51,\"G\":51,\"B\":51,\"A\":255},\"neutralDark\":{\"R\":33,\"G\":33,\"B\":33,\"A\":255},\"black\":{\"R\":0,\"G\":0,\"B\":0,\"A\":255},\"white\":{\"R\":255,\"G\":255,\"B\":255,\"A\":255},\"primaryBackground\":{\"R\":255,\"G\":255,\"B\":255,\"A\":255},\"primaryText\":{\"R\":51,\"G\":51,\"B\":51,\"A\":255}},\"cacheToken\":\"\",\"isDefault\":true,\"version\":\"\"}'}",
                SharePointTheme.Green => "{'name':'Green','themeJson':'{\"backgroundImageUri\":\"\",\"palette\":{\"themePrimary\":{\"R\":73,\"G\":130,\"B\":5,\"A\":255},\"themeLighterAlt\":{\"R\":246,\"G\":250,\"B\":240,\"A\":255},\"themeLighter\":{\"R\":219,\"G\":235,\"B\":199,\"A\":255},\"themeLight\":{\"R\":189,\"G\":218,\"B\":155,\"A\":255},\"themeTertiary\":{\"R\":133,\"G\":180,\"B\":76,\"A\":255},\"themeSecondary\":{\"R\":90,\"G\":145,\"B\":23,\"A\":255},\"themeDarkAlt\":{\"R\":66,\"G\":117,\"B\":5,\"A\":255},\"themeDark\":{\"R\":56,\"G\":99,\"B\":4,\"A\":255},\"themeDarker\":{\"R\":41,\"G\":73,\"B\":3,\"A\":255},\"accent\":{\"R\":3,\"G\":131,\"B\":135,\"A\":255},\"neutralLighterAlt\":{\"R\":248,\"G\":248,\"B\":248,\"A\":255},\"neutralLighter\":{\"R\":244,\"G\":244,\"B\":244,\"A\":255},\"neutralLight\":{\"R\":234,\"G\":234,\"B\":234,\"A\":255},\"neutralQuaternaryAlt\":{\"R\":218,\"G\":218,\"B\":218,\"A\":255},\"neutralQuaternary\":{\"R\":208,\"G\":208,\"B\":208,\"A\":255},\"neutralTertiaryAlt\":{\"R\":200,\"G\":200,\"B\":200,\"A\":255},\"neutralTertiary\":{\"R\":166,\"G\":166,\"B\":166,\"A\":255},\"neutralSecondary\":{\"R\":102,\"G\":102,\"B\":102,\"A\":255},\"neutralPrimaryAlt\":{\"R\":60,\"G\":60,\"B\":60,\"A\":255},\"neutralPrimary\":{\"R\":51,\"G\":51,\"B\":51,\"A\":255},\"neutralDark\":{\"R\":33,\"G\":33,\"B\":33,\"A\":255},\"black\":{\"R\":0,\"G\":0,\"B\":0,\"A\":255},\"white\":{\"R\":255,\"G\":255,\"B\":255,\"A\":255},\"primaryBackground\":{\"R\":255,\"G\":255,\"B\":255,\"A\":255},\"primaryText\":{\"R\":51,\"G\":51,\"B\":51,\"A\":255}},\"cacheToken\":\"\",\"isDefault\":true,\"version\":\"\"}'}",
                SharePointTheme.Gray => "{'name':'Gray','themeJson':'{\"backgroundImageUri\":\"\",\"palette\":{\"themePrimary\":{\"R\":105,\"G\":121,\"B\":126,\"A\":255},\"themeLighterAlt\":{\"R\":248,\"G\":249,\"B\":250,\"A\":255},\"themeLighter\":{\"R\":228,\"G\":233,\"B\":234,\"A\":255},\"themeLight\":{\"R\":205,\"G\":213,\"B\":216,\"A\":255},\"themeTertiary\":{\"R\":159,\"G\":173,\"B\":177,\"A\":255},\"themeSecondary\":{\"R\":120,\"G\":136,\"B\":141,\"A\":255},\"themeDarkAlt\":{\"R\":93,\"G\":108,\"B\":112,\"A\":255},\"themeDark\":{\"R\":79,\"G\":91,\"B\":95,\"A\":255},\"themeDarker\":{\"R\":58,\"G\":67,\"B\":70,\"A\":255},\"accent\":{\"R\":0,\"G\":120,\"B\":212,\"A\":255},\"neutralLighterAlt\":{\"R\":248,\"G\":248,\"B\":248,\"A\":255},\"neutralLighter\":{\"R\":244,\"G\":244,\"B\":244,\"A\":255},\"neutralLight\":{\"R\":234,\"G\":234,\"B\":234,\"A\":255},\"neutralQuaternaryAlt\":{\"R\":218,\"G\":218,\"B\":218,\"A\":255},\"neutralQuaternary\":{\"R\":208,\"G\":208,\"B\":208,\"A\":255},\"neutralTertiaryAlt\":{\"R\":200,\"G\":200,\"B\":200,\"A\":255},\"neutralTertiary\":{\"R\":166,\"G\":166,\"B\":166,\"A\":255},\"neutralSecondary\":{\"R\":102,\"G\":102,\"B\":102,\"A\":255},\"neutralPrimaryAlt\":{\"R\":60,\"G\":60,\"B\":60,\"A\":255},\"neutralPrimary\":{\"R\":51,\"G\":51,\"B\":51,\"A\":255},\"neutralDark\":{\"R\":33,\"G\":33,\"B\":33,\"A\":255},\"black\":{\"R\":0,\"G\":0,\"B\":0,\"A\":255},\"white\":{\"R\":255,\"G\":255,\"B\":255,\"A\":255},\"primaryBackground\":{\"R\":255,\"G\":255,\"B\":255,\"A\":255},\"primaryText\":{\"R\":51,\"G\":51,\"B\":51,\"A\":255}},\"cacheToken\":\"\",\"isDefault\":true,\"version\":\"\"}'}",
                SharePointTheme.DarkYellow => "{'name':'Dark Yellow','themeJson':'{\"backgroundImageUri\":\"\",\"palette\":{\"themePrimary\":{\"R\":255,\"G\":200,\"B\":61,\"A\":255},\"themeLighterAlt\":{\"R\":10,\"G\":8,\"B\":2,\"A\":255},\"themeLighter\":{\"R\":41,\"G\":32,\"B\":10,\"A\":255},\"themeLight\":{\"R\":77,\"G\":60,\"B\":18,\"A\":255},\"themeTertiary\":{\"R\":153,\"G\":120,\"B\":37,\"A\":255},\"themeSecondary\":{\"R\":224,\"G\":176,\"B\":54,\"A\":255},\"themeDarkAlt\":{\"R\":255,\"G\":206,\"B\":81,\"A\":255},\"themeDark\":{\"R\":255,\"G\":213,\"B\":108,\"A\":255},\"themeDarker\":{\"R\":255,\"G\":224,\"B\":146,\"A\":255},\"accent\":{\"R\":255,\"G\":200,\"B\":61,\"A\":255},\"neutralLighterAlt\":{\"R\":40,\"G\":40,\"B\":40,\"A\":255},\"neutralLighter\":{\"R\":49,\"G\":49,\"B\":49,\"A\":255},\"neutralLight\":{\"R\":63,\"G\":63,\"B\":63,\"A\":255},\"neutralQuaternaryAlt\":{\"R\":72,\"G\":72,\"B\":72,\"A\":255},\"neutralQuaternary\":{\"R\":79,\"G\":79,\"B\":79,\"A\":255},\"neutralTertiaryAlt\":{\"R\":109,\"G\":109,\"B\":109,\"A\":255},\"neutralTertiary\":{\"R\":200,\"G\":200,\"B\":200,\"A\":255},\"neutralSecondary\":{\"R\":208,\"G\":208,\"B\":208,\"A\":255},\"neutralPrimaryAlt\":{\"R\":218,\"G\":218,\"B\":218,\"A\":255},\"neutralPrimary\":{\"R\":255,\"G\":255,\"B\":255,\"A\":255},\"neutralDark\":{\"R\":244,\"G\":244,\"B\":244,\"A\":255},\"black\":{\"R\":248,\"G\":248,\"B\":248,\"A\":255},\"white\":{\"R\":31,\"G\":31,\"B\":31,\"A\":255},\"primaryBackground\":{\"R\":31,\"G\":31,\"B\":31,\"A\":255},\"primaryText\":{\"R\":255,\"G\":255,\"B\":255,\"A\":255}},\"cacheToken\":\"\",\"isDefault\":true,\"isInverted\":true,\"version\":\"\"}'}",
                SharePointTheme.DarkBlue => "{'name':'Dark Blue','themeJson':'{\"backgroundImageUri\":\"\",\"palette\":{\"themePrimary\":{\"R\":58,\"G\":150,\"B\":221,\"A\":255},\"themeLighterAlt\":{\"R\":2,\"G\":6,\"B\":9,\"A\":255},\"themeLighter\":{\"R\":9,\"G\":24,\"B\":35,\"A\":255},\"themeLight\":{\"R\":17,\"G\":45,\"B\":67,\"A\":255},\"themeTertiary\":{\"R\":35,\"G\":90,\"B\":133,\"A\":255},\"themeSecondary\":{\"R\":51,\"G\":133,\"B\":195,\"A\":255},\"themeDarkAlt\":{\"R\":75,\"G\":160,\"B\":225,\"A\":255},\"themeDark\":{\"R\":101,\"G\":174,\"B\":230,\"A\":255},\"themeDarker\":{\"R\":138,\"G\":194,\"B\":236,\"A\":255},\"accent\":{\"R\":58,\"G\":150,\"B\":221,\"A\":255},\"neutralLighterAlt\":{\"R\":29,\"G\":43,\"B\":60,\"A\":255},\"neutralLighter\":{\"R\":34,\"G\":50,\"B\":68,\"A\":255},\"neutralLight\":{\"R\":43,\"G\":61,\"B\":81,\"A\":255},\"neutralQuaternaryAlt\":{\"R\":50,\"G\":68,\"B\":89,\"A\":255},\"neutralQuaternary\":{\"R\":55,\"G\":74,\"B\":95,\"A\":255},\"neutralTertiaryAlt\":{\"R\":79,\"G\":99,\"B\":122,\"A\":255},\"neutralTertiary\":{\"R\":200,\"G\":200,\"B\":200,\"A\":255},\"neutralSecondary\":{\"R\":208,\"G\":208,\"B\":208,\"A\":255},\"neutralPrimaryAlt\":{\"R\":218,\"G\":218,\"B\":218,\"A\":255},\"neutralPrimary\":{\"R\":255,\"G\":255,\"B\":255,\"A\":255},\"neutralDark\":{\"R\":244,\"G\":244,\"B\":244,\"A\":255},\"black\":{\"R\":248,\"G\":248,\"B\":248,\"A\":255},\"white\":{\"R\":24,\"G\":37,\"B\":52,\"A\":255},\"primaryBackground\":{\"R\":24,\"G\":37,\"B\":52,\"A\":255},\"primaryText\":{\"R\":255,\"G\":255,\"B\":255,\"A\":255}},\"cacheToken\":\"\",\"isDefault\":true,\"isInverted\":true,\"version\":\"\"}'}",
                // Is Teal theme + default value
                _ => "{'name':'Teal','themeJson':'{\"backgroundImageUri\":\"\",\"palette\":{\"themePrimary\":{\"R\":3,\"G\":120,\"B\":124,\"A\":255},\"themeLighterAlt\":{\"R\":240,\"G\":249,\"B\":250,\"A\":255},\"themeLighter\":{\"R\":197,\"G\":233,\"B\":234,\"A\":255},\"themeLight\":{\"R\":152,\"G\":214,\"B\":216,\"A\":255},\"themeTertiary\":{\"R\":73,\"G\":174,\"B\":177,\"A\":255},\"themeSecondary\":{\"R\":19,\"G\":137,\"B\":141,\"A\":255},\"themeDarkAlt\":{\"R\":2,\"G\":109,\"B\":112,\"A\":255},\"themeDark\":{\"R\":2,\"G\":92,\"B\":95,\"A\":255},\"themeDarker\":{\"R\":1,\"G\":68,\"B\":70,\"A\":255},\"accent\":{\"R\":79,\"G\":107,\"B\":237,\"A\":255},\"neutralLighterAlt\":{\"R\":248,\"G\":248,\"B\":248,\"A\":255},\"neutralLighter\":{\"R\":244,\"G\":244,\"B\":244,\"A\":255},\"neutralLight\":{\"R\":234,\"G\":234,\"B\":234,\"A\":255},\"neutralQuaternaryAlt\":{\"R\":218,\"G\":218,\"B\":218,\"A\":255},\"neutralQuaternary\":{\"R\":208,\"G\":208,\"B\":208,\"A\":255},\"neutralTertiaryAlt\":{\"R\":200,\"G\":200,\"B\":200,\"A\":255},\"neutralTertiary\":{\"R\":166,\"G\":166,\"B\":166,\"A\":255},\"neutralSecondary\":{\"R\":102,\"G\":102,\"B\":102,\"A\":255},\"neutralPrimaryAlt\":{\"R\":60,\"G\":60,\"B\":60,\"A\":255},\"neutralPrimary\":{\"R\":51,\"G\":51,\"B\":51,\"A\":255},\"neutralDark\":{\"R\":33,\"G\":33,\"B\":33,\"A\":255},\"black\":{\"R\":0,\"G\":0,\"B\":0,\"A\":255},\"white\":{\"R\":255,\"G\":255,\"B\":255,\"A\":255},\"primaryBackground\":{\"R\":255,\"G\":255,\"B\":255,\"A\":255},\"primaryText\":{\"R\":51,\"G\":51,\"B\":51,\"A\":255}},\"cacheToken\":\"\",\"isDefault\":true,\"version\":\"\"}'}",
            };
        }

        public async Task SetThemeAsync(SharePointTheme theme)
        {
            var apiCall = BuildSetThemesApiCall(GetThemeJsonAsString(theme));

            await (context.Web as Web).RawRequestAsync(apiCall, HttpMethod.Post, "SetTheme").ConfigureAwait(false);
        }

        public void SetTheme(SharePointTheme theme)
        {
            SetThemeAsync(theme).GetAwaiter().GetResult();
        }

        public async Task SetThemeBatchAsync(Batch batch, SharePointTheme theme)
        {
            var apiCall = BuildSetThemesApiCall(GetThemeJsonAsString(theme));

            await (context.Web as Web).RawRequestBatchAsync(batch, apiCall, HttpMethod.Post, "SetTheme").ConfigureAwait(false);
        }

        public void SetThemeBatch(Batch batch, SharePointTheme theme)
        {
            SetThemeBatchAsync(batch, theme).GetAwaiter().GetResult();
        }

        public async Task SetThemeBatchAsync(SharePointTheme theme)
        {
            await SetThemeBatchAsync(context.CurrentBatch, theme).ConfigureAwait(false);
        }

        public void SetThemeBatch(SharePointTheme theme)
        {
            SetThemeBatchAsync(theme).GetAwaiter().GetResult();
        }

        public async Task SetThemeAsync(ITheme theme)
        {
            var apiCall = BuildSetThemesApiCall(theme.ThemeJson);

            await (context.Web as Web).RawRequestAsync(apiCall, HttpMethod.Post, "SetTheme").ConfigureAwait(false);
        }

        public void SetTheme(ITheme theme)
        {
            SetThemeAsync(theme).GetAwaiter().GetResult();
        }

        public async Task SetThemeBatchAsync(Batch batch, ITheme theme)
        {
            var apiCall = BuildSetThemesApiCall(theme.ThemeJson);

            await (context.Web as Web).RawRequestBatchAsync(batch, apiCall, HttpMethod.Post, "SetTheme").ConfigureAwait(false);
        }

        public void SetThemeBatch(Batch batch, ITheme theme)
        {
            SetThemeBatchAsync(batch, theme).GetAwaiter().GetResult();
        }

        public async Task SetThemeBatchAsync(ITheme theme)
        {
            await SetThemeBatchAsync(context.CurrentBatch, theme).ConfigureAwait(false);
        }

        public void SetThemeBatch(ITheme theme)
        {
            SetThemeBatchAsync(theme).GetAwaiter().GetResult();
        }

        private static ApiCall BuildSetThemesApiCall(string themeJsonString)
        {
            return new ApiCall("_api/ThemeManager/ApplyTheme", ApiType.SPORest, themeJsonString);
        }

        #endregion

        #region Site chrome
        public async Task<IChromeOptions> GetChromeOptionsAsync()
        {
            var batch = context.NewBatch();

            var web = await context.Web.GetBatchAsync(batch,
                                        p => p.HeaderEmphasis,
                                        p => p.HeaderLayout,
                                        p => p.HideTitleInHeader,
                                        p => p.FooterEmphasis,
                                        p => p.FooterEnabled,
                                        p => p.FooterLayout,
                                        p => p.LogoAlignment,
                                        p => p.MegaMenuEnabled,
                                        p => p.QuickLaunchEnabled,
                                        p => p.HorizontalQuickLaunch,
                                        // Load these properties now as they're needed in the HasCommunicationSiteFeaturesAsync method
                                        p => p.WebTemplate,
                                        p => p.Features).ConfigureAwait(false);

            var menuState = await GetMenuStateBatchAsync(batch).ConfigureAwait(false);

            // Execute batch
            await context.ExecuteAsync(batch).ConfigureAwait(false);

            var hasCommunicationSiteFeatures = await web.Result.HasCommunicationSiteFeaturesAsync().ConfigureAwait(false);

            var chromeOptions = new ChromeOptions(context);

            ProcessChromeOptionsResponse(web.Result, hasCommunicationSiteFeatures, menuState.Result, chromeOptions);

            return chromeOptions;
        }

        private void ProcessChromeOptionsResponse(IWeb web, bool hasCommunicationSiteFeatures, MenuState menuState, ChromeOptions chromeOptions)
        {
            chromeOptions.Header = new HeaderOptions(context)
            {
                Layout = web.HeaderLayout,
                LogoAlignment = web.LogoAlignment,
                HideTitle = web.HideTitleInHeader,
                Emphasis = web.HeaderEmphasis
            };

            chromeOptions.Navigation = new NavigationOptions
            {
                MegaMenuEnabled = web.MegaMenuEnabled,
                Visible = web.QuickLaunchEnabled,
                HorizontalQuickLaunch = hasCommunicationSiteFeatures ? true : web.HorizontalQuickLaunch
            };

            if (hasCommunicationSiteFeatures)
            {
                chromeOptions.Footer = new FooterOptions(context)
                {
                    Layout = web.FooterLayout,
                    Emphasis = web.FooterEmphasis,
                    Enabled = web.FooterEnabled
                };

                if (menuState != null)
                {
                    (chromeOptions.Footer as FooterOptions).MenuState = menuState;

                    var titleNode = menuState.Nodes.FirstOrDefault(p => p.Title == "7376cd83-67ac-4753-b156-6a7b3fa0fc1f");
                    if (titleNode != null)
                    {
                        var displayNameNode = titleNode.Nodes.FirstOrDefault(p => p.NodeType == 0);
                        if (displayNameNode != null)
                        {
                            chromeOptions.Footer.DisplayName = displayNameNode.Title;
                        }
                    }
                }
            }
        }

        public IChromeOptions GetChromeOptions()
        {
            return GetChromeOptionsAsync().GetAwaiter().GetResult();
        }

        public async Task<IBatchSingleResult<IChromeOptions>> GetChromeOptionsBatchAsync(Batch batch)
        {
            var web = await context.Web.GetBatchAsync(batch,
                                                p => p.HeaderEmphasis,
                                                p => p.HeaderLayout,
                                                p => p.HideTitleInHeader,
                                                p => p.FooterEmphasis,
                                                p => p.FooterEnabled,
                                                p => p.FooterLayout,
                                                p => p.LogoAlignment,
                                                p => p.MegaMenuEnabled,
                                                p => p.QuickLaunchEnabled,
                                                p => p.HorizontalQuickLaunch,
                                                // Load these properties now as they're needed in the HasCommunicationSiteFeaturesAsync method
                                                p => p.WebTemplate,
                                                p => p.Features).ConfigureAwait(false);
            
            var menuState = await GetMenuStateBatchAsync(batch).ConfigureAwait(false);

            // Add this extra request as we we need the web load request fully processed before we can use it in the event
            // handler below. Adding the event handler to the web batch requests will result in the handler firing
            // before the web instance is fully loaded. Ensure a REST only property is loaded as the previous call is also REST 
            // and we don't want to end up with a mixed batch
            var site = await context.Site.GetBatchAsync(batch, p => p.GroupId).ConfigureAwait(false);

            var chromeOptions = new ChromeOptions(context);

            var lastRequestId = batch.PrepareLastAddedRequestForBatchProcessing(async (json, apiCall) =>
            {
                bool hasCommunicationSiteFeatures = await web.Result.HasCommunicationSiteFeaturesAsync().ConfigureAwait(false);                   
                ProcessChromeOptionsResponse(web.Result, hasCommunicationSiteFeatures, menuState.Result, apiCall.RawSingleResult as ChromeOptions);
            }, chromeOptions);

            return new BatchSingleResult<IChromeOptions>(batch, lastRequestId, chromeOptions);

        }

        public IBatchSingleResult<IChromeOptions> GetChromeOptionsBatch(Batch batch)
        {
            return GetChromeOptionsBatchAsync(batch).GetAwaiter().GetResult();  
        }

        public async Task<IBatchSingleResult<IChromeOptions>> GetChromeOptionsBatchAsync()
        {
            return await GetChromeOptionsBatchAsync(context.CurrentBatch).ConfigureAwait(false);
        }

        public IBatchSingleResult<IChromeOptions> GetChromeOptionsBatch()
        {
            return GetChromeOptionsBatchAsync().GetAwaiter().GetResult();
        }

        public async Task SetChromeOptionsAsync(IChromeOptions chromeOptions)
        {
            // Setting chrome options takes two calls, so batch them for performance reasons
            var batch = context.NewBatch();

            await BuildSetChromeOptionsRequests(chromeOptions, batch).ConfigureAwait(false);

            // Execute the batch
            await context.ExecuteAsync(batch).ConfigureAwait(false);

            // Update the chrome updates in the current web
            ReflectChromeUpdatesInCurrentWeb(context, chromeOptions);
        }

        private async Task BuildSetChromeOptionsRequests(IChromeOptions chromeOptions, Batch batch)
        {
            // Update chrome options
            await (context.Web as Web).RawRequestBatchAsync(batch, BuildSetChromeOptionsApiCall(chromeOptions), HttpMethod.Post, "SetChromeOptions").ConfigureAwait(false);

            if (chromeOptions.Navigation != null)
            {
                // Update the navigation visibility
                await (context.Web as Web).RawRequestBatchAsync(batch, BuildQuickLaunchEnabledApiCall(chromeOptions), new HttpMethod("PATCH"), "Update").ConfigureAwait(false);

                // Update the footer displayName
                if (chromeOptions.Footer != null)
                {
                    await BuildAndAddSaveMenuStateRequestAsync(chromeOptions.Footer as FooterOptions, null, batch).ConfigureAwait(false);
                }
            }

            // Get notified when the batch is processed so that 
            batch.BatchExecuted = () => {
                if (!batch.HasErrors)
                {
                    // Update the chrome updates in the current web
                    ReflectChromeUpdatesInCurrentWeb(context, chromeOptions);
                }
            };
        }

        public void SetChromeOptions(IChromeOptions chromeOptions)
        {
            SetChromeOptionsAsync(chromeOptions).GetAwaiter().GetResult();
        }

        private static ApiCall BuildSetChromeOptionsApiCall(IChromeOptions chromeOptions)
        {
            var body = new
            {
                headerLayout = chromeOptions.Header.Layout,
                headerEmphasis = chromeOptions.Header.Emphasis,
                hideTitleInHeader = chromeOptions.Header.HideTitle,
                logoAlignment = chromeOptions.Header.LogoAlignment,
                megaMenuEnabled = chromeOptions.Navigation != null ? chromeOptions.Navigation.MegaMenuEnabled : false,
                horizontalQuickLaunch = chromeOptions.Navigation != null ? chromeOptions.Navigation.HorizontalQuickLaunch : false,
                footerEnabled = chromeOptions.Footer != null ? chromeOptions.Footer.Enabled : false,
                footerLayout = chromeOptions.Footer != null ? chromeOptions.Footer.Layout : FooterLayoutType.Simple,
                footerEmphasis = chromeOptions.Footer != null ? chromeOptions.Footer.Emphasis : FooterVariantThemeType.Strong
            };

            string jsonBody = JsonSerializer.Serialize(body);

            return new ApiCall("_api/web/SetChromeOptions", ApiType.SPORest, jsonBody);
        }

        private static ApiCall BuildQuickLaunchEnabledApiCall(IChromeOptions chromeOptions)
        {
            var body = new
            {
                __metadata = new
                {
                    type = "SP.Web"
                },
                QuickLaunchEnabled = chromeOptions.Navigation != null ? chromeOptions.Navigation.Visible : true,
            };

            string jsonBody = JsonSerializer.Serialize(body);

            return new ApiCall("_api/web", ApiType.SPORest, jsonBody);
        }

        public async Task SetChromeOptionsBatchAsync(Batch batch, IChromeOptions chromeOptions)
        {
            await BuildSetChromeOptionsRequests(chromeOptions, batch).ConfigureAwait(false);
        }

        public void SetChromeOptionsBatch(Batch batch, IChromeOptions chromeOptions)
        {
            SetChromeOptionsBatchAsync(batch, chromeOptions).GetAwaiter().GetResult();
        }

        public async Task SetChromeOptionsBatchAsync(IChromeOptions chromeOptions)
        {
            await SetChromeOptionsBatchAsync(context.CurrentBatch, chromeOptions).ConfigureAwait(false);
        }

        public void SetChromeOptionsBatch(IChromeOptions chromeOptions)
        {
            SetChromeOptionsBatchAsync(chromeOptions).GetAwaiter().GetResult();
        }

        internal async Task<IBatchSingleResult<MenuState>> GetMenuStateBatchAsync(Batch batch)
        {
            ApiCall apiCall = BuildGetMenuStateApiCall();

            // Since we're doing a raw batch request the processing of the batch response needs be implemented
            apiCall.RawSingleResult = new MenuState();
            apiCall.RawResultsHandler = (json, apiCall) =>
            {
                ProcessGetMenuStateResponse(json, (MenuState)apiCall.RawSingleResult);
            };

            // Add the request to the batch
            var batchRequest = await (context.Web as Web).RawRequestBatchAsync(batch, apiCall, HttpMethod.Get).ConfigureAwait(false);

            // Return the batch result as Enumerable
            return new BatchSingleResult<MenuState>(batch, batchRequest.Id, (MenuState)apiCall.RawSingleResult);
        }

        private static ApiCall BuildGetMenuStateApiCall()
        {
            return new ApiCall("_api/navigation/MenuState?menuNodeKey='13b7c916-4fea-4bb2-8994-5cf274aeb530'", ApiType.SPORest);
        }

        internal static void ProcessGetMenuStateResponse(string jsonString, MenuState menuState)
        {
            var jsonMenuState = JsonSerializer.Deserialize<MenuState>(jsonString);

            menuState.FriendlyUrlPrefix = jsonMenuState.FriendlyUrlPrefix;
            menuState.SimpleUrl = jsonMenuState.SimpleUrl;
            menuState.SPSitePrefix = jsonMenuState.SPSitePrefix;
            menuState.SPWebPrefix = jsonMenuState.SPWebPrefix;
            menuState.StartingNodeKey = jsonMenuState.StartingNodeKey;
            menuState.StartingNodeTitle = jsonMenuState.StartingNodeTitle;
            menuState.Version = jsonMenuState.Version;
            menuState.Nodes = jsonMenuState.Nodes;
        }

        private async Task BuildAndAddSaveMenuStateRequestAsync(FooterOptions footerOptions, string serverRelativeUrl, Batch batch)
        {
            await (context.Web as Web).RawRequestBatchAsync(batch, BuildSaveMenuStateApiCall(footerOptions, serverRelativeUrl), HttpMethod.Post, "SaveMenuState").ConfigureAwait(false);
        }

        internal static ApiCall BuildSaveMenuStateApiCall(FooterOptions footerOptions, string serverRelativeUrl)
        {
            string jsonBody = JsonSerializer.Serialize(footerOptions.GetMenuStateToPersist(serverRelativeUrl));

            var apiCall = new ApiCall("_api/navigation/SaveMenuState", ApiType.SPORest, jsonBody)
            {
                // The provided JSON is of the minimal odata type
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json;odata.metadata=nometadata" },
                }
            };
            return apiCall;
        }

        private static void ReflectChromeUpdatesInCurrentWeb(PnPContext context, IChromeOptions chromeOptions)
        {
            context.Web.SetSystemProperty(p => p.HeaderEmphasis, chromeOptions.Header.Emphasis);
            context.Web.SetSystemProperty(p => p.HeaderLayout, chromeOptions.Header.Layout);
            context.Web.SetSystemProperty(p => p.HideTitleInHeader, chromeOptions.Header.HideTitle);
            context.Web.SetSystemProperty(p => p.LogoAlignment, chromeOptions.Header.LogoAlignment);

            if (chromeOptions.Navigation != null)
            {
                context.Web.SetSystemProperty(p => p.MegaMenuEnabled, chromeOptions.Navigation.MegaMenuEnabled);
                context.Web.SetSystemProperty(p => p.QuickLaunchEnabled, chromeOptions.Navigation.Visible);
            }

            if (chromeOptions.Footer != null)
            {
                context.Web.SetSystemProperty(p => p.FooterEmphasis, chromeOptions.Footer.Emphasis);
                context.Web.SetSystemProperty(p => p.FooterEnabled, chromeOptions.Footer.Enabled);
                context.Web.SetSystemProperty(p => p.FooterLayout, chromeOptions.Footer.Layout);
            }
        }

        #endregion
    }
}

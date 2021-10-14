namespace PnP.Core.Transformation.SharePoint.MappingProviders.HtmlMapping
{
    /// <summary>
    /// Methods to convert colors
    /// </summary>
    internal static class CodeConversion
    {
        /// <summary>
        /// Map wiki table style to a RTE compatible style
        /// </summary>
        /// <param name="tableStyleCode">Code used for the wiki table style</param>
        /// <returns>RTE compatible table style</returns>
        internal static string TableStyleCodeToName(int tableStyleCode)
        {
            //ms-rteTable-default: basic grid lines
            //ms-rteTable-0: no grid
            //ms-rteTable-1: table style 2, light banded: no header, alternating light gray rows, no grid
            //ms-rteTable-2: table style 4, light lines: header, no alternating rows, no grid
            //ms-rteTable-3: table style 5, grid: no header, no alternating rows, grid
            //ms-rteTable-4: table style 6, Accent 1: header, no alternating rows, grid, blue colors
            //ms-rteTable-5: table style 7, Accent 2: header, no alternating rows, grid, light blue colors
            //ms-rteTable-6: table style 3, medium two tones: header with alternating blue colors, no grid
            //ms-rteTable-7: table style 8, Accent 3: header, no alternating rows, grid, green colors
            //ms-rteTable-8: table style 9, Accent 4: header, no alternating rows, grid, brownish colors
            //ms-rteTable-9: table style 10, Accent 5: header, no alternating rows, grid, red colors
            //ms-rteTable-10: table style 11, Accent 6: header, no alternating rows, grid, purple colors

            switch (tableStyleCode)
            {
                case 0:
                case 3:
                    {
                        return "simpleTableStyleNeutral";
                    }
                case 1:
                    {
                        return "bandedRowTableStyleNeutral";
                    }
                case 2:
                    {
                        return "filledHeaderTableStyleNeutral";
                    }
                case 4:
                case 5:
                case 7:
                case 8:
                case 9:
                case 10:
                    {
                        return "filledHeaderTableStyleTheme";
                    }
                case 6:
                    {
                        return "bandedRowTableStyleTheme";
                    }
            }

            return "borderHeaderTableStyleNeutral";
        }

        /// <summary>
        /// Translates SharePoint wiki font size (e.g. ms-rtefontsize-3 means font size 3) to RTE font size name
        /// </summary>
        /// <param name="fontCode">Wiki font size code</param>
        /// <returns>RTE font size name</returns>
        internal static string FontCodeToName(int fontCode)
        {
            switch (fontCode)
            {
                case 1: //8pt
                    {
                        return "Small";
                    }
                case 2: //10t
                    {
                        return "Medium";
                    }
                case 3: //12pt
                    {
                        return "MediumPlus";
                    }
                case 4: //18pt 
                    {
                        // Return empty as this will be mapped to default size
                        return "";
                    }
                case 5: //24pt
                    {
                        return "XxLarge";
                    }
                case 6: //36pt
                    {
                        return "XxxLarge";
                    }
                case 7: //48pt
                    {
                        return "XxLargePlus";
                    }
                case 8: //72pt
                    {
                        return "Super";
                    }
            }

            return null;
        }

        /// <summary>
        /// Translated SharePoint Wiki foreground color number (ms-rteforecolor-2 means number 2 is used) to RTE compatible color name
        /// </summary>
        /// <param name="colorCode">Used color number</param>
        /// <returns>RTE color string</returns>
        internal static string ColorCodeToForegroundColorName(int colorCode)
        {
            return colorCode switch
            {
                1 => "RedDark",
                2 => "Red",
                3 => "Yellow",
                4 => "YellowLight",
                5 => "GreenLight",
                6 => "Green",
                7 => "BlueLight",
                8 => "Blue",
                9 => "BlueDark",
                10 => "Purple",
                _ => null
            };
        }

        /// <summary>
        /// Translated SharePoint Wiki foreground theme color number (e.g. ms-rteThemeForeColor-6-1) to RTE compatible color name
        /// </summary>
        /// <param name="themeCode">Theme color code</param>
        /// <returns>RTE color string</returns>
        internal static string ThemeCodeToForegroundColorName(int themeCode)
        {
            return themeCode switch
            {
                0 =>
                    // 0 (light) will go to NeutralPrimary which is the default, hence returning null
                    null,
                1 => "ThemeSecondary",
                2 => "ThemePrimary",
                3 => "ThemeDarkAlt",
                4 => "ThemeDark",
                5 => "ThemeDarker",
                _ => null
            };
        }

        /// <summary>
        /// Translated SharePoint Wiki background color number (ms-rtebackcolor-5 means number 5 is used) to RTE compatible color name
        /// </summary>
        /// <param name="colorCode">Used color number</param>
        /// <returns>RTE color string</returns>
        internal static string ColorCodeToBackgroundColorName(int colorCode)
        {
            return colorCode switch
            {
                1 => "Maroon",
                2 => "Red",
                3 => "Yellow",
                4 => "Yellow",
                5 => "Green",
                6 => "Green",
                7 => "Aqua",
                8 => "Blue",
                9 => "DarkBlue",
                10 => "Purple",
                _ => null
            };
        }
    }
}

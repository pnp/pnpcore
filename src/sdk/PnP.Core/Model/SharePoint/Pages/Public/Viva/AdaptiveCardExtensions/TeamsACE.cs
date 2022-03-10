using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint.Pages.Public.Viva.AdaptiveCardExtensions
{
    /// <summary>
    /// Represents Teams App ACE
    /// </summary>
    public class TeamsACE: AdaptiveCardExtension<TeamsACEProperties>
    {
        public TeamsACE()
        {
            Id = "3f2506d3-390c-426e-b272-4b4ec0ee4d2d";
            Title = "Teams App";
            Description = "When a user selects this card, it will open a Teams app. Select from a variety of Personal apps or Bots by searching for the one you want to use.";
        }
    }
}

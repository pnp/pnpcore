using System;
using System.Collections.Generic;

namespace PnP.Core.Provisioning.ObjectHandlers.Utilities
{
    /// <summary>
    /// One page the extractor has decided to export, with everything needed to place it in the
    /// template correctly.
    /// </summary>
    internal sealed class PageToExport
    {
        /// <summary>The page's name, including any folder, relative to the pages library.</summary>
        internal string PageName { get; set; }

        /// <summary>The page's server relative url.</summary>
        internal string PageUrl { get; set; }

        /// <summary>Whether this page is the web's welcome page.</summary>
        internal bool IsHomePage { get; set; }

        /// <summary>Whether this page lives in the pages library's templates folder.</summary>
        internal bool IsTemplate { get; set; }

        /// <summary>Whether this page is a translation of another page.</summary>
        internal bool IsTranslation { get; set; }

        /// <summary>The page's unique id, used to pair translations with their source.</summary>
        internal Guid PageId { get; set; }

        /// <summary>The unique id of the page this one translates, when it is a translation.</summary>
        internal Guid SourcePageId { get; set; }

        /// <summary>The name of the page this one translates, resolved from <see cref="SourcePageId"/>.</summary>
        internal string SourcePageName { get; set; }

        /// <summary>The culture of this translation, for example <c>nl-NL</c>.</summary>
        internal string Language { get; set; }

        /// <summary>The cultures this page has been translated into.</summary>
        internal List<string> TranslatedLanguages { get; set; }
    }
}

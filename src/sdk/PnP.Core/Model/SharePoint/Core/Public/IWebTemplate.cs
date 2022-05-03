namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Specifies a site definition or a site template that is used to instantiate a site.
    /// </summary>
    [ConcreteType(typeof(WebTemplate))]
    public interface IWebTemplate
    {
        /// <summary>
        /// Specifies the description for the site definition configuration or site template.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Specifies the display name for the category that this site definition configuration or site template is a part of.
        /// </summary>
        public string DisplayCategory { get; }

        /// <summary>
        /// Specifies the unique identifier for the site definition configuration or the site template.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Specifies the URI for the image that is associated with the site definition configuration or site template.
        /// </summary>
        public string ImageUrl { get; }

        /// <summary>
        /// Specifies whether the site definition configuration or site template can only be applied to the top-level site in the site collection.
        /// </summary>
        public bool IsHidden { get; }

        /// <summary>
        /// Specifies whether the site definition configuration or site template can only be applied to subsites created within the site collection.
        /// </summary>
        public bool IsRootWebOnly { get; }

        /// <summary>
        /// Specifies whether the template can only be applied to subwebs
        /// </summary>
        public bool IsSubWebOnly { get; }

        /// <summary>
        /// Specifies the LCID for the site definition configuration or site template.
        /// </summary>
        public int Lcid { get; }

        /// <summary>
        /// Specifies the name for the site definition configuration or site template.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Specifies the display name for the site definition configuration or site template.
        /// </summary>
        public string Title { get; }
    }
}

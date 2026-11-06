namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// An image rendition as reported by the publishing CSOM API.
    /// </summary>
    internal sealed class ImageRenditionInfo
    {
        /// <summary>
        /// Server-assigned rendition id.
        /// </summary>
        internal int Id { get; set; }

        /// <summary>
        /// The rendition's name, which is what a template refers to it by.
        /// </summary>
        internal string Name { get; set; }

        /// <summary>
        /// Rendition width in pixels.
        /// </summary>
        internal int Width { get; set; }

        /// <summary>
        /// Rendition height in pixels.
        /// </summary>
        internal int Height { get; set; }

        /// <summary>
        /// Server-assigned version, bumped whenever the rendition changes.
        /// </summary>
        internal int Version { get; set; }
    }
}

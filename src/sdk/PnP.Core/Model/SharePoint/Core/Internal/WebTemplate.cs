namespace PnP.Core.Model.SharePoint
{
    internal sealed class WebTemplate : IWebTemplate
    {
        public string Description { get; set; }

        public string DisplayCategory { get; set; }

        public int Id { get; set; }

        public string ImageUrl { get; set; }

        public bool IsHidden { get; set; }

        public bool IsRootWebOnly { get; set; }

        public bool IsSubWebOnly { get; set; }

        public int Lcid { get; set; }

        public string Name { get; set; }

        public string Title { get; set; }
    }
}

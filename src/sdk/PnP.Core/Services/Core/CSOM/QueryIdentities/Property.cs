namespace PnP.Core.Services.Core.CSOM.QueryIdentities
{
    internal class Property : Identity
    {
        internal int ParentId { get; set; }

        public override string ToString()
        {
            return $"<Property Id=\"{Id}\" ParentId=\"{ParentId}\" Name=\"{Name}\" />";
        }
    }
}

namespace PnP.Core.Services.Core.CSOM.QueryIdentities
{
    internal class StaticProperty : Identity
    {
        internal string TypeId { get; set; }
        public override string ToString()
        {
            return $"<StaticProperty Id=\"{Id}\" TypeId=\"{TypeId}\" Name=\"{Name}\" />";
        }
    }
}

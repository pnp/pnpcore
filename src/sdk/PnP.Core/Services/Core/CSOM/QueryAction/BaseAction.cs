namespace PnP.Core.Services.Core.CSOM.QueryAction
{
    internal class BaseAction
    {
        internal int Id { get; set; }
        internal string ObjectPathId { get; set; }
        public override string ToString()
        {
            return $"<ObjectPath Id=\"{Id}\" ObjectPathId=\"{ObjectPathId}\" />";
        }
    }
}

namespace PnP.Core.Services.Core.CSOM.QueryAction
{
    internal sealed class QueryAction : BaseAction
    {
        internal SelectQuery SelectQuery { get; set; }
        internal ChildItemQuery ChildItemQuery { get; set; }

        public override string ToString()
        {
            return $"<Query Id=\"{Id}\" ObjectPathId=\"{ObjectPathId}\" >{SelectQuery}{ChildItemQuery}</Query>";
        }
    }
}

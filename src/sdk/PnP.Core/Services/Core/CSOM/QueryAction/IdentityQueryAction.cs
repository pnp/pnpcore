namespace PnP.Core.Services.Core.CSOM.QueryAction
{
    internal class IdentityQueryAction : BaseAction
    {
        public override string ToString()
        {
            return $"<ObjectIdentityQuery Id=\"{Id}\" ObjectPathId=\"{ObjectPathId}\" />";
        }
    }
}

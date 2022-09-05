namespace PnP.Core.Services.Core.CSOM.QueryAction
{
    internal sealed class IdentityQueryAction : BaseAction
    {
        public override string ToString()
        {
            return $"<ObjectIdentityQuery Id=\"{Id}\" ObjectPathId=\"{ObjectPathId}\" />";
        }
    }
}

namespace PnP.Core.Services.Core.CSOM.Utils
{
    internal sealed class IteratorIdProvider : IIdProvider
    {
        internal int Id { get; private set; }

        public int GetActionId()
        {
            Id++;
            return Id;
        }
    }
}

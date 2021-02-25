namespace PnP.Core.Services.Core.CSOM.Utils
{
    internal class IteratorIdProvider : IIdProvider
    {
        protected int Id { get; private set; }

        public int GetActionId()
        {
            Id++;
            return Id;
        }
    }
}

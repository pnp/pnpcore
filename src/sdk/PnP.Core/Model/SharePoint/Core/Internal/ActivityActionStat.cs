namespace PnP.Core.Model.SharePoint
{
    internal class ActivityActionStat : IActivityActionStat
    {
        public int ActionCount { get; set; }

        public int ActorCount { get; set; }

        public int TimeSpentInSeconds { get; set; }
    }
}

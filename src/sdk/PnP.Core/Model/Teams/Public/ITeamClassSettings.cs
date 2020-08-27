namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Public interface to define the settings for a classroom
    /// </summary>
    public interface ITeamClassSettings
    {
        /// <summary>
        /// Do guardians need to be notified about assignments?
        /// </summary>
        public bool NotifyGuardiansAboutAssignments { get; set; }
    }
}

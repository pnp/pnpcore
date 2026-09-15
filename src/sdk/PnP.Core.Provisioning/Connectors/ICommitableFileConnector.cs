namespace PnP.Core.Provisioning.Connectors
{
    /// <summary>
    /// Interface for File Connectors
    /// </summary>
    public interface ICommitableFileConnector
    {
        /// <summary>
        /// Commits the file
        /// </summary>
        void Commit();
    }
}

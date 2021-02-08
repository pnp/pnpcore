namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Public interface to define a collection of TeamApp objects of Microsoft Teams
    /// </summary>
    [ConcreteType(typeof(TeamAppCollection))]
    public interface ITeamAppCollection : IDataModelCollection<ITeamApp>, ISupportPaging<ITeamApp>, ISupportLoad<ITeamApp>
    {
    }
}

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Members on this team tag
    /// </summary>
    [ConcreteType(typeof(TeamTagMemberCollection))]
    public interface ITeamTagMemberCollection : IDataModelCollection<ITeamTagMember>, IDataModelCollectionLoad<ITeamTagMember>, ISupportQuery<ITeamTagMember>, ISupportModules<ITeamTagMemberCollection>
    {
    }
}

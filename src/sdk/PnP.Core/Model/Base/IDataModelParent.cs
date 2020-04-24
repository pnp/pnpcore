namespace PnP.Core.Model
{
    /// <summary>
    /// Interface to implement parent concept on all model objects
    /// </summary>
    public interface IDataModelParent
    {
        IDataModelParent Parent { get; set; }
    }
}
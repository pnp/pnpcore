namespace CamlBuilder
{
    /// <summary>
    /// Defines a CAML statement. It can be a <see cref="LogicalJoin"/> or a <see cref="Operator"/>. 
    /// </summary>
    internal abstract class Statement
    {
        public abstract string GetCaml();
    }
}

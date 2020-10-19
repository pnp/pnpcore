using System;
using System.Linq.Expressions;

namespace PnP.Core.Model
{

    /// <summary>
    /// Interface describing a complex type
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1040:Avoid empty interfaces", Justification = "<Pending>")]
    public interface IComplexType
    {
    }

    /// <summary>
    /// Interface describing a complex type
    /// </summary>
    public interface IComplexType<TModel>: IComplexType
    {
        /// <summary>
        /// Checks if a property is loaded or not
        /// </summary>
        /// <param name="expression">Expression listing the property to load</param>
        /// <returns>True if property was loaded, false otherwise</returns>
        bool IsPropertyAvailable(Expression<Func<TModel, object>> expression);
    }
}

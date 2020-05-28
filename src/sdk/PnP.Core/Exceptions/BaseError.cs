using System;
using System.Collections.Generic;

namespace PnP.Core
{
    /// <summary>
    /// Base error information
    /// </summary>
    public abstract class BaseError
    {
        public BaseError(ErrorType type)
        {
            Type = type;
        }

        /// <summary>
        /// <see cref="ErrorType"/> of the error
        /// </summary>
        public ErrorType Type { get; private set; }

        /// <summary>
        /// Correlation for a PnP Core SDK operation
        /// </summary>
        public Guid PnPCorrelationId { get; internal set; }

        /// <summary>
        /// Additional data linked to an error
        /// </summary>
        public IDictionary<string, object> AdditionalData { get; internal set; }

        protected void AddAdditionalData(string propertyName, object propertyValue)
        {
            if (AdditionalData == null)
            {
                AdditionalData = new Dictionary<string, object>();
            }
            AdditionalData.Add(propertyName, propertyValue);
        }
    }
}

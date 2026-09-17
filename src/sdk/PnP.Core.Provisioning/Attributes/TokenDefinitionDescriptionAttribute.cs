using System;

namespace PnP.Core.Provisioning.Attributes
{
    /// <summary>
    /// Documents a provisioning engine token: what it looks like, what it means, and what it
    /// resolves to. Applied to <see cref="ObjectHandlers.TokenDefinitions.TokenDefinition"/>
    /// subclasses, once per token the definition supplies.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class TokenDefinitionDescriptionAttribute : Attribute
    {
        /// <summary>
        /// The token itself, with placeholders in square brackets - e.g. <c>{fieldid:[internalname]}</c>.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// What the token resolves to, in prose.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// A sample resolved value.
        /// </summary>
        public string Returns { get; set; }

        /// <summary>
        /// A concrete use of the token, with the placeholders filled in.
        /// </summary>
        public string Example { get; set; }
    }
}

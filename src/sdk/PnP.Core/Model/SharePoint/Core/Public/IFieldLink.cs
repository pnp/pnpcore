using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a FieldLink object
    /// </summary>
    [ConcreteType(typeof(FieldLink))]
    public interface IFieldLink : IDataModel<IFieldLink>, IDataModelGet<IFieldLink>, IDataModelUpdate, IDataModelDelete
    {
        /// <summary>
        /// Gets or sets the display name of the field in the field link.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the field internal name specified in the field link.
        /// </summary>
        public string FieldInternalName { get; set; }

        /// <summary>
        /// Gets or sets a Boolean value that specifies whether the field is displayed in forms that can be edited.
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// Gets a value that specifies the GUID of the FieldLink.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets a value that specifies the name of the FieldLink.
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// Gets or sets whether the field is read-only.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>")]
        public bool ReadOnly { get; set; }

        /// <summary>
        /// Gets or sets whether the field is required.
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Gets or sets whether the field should be shown in display form.
        /// </summary>
        public bool ShowInDisplayForm { get; set; }
    }
}

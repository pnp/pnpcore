using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Options for creating a view
    /// </summary>
    public class ViewOptions
    {
        
        /// <summary>
        /// Gets or sets the associated content type id
        /// </summary>
        public string AssociatedContentTypeId { get; set; }
        
        /// <summary>
        /// Gets or sets the base view Id
        /// </summary>
        [SharePointProperty("BaseViewId", JsonPath="baseViewId")]
        public int? BaseViewId { get; set; }
        
        /// <summary>
        /// Gets of sets the Calendar view styles
        /// </summary>
        public string CalendarViewStyles { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether the new list view is a paged view.
        /// </summary>
        public bool Paged { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether the new list view is a personal view. If the value is false, the new list view is a public view.
        /// </summary>
        public bool PersonalView { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the query for the new list view.
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the maximum number of list items that the new list view displays on a visual page of the list view.
        /// </summary>
        public int? RowLimit { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether the new list view is the default list view.
        /// </summary>
        public bool SetAsDefaultView { get; set; }

        /// <summary>
        /// Gets or sets the value that specifies the display name of the new list view.
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Gets or sets the view data
        /// </summary>
        public string ViewData { get; set; }

        /// <summary>
        /// Gets or sets the value that specifies the collection of field internal names for the list fields in the new list view.
        /// </summary>
#pragma warning disable CA1819 // Properties should not return arrays
        public string[] ViewFields { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays
        /// <summary>
        /// Gets or sets a value that specifies the type of the new list view.
        /// </summary>
        public int? ViewTypeKind { get; set; }
        
        /// <summary>
        /// Gets or sets the View Type 2 information
        /// </summary>
        public string ViewType2 { get; set; }
    }
}

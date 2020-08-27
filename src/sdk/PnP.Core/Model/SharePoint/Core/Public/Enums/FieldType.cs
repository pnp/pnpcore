namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Specifies the type of the field.
    /// https://docs.microsoft.com/en-us/previous-versions/office/sharepoint-server/ee540543(v=office.15)
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "<Pending>")]
    public enum FieldType
    {
        /// <summary>
        /// Must not be used. The value = 0.
        /// </summary>
        Invalid = 0,
        /// <summary>
        /// Specifies that the field contains an integer value. The value = 1.
        /// </summary>
        Integer = 1,
        /// <summary>
        /// Specifies that the field contains a single line of text. The value = 2.
        /// </summary>
        Text = 2,
        /// <summary>
        /// Specifies that the field contains multiple lines of text. The value = 3.
        /// </summary>
        Note = 3,
        /// <summary>
        /// Specifies that the field contains a date and time value or a date-only value. The value = 4.
        /// </summary>
        DateTime = 4,
        /// <summary>
        /// Specifies that the field contains a monotonically increasing integer. The value = 5.
        /// </summary>
        Counter = 5,
        /// <summary>
        /// Specifies that the field contains a single value from a set of specified values. The value = 6.
        /// </summary>
        Choice = 6,
        /// <summary>
        /// Specifies that the field is a lookup field. The value = 7.
        /// </summary>
        Lookup = 7,
        /// <summary>
        /// Specifies that the field contains a Boolean value. The value = 8.
        /// </summary>
        Boolean = 8,
        /// <summary>
        /// Specifies that the field contains a floating-point number value. The value = 9.
        /// </summary>
        Number = 9,
        /// <summary>
        /// Specifies that the field contains a currency value. The value = 10.
        /// </summary>
        Currency = 10,
        /// <summary>
        /// Specifies that the field contains a URI and an optional description of the URI. The value = 11.
        /// </summary>
        URL = 11,
        /// <summary>
        /// Specifies that the field is a computed field. The value = 12.
        /// </summary>
        Computed = 12,
        /// <summary>
        /// Specifies that the field indicates the thread for a discussion item in a threaded view of a discussion board. The value = 13.
        /// </summary>
        Threading = 13,
        /// <summary>
        /// Specifies that the field contains a GUID value. The value = 14.
        /// </summary>
        Guid = 14,
        /// <summary>
        /// Specifies that the field contains one or more values from a set of specified values. The value = 15.
        /// </summary>
        MultiChoice = 15,
        /// <summary>
        /// Specifies that the field contains rating scale values for a survey list. The value = 16.
        /// </summary>
        GridChoice = 16,
        /// <summary>
        /// Specifies that the field is a calculated field. The value = 17.
        /// </summary>
        Calculated = 17,
        /// <summary>
        /// Specifies that the field contains the leaf name of a document as a value. The value = 18.
        /// </summary>
        File = 18,
        /// <summary>
        /// Specifies that the field indicates whether the list item has attachments. The value = 19.
        /// </summary>
        Attachments = 19,
        /// <summary>
        /// Specifies that the field contains one or more users and groups as values. The value = 20.
        /// </summary>
        User = 20,
        /// <summary>
        /// 	Specifies that the field indicates whether a meeting in a calendar list recurs. The value = 21.
        /// </summary>
        Recurrence = 21,
        /// <summary>
        /// Specifies that the field contains a link between projects in a Meeting Workspace site. The value = 22.
        /// </summary>
        CrossProjectLink = 22,
        /// <summary>
        /// Specifies that the field indicates moderation status. The value = 23.
        /// </summary>
        ModStat = 23,
        /// <summary>
        /// Specifies that the type of the field was set to an invalid value. The value = 24.
        /// </summary>
        Error = 24,
        /// <summary>
        /// Specifies that the field contains a content type identifier as a value. The value = 25.
        /// </summary>
        ContentTypeId = 25,
        /// <summary>
        /// Specifies that the field separates questions in a survey list onto multiple pages. The value = 26.
        /// </summary>
        PageSeparator = 26,
        /// <summary>
        /// Specifies that the field indicates the position of a discussion item in a threaded view of a discussion board. The value = 27.
        /// </summary>
        ThreadIndex = 27,
        /// <summary>
        /// Specifies that the field indicates the status of a workflow instance on a list item. The value = 28.
        /// </summary>
        WorkflowStatus = 28,
        /// <summary>
        /// Specifies that the field indicates whether a meeting in a calendar list is an all-day event. The value = 29.
        /// </summary>
        AllDayEvent = 29,
        /// <summary>
        /// Specifies that the field contains the most recent event in a workflow instance. The value = 30.
        /// </summary>
        WorkflowEventType = 30,
        /// <summary>
        /// Must not be used. The value = 31.
        /// </summary>
        MaxItems = 35,

        // NOT Supported for now
        // TODO Check the actual values
        //Geolocation = 31,
        //OutcomeChoice = 32,
        //Location = 33,
        //Thumbnail = 34,
    }
}

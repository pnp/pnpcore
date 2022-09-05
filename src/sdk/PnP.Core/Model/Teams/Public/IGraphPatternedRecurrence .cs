namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// The recurrence pattern and range. This shared object is used to define the recurrence of the following objects:
    ///    - accessReviewScheduleDefinition objects in Azure AD access reviews APIs
    ///    - event objects in the calendar API
    ///    - unifiedRoleAssignmentScheduleRequest and unifiedRoleEligibilityScheduleRequest objects in PIM
    ///    - accessPackageAssignment objects in Azure AD entitlement management
    /// </summary>
    [ConcreteType(typeof(GraphPatternedRecurrence))]
    public interface IGraphPatternedRecurrence : IDataModel<IGraphPatternedRecurrence>
    {
        /// <summary>
        /// The frequency of an event. 
        /// </summary>
        public IGraphRecurrencePattern Pattern { get; set; }

        /// <summary>
        /// The duration of an event.
        /// </summary>
        public IGraphRecurrenceRange Range { get; set; }
    }
}

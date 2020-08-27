namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Defines the Specialization options for a Team
    /// </summary>
    public enum TeamSpecialization
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,

        /// <summary>
        /// Education Standard
        /// </summary>
        EducationStandard = 1,
        
        /// <summary>
        /// Education class
        /// </summary>
        EducationClass = 2,
        
        /// <summary>
        /// Education professional learning community
        /// </summary>
        EducationProfessionalLearningCommunity = 3,
        
        /// <summary>
        /// Education staff
        /// </summary>
        EducationStaff = 4,

        /// <summary>
        /// Reserved for future use
        /// </summary>
        UnknownFutureValue = 7
    }
}

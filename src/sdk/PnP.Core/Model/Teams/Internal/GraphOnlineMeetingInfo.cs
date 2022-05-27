using System.Collections.Generic;

namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal sealed class GraphOnlineMeetingInfo : BaseDataModel<IGraphOnlineMeetingInfo>, IGraphOnlineMeetingInfo
    {
        #region Construction
        public GraphOnlineMeetingInfo()
        {
        }

        #endregion

        #region Properties


        public string ConferenceId { get => GetValue<string>(); set => SetValue(value); }

        public string JoinUrl { get => GetValue<string>(); set => SetValue(value); }

        public IGraphPhoneCollection Phones { get => GetModelCollectionValue<IGraphPhoneCollection>(); set => SetValue(value); }

        public string QuickDial { get => GetValue<string>(); set => SetValue(value); }

        public List<string> TollFreeNumbers { get => GetValue<List<string>>(); set => SetValue(value); }

        public string TollNumber { get => GetValue<string>(); set => SetValue(value); }

        #endregion
    }
}

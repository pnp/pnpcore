using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.ASPNetCore.Models
{
    public class TeamInfoViewModel
    {
        public string DisplayName { get; set; }

        public string Description { get; set; }

        public List<ChannelViewModel> Channels { get; set; }
    }

    public class ChannelViewModel
    {
        public string DisplayName { get; set; }

        public string Id { get; set; }
    }
}

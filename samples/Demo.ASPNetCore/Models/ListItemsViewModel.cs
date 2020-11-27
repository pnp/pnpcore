using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.ASPNetCore.Models
{
    public class ListItemsViewModel
    {
        public string ListTitle { get; set; }

        public List<ListItemViewModel> Items { get; set; }
    }

    public class ListItemViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }
    }
}

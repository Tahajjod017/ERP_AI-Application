using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.DataTables
{
    public class Order
    {
        public int Column { get; set; }
        public string Dir { get; set; }

        public ListSortDirection GetSortDirection()
        {
            if (string.IsNullOrWhiteSpace(Dir))
            {
                return ListSortDirection.Ascending;
            }

            return Dir.Equals("asc", StringComparison.CurrentCultureIgnoreCase) ? ListSortDirection.Ascending : ListSortDirection.Descending;
        }
    }
}

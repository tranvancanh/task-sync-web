using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace task_sync_web.Models
{
    public class PageViewModel
    {
        public int PageNumber { get; set; }

        public int PageRowCount { get; set; }
        public int PageRowStartNumber { get; set; }
        public int PageRowEndNumber { get; set; }
    }
}

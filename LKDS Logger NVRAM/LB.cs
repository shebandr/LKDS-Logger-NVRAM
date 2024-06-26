using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LKDS_Logger_NVRAM
{
    public class LB
    {
        public string LBName { get; set; }
        public int LBId { get; set; }
        public string LBKey { get; set; }
        public string LBIpString { get; set; }
        public int LBPort { get; set; }
        public string LBLastChange { get; set; }
        public string LBLastDump { get; set; }
        public string LBStatus { get; set; }
    }
}

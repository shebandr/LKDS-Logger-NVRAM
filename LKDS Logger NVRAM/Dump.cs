using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LKDS_Logger_NVRAM
{
    public class Dump
    {
        public string TimeDate { get; set; }
        public int LBId { get; set; }
        public bool IsChanged { get; set; }
        public List<byte> Data { get; set; }
    }
}

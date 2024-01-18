using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tardis.Models
{
    public class NeighborNode
    {
        public string Name { get; set; }
        public DateTime LastCommunication { get; set; }
        public string Status { get; set; }
    }
}

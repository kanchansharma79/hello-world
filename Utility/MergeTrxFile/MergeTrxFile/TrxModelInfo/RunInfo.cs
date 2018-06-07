using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MergeTrxFile.TrxModelInfo
{
    public class RunInfo
    {
        // Attributes
        public string ComputerName { get; set; }
        public string Outcome { get; set; }
        public string Timestamp { get; set; }
        // Inner Text
        public string Text { get; set; }
        //Child node
        public string Exception { get; set; }
    } 
}

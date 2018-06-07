using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MergeTrxFile.TrxModelInfo
{
    public class ResultSummary
    {
        //Attributes of ResultSummary tag
        public string Outcome { get; set; }

        //:Child nodes
        public Counters Counters { get; set; }
        public List<RunInfo> RunInfos { get; set; }
    }
}

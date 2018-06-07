using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MergeTrxFile.TrxModelInfo
{
    public class UnitTest
    {
        //Attributes of UnitTest tag
        public string Name { get; set; }
        public string Storage { get; set; }
        public string Id { get; set; }
        
        //Child nodes
        public TcmInformation TcmInformation { get; set; }
        public Execution Execution { get; set; }
        public TestMethod TestMethod { get; set; }
    }

}

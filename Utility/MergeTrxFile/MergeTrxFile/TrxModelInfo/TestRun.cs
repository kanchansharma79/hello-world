using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MergeTrxFile.TrxModelInfo
{
    public class TestRun
    {
        //Attributes of root TestRun tag
        public string Id { get; set; }
        public string Name { get; set; }
        public string RunUser { get; set; }

        #region :Child nodes
        // Get the total duration from start and end of all tests
        public Times Times { get; set; }
        public string Duration
        {
            get
            {
                return (DateTime.Parse(Times.Finish) - DateTime.Parse(Times.Start)).ToString("hh\\:mm\\:ss");
            }
        }
        
        // Test run result summary outcome
        public ResultSummary ResultSummary { get; set; }
        public List<UnitTest> TestDefinitions { get; set; }
        public List<TestList> TestLists { get; set; }
        public List<TestEntry> TestEntries { get; set; }
        public List<UnitTestResult> Results { get; set; }

        #endregion
    }
}

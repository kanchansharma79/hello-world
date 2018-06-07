using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MergeTrxFile.Utilities;
using MergeTrxFile.TrxModelInfo;
using System.Configuration;
using System.IO;

namespace MergeTrxFile
{
    public static class TestRunMerger
    {
        public static TestRun MergeTRXsAndSave(List<string> trxFiles, string outputTrxFileName)
        {
            Console.WriteLine("De-serializing trx files:");
            List<TestRun> runs = new List<TestRun>();

            foreach (var trx in trxFiles)
            {
                Console.WriteLine(trx);
                runs.Add(TRXSerializationUtils.DeserializeTRX(trx));
            }

            Console.WriteLine("Combining de-serialized trx files...");
            var combinedTestRun = MergeTestRuns(runs);

            Console.WriteLine("Saving result...");
            var savedFile = TRXSerializationUtils.SerializeAndSaveTestRun(combinedTestRun, outputTrxFileName);

            Console.WriteLine("Operation completed:");
            Console.WriteLine("\tCombined trx files: " + trxFiles.Count);
            Console.WriteLine("\tResult trx file: " + savedFile);

            return combinedTestRun;
        }

        private static TestRun MergeTestRuns(List<TestRun> testRuns)
        {
            List<string> runDateTimeStart = new List<string>();

            foreach (var run in testRuns)
            {
                runDateTimeStart.Add(run.Times.Start);
            }
            runDateTimeStart.Sort();

            string name = testRuns[0].Name;
            string runUser = testRuns[0].RunUser;
            
            string startString = "";
            DateTime startDate = DateTime.MaxValue;

            string endString = "";
            DateTime endDate = DateTime.MinValue;
            
            List<UnitTestResult> allResults = new List<UnitTestResult>();
            List<UnitTest> allTestDefinitions = new List<UnitTest>();
            List<TestEntry> allTestEntries = new List<TestEntry>();
            List<TestList> allTestLists = new List<TestList>();

            var resultSummary = new ResultSummary
            {
                Counters = new Counters(),
                RunInfos = new List<RunInfo>(),
            };
            bool resultSummaryPassed = true;
            List<string> testCaseId = new List<string>();
            foreach (var tr in testRuns)
            {
                List<string> tempTestCaseId = new List<string>();
                List<string> indexCollection = new List<string>();

                foreach (var item in tr.TestDefinitions)
                    tempTestCaseId.Add(item.TcmInformation.TestCaseId);

                if (testCaseId.Count > 0)
                    indexCollection = GetDuplicateListItems(testCaseId, tempTestCaseId, true);

                int abortedCount = 0; int completedCount = 0; int disconnectedCount = 0; int executedCount = 0; int failedCount = 0;
                int inconclusiveCount = 0; int inprogressCount = 0; int notexecutedCount = 0; int notrunnableCount = 0; int passedCount = 0;
                int passedbutrunabortedCount = 0; int pendingCount = 0; int timeoutCount = 0; int totalCount = 0; int warningCount = 0;

                // Remove duplicate test results record
                try
                {
                    if (indexCollection.Count > 0)
                    {
                        foreach (var indexToRemove in indexCollection)
                        {
                            #region outcome values
                            if (tr.Results[Convert.ToInt32(indexToRemove)].Outcome == "Passed")
                                passedCount++;
                            if (tr.Results[Convert.ToInt32(indexToRemove)].Outcome == "abortedCount")
                                abortedCount++;
                            if (tr.Results[Convert.ToInt32(indexToRemove)].Outcome == "completedCount")
                                completedCount++;
                            if (tr.Results[Convert.ToInt32(indexToRemove)].Outcome == "disconnectedCount")
                                disconnectedCount++;
                            if (tr.Results[Convert.ToInt32(indexToRemove)].Outcome == "executedCount")
                                executedCount++;
                            if (tr.Results[Convert.ToInt32(indexToRemove)].Outcome == "failedCount")
                                failedCount++;
                            if (tr.Results[Convert.ToInt32(indexToRemove)].Outcome == "inconclusiveCount")
                                inconclusiveCount++;
                            if (tr.Results[Convert.ToInt32(indexToRemove)].Outcome == "inprogressCount")
                                inprogressCount++;
                            if (tr.Results[Convert.ToInt32(indexToRemove)].Outcome == "notexecutedCount")
                                notexecutedCount++;
                            if (tr.Results[Convert.ToInt32(indexToRemove)].Outcome == "notrunnableCount")
                                notrunnableCount++;
                            if (tr.Results[Convert.ToInt32(indexToRemove)].Outcome == "passedbutrunabortedCount")
                                passedbutrunabortedCount++;
                            if (tr.Results[Convert.ToInt32(indexToRemove)].Outcome == "pendingCount")
                                pendingCount++;
                            if (tr.Results[Convert.ToInt32(indexToRemove)].Outcome == "timeoutCount")
                                timeoutCount++;
                            if (tr.Results[Convert.ToInt32(indexToRemove)].Outcome == "totalCount")
                                totalCount++;
                            if (tr.Results[Convert.ToInt32(indexToRemove)].Outcome == "warningCount")
                                warningCount++;
                            #endregion

                            totalCount = passedCount + abortedCount + failedCount + pendingCount;
                            tr.Results.RemoveAt(Convert.ToInt32(indexToRemove));
                            tr.TestDefinitions.RemoveAt(Convert.ToInt32(indexToRemove));
                            tr.TestEntries.RemoveAt(Convert.ToInt32(indexToRemove));
                            tempTestCaseId.RemoveAt(Convert.ToInt32(indexToRemove));
                            //tr.TestLists.RemoveAt(Convert.ToInt32(indexToRemove));
                        }
                    }
                }
                catch (Exception e)
                {
                }
                allResults = allResults.Concat(tr.Results).ToList();
                allTestDefinitions = allTestDefinitions.Concat(tr.TestDefinitions).ToList();
                allTestEntries = allTestEntries.Concat(tr.TestEntries).ToList();
                allTestLists = allTestLists.Concat(tr.TestLists).ToList();

                DateTime currStart = DateTime.Parse(tr.Times.Start);
                if (currStart < startDate)
                {
                    startDate = currStart;
                    startString = tr.Times.Start;
                }

                DateTime currEnd = DateTime.Parse(tr.Times.Finish);
                if (currEnd > endDate)
                {
                    endDate = currEnd;
                    endString = tr.Times.Finish;
                }

                resultSummaryPassed &= tr.ResultSummary.Outcome == "Passed";
                resultSummary.RunInfos = resultSummary.RunInfos.Concat(tr.ResultSummary.RunInfos).ToList();
                resultSummary.Counters.Aborted += tr.ResultSummary.Counters.Aborted-abortedCount;
                resultSummary.Counters.Completed += tr.ResultSummary.Counters.Completed-completedCount;
                resultSummary.Counters.Disconnected += tr.ResultSummary.Counters.Disconnected-disconnectedCount;
                resultSummary.Counters.Еxecuted += tr.ResultSummary.Counters.Еxecuted-totalCount;
                resultSummary.Counters.Failed += tr.ResultSummary.Counters.Failed-failedCount;
                resultSummary.Counters.Inconclusive += tr.ResultSummary.Counters.Inconclusive-inconclusiveCount;
                resultSummary.Counters.InProgress += tr.ResultSummary.Counters.InProgress-inprogressCount;
                resultSummary.Counters.NotExecuted += tr.ResultSummary.Counters.NotExecuted-notexecutedCount;
                resultSummary.Counters.NotRunnable += tr.ResultSummary.Counters.NotRunnable-notrunnableCount;
                resultSummary.Counters.Passed += tr.ResultSummary.Counters.Passed - passedCount;
                resultSummary.Counters.PassedButRunAborted += tr.ResultSummary.Counters.PassedButRunAborted-passedbutrunabortedCount;
                resultSummary.Counters.Pending += tr.ResultSummary.Counters.Pending-pendingCount;
                resultSummary.Counters.Timeout += tr.ResultSummary.Counters.Timeout-timeoutCount;
                resultSummary.Counters.Total += tr.ResultSummary.Counters.Total - totalCount;
                resultSummary.Counters.Warning += tr.ResultSummary.Counters.Warning-warningCount;

                // Add list items to master list
                testCaseId.AddRange(tempTestCaseId);
            }

            resultSummary.Outcome = resultSummaryPassed ? "Passed" : "Failed";

            return new TestRun
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                RunUser = runUser,
                Times = new Times
                {
                    Start = startString,
                    Queuing = startString,
                    Creation = startString,
                    Finish = endString,
                },
                Results = allResults,
                TestDefinitions = allTestDefinitions,
                TestEntries = allTestEntries,
                TestLists = allTestLists,
                ResultSummary = resultSummary,
            };
        }

        private static List<string> GetDuplicateListItems(List<string> MasterListItems, List<string> ListItemsToIterateToGetDuplicate, bool GetIndex)
        {
            List<string> temp1 = new List<string>();
            try
            {
                string differenceListItem = string.Empty;

                if (MasterListItems.Count > 0)
                {
                    if (ListItemsToIterateToGetDuplicate.Count > 0)
                    {
                        int index = -1;
                        //Loop scans all the actual values to find out if it equals the expected value passed as parameter
                        foreach (var item in MasterListItems)
                        {
                            if (GetIndex)
                            {
                                index = ListItemsToIterateToGetDuplicate.IndexOf(item);
                                if (index != -1)
                                    temp1.Add(index.ToString());
                            }
                        }
                    }

                }
                else
                {
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("CompareListItems: Method Exception:" + e.Message, "ERROR", true);
            }
            return temp1;
        }

        /// <summary>
        /// Gets DateTime String, normally used to create a unique filename/claimnumber...
        /// </summary>
        /// <param name="strFormat">Format to use YYMMDDHHMMSS and HHMMSS are currently supported formats</param>
        /// <returns>datetime string</returns>
        public static string GetDateTimeString(string strFormat)
        {
            string strReturn = "";

            try
            {
                DateTime dt = DateTime.Now;
                var RnD = DateTime.Parse(dt.ToString()).ToShortDateString();

                var k = DateTime.Today.ToShortDateString();

                if (string.IsNullOrEmpty(strFormat))
                {
                    string strMonth = dt.Month.ToString().PadLeft(2, '0');
                    string strDay = dt.Day.ToString().PadLeft(2, '0');
                    string strMinute = dt.Minute.ToString().PadLeft(2, '0');
                    string strSecond = dt.Second.ToString().PadLeft(2, '0');
                    string strHour = dt.Hour.ToString().PadLeft(2, '0');
                    string strYear = dt.Year.ToString();
                    strReturn = strYear + strMonth + strDay + strHour + strMinute + strSecond;
                }
                else
                {
                    if (strFormat == "YYMMDDHHMMSS")
                    {
                        strReturn = dt.ToString("yyMMddHHmmss");
                    }
                    else if (strFormat == "HHMMSS")
                    {
                        strReturn = dt.ToString("HHmmss");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("CreateDateTimeString Exception:" + e);
            }

            return strReturn;
        }
        private static string GetDateTimeString()
        {
            return GetDateTimeString(null);
        }

        public static void AddOrUpdateAppSettings(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error writing app settings");
            }
        }

        public enum extensionTypes { jpg, gif, png, trx}
        public static List<string> GetFilesWithExtension(string dir, string extensionTypes)
        {
            List<string> files = new List<string>();
            try
            {
                var ext = new List<string> { extensionTypes };
                files = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories)
                     .Where(s => ext.Contains(Path.GetExtension(s))).ToList();

                if (files .Count<=0)
                    files = Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories)
                        .Where(s => s.EndsWith("." + extensionTypes)).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: - " + e);
            }
            return files;
        }
    }
}

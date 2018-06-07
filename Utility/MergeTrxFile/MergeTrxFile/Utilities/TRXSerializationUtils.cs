using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using MergeTrxFile.TrxModelInfo;

namespace MergeTrxFile.Utilities
{
    public static class TRXSerializationUtils
    {
        private static string ns = "{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}";

        #region Deserializers
        internal static TestRun DeserializeTRX(string trxPath)
        {
            TestRun testRun = new TestRun();

            using (Stream trxStream = new FileStream(trxPath, FileMode.Open, FileAccess.Read))
            {
                XDocument doc = XDocument.Load(trxStream);
                var run = doc.Root;

                testRun.Id = run.Attribute("id").Value;
                testRun.Name = run.Attribute("name").Value;
                testRun.RunUser = run.Attribute("runUser").Value;
                
                testRun.Times = DeserializeTimes(doc.Descendants(ns + "Times").FirstOrDefault());
                testRun.ResultSummary = DeserializeResultSummary(doc.Descendants(ns + "ResultSummary").FirstOrDefault());                
                testRun.TestDefinitions = DeserializeTestDefinitions(doc.Descendants(ns + "UnitTest"));
                testRun.TestLists = DeserializeTestLists(doc.Descendants(ns + "TestList"));
                testRun.TestEntries = DeserializeTestEntries(doc.Descendants(ns + "TestEntry"));
                testRun.Results = DeserializeResults(doc.Descendants(ns + "UnitTestResult"));
            }
            return testRun;
        }

        private static Times DeserializeTimes(XElement xElement)
        {
            return new Times
            {
                Creation = xElement.Attribute("creation").Value,
                Finish = xElement.Attribute("finish").Value,
                Queuing = xElement.Attribute("queuing").Value,
                Start = xElement.Attribute("start").Value,
            };
        }

        #region ResultSummary
        /// <summary>
        /// De-Serializes the ResultSummary part of automation trx file
        /// </summary>
        /// <param name="resultSummary">resultSummary element</param>
        /// <returns>ResultSummary</returns>
        private static ResultSummary DeserializeResultSummary(XElement resultSummary)
        {
            ResultSummary res = new ResultSummary
            {
                Outcome = resultSummary.GetAttributeValue("outcome"),
                Counters = DeserializeCounters(resultSummary),
                RunInfos = DeserializeRunInfos(resultSummary.Descendants(ns + "RunInfo"))
            };

            return res;
        }
        private static Counters DeserializeCounters(XElement resultSummary)
        {
            var cc = resultSummary.Descendants(ns + "Counters").FirstOrDefault();
            if (cc == null)
                return null;

            return new Counters
            {
                Aborted = int.Parse(cc.GetAttributeValue("aborted")),
                Completed = int.Parse(cc.GetAttributeValue("completed")),
                Disconnected = int.Parse(cc.GetAttributeValue("disconnected")),
                Еxecuted = int.Parse(cc.GetAttributeValue("executed")),
                Failed = int.Parse(cc.GetAttributeValue("failed")),
                Inconclusive = int.Parse(cc.GetAttributeValue("inconclusive")),
                InProgress = int.Parse(cc.GetAttributeValue("inProgress")),
                NotExecuted = int.Parse(cc.GetAttributeValue("notExecuted")),
                NotRunnable = int.Parse(cc.GetAttributeValue("notRunnable")),
                Passed = int.Parse(cc.GetAttributeValue("passed")),
                PassedButRunAborted = int.Parse(cc.GetAttributeValue("passedButRunAborted")),
                Pending = int.Parse(cc.GetAttributeValue("pending")),
                Timeout = int.Parse(cc.GetAttributeValue("timeout")),
                Total = int.Parse(cc.GetAttributeValue("total")),
                Warning = int.Parse(cc.GetAttributeValue("warning")),
            };
        }
        /// <summary>
        /// Returns de-serialized run info details required to be part of ResultSummary
        /// </summary>
        /// <param name="runInfos"></param>
        /// <returns>Run ifo details</returns>
        private static List<RunInfo> DeserializeRunInfos(IEnumerable<XElement> runInfos)
        {
            List<RunInfo> result = new List<RunInfo>();

            foreach (var rf in runInfos)
            {
                var computerName = rf.GetAttributeValue("computerName");
                var outcome = rf.GetAttributeValue("outcome");
                var timestamp = rf.GetAttributeValue("timestamp");

                result.Add(new RunInfo
                {
                    ComputerName = computerName,
                    Outcome = outcome,
                    Timestamp = timestamp,
                    Text = DeserializeText(rf),
                });
            }
            return result;
        }
        private static string DeserializeText(XElement rf)
        {
            var txt = rf.Descendants(ns + "Text").FirstOrDefault();
            if (txt == null)
                return null;

            return txt.Value;
        }
        #endregion

        #region TestDefinitions
        private static List<UnitTest> DeserializeTestDefinitions(IEnumerable<XElement> testDefinitions)
        {
            List<UnitTest> result = new List<UnitTest>();

            foreach (var def in testDefinitions)
            {
                var name = def.GetAttributeValue("name");
                var storage = def.GetAttributeValue("storage");
                var id = def.GetAttributeValue("id");
                var TcmInfo = DeserializeTcmInformation(def);
                var Execution = DeserializeExecution(def);
                var TestMethod = DeserializeTestMethod(def);

                result.Add(new UnitTest
                {
                    Id = id,
                    Name = name,
                    Storage = storage,
                    TcmInformation = TcmInfo,
                    Execution = Execution,
                    TestMethod = TestMethod
                });
            }
            return result;
        }

        private static TcmInformation DeserializeTcmInformation(XElement unitTest)
        {
            var exec = unitTest.Descendants(ns + "TcmInformation").FirstOrDefault();
            if (exec == null)
                return null;

            return new TcmInformation
            {
                TestCaseId = exec.GetAttributeValue("testCaseId"),
                TestRunId = exec.GetAttributeValue("testRunId"),
                TestResultId = exec.GetAttributeValue("testResultId"),
            };
        }

        private static Execution DeserializeExecution(XElement unitTest)
        {
            var exec = unitTest.Descendants(ns + "Execution").FirstOrDefault();
            if (exec == null)
                return null;

            return new Execution
            {
                Id = exec.GetAttributeValue("id"),
            };
        }

        private static TestMethod DeserializeTestMethod(XElement unitTest)
        {
            var tm = unitTest.Descendants(ns + "TestMethod").FirstOrDefault();
            if (tm == null)
                return null;

            return new TestMethod
            {
                CodeBase = tm.GetAttributeValue("codeBase"),
                AdapterTypeName = tm.GetAttributeValue("adapterTypeName"),
                ClassName = tm.GetAttributeValue("className"),
                Name = tm.GetAttributeValue("name"),
            };
        }

        #endregion

        #region TestList
        private static List<TestList> DeserializeTestLists(IEnumerable<XElement> testList)
        {
            List<TestList> result = new List<TestList>();

            foreach (var tl in testList)
            {
                var name = tl.GetAttributeValue("name");
                var id = tl.GetAttributeValue("id");

                result.Add(new TestList
                {
                    Id = id,
                    Name = name,
                });
            }
            return result;
        }
        #endregion

        #region TestEntries
        private static List<TestEntry> DeserializeTestEntries(IEnumerable<XElement> testEntries)
        {
            List<TestEntry> result = new List<TestEntry>();

            foreach (var def in testEntries)
            {
                var testId = def.GetAttributeValue("testId");
                var executionId = def.GetAttributeValue("executionId");
                var testListId = def.GetAttributeValue("testListId");

                result.Add(new TestEntry
                {
                    TestId = testId,
                    ExecutionId = executionId,
                    TestListId = testListId,
                });
            }
            return result;
        }
        #endregion

        #region Results
        private static List<UnitTestResult> DeserializeResults(IEnumerable<XElement> results)
        {
            List<UnitTestResult> result = new List<UnitTestResult>();

            try
            {
                foreach (var res in results)
                {
                    var computerName = res.GetAttributeValue("computerName");
                    var duration = res.GetAttributeValue("duration");
                    var endTime = res.GetAttributeValue("endTime");
                    var executionId = res.GetAttributeValue("executionId");
                    var outcome = res.GetAttributeValue("outcome");
                    var relativeResultsDirectory = res.GetAttributeValue("relativeResultsDirectory");
                    var startTime = res.GetAttributeValue("startTime");
                    var testId = res.GetAttributeValue("testId");
                    var testListId = res.GetAttributeValue("testListId");
                    var testName = res.GetAttributeValue("testName");
                    var testType = res.GetAttributeValue("testType");

                    var Output = new UnitTestResultOutput
                    {
                        StdOut = DeserializeStdOut(res),
                        StdErr = DeserializeStdErr(res),
                        ErrorInfo = DeserializeErrorInfo(res),
                    };

                    var ResultFilePaths = DeserializeResultsFile(res.Descendants(ns + "ResultFile"));

                    result.Add(new UnitTestResult
                    {
                        ExecutionId = executionId,
                        TestId = testId,
                        TestName = testName,
                        ComputerName = computerName,
                        Duration = string.IsNullOrEmpty(duration) ? "0:0:0" : duration,
                        StartTime = startTime,
                        EndTime = endTime,
                        TestType = testType,
                        Outcome = outcome,
                        TestListId = testListId,
                        RelativeResultsDirectory = relativeResultsDirectory,
                        Output = Output,
                        ResultFiles = ResultFilePaths
                    });
                }
            }
            catch (System.Exception e)
            {
            }
            return result;
        }

        private static string DeserializeStdOut(XElement unitTestResult)
        {
            var stdOut = unitTestResult.Descendants(ns + "StdOut").FirstOrDefault();
            if (stdOut == null)
                return null;

            return unitTestResult.Value;
        }

        private static string DeserializeStdErr(XElement unitTestResult)
        {
            var stdErr = unitTestResult.Descendants(ns + "StdErr").FirstOrDefault();
            if (stdErr == null)
                return null;

            return stdErr.Value;
        }

        //private static ErrorInfo DeserializeErrorInfo(XElement unitTestResult)
        //{
        //    var err = unitTestResult.Descendants(ns + "ErrorInfo").FirstOrDefault();
        //    if (err == null)
        //        return null;

        //    return new ErrorInfo
        //    {
        //        Message = err.Descendants(ns + "Message").FirstOrDefault().Value, 
        //        StackTrace = err.Descendants(ns + "StackTrace").FirstOrDefault().Value,
        //    };
        //}

        private static ErrorInfo DeserializeErrorInfo(XElement unitTestResult)
        {
            ErrorInfo erInfo = new ErrorInfo();
            try
            {
                var err = unitTestResult.Descendants(ns + "ErrorInfo").FirstOrDefault();
                if (err == null)
                    return null;
                erInfo.Message = err.Descendants(ns + "Message").FirstOrDefault().Value;
                try
                {
                    erInfo.StackTrace = err.Descendants(ns + "StackTrace").FirstOrDefault().Value;
                }
                catch (System.Exception)
                {
                    erInfo.StackTrace = null;
                }
            }
            catch (System.Exception e)
            {
            }
            return erInfo;
        }

        private static List<ResultFile> DeserializeResultsFile(IEnumerable<XElement> resultFiles)
        {
            List<ResultFile> resultFile = new List<ResultFile>();
            //var ResultFilePaths = unitTestResult.Descendants(ns + "ResultFiles").FirstOrDefault();
            try
            {
                foreach (var resFile in resultFiles)
                {
                    var resultFilePath = resFile.GetAttributeValue("path");
                    resultFile.Add(new ResultFile { path = resultFilePath });                    
                }
            }
            catch (System.Exception e)
            {
                resultFiles = null;
            }
            return resultFile;
        }
        #endregion
        
        
        #endregion

        #region Serializers
        internal static string SerializeAndSaveTestRun(TestRun testRun, string targetPath)
        {
            XNamespace xmlns = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";
            XDocument doc =
                new XDocument(
                  new XElement("TestRun",
                        new XAttribute("id", testRun.Id),
                        new XAttribute("name", testRun.Name),
                        new XAttribute("runUser", testRun.RunUser),
                        new XElement("Times",
                            new XAttribute("creation", testRun.Times.Creation),
                            new XAttribute("queuing", testRun.Times.Queuing),
                            new XAttribute("start", testRun.Times.Start),
                            new XAttribute("finish", testRun.Times.Finish)),
                        new XElement("Results",
                            testRun.Results.Select(
                                utr =>
                                    new XElement("UnitTestResult",
                                        utr.ExecutionId == null ? null : new XAttribute("executionId", utr.ExecutionId),
                                        new XAttribute("testId", utr.TestId),
                                        new XAttribute("testName", utr.TestName),
                                        new XAttribute("computerName", utr.ComputerName),
                                        utr.Duration == null ? null : new XAttribute("duration", utr.Duration),
                                        utr.StartTime == null ? null : new XAttribute("startTime", utr.StartTime), 
                                        utr.EndTime == null ? null : new XAttribute("endTime", utr.EndTime),
                                        new XAttribute("testType", utr.TestType),
                                        utr.Outcome == null ? null : new XAttribute("outcome", utr.Outcome),
                                        new XAttribute("testListId", utr.TestListId),
                                        new XElement("Output",
                                             utr.Output.StdOut == null ? null : new XElement("StdOut", utr.Output.StdOut),
                                             utr.Output.StdErr == null ? null : new XElement("StdErr", utr.Output.StdErr),
                                             utr.Output.ErrorInfo == null ? null :
                                             new XElement("ErrorInfo",
                                                  utr.Output.ErrorInfo.Message == null ? null : new XElement("Message", utr.Output.ErrorInfo.Message),
                                                  utr.Output.ErrorInfo.StackTrace == null ? null : new XElement("StackTrace", utr.Output.ErrorInfo.StackTrace)
                                                  )),
                                                  new XElement("ResultFiles", 
                                                      utr.ResultFiles.Select(
                                                      resFile => 
                                                          new XElement("ResultFile", resFile.path==null?null:new XAttribute("path",resFile.path))))))),
                      new XElement("TestDefinitions",
                           testRun.TestDefinitions.Select(
                                td => new XElement("UnitTest",
                                         new XAttribute("id", td.Id),
                                         new XAttribute("name", td.Name),
                                         new XAttribute("storage", td.Storage),
                                         new XElement("TcmInformation",
                                            new XAttribute("testCaseId", td.TcmInformation.TestCaseId),
                                            new XAttribute("testRunId", td.TcmInformation.TestRunId),
                                            new XAttribute("testResultId", td.TcmInformation.TestResultId)),
                                         new XElement("Execution",
                                            new XAttribute("id", td.Execution.Id)),
                                         new XElement("TestMethod",
                                            new XAttribute("codeBase", td.TestMethod.CodeBase),
                                            new XAttribute("className", td.TestMethod.ClassName),
                                            new XAttribute("name", td.TestMethod.Name),
                                            td.TestMethod.AdapterTypeName == null ? null : new XAttribute("adapterTypeName", td.TestMethod.AdapterTypeName))))),
                        new XElement("TestEntries",
                             testRun.TestEntries.Select(
                                te => new XElement("TestEntry",
                                          new XAttribute("testId", te.TestId),
                                          new XAttribute("executionId", te.ExecutionId),
                                          new XAttribute("testListId", te.TestListId)))),
                        new XElement("TestLists",
                            testRun.TestLists.Distinct().Select(
                                 tl => new XElement("TestList",
                                     new XAttribute("name", tl.Name),
                                     new XAttribute("id", tl.Id)))),
                        new XElement("ResultSummary",
                            new XAttribute("outcome", testRun.ResultSummary.Outcome),
                            new XElement("Counters",
                                new XAttribute("aborted", testRun.ResultSummary.Counters.Aborted),
                                new XAttribute("completed", testRun.ResultSummary.Counters.Completed),
                                new XAttribute("disconnected", testRun.ResultSummary.Counters.Disconnected),
                                new XAttribute("executed", testRun.ResultSummary.Counters.Еxecuted),
                                new XAttribute("failed", testRun.ResultSummary.Counters.Failed),
                                new XAttribute("inconclusive", testRun.ResultSummary.Counters.Inconclusive),
                                new XAttribute("inProgress", testRun.ResultSummary.Counters.InProgress),
                                new XAttribute("notExecuted", testRun.ResultSummary.Counters.NotExecuted),
                                new XAttribute("notRunnable", testRun.ResultSummary.Counters.NotRunnable),
                                new XAttribute("passed", testRun.ResultSummary.Counters.Passed),
                                new XAttribute("passedButRunAborted", testRun.ResultSummary.Counters.PassedButRunAborted),
                                new XAttribute("pending", testRun.ResultSummary.Counters.Pending),
                                new XAttribute("timeout", testRun.ResultSummary.Counters.Timeout),
                                new XAttribute("total", testRun.ResultSummary.Counters.Total),
                                new XAttribute("warning", testRun.ResultSummary.Counters.Warning)),
                            new XElement("RunInfos",
                                testRun.ResultSummary.RunInfos.Select(
                                    ri => new XElement("RunInfo",
                                        new XAttribute("computerName", ri.ComputerName),
                                        new XAttribute("outcome", ri.Outcome),
                                        new XAttribute("timestamp", ri.Timestamp),
                                        new XElement("Text", ri.Text)))))
                             )
                );


            doc.Root.SetDefaultXmlNamespace("http://microsoft.com/schemas/VisualStudio/TeamTest/2010");

            if (File.Exists(targetPath))
                File.Delete(targetPath);

            doc.Save(targetPath);

            var savedFileInfo = new FileInfo(targetPath);

            return savedFileInfo.FullName;
        }
        #endregion
    }
}

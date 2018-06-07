﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MergeTrxFile.TrxToHtml;
using System.Configuration;

namespace MergeTrxFile
{
    class Program
    {
        static string dateValueToAppend = string.Empty;
        static string FilesToMergeDir = string.Empty;
        static string MergedFileName = string.Empty;
        static string FilesToMoveAfterMergeDir = string.Empty;
        static string GeneratedTrxFile = string.Empty;
        static List<string> trxFilesToMerge = new List<string>();
        public static int Main(string[] args)
        {
            try
            {
                dateValueToAppend = TestRunMerger.GetDateTimeString("YYMMDDHHMMSS");


                //string arg = "MergeTrxFile.EXE /trx:D:\\TestResults\\tmiRun.trx,D:\\TestResults\\tmiRun1.trx /output:D:\\TestResults\\tmi.trx";
                //args = arg.Split(' ');
                if (!args.Any())
                {
                    FilesToMergeDir = ConfigurationManager.AppSettings["FilesToMergeDir"];
                    FilesToMoveAfterMergeDir = ConfigurationManager.AppSettings["FilesToMoveAfterMergeDir"];
                    MergedFileName = ConfigurationManager.AppSettings["MergedFileName"];

                    trxFilesToMerge = TestRunMerger.GetFilesWithExtension(FilesToMergeDir, "trx");
                }
                else
                {
                    if (args.Length == 0 || args.Contains("/h") || args.Contains("/help") || args.Contains("/?"))
                    {
                        DisplayHelp();
                        return 1;
                    }
                    else
                    {
                        if (args.Where(a => a.StartsWith("/trx")).FirstOrDefault() == null)
                        {
                            Console.WriteLine("/trx parameter is required");
                            return 1;
                        }

                        string trxArg = args.Where(a => a.StartsWith("/trx")).FirstOrDefault();
                        trxFilesToMerge = ResolveTrxFilePaths(trxArg, args.Contains("/r"));
                        if (trxFilesToMerge.Count == 0)
                        {
                            Console.WriteLine("No trx files found!");
                            return 1;
                        }
                        if (args.Where(a => a.StartsWith("/output")).FirstOrDefault() == null)
                        {
                            Console.WriteLine("/output parameter is required, when there are multiple trx files in /trx argument");
                            return 1;
                        }

                        MergedFileName = ResolveOutputFileName(args.Where(a => a.StartsWith("/output")).FirstOrDefault());
                        if (MergedFileName.StartsWith("Error: "))
                        {
                            Console.WriteLine(MergedFileName);
                            return 1;
                        }

                        if (trxFilesToMerge.Contains(MergedFileName))
                            trxFilesToMerge.Remove(MergedFileName);
                    }
                }
                
                if (trxFilesToMerge.Count > 0)
                {
                    try
                    {
                        // Update trx file directory
                        if (string.IsNullOrEmpty(FilesToMergeDir))
                            FilesToMergeDir = System.IO.Path.GetDirectoryName(trxFilesToMerge[0]);
                        if (!FilesToMergeDir.EndsWith(@"\"))
                            FilesToMergeDir += @"\";

                        // Update directory information where merged file are to be moved
                        if (string.IsNullOrEmpty(FilesToMoveAfterMergeDir))
                            FilesToMoveAfterMergeDir = FilesToMergeDir +@"Merged";

                        if (!FilesToMoveAfterMergeDir.EndsWith(@"\"))
                            FilesToMoveAfterMergeDir += @"\";

                        FilesToMoveAfterMergeDir = FilesToMoveAfterMergeDir + dateValueToAppend+@"\";
                        
                        if (!Directory.Exists(FilesToMoveAfterMergeDir))
                            Directory.CreateDirectory(FilesToMoveAfterMergeDir);
                        if (args.Length>0)
                            GeneratedTrxFile = MergedFileName;
                        else
                            GeneratedTrxFile = FilesToMergeDir + Path.GetFileName(MergedFileName) + dateValueToAppend + ".trx";
                        
                        
                        var combinedTestRun = TestRunMerger.MergeTRXsAndSave(trxFilesToMerge, GeneratedTrxFile);

                        foreach (var item in trxFilesToMerge)
                        {
                            string destFile = FilesToMoveAfterMergeDir + Path.GetFileName(item);

                            //To copy a file to another location and overwrite the destination file if it already exists.
                            System.IO.File.Copy(item, destFile, true);
                            File.Delete(item);
                        }

                        TestRunMerger.AddOrUpdateAppSettings("TrxFile", GeneratedTrxFile);
                        
                        ////outputTrxFile is source trx file for GenerateXmlToXhtml()
                        //ReportGeneratore.GenerateXmlToXhtml(outputTrxFile, template, outputReportFile);

                        //var ka = ReportGeneratore.GenerateXmlToXhtmlSample(outputTrxFile, template, outputReportFile);
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
            catch (Exception)
            {
            }
            return 1; ;
        }
        public static void DisplayHelp()
        {
            Console.WriteLine(@"
                PARAMETERS:

                /trx - parameter that determines which trx files will be merged. *REQUIRED PARAMETER

                This parameter will accept one of the following:
		                - file(s) name: looks for trx files in the current directory.File extension is required 
			                example: /trx:testResults1.trx,testResults2.trx,testResults3.trx
		                - file(s) path: full path to trx files.File extension is required 
			                example: /trx:c:\TestResults\testResults1.trx,c:\TestResults\testResults2.trx,c:\TestResults\testResults3.trx
		                - directory(s): directory containing trx files. it gets all trx files in the directory	
			                example: /trx:c:\TestResults,c:\TestResults1
		                - empty: gets all trx files in the current directory
			                example: /trx
                        - combination: you can pass files and directories at the same time:
                            example: /trx:c:\TestResults,c:\TestResults1\testResults2.trx

                /output - the name of the output trx file. File extension is required. REQIRED if more than one trx file is defined in the /trx parameter. If only one trx is present in /trx this parameter should not be passed.
	                - name: saves the file in the current directory
		                example: /output:combinedTestResults.trx
	                - path and name: saves the file in specified directory.
		                example: /output:c:\TestResults\combinedTestResults.trx

                /r - recursive search in directories.OPTIONAL PARAMETER.\nWhen there is a directory in /trx param (ex: /trx:c:\TestResuts), and this parameter is passed, the rearch for trx files will be recursive
                    example: /trx:c:\TestResults,c:\TestResults1\testResults2.trx /r /output:combinedTestResults.trx

                /report - generates a html report from a trx file. REQUIRED if one trx is specified in /trx parameter and OPTIONAL otherwise.\n If one trx is passed to the utility, the report is for it, otherwise, the report is generated for the /output result
                    - fill path to where the report should be saved. including the name of the file and extension. 
                    example /report:c:\Tests\report.html

                /screenshots - path to a folder which contains screenshots corresponding to failing tests. OPTIONAL PARAMETER
                    - in order a screenshot to be shown in the report for a given test, the screenshot should contain the name of the test method."
            );
        }
        private static string ResolveOutputFileName(string outputParam)
        {
            var splitOutput = outputParam.Split(new char[] { ':' });

            if (splitOutput.Length == 1
                || !outputParam.EndsWith(".trx"))
                return "Error: /output parameter is in the incorrect format. Expected /output:<file name | directory and file name>. Execute /help for more information";

            return outputParam.Substring(8, outputParam.Length - 8);
        }
        private static string ResolveReportLocation(string reportParam)
        {
            if (string.IsNullOrEmpty(reportParam))
                return null;

            var splitReport = reportParam.Split(new char[] { ':' });

            if (splitReport.Length == 1
                || !reportParam.EndsWith(".html"))
                return "Error: /report parameter is in the correct format. Expected /report:<file name | directory and file name>. Execute /help for more information";

            return reportParam.Substring(8, reportParam.Length - 8);
        }
        private static string ResolveScreenshotLocation(string screenshots)
        {
            if (string.IsNullOrEmpty(screenshots))
                return null;

            var splitScreenshots = screenshots.Split(new char[] { ':' });

            if (splitScreenshots.Length == 1)
                return "Error: /screenshots parameter is in the correct format. Expected /screenshots:<directory name>. Execute /help for more information";

            var screenshotsLocation = screenshots.Substring(13, screenshots.Length - 13);
            if (!Directory.Exists(screenshotsLocation))
                return "Error: Folder: " + screenshotsLocation + "does not exists";

            return screenshotsLocation;
        }
        private static string ResolveReportTitle(string reportTitle)
        {
            if (string.IsNullOrEmpty(reportTitle))
                return null;

            var splitScreenshots = reportTitle.Split(new char[] { ':' });

            if (splitScreenshots.Length == 1)
                return "Error: /reportTitle parameter is in the correct format. Expected /reportTitle:<title>. Execute /help for more information";

            var screenshotsLocation = reportTitle.Substring("/reportTitle".Length + 1);

            return screenshotsLocation;
        }
        private static List<string> ResolveTrxFilePaths(string trxParams, bool recursive)
        {
            List<string> paths = new List<string>();

            var splitTrx = trxParams.Split(new char[] { ':' });

            var searchOpts = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            if (splitTrx.Length == 1)
                return Directory.GetFiles(Directory.GetCurrentDirectory(), "*.trx", searchOpts).ToList();

            var args = trxParams.Substring(5, trxParams.Length - 5).Split(new char[] { ',' }).ToList();

            foreach (var a in args)
            {
                bool isTrxFile = File.Exists(a) && a.EndsWith(".trx");
                bool isDir = Directory.Exists(a);

                if (!isTrxFile && !isDir)
                    return new List<string>
                    {
                        string.Format("Error: {0} is not a trx file or directory", a)
                    };

                if (isTrxFile)
                    paths.Add(a);

                if (isDir)
                    paths.AddRange(Directory.GetFiles(a, "*.trx", searchOpts).ToList());

            }

            return paths;
        }
    }
}

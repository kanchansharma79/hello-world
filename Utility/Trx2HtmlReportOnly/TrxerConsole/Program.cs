using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Xsl;
using System.Configuration;
using System.Net; // Used to access  app.config file

namespace Trx2HtmlConsole
{
    partial class Program
    {   
        /// <summary>
        /// Embedded Resource name
        /// </summary>
        private const string XSLT_FILE = "Trx.xslt";
        /// <summary>
        /// Embedded Resource name
        /// </summary>
        private const string EMAIL_XSLT_FILE = "EmailTrx.xslt";

        /// <summary>
        /// Trxer output format
        /// </summary>
        private const string OUTPUT_FILE_EXT = ".html";

        /// <summary>
        /// Main entry of Trx2HtmlConsole
        /// </summary>
        /// <param name="args">First cell shoud be TRX path</param>
        static void Main(string[] args)
        {
            string outputFilePath = string.Empty;
            string inputTrxFile = string.Empty;
            bool emailReport = false;
            string ReportFile = string.Empty;
            string EmailReportFile = string.Empty;
            
            #region arguments managed

            switch (args.Length)
            {
                case 0:
                    inputTrxFile = ConfigurationManager.AppSettings["TrxFile"];
                    GetAppSettingsKeyValue("HtmlOutputFile", out outputFilePath);
                    emailReport = Convert.ToBoolean(ConfigurationManager.AppSettings["emailReport"]);
                    break;
                case 1:
                    inputTrxFile = args[0];
                    GetAppSettingsKeyValue("HtmlOutputFile", out outputFilePath);
                    emailReport = Convert.ToBoolean(ConfigurationManager.AppSettings["emailReport"]);
                    break;
                case 2:
                    inputTrxFile = args[0];
                    outputFilePath = args[1];                    
                    emailReport = Convert.ToBoolean(ConfigurationManager.AppSettings["emailReport"]);
                    break;
                case 3:
                    inputTrxFile = args[0];
                    outputFilePath = args[1];
                    emailReport = Convert.ToBoolean(args[2]);
                    break;
                default:
                    DisplayHelp();
                    break;
            }
            #endregion
            
            //transform trx to html file
            ReportFile = Transform(inputTrxFile, PrepareXsl(XSLT_FILE), outputFilePath, false);
            EmailReportFile = Transform(inputTrxFile, PrepareXsl(EMAIL_XSLT_FILE), outputFilePath, true);

            AddOrUpdateAppSettings("ReportFile", ReportFile);
            AddOrUpdateAppSettings("EmailFile", EmailReportFile);
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
    }
}

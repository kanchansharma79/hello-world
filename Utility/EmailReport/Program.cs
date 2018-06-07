using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailReport
{
    class Program
    {
        static string dbConnectionString = string.Empty;
        static string commonReportFilePath = string.Empty;
        static string emailReportFile = string.Empty;
        static string displayReportFile = string.Empty;
        static bool storeInDb = false;
        static bool emailReport = false;
        static string emailReportSubject = string.Empty;
        static string dbStoredReportFileNamePath = string.Empty;
        static string dbStoredEmailReportFileNamePath = string.Empty;
        static ConnectionStringSettings dbConnection = new ConnectionStringSettings();
        static void Main(string[] args)
        {
            Generic gm = new Generic();
            try
            {
                emailReportFile = ConfigurationManager.AppSettings["EmailFile"];
                displayReportFile = ConfigurationManager.AppSettings["ReportFile"];

                storeInDb = Convert.ToBoolean(ConfigurationManager.AppSettings["storeInDatabase"]);
                emailReport = Convert.ToBoolean(ConfigurationManager.AppSettings["emailReport"]);
                emailReportSubject = ConfigurationManager.AppSettings["emailReportSubject"];
                if (File.Exists(emailReportFile))
                {
                    if (storeInDb)
                    {
                        commonReportFilePath = ConfigurationManager.AppSettings["commonreportlocation"];
                        dbConnection = ConfigurationManager.ConnectionStrings["TestAutomationConnection"];

                        if (String.IsNullOrEmpty(commonReportFilePath) || dbConnection == null)
                        {
                            if (String.IsNullOrEmpty(commonReportFilePath))
                                Generic.AddOrUpdateAppSettings("commonreportlocation", "");

                            Console.WriteLine("Please define appsetting key 'commonreportlocation' value as location where report can be stored if not defined");
                            Console.WriteLine("Please define connectionstring key 'TestAutomationConnection' value as db connection string in config/conn.config file");
                            Console.WriteLine("Else you can set appsetting key 'storeInDatabase' value as false and only email will work");
                            return;
                        }
                        else if (Directory.Exists(commonReportFilePath))
                        {
                            dbConnectionString = dbConnection.ToString();
                            if (!commonReportFilePath.EndsWith(@"\"))
                                commonReportFilePath += @"\";

                            #region Move file to shared common location
                            string todayDate = gm.GetDateTimeString("MMDDYY");
                            string webFileName = Path.GetFileNameWithoutExtension(displayReportFile);
                            string emailFileName = Path.GetFileNameWithoutExtension(emailReportFile);

                            string dbStoredReportFileName = todayDate + webFileName + ".html";
                            string dbStoredEmailReportFileName = todayDate + emailFileName + ".html";

                            dbStoredReportFileNamePath = commonReportFilePath + dbStoredReportFileName;
                            dbStoredEmailReportFileNamePath = commonReportFilePath + dbStoredEmailReportFileName;

                            //copy a web and email content html file to common shared location and overwrite the destination file if it already exists.
                            System.IO.File.Copy(displayReportFile, dbStoredReportFileNamePath, true);
                            System.IO.File.Copy(emailReportFile, dbStoredEmailReportFileNamePath, true);
                            #endregion

                            gm.StoreHtmlReportFileInDatabase(ref dbStoredReportFileNamePath, ref dbStoredEmailReportFileNamePath, dbConnectionString);
                        }
                    }

                    if (!string.IsNullOrEmpty(dbStoredEmailReportFileNamePath))
                        emailReportFile = dbStoredEmailReportFileNamePath;

                    if (emailReport && File.Exists(emailReportFile))
                    {
                        string todayDate = gm.GetDateTimeString("MMDDYY");
                        string mailBody = gm.GetHtmlContent(emailReportFile);
                        gm.SendMail(null, emailReportSubject + todayDate, mailBody, emailReportFile, 0, true);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}

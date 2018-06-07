using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EmailReport
{
    class Generic
    {
        #region Database methods
        /// <summary>
        /// ExecuteSQL (ExecuteNonQuery)
        /// </summary>
        /// <param name="strSQL">sql to executwe</param>
        /// <param name="strConnection">Connection string</param>
        /// <param name="nCommandTimeout">Time out, specifying 0 will use default</param>
        /// <returns>Number of rows affected</returns>
        public static int ExecuteSQL(string strSQL, string strConnection, int nCommandTimeout)
        {
            string strResult = "";
            int nID = 0;

            SqlConnection myUpdConnection = new SqlConnection(strConnection);

            try
            {
                myUpdConnection.Open();
                SqlCommand myUpdCommand = new SqlCommand(strSQL, myUpdConnection);

                if (nCommandTimeout > 0)
                {
                    myUpdCommand.CommandTimeout = nCommandTimeout;
                }

                nID = myUpdCommand.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                strResult = "Exception SQL=" + strSQL + ":" + e;
                Console.WriteLine("WriteToFile:" + strResult);
                nID = -1;
            }
            finally
            {
                myUpdConnection.Close();
            }

            return nID;
        }

        /// <summary>
        /// Queries a single value from given db
        /// </summary>
        /// <param name="strValue">Column to query value from</param>
        /// <param name="strTable">Table to query value from, if null then it means query string doesn't need any building, i.e. it's already complete</param>
        /// <param name="strWHERE">Where condition</param>
        /// <param name="strConnection">WHERE clause</param>
        /// <param name="bValidate">if true will default return value to null else empty string</param>
        /// <param name="bConcatValues">if true will concatenate all values returned</param>
        /// <returns>Value</returns>
        public string QuerySQLValue(string strValue, string strTable, string strWHERE, string strConnection, bool bValidate = false, bool bConcatValues = false)
        {

            string strSQLquery;
            string strQValue = "";

            if (bValidate) strQValue = null; //defaults to null

            SqlConnection SQLConnection = new SqlConnection(strConnection);
            //if table is null that means the query string doesn't need any building, i.e. it's already complete
            if (string.IsNullOrEmpty(strTable))
            {
                strSQLquery = strValue;
            }
            else
            {
                strSQLquery = "SELECT " + strValue + " FROM " + strTable + " WHERE " + strWHERE;
            }

            try
            {
                strSQLquery = strSQLquery.Trim();

                SQLConnection.Open();

                SqlCommand SQLCommand = new SqlCommand(strSQLquery, SQLConnection);
                SqlDataReader myReader = SQLCommand.ExecuteReader();

                while (myReader.Read())
                {
                    Type tType = myReader.GetValue(0).GetType();

                    if (tType.Name == "DBNull")
                    {
                        strQValue = null;
                    }
                    else
                    {
                        strQValue = strQValue + myReader.GetValue(0);
                    }

                    if (!bConcatValues) break;
                }

                myReader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("QuerySQLValues_string, strSQLquery=" + strSQLquery + ", strConnection=" + strConnection + ", Exception=" + e, "ERROR");
                strQValue = "ExceptionRaised";
            }
            finally
            {
                SQLConnection.Close();
            }

            return strQValue;
        }

        /// <summary>
        /// Queries a single value from given db
        /// </summary>
        /// <param name="strSqlQuery">sql query</param>
        /// <param name="strConnection">sql connection string</param>
        /// <returns>value</returns>
        public string QuerySQLValue(string strSqlQuery, string strConnection)
        {
            return QuerySQLValue(strSqlQuery, null, null, strConnection);
        }

        public string InsertReportQuery()
        {
            string insertedID = string.Empty;
            //string htmlContent = "INSERT INTO CuitReportHtml ( 'WebReport', 'EmailReport', 'Creation') VALUES (@WebReport, @EmailReport, @TimeStamp)";
            //string htmlContentFiles = "INSERT INTO CuitReportFiles ( 'WebReport', 'EmailReport', 'Creation') VALUES (@WebReport, @EmailReport, @TimeStamp)";
            //try
            //{
            //    using (SqlConnection myConnection = new SqlConnection(strConnection))
            //    {
            //        myConnection.Open();

            //        SqlCommand myCommand = new SqlCommand(insertSql, myConnection);

            //        myCommand.Parameters.AddWithValue("@UserId", newUserId);
            //        myCommand.Parameters.AddWithValue("@GameId", newGameId);

            //        primaryKey = Convert.ToInt32(myCommand.ExecuteScalar());

            //        myConnection.Close();
            //    }
            //}
            //catch (Exception e)
            //{
                
            //}
            return insertedID;
        }
        #endregion

        public void StoreHtmlReportFileInDatabase(ref string webFilePath, ref string emailFilePath, string connectionstring)
        {
            try
            {
                // To DO 
                /***************************************************************************************************
                 * code to check if file record already exists in datbase or not 
                 * If exists update query should be run
                 * else insert query should be excuted 
                ***************************************************************************************************/

                string todayDate = GetDateTimeString("MMddYYYY");

                // Extract html content for Web and Email file and then Encode the content for storing in Sql server.
                string htmlWebReportEncoded = WebUtility.HtmlEncode(GetHtmlContent(webFilePath));
                string htmlEmailReportEncoded = WebUtility.HtmlEncode(GetHtmlContent(emailFilePath));

                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("upStoreRunDetails", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@WebReport", SqlDbType.VarChar).Value = htmlWebReportEncoded;
                        cmd.Parameters.Add("@WebReportFile", SqlDbType.VarChar).Value = webFilePath;
                        cmd.Parameters.Add("@EmailReport", SqlDbType.VarChar).Value = htmlEmailReportEncoded;
                        cmd.Parameters.Add("@EmailReportFile", SqlDbType.VarChar).Value = emailFilePath;
                        cmd.Parameters.Add("@TimeStamp", SqlDbType.VarChar).Value = todayDate;
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception:- " + e.Message);
            }
        }
        public void GenerateHtmlReportFileFromDatabase(ref string filePath, int ReportID, string connectionstring)
        {
            Console.WriteLine("Transforming ... to html");
            try
            {
                string htmlContentQuery = "SELECT WebReport FROM CuitReport WHERE TR_ID = "+ReportID;
                var htmlContent = QuerySQLValue(htmlContentQuery, connectionstring);

                // Encode the content for storing in Sql server.
                string htmlEncoded = GetHtmlEncode(htmlContent);

                filePath = filePath + "\\AutomationReport"+ReportID+".html";
                // Create a file to write to.
                WriteToFile(htmlEncoded, filePath, false, true);
                string insertFilePath = "UPDATE CuitReport SET  WebReportFilePath = '" + filePath + "' WHERE TR_ID = " + ReportID;                
                ExecuteSQL(insertFilePath, connectionstring, 0);
            }
            catch (Exception)
            {
            }
        }

        public string GetHtmlEncode(string HtmlEncodedValueFromDatabase)
        {
            try
            {
                return WebUtility.HtmlDecode(HtmlEncodedValueFromDatabase);
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        public string GetHtmlContent(string filePath)
        {
            string htmlFile = string.Empty;
            try
            {
                htmlFile = File.ReadAllText(filePath);
            }
            catch (Exception e)
            {
            }
            return htmlFile;
        }
        public MailSettingsSectionGroup GetMailSettingsFromConfigFile()
        {
            MailSettingsSectionGroup mailSettings = new MailSettingsSectionGroup();
            try
            {
                Configuration oConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                mailSettings = oConfig.GetSectionGroup("system.net/mailSettings") as MailSettingsSectionGroup;
            }
            catch (Exception e)
            {
                Console.WriteLine("GetMailSettingsFromConfigFile Exception:" + e.Message, "ERROR");
            }

            return mailSettings;
        }
        public void SendMail(string sendMailTo, string subject, string body, string attachmentFilename, int nPriority, bool bIsHtml)
        {
            try
            {
                MailSettingsSectionGroup mailSettings = GetMailSettingsFromConfigFile();
                var settings = ConfigurationManager.AppSettings;

                #region MyRegion
                if (mailSettings != null)
                {
                    int port = mailSettings.Smtp.Network.Port;
                    string from = mailSettings.Smtp.From;
                    string host = mailSettings.Smtp.Network.Host;
                    string pwd = mailSettings.Smtp.Network.Password;
                    string uid = mailSettings.Smtp.Network.UserName;

                    var message = new MailMessage
                    {
                        From = new MailAddress(@from)
                    };

                    if (string.IsNullOrEmpty(sendMailTo))
                        sendMailTo = settings["EmailAddressesTo"];
                    string sendMailCC = settings["EmailAddressesCc"];
                    string sendMailBcc = settings["EmailAddressesBcc"];


                    if (!string.IsNullOrEmpty(sendMailTo))
                    {
                        string[] emailToCollection = null;

                        if (sendMailTo.Contains(';'))
                            emailToCollection = sendMailTo.Split(';');
                        else if (sendMailTo.Contains(','))
                            emailToCollection = sendMailTo.Split(',');

                        if (emailToCollection != null)
                            foreach (var emailTo in emailToCollection)
                                message.To.Add(new MailAddress(emailTo));
                        else
                            message.To.Add(new MailAddress(sendMailTo));
                    }

                    if (!string.IsNullOrEmpty(sendMailCC))
                    {
                        string[] emailCCCollection = null;

                        if (sendMailCC.Contains(';'))
                            emailCCCollection = sendMailCC.Split(';');
                        else if (sendMailCC.Contains(','))
                            emailCCCollection = sendMailCC.Split(',');

                        if (emailCCCollection != null)
                            foreach (var emailCC in emailCCCollection)
                                message.To.Add(new MailAddress(emailCC));
                        else
                            message.To.Add(new MailAddress(sendMailCC));
                    }

                    if (!string.IsNullOrEmpty(sendMailBcc))
                    {
                        string[] emailBccCollection = null;

                        if (sendMailBcc.Contains(';'))
                            emailBccCollection = sendMailBcc.Split(';');
                        else if (sendMailBcc.Contains(','))
                            emailBccCollection = sendMailBcc.Split(',');

                        if (emailBccCollection != null)
                            foreach (var emailBcc in emailBccCollection)
                                message.Bcc.Add(new MailAddress(emailBcc));
                        else
                            message.To.Add(new MailAddress(sendMailCC));
                    }   


                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = bIsHtml;
                    
                    if (attachmentFilename != null)
                        message.Attachments.Add(new Attachment(attachmentFilename));

                    if (nPriority == -1)
                        message.Priority = MailPriority.Low;
                    else if (nPriority == 0)
                        message.Priority = MailPriority.Normal;
                    else
                        message.Priority = MailPriority.High;

                    var client = new SmtpClient
                    {
                        Host = host,
                        Port = port,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(uid, pwd),
                        DeliveryFormat = SmtpDeliveryFormat.International
                    };

                    try
                    {
                        client.Send(message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    finally
                    {
                        client.Dispose();
                    }
                }
                #endregion
            }
            catch (Exception e)
            {
                Console.WriteLine("SendMail Exception:" + e.Message, "ERROR");
            }
        }
        public void SendMail(string sendMailTo, string subject, string body, int nPriority, bool bIsHtml)
        {
            SendMail(sendMailTo, subject, body, null, nPriority, bIsHtml);
        }

        public bool WaitForFileAccess(string strPathToFile, int nMaxSecondsToWait)
        {
            return WaitForFileAccess(strPathToFile, nMaxSecondsToWait, false);
        }

        public bool WaitForFileAccess(string strPathToFile, int nMaxSecondsToWait, bool bIgNoreIfFileDoesntExist)
        {
            bool bFileReady = false;
            int nSecondsWaited = 0;
            string strLastExecption = "";

            if (bIgNoreIfFileDoesntExist)
            {
                if (!File.Exists(strPathToFile)) return true;
            }

            FileStream stream = null;

            while (!bFileReady && nSecondsWaited < nMaxSecondsToWait)
            {

                try
                {
                    stream = new FileStream(strPathToFile, FileMode.Open);
                }
                catch (Exception e)
                {
                    //the file is unavailable because it is: still being written to             
                    //or being processed by another thread  or does not exist (has already been processed)  
                    strLastExecption = e.ToString();

                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Close();
                        bFileReady = true;
                    }
                    else
                    {
                        Thread.Sleep(1000);
                        nSecondsWaited++;
                    }
                }
            }

            if (!bFileReady)
            {
                Console.WriteLine("WaitForFileAccess:" + strLastExecption + ": after waiting " + nMaxSecondsToWait, "INFO");
            }
            return bFileReady;

        }

        public void WriteToFile(string strText, string strPathToFile, bool bAppend, bool bWriteLine)
        {
            WaitForFileAccess(strPathToFile, 5, true);

            try
            {
                StreamWriter sw = new StreamWriter(strPathToFile, bAppend);
                if (bWriteLine)
                {
                    sw.WriteLine(strText);
                }
                else
                {
                    sw.Write(strText);
                }
                sw.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("WriteToFile:" + e);
                Console.WriteLine("WriteToFile:, Error: strText=" + strText + ", strPathToFile="
                    + strPathToFile + ", bAppend=" + bAppend + ", Exception=" + e, "ERROR");
            }
        }

        /// <summary>
        /// Copies files from a source directory to a destination directory.
        /// </summary>
        /// <param name="sourceDirName"></param>
        /// <param name="destDirName"></param>
        /// <param name="filesCopied">If not null, a list of file names will be appended to that were copied in this operation</param>
        /// <returns></returns>
        public bool CopyReportFileInDirectory(string sourceDirName, string destDirName, List<String> filesCopied = null)
        {
            try
            {
                var dir = new DirectoryInfo(sourceDirName);
                if (!dir.Exists)
                {
                    throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
                }
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    string temppath = Path.Combine(destDirName, file.Name);
                    if (filesCopied != null) filesCopied.Add(file.Name);
                    file.CopyTo(temppath, true);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Copy Files from location failed.  " + ex, "ERROR", true);
            }
            return false;
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

        /// <summary>
        /// Gets DateTime String, normally used to create a unique filename/claimnumber...
        /// </summary>
        /// <param name="strFormat">Format to use YYMMDDHHMMSS and HHMMSS are currently supported formats</param>
        /// <returns>datetime string</returns>
        public string GetDateTimeString(string strFormat)
        {
            string strReturn = "";

            try
            {
                DateTime dt = DateTime.Now;
                string strMonth = dt.Month.ToString().PadLeft(2, '0');
                string strDay = dt.Day.ToString().PadLeft(2, '0');
                string strMinute = dt.Minute.ToString().PadLeft(2, '0');
                string strSecond = dt.Second.ToString().PadLeft(2, '0');
                string strHour = dt.Hour.ToString().PadLeft(2, '0');
                string strYear = dt.Year.ToString();

                switch (strFormat.ToUpper())
                {
                    case "YYMMDDHHMMSS":
                        strReturn = dt.ToString("yyMMddHHmmss");
                        break;
                    case "HHMMSS":
                        strReturn = dt.ToString("HHmmss");
                        break;
                    case "DDMMYY":
                        strReturn = dt.ToString("ddMMyy");
                        break;
                    case "DDMMYYYY":
                        strReturn = dt.ToString("ddMMyyyy");
                        break;
                    case "MMDDYY":
                        strReturn = dt.ToString("MMddyy");
                        break;
                    case "MMDDYYYY":
                        strReturn = dt.ToString("MMddyyyy");
                        break;
                    default:
                        strReturn = strYear + strMonth + strDay + strHour + strMinute + strSecond;
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("CreateDateTimeString Exception:" + e);
            }

            return strReturn;
        }
    }
}

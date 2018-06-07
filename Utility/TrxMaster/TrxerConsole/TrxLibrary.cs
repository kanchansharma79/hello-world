using Microsoft.VisualStudio.TestTools.TestAuto;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;

namespace Trx2HtmlConsole
{
    partial class Program
    {
        public static string TransformTrxToHtml(string fileName, XmlDocument xsl, string outputFile, bool EmailReport, bool storeInDatabase, string dbConnectionstring)
        {
            return Transform(fileName, xsl, outputFile, EmailReport, true, connectionstring);
        }
        public static string TransformTrxToHtml(string fileName, XmlDocument xsl, string outputFile)
        {
            return Transform(fileName, xsl, outputFile, true);
        }
        public static string TransformTrxToHtml(string fileName, XmlDocument xsl)
        {
            return Transform(fileName, xsl);
        }


        /// <summary>
        /// Transforms trx int html document using xslt
        /// </summary>
        /// <param name="inputFileName">Trx file path</param>
        /// <param name="xsl">Xsl document</param>
        private static string Transform(string inputFileName, XmlDocument xsl, string outputFile, bool EmailReport, bool storeInDatabase, string dbConnectionstring)
        {
            try
            {
                XslCompiledTransform x = new XslCompiledTransform(true);
                x.Load(xsl, new XsltSettings(true, true), null);
                Console.WriteLine("Transforming...");
                if (EmailReport)
                    outputFile = outputFile+ "_email.html";
                else
                    outputFile = outputFile + ".html";

                x.Transform(inputFileName, outputFile);
                Console.WriteLine("Done transforming xml into html at:- " + outputFile);

                if (storeInDatabase)
                {
                    string todayDate = DateTime.Today.Date.ToShortDateString();
                    string htmlContent = gm.GetHtmlContent(outputFile);

                    // Encode the content for storing in Sql server.
                    string htmlEncoded = WebUtility.HtmlEncode(htmlContent);
                    string insertHtmlContentInDatabase = "INSERT INTO CuitReport (HtmlContent,Creation) values ('" + htmlEncoded + "','" + todayDate + "')";

                    gm.ExecuteSQL(insertHtmlContentInDatabase, dbConnectionstring, 0);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return outputFile;
        }
        private static string Transform(string fileName, XmlDocument xsl, string outputFile, bool EmailReport)
        {
            return Transform(fileName, xsl, outputFile, EmailReport, false, null);
        }
        private static string Transform(string fileName, XmlDocument xsl)
        {
            string outputFile = gm.GetBaseDirectory()+"AutomationReport_email.html";
            return Transform(fileName, xsl, outputFile, true);
        }

        public static void GenerateHtmlReportFileFromDatabase(ref string filePath, string connectionstring)
        {
            Console.WriteLine("Transforming ... to html");
            try
            {
                string htmlContentQuery = "SELECT HtmlContent FROM CuitReport WHERE TR_ID = 2";
                var htmlContent = gm.QuerySQLValue(htmlContentQuery, connectionstring);

                // Encode the content for storing in Sql server.
                string htmlEncoded = gm.GetHtmlEncode(htmlContent);

                filePath = filePath + "AutomationReport.html";
                // Create a file to write to.
                gm.WriteToFile(htmlEncoded, filePath, false, true);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Loads xslt form embedded resource
        /// </summary>
        /// <returns>Xsl document</returns>
        private static XmlDocument PrepareXsl(string XSLT_FILE)
        {
            XmlDocument xslDoc = new XmlDocument();
            Console.WriteLine("Loading xslt template...");
            xslDoc.Load(ResourceReader.StreamFromResource(XSLT_FILE));
            MergeCss(xslDoc);
            MergeJavaScript(xslDoc);
            return xslDoc;
        }

        /// <summary>
        /// Merges all javascript linked to page into Trxer html report itself
        /// </summary>
        /// <param name="xslDoc">Xsl document</param>
        private static void MergeJavaScript(XmlDocument xslDoc)
        {
            Console.WriteLine("Loading javascript...");
            XmlNode scriptEl = xslDoc.GetElementsByTagName("script")[0];
            XmlAttribute scriptSrc = scriptEl.Attributes["src"];
            string script = ResourceReader.LoadTextFromResource(scriptSrc.Value);
            scriptEl.Attributes.Remove(scriptSrc);
            scriptEl.InnerText = script;
        }

        /// <summary>
        /// Merges all css linked to page ito Trxer html report itself
        /// </summary>
        /// <param name="xslDoc">Xsl document</param>
        private static void MergeCss(XmlDocument xslDoc)
        {
            Console.WriteLine("Loading css...");
            XmlNode headNode = xslDoc.GetElementsByTagName("head")[0];
            XmlNodeList linkNodes = xslDoc.GetElementsByTagName("link");
            List<XmlNode> toChangeList = linkNodes.Cast<XmlNode>().ToList();

            foreach (XmlNode xmlElement in toChangeList)
            {
                XmlElement styleEl = xslDoc.CreateElement("style");
                styleEl.InnerText = ResourceReader.LoadTextFromResource(xmlElement.Attributes["href"].Value);
                headNode.ReplaceChild(styleEl, xmlElement);
            }
        }

        /*
        
        public string sqlConnectionString = ConfigurationManager.ConnectionStrings["TestAutomationReport"].ConnectionString;

        public void dbAddHtmlContentExecuteQuery(string sqlQuery, string param, object paramValue)
        {
            SqlConnection sqlConnection = null;
            try
            {

                using (var conn = new SqlConnection(sqlConnectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = sqlQuery;
                    //SqlCom.Parameters.Add(new SqlParameter("@FileData", (object)paramValue));
                    cmd.Parameters.AddWithValue(param, paramValue);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
            }
            finally
            {
                //sqlConnection.Close();
            }
        }

        

        

        //Open file in to a filestream and read data in a byte array.
        public byte[] ReadFile(string sPath)
        {
            //Initialize byte array with a null value initially.
            byte[] data = null;

            //Use FileInfo object to get file size.
            FileInfo fInfo = new FileInfo(sPath);
            long numBytes = fInfo.Length;

            //Open FileStream to read file
            FileStream fStream = new FileStream(sPath, FileMode.Open, FileAccess.Read);

            //Use BinaryReader to read file stream into byte array.
            BinaryReader br = new BinaryReader(fStream);

            //When you use BinaryReader, you need to supply number of bytes to read from file.
            //In this case we want to read entire file. So supplying total number of bytes.
            data = br.ReadBytes((int)numBytes);

            //Close BinaryReader
            br.Close();

            //Close FileStream
            fStream.Close();

            return data;
        }

        

        private void cmdSave_Click()
        {
            try
            {
                ////Set insert query
                //string qry = "insert into FilesStore (OriginalPath,FileData) values(@OriginalPath, @FileData)";

                ////Initialize SqlCommand object for insert.
                //SqlCommand SqlCom = new SqlCommand(qry, CN);

                ////We are passing Original File Path and file byte data as sql parameters.
                //SqlCom.Parameters.Add(new SqlParameter("@OriginalPath", (object)txtFilePath.Text));
                //SqlCom.Parameters.Add(new SqlParameter("@FileData", (object)FileData));

                ////Open connection and execute insert query.
                //CN.Open();
                //SqlCom.ExecuteNonQuery();
                //CN.Close();

                ////Close form and return to list or Files.
                //this.Close();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
        }
         * */
    }
}

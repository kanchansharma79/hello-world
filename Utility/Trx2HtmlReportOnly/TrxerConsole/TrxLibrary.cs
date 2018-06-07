using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;

namespace Trx2HtmlConsole
{
    partial class Program
    {
        /// <summary>
        /// Transforms trx int html document using xslt
        /// </summary>
        /// <param name="inputFileName">Trx file path</param>
        /// <param name="xsl">Xsl document</param>
        private static string Transform(string inputFileName, XmlDocument xsl, string outputFile, bool EmailReport)
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
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return outputFile;
        }
        
        private static string Transform(string fileName, XmlDocument xsl)
        {
            string outputFile = GetBaseDirectory()+"AutomationReport_email.html";
            return Transform(fileName, xsl, outputFile, true);
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
            try
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
            catch (Exception e)
            {
                Console.WriteLine("WriteToFile:" + e);
            }
        }

        public static void GetAppSettingsKeyValue(string AppKey, out string AppKeyValue)
        {
            AppKeyValue = null;
            try
            {
                AppKeyValue = ConfigurationManager.AppSettings[AppKey];
            }
            catch (Exception e)
            {
                Console.WriteLine("WriteToFile:" + e);
            }
        }

        public static string GetBaseDirectory()
        {
            string dir = string.Empty;
            try
            {
                dir = AppDomain.CurrentDomain.BaseDirectory;
            }
            catch (Exception e)
            {
                Console.WriteLine("WriteToFile:" + e);
            }
            return dir;
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
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace MergeTrxFile.TrxToHtml
{
    public static class ReportGeneratore
    {
        public static string GenerateXmlToXhtmlSample(string myXmlFile, string myStyleSheet, string outputDir)
        {
            StringWriter results = new StringWriter();
            try
            {
                // Read in XSLT file.
                XslTransform myXslTransform = new XslTransform();
                myXslTransform.Load(myStyleSheet);
                
                // Read in XML data.
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(myXmlFile);
                XPathNavigator xpathnav = xDoc.CreateNavigator();

                // Generate html body.
                StringBuilder sb3 = new StringBuilder(); 
                XmlTextWriter xmlwriter = new XmlTextWriter(new System.IO.StringWriter(sb3));

                myXslTransform.Transform(xpathnav, null, xmlwriter, null);
                //myXslTransform.Transform(myXmlFile, outputDir);

                //XslCompiledTransform transform = new XslCompiledTransform();

                //using (XmlReader reader = XmlReader.Create(new StringReader(myXmlFile)))
                //{
                //    transform.Load(reader);
                //}

                //using (XmlReader reader = XmlReader.Create(new StringReader(myXmlFile)))
                //{
                //    transform.Transform(reader, null, results);
                //}
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
            }
            return results.ToString();
        }

        public static string GenerateXmlToXhtml(string myXmlFile, string myStyleSheet, string outputDir)
        {
            string fileNameWithPath = string.Empty;
            try 
	        {
                string filename = System.IO.Path.GetFileName(outputDir);
                if (string.IsNullOrEmpty(filename))
                    fileNameWithPath = outputDir + "\\Result.html";
                else
                    fileNameWithPath = outputDir;

                XslCompiledTransform myXslTrans = new XslCompiledTransform();
                XPathDocument myXPathDoc = new XPathDocument(myXmlFile);
                myXslTrans.Load(myStyleSheet);
                XmlTextWriter myWriter = new XmlTextWriter(fileNameWithPath, null);
                myXslTrans.Transform(myXPathDoc, null, myWriter);
                myWriter.Close();
                //var ab = myWriter;
	        }
	        catch (Exception e)
	        {
                Console.WriteLine(e.Message.ToString());
	        }
            return fileNameWithPath;
        }
    }
}

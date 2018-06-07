<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl"
                xmlns:trxreport="urn:myScripts"
                xmlns:t="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  
  <xsl:output method="html" indent="yes"/>
  <xsl:key name="TestMethods" match="t:TestMethod" use="@className"/>
  <xsl:key name="UnitTestResults" match="t:UnitTestResult" use="@className"/>
  
  <!--My scripts in c#--><!--
  <msxsl:script language="C#" implements-prefix="trxreport">
    public string RemoveAssemblyName(string asm) {
    return asm.Substring(0,asm.IndexOf(','));
    }
    --><!--public string RemoveNamespace(string asm) {
    int coma = asm.IndexOf(',');
    return asm.Substring(coma + 2, asm.Length - coma - 2);--><!--
    }
  </msxsl:script>-->
  
  <xsl:template match="/">
    <html>
      <head>
        <div id ="root" class="mainhead">
          <h1 align="center"> Test Run Summary!!! </h1>
        </div>
      </head>
      
      <body>        
        <div class="contents">
          <a href="#totals">Totals</a>
          |
          <a href="#summary">Summary</a>
          |
          <a href="#detail">Detail</a>
          |
          <a href="#envInfo">Environment Information</a>
        </div>
        <br />
        <a name="totals" />
        
        <table id="tMainSummary" border="1">
          <tr>
            <!--<th>Percent</th>-->
            <th>Status</th>
            <th>TotalTests</th>
            <th>Passed</th>
            <th>Failed</th>
            <th>Aborted</th>
            <th>Executed</th>
            <th>Not Executed</th>
            <th>TimeTaken</th>
          </tr>
          <tr>
            <td width="250px" style="vertical-align:middle;font-size:100%">
              <xsl:value-of select="/t:TestRun/t:ResultSummary/@outcome"/>
            </td>
            <td width="150px" style="vertical-align:middle;font-size:100%">
              <xsl:value-of select="/t:TestRun/t:ResultSummary/t:Counters/@total"/>
            </td>
            <td width="150px" style="vertical-align:middle;font-size:100%">
              <xsl:value-of select="/t:TestRun/t:ResultSummary/t:Counters/@passed"/>
            </td>
            <td width="150px" style="vertical-align:middle;font-size:100%">
              <xsl:value-of select="/t:TestRun/t:ResultSummary/t:Counters/@failed"/>
            </td>
            <td width="150px" style="vertical-align:middle;font-size:100%">
              <xsl:value-of select="/t:TestRun/t:ResultSummary/t:Counters/@aborted"/>
            </td>
            <td width="150px" style="vertical-align:middle;font-size:100%">
              <xsl:value-of select="/t:TestRun/t:ResultSummary/t:Counters/@executed"/>
            </td>
            <td width="150px" style="vertical-align:middle;font-size:100%">
              <xsl:value-of select="/t:TestRun/t:ResultSummary/t:Counters/@notExecuted"/>
            </td>
            <td width="550px" style="vertical-align:middle;font-size:100%">
              <xsl:value-of select="/t:TestRun/t:Times/@start"/>
            </td>
          </tr>
        </table>
        <br />
        <a name="detail" />
        
        <i>Test Class Detail</i>
   <table id="tTestDetail" border="1">
          <tr bgcolor="#8470ff">
            <th>Test Case ID</th>
            <th>Test Case Name</th>
            <th>Outcome</th>
            <th>Execution Machine Name</th>
            <th>Duration</th>
            <th>Error Message</th>
          </tr>
          <xsl:for-each select="/t:TestRun/t:Results/t:UnitTestResult">
          <tr width="150px" style="vertical-align:middle;font-size:100%">
            <!--Get Test Case ID-->
            <td width="150px" style="vertical-align:middle;font-size:100%">
              <span style="color:#ff0000">
                <xsl:value-of select="/t:TestRun/t:UnitTest/t:TcmInformation/@testCaseId"/>
                <br/>
              </span>
              <!--<xsl:call-template name="Kanchan"/>-->
            </td>
            
            <td width="150px" style="vertical-align:middle;font-size:100%">              
              <xsl:value-of select="/t:TestRun/t:Results/t:UnitTestResult/@testName"/>            
            </td>
            
            <td bgcolor="#9acd32" width="150px" style="vertical-align:middle;font-size:100%">              
              <xsl:value-of select="/t:TestRun/t:Results/t:UnitTestResult/@outcome"/>            
            </td>
            
            <td width="150px" style="vertical-align:middle;font-size:100%">
              <xsl:value-of select="/t:TestRun/t:Results/t:UnitTestResult/@computerName"/>
            </td>
            
            <td width="150px" style="vertical-align:middle;font-size:100%">
              <xsl:value-of select="/t:TestRun/t:Results/t:UnitTestResult/@duration"/>
            </td>
            
          </tr>
        </xsl:for-each>
        </table>     green color #9acd32
        
        
        <table id="tTestDetail" border="1">
          <tr bgcolor="#8470ff">
            <th>Test Case ID</th>
            <th>Test Case Name</th>
            <th>Outcome</th>
            <th>Execution Machine Name</th>
            <th>Duration</th>
            <th>Error Message</th>
          </tr>
          <xsl:for-each select="TestRun/Results/UnitTestResult">
          <tr width="150px" style="vertical-align:middle;font-size:100%">
            <td width="150px" style="vertical-align:middle;font-size:100%">
              <xsl:value-of select="TestRun/Results/UnitTestResult/testName"/>
            </td>
            <td bgcolor="#9acd32" width="150px" style="vertical-align:middle;font-size:100%">
              <xsl:value-of select="/t:TestRun/t:Results/t:UnitTestResult/@outcome"/>
            </td>
            <td width="150px" style="vertical-align:middle;font-size:100%">
              <xsl:value-of select="/t:TestRun/t:Results/t:UnitTestResult/@computerName"/>
            </td>
            <td width="150px" style="vertical-align:middle;font-size:100%">
              <xsl:value-of select="/t:TestRun/t:Results/t:UnitTestResult/@duration"/>
            </td>
          </tr>
        </xsl:for-each>
        </table>
        
        <a href="#__top">Back to top</a>
        <br />
      </body>
    </html>
  </xsl:template>

  <!--Template for test case id-->
  <xsl:template name="Kanchan" match ="TestDefinitions">
    <!--<span style="color:#ff0000">Kanchan</span>-->
    <xsl:for-each select="/t:TestRun/t:UnitTest">
      <span style="color:#ff0000">
        <xsl:value-of select="/t:TestRun/t:UnitTest/t:TcmInformation/@testCaseId"/>
        <br/>
      </span>
    </xsl:for-each>
  </xsl:template>

</xsl:stylesheet>

  

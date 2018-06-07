<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
    xmlns:t="http://microsoft.com/schemas/VisualStudio/TeamTest/2010"
    xmlns:trxreport="urn:my-scripts"
>
  <xsl:output method="html" indent="yes"/>
  <xsl:key name="TestMethods" match="t:TestMethod" use="@className"/>
  <!--<xsl:namespace-alias stylesheet-prefix="t" result-prefix="#default"/>-->

  <msxsl:script language="C#" implements-prefix="trxreport">
    <![CDATA[
    public string RemoveAssemblyName(string asm) 
    {
        int idx = asm.IndexOf(',');
        if (idx == -1)
            return asm;
        return asm.Substring(0, idx);
    }
    public string GenerateClassName(string asm)
    {
        int firstIndexComma = asm.IndexOf(',')+1;
        int secondIndexComma = asm.IndexOf(',', asm.IndexOf(',')+1);
        int len = secondIndexComma-firstIndexComma;
        if (firstIndexComma == -1)
            return asm;
        else
            return asm.Substring(firstIndexComma + 1, len - 1).Trim();
    }
    public string RemoveNamespace(string asm) 
    {
      int coma = asm.IndexOf(',');
      return asm.Substring(coma + 2, asm.Length - coma - 2);
    }
    public string GetShortDateTime(string time)
    {
      if( string.IsNullOrEmpty(time) )
      {
        return string.Empty;
      }
      
      return DateTime.Parse(time).ToString();
    }
    public string GetShortDateString(string time)
    {
      if( string.IsNullOrEmpty(time) )
      {
        return string.Empty;
      }
      
      return DateTime.Parse(time).ToShortDateString();
    }
    
    private string ToExtactTime(double ms)
    {
      if (ms < 1000)
        return ms + " ms";

      if (ms >= 1000 && ms < 60000)
        return string.Format("{0:0.00} seconds", TimeSpan.FromMilliseconds(ms).TotalSeconds);

      if (ms >= 60000 && ms < 3600000)
        return string.Format("{0:0.00} minutes", TimeSpan.FromMilliseconds(ms).TotalMinutes);

      return string.Format("{0:0.00} hours", TimeSpan.FromMilliseconds(ms).TotalHours);
    }
    
    public string ToExactTimeDefinition(string duration)
    {
      if( string.IsNullOrEmpty(duration) )
      {
        return string.Empty;
      } 
    
      return  ToExtactTime(TimeSpan.Parse(duration).TotalMilliseconds);
    }
    
    public string ToExactTimeDefinition(string start,string finish)
    {
      TimeSpan datetime=DateTime.Parse(finish)-DateTime.Parse(start);
      return ToExtactTime(datetime.TotalMilliseconds);
    }
    
    public string CurrentDateTime()
    {
      return DateTime.Now.ToString();
    }
    
    public string ExtractImageUrl(string text)
    {
       Match match = Regex.Match(text, "('|\")([^\\s]+(\\.(?i)(jpg|png|gif|bmp)))('|\")",
	      RegexOptions.IgnoreCase);

	
	   if (match.Success)
	   {
	      return match.Value.Replace("\'",string.Empty).Replace("\"",string.Empty).Replace("\\","\\\\");
	    }
      return string.Empty;
    }
        
    ]]>
  </msxsl:script>

  <xsl:template match="/" >
    <xsl:text disable-output-escaping='yes'>&lt;!DOCTYPE html></xsl:text>
    <html>
      <head>
        <meta charset="utf-8"/>
        <link rel="stylesheet" type="text/css" href="Trxer.css"/>
        <link rel="stylesheet" type="text/css" href="TrxerTable.css"/>
        <script language="javascript" type="text/javascript" src="functions.js"></script>
        <title>
          <xsl:value-of select="/t:TestRun/@name"/>
        </title>
      </head>
      <body>
        <div id="divToRefresh" class="wrapOverall">
          <div id="floatingImageBackground" class="floatingImageBackground" style="visibility: hidden;">
            <div class="floatingImageCloseButton" onclick="hide('floatingImageBackground');"></div>
            <img src="" id="floatingImage"/>
          </div>
          <br />
          <xsl:variable name="testRunOutcome" select="t:TestRun/t:ResultSummary/@outcome"/>

          <div class="StatusBar statusBar{$testRunOutcome}">
            <div class="statusBar{$testRunOutcome}Inner">
              <center>
                <h1 class="hWhite">
                  <div class="titleCenterd">
                    <xsl:value-of select="/t:TestRun/@name"/>
                  </div>
                </h1>
              </center>
            </div>
          </div>
          <div class="SummaryMain">
            <div class="resultSummaryDiv">
              <div class="ResultSummary">Results Summary</div>
              <div class="ResultSummaryPie">Pie View</div>
              <div id="dataViewer"></div>
            </div>

            <div class="testStatusDiv">
              <div class="DetailsTable StatusesTable">Tests Statuses</div>
              <div class="statusDiv">
                <div class="column1">Total</div>
                <div class="statusCount">
                  <xsl:value-of select="/t:TestRun/t:ResultSummary/t:Counters/@total" />
                </div>
              </div>
              <div class="statusDiv">
                <div class="column1">Executed</div>
                <div class="statusCount">
                  <xsl:value-of select="/t:TestRun/t:ResultSummary/t:Counters/@executed" />
                </div>
              </div>
              <div class="statusDiv">
                <div class="column1">Passed</div>
                <div class="statusCount">
                  <xsl:value-of select="/t:TestRun/t:ResultSummary/t:Counters/@passed" />
                </div>
              </div>
              <div class="statusDiv">
                <div class="column1">Failed</div>
                <div class="statusCount">
                  <xsl:value-of select="/t:TestRun/t:ResultSummary/t:Counters/@failed" />
                </div>
              </div>
              <div class="statusDiv">
                <div class="column1">Inconclusive</div>
                <div class="statusCount">
                  <xsl:value-of select="/t:TestRun/t:ResultSummary/t:Counters/@inconclusive" />
                </div>
              </div>
              <div class="statusDiv">
                <div class="column1">Error</div>
                <div class="statusCount">
                  <xsl:value-of select="/t:TestRun/t:ResultSummary/t:Counters/@error" />
                </div>
              </div>
              <div class="statusDiv">
                <div class="column1">Warning</div>
                <div class="statusCount">
                  <xsl:value-of select="/t:TestRun/t:ResultSummary/t:Counters/@warning" />
                </div>
              </div>
              <div class="statusDiv">
                <div class="column1">Timeout</div>
                <div class="statusCount">
                  <xsl:value-of select="/t:TestRun/t:ResultSummary/t:Counters/@timeout" />
                </div>
              </div>
            </div>

            <div class="runtimeSummaryDiv">
              <div class="DetailsTable StatusesTable">Run Time Summary</div>
              <xsl:for-each select="/t:TestRun/t:Times">
              <div class="statusDiv">
                <div class="column1">Start Time</div>
                <div class="statusCount">
                  <xsl:value-of select="trxreport:GetShortDateTime(@start)" />
                </div>
              </div>
              <div class="statusDiv">
                <div class="column1">End Time</div>
                <div class="statusCount">
                  <xsl:value-of select="trxreport:GetShortDateTime(@finish)" />
                </div>
              </div>
              <div class="statusDiv">
                <div class="column1">Duration</div>
                <div class="statusCount">
                  <xsl:value-of select="trxreport:ToExactTimeDefinition(@start,@finish)"/>
                </div>
              </div>
              </xsl:for-each>
            </div>

            <div class="testDetailDiv">
              <div class="DetailsTable StatusesTable">Tests Details</div>
                <div class="statusDiv">
                  <div class="column1">User</div>
                  <div class="statusCount">
                    <xsl:value-of select="/t:TestRun/@runUser" />
                  </div>
                </div>
                <div class="statusDiv">
                  <div class="column1">Machine</div>
                  <div class="statusCount">
                    <xsl:value-of select="//t:UnitTestResult/@computerName" />
                  </div>
                </div>
                <div class="statusDiv">
                  <div class="column1">Description</div>
                  <div class="statusCount">
                    <xsl:value-of select="/t:TestRun/t:TestRunConfiguration/t:Description"/>
                  </div>
                </div>
            </div>
          </div>                 
          <xsl:variable name="testsFailedSet" select="//t:TestRun/t:Results/t:UnitTestResult[@outcome='Failed']" />
          <xsl:variable name="testsFailedCount" select="count(testsFailedSet)" />
          <xsl:if test="$testsFailedSet">
            <div class="ReportsTableDiv">
              <div class="caption">All Failed Tests</div>
              <div class="FailedTestInfo">
                <div class="column1Failed">                 </div>
                <div class="Description">Failed Test Cases</div>
                <div class="NumberTagRedTest">
                  <xsl:value-of select="/t:TestRun/t:ResultSummary/t:Counters/@failed" />
                </div>
                <div class="OpenMoreButton" onclick="ShowHide('{generate-id(faileds)}TestsContainer','{generate-id(faileds)}Button','Show Tests','Hide Tests');">
                  <div class="MoreButtonText" id="{generate-id(faileds)}Button">Hide Tests</div>
                </div>
              </div>
            </div>
            <div class="ReportsTableFailedDiv">
              <div id="{generate-id(faileds)}TestsContainer" class="hiddenRow"> <!--visibleRowError-->
                <div id="exceptionArrow">↳</div>
                <div>
                  <div class="divTable">
                    <div class="divTableFailedHeading">
                    <div class="divTableRow">
                    <div class="divTableHead">Time</div>
                    <div class="divTableHeadID">TestID</div>
                    <div class="divTableHeadTName">Test Name</div>
                    <div class="divTableHeadStatus">Status</div>
                    <div class="divTableHeadMessage">Message</div>
                    <div class="divTableHeadMachine">Computer</div>
                    <div class="divTableHeadDuration">Duration</div>
                    </div>
                    </div>
                    <div class="divTableBody">
                      <!--Start of package content-->
                      <xsl:for-each select="$testsFailedSet">
                        <xsl:call-template name="tDetails">
                          <xsl:with-param name="testId" select="@testId" />
                          <xsl:with-param name="testDescription" select="./../t:Description" />
                        </xsl:call-template>
                      </xsl:for-each>
                    </div>
                  </div>
                    <!--End of package content-->
                </div>
              </div>
            </div>
            <!--Start of package content--><!--
                        <xsl:for-each select="$testsFailedSet">
                          <xsl:call-template name="tDetails">
                            <xsl:with-param name="testId" select="@testId" />
                            <xsl:with-param name="testDescription" select="./../t:Description" />
                          </xsl:call-template>
                        </xsl:for-each>
                      </tbody>
                      -->
            <!--End of package content--><!--
                    </table>
                  </td>
                </tr>
              </tbody>
            </table>-->
          </xsl:if>          
          <xsl:variable name="classSet" select="//t:TestMethod[generate-id(.)=generate-id(key('TestMethods', @className))]" />
          <xsl:variable name="classCount" select="count($classSet)" />
          <div class="divTableClasses">
            <div class="caption">All Tests Group By Classes/Main Feature Area</div>
            <div class="divTableClassHeading">
              <div class="divTableRow">
                <div class="divTableHeadRow">Execution Date</div>
                <div class="divTableHeadRow">Status</div>
                <div class="divTableHeadRow">
                  Classes
                  <div class="NumberTag">
                    <xsl:value-of select="$classCount" />
                  </div>
                </div>
                <div class="divTableHeadRow">Test Count</div>
                <div class="divTableHeadRow">More</div>
              </div>
            </div>
          </div>
          <div class="divTableClassBody">
              <xsl:for-each select="$classSet">
                <xsl:variable name="testsSet" select="key('TestMethods', @className)" />
                <xsl:variable name="testsCount" select="count($testsSet)" />
                <div class="divTableClassRow">
                  <div class="divTableClassCell"><xsl:value-of select="trxreport:GetShortDateString(/t:TestRun/t:Times/@creation)"/></div>
                  <div class="divTableClassCell"><canvas id="{generate-id(@className)}canvas" height="15px" width="20%"></canvas></div>
                  <div class="divTableClassCell"><xsl:value-of select="trxreport:GenerateClassName(@className)" /></div>
                  <div class="divTableClassCell"><xsl:value-of select="concat($testsCount,' Tests')" /></div>
                  <div class="divTableClassCell">
                    <div class="OpenMoreButton" onclick="ShowHide1('{generate-id(@className)}TestsContainer','{generate-id(@className)}Button','Show Tests','Hide Tests');">
                        <div class="MoreButtonText" id="{generate-id(@className)}Button">Show Tests</div>
                      </div>
                  </div>
                </div>
                <script>
                  CalculateTestsStatuses('<xsl:value-of select="generate-id(@className)"/>TestsContainer','<xsl:value-of select="generate-id(@className)"/>canvas');
                </script>
                <div class="ReportsTableClassesTestDiv">
                    <div id="{generate-id(@className)}TestsContainer" class="hiddenRow">
                      <div id="exceptionClassArrow">↳</div>
                        <div class="divTable">
                          <div class="divTableHeading">
                            <div class="divTableRow">
                              <div class="divTableHead">Time</div>
                              <div class="divTableHeadID">TestID</div>
                              <div class="divTableHeadTName">Test Name</div>
                              <div class="divTableHeadStatus">Status</div>
                              <div class="divTableHeadMessage">Message</div>
                              <div class="divTableHeadMachine">Computer</div>
                              <div class="divTableHeadDuration">Duration</div>
                            </div>
                          </div>
                          <div class="divTableBody">
                            <xsl:for-each select="$classSet">
                                <xsl:call-template name="tDetails">
                                  <xsl:with-param name="testId" select="./../@id" />
                                  <xsl:with-param name="testDescription" select="./../t:Description" />
                                </xsl:call-template>
                              </xsl:for-each>
                          </div>
                        </div>
                      </div>
                  </div>
              </xsl:for-each>
            </div>
         </div>
        
          <table id="ReportsTable">
            <caption>All Tests Group By Classes/Main Feature Area</caption>
            <thead>
              <tr class="odd">
                <th scope="col">Execution Date</th>
                <th scope="col" abbr="Status">Status</th>
                <th scope="col" abbr="Test">
                  Classes <div class="NumberTag">
                    <xsl:value-of select="$classCount" />
                  </div>
                </th>
                <th scope="col" abbr="TCount">Test Count</th>
                <th scope="col" abbr="Exception">More</th>
              </tr>
            </thead>
            <tbody>
              <xsl:for-each select="$classSet">
                <xsl:variable name="testsSet" select="key('TestMethods', @className)" />
                <xsl:variable name="testsCount" select="count($testsSet)" />
                <tr>
                  <th scope="row" class="column1">
                    <xsl:value-of select="trxreport:GetShortDateString(/t:TestRun/t:Times/@creation)"/>
                  </th>
                  <td class="PackageStatus">
                    <canvas id="{generate-id(@className)}canvas" width="100" height="25"> </canvas>
                  </td>
                  <td class="Function">
                    <xsl:value-of select="trxreport:GenerateClassName(@className)" />
                  </td>
                  <td class="Message" name="{generate-id(@className)}Id">
                    <xsl:value-of select="concat($testsCount,' Tests')" />
                  </td>
                  <td class="ex">
                    <div class="OpenMoreButton" onclick="ShowHide('{generate-id(@className)}TestsContainer','{generate-id(@className)}Button','Show Tests','Hide Tests');">
                      <div class="MoreButtonText" id="{generate-id(@className)}Button">Show Tests</div>
                    </div>
                  </td>
                </tr>
                <tr id="{generate-id(@className)}TestsContainer" class="hiddenRow">
                  <td colspan="5">
                    <div id="exceptionArrow">↳</div>
                    <table>
                      <thead>
                        <tr class="odd">
                          <th scope="col" class="TestsTable">Time</th>
                          <th scope="col" class="TestsTable" abbr="TID">TestID</th>
                          <th scope="col" class="TestsTable" abbr="Test">Test Name</th>
                          <th scope="col" class="TestsTable" abbr="Status">Status</th>
                          <th scope="col" class="TestsTable" abbr="Message">Message</th>
                          <th scope="col" class="TestsTable" abbr="Machine">Computer</th>
                          <th scope="col" class="TestsTable" abbr="Time">Duration</th>
                        </tr>
                      </thead>
                      <tbody>
                        <!--Start of package content-->
                        <xsl:for-each select="$testsSet">
                          <xsl:call-template name="tDetails">
                            <xsl:with-param name="testId" select="./../@id" />
                            <xsl:with-param name="testDescription" select="./../t:Description" />
                          </xsl:call-template>
                        </xsl:for-each>
                      </tbody>
                      <!--End of package content-->
                    </table>
                  </td>
                </tr>
                <script>
                  CalculateTestsStatuses('<xsl:value-of select="generate-id(@className)"/>TestsContainer','<xsl:value-of select="generate-id(@className)"/>canvas');
                </script>
              </xsl:for-each>
            </tbody>
          </table>
          
          <div class="performancemain">
          <div class="perfcap">Five most slowest tests</div>
          <div class="perfHead">
            <div class="perHeadTimeCol">Time</div>
            <div class="perHeadTidCol">TestID</div>
            <div class="perHeadTNameCol">Test Name</div>
            <div class="perHeadStatusCol">Status</div>
            <div class="perHeadSystemCol">Computer</div>
            <div class="perHeadperfCol">Duration</div>
          </div>
          <div class="perfData">
            <xsl:for-each select="/t:TestRun/t:Results/t:UnitTestResult">
              <xsl:sort select="@duration" order="descending"/>
              <xsl:if test="position() &gt;= 1 and position() &lt;=5">
                <xsl:variable name="testId" select="@testId" />
                <div class="perDataCol"></div>
                <div class="perDataTimeCol">
                  <xsl:value-of select="trxreport:GetShortDateTime(@startTime)" />
                </div>
                <div class="perDataTidCol">
                  <xsl:value-of select="/t:TestRun/t:TestDefinitions/t:UnitTest[@id=$testId]/t:TcmInformation/@testCaseId" />
                </div>
                <div class="perDataTNameCol">
                  <xsl:value-of select="@testName"/>
                </div>
                <div class="perDataStatusCol">
                  <xsl:call-template name="tStatus">
                    <xsl:with-param name="testId" select="@testId" />
                  </xsl:call-template>
                </div>
                <div class="perDataSystemCol">
                  <xsl:value-of select="@computerName" />
                </div>
                <div class="perDataperfCol">
                  <xsl:value-of select="trxreport:ToExactTimeDefinition(@duration)"/>
                </div>
              </xsl:if>
            </xsl:for-each>
          </div>
        </div>
          <div class="divTableFoot">
          <td colspan="5">
            <h6>Automation result summary log - {CompanyName}. Developed by Kanchan Sharma</h6>
          </td>
        </div>
        
      </body>
      <script>
        CalculateTotalPrecents();
      </script>
    </html>
  </xsl:template>
  
  <xsl:template name="tStatus">
    <xsl:param name="testId" />
    <xsl:for-each select="/t:TestRun/t:Results/t:UnitTestResult[@testId=$testId]">
      <xsl:choose>
        <xsl:when test="@outcome='Passed'">
          <div class="passed">PASSED</div>
        </xsl:when>
        <xsl:when test="@outcome='Failed'">
          <div class="failed">FAILED</div>
        </xsl:when>
        <xsl:when test="@outcome='Inconclusive'">
          <div class="warn">Inconclusive</div>
        </xsl:when>
        <xsl:when test="@outcome='Timeout'">
          <div class="failed">Timeout</div>
        </xsl:when>
        <xsl:when test="@outcome='Error'">
          <div class="failed">Error</div>
        </xsl:when>
        <xsl:when test="@outcome='Warn'">
          <div class="warn">Warn</div>
        </xsl:when>
        <xsl:otherwise>
          <div class="info">OTHER</div>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>
  </xsl:template>
  <xsl:template name="tDetails">
    <xsl:param name="testId" />
    <xsl:param name="testDescription" />
    <xsl:for-each select="/t:TestRun/t:Results/t:UnitTestResult[@testId=$testId]">      
      <div class="divTableRow">
        <div class="divTableHead"><xsl:value-of select="trxreport:GetShortDateTime(@startTime)" /></div>
        <div class="divTableCellID"><xsl:value-of select="/t:TestRun/t:TestDefinitions/t:UnitTest[@id=$testId]/t:TcmInformation/@testCaseId" /></div>
        <div class="divTableCellTName"><xsl:value-of select="@testName" /></div>
        <div class="divTableCellStatus">
          <xsl:call-template name="tStatus">
            <xsl:with-param name="testId" select="$testId" />
          </xsl:call-template>
        </div>
        <div class="divTableCellMessage">          
          <div class="Message">
            <xsl:call-template name="debugInfo">
              <xsl:with-param name="testId" select="$testId" />
            </xsl:call-template>
          </div>
        </div>
        <div class="divTableCellMachine"><xsl:value-of select="@computerName" /></div>
        <div class="divTableCellDuration"><xsl:value-of select="trxreport:ToExactTimeDefinition(@duration)" /></div>
      </div>
      
      <div id="{generate-id($testId)}Stacktrace" class="hiddenRow">
        <div id="exceptionErrorArrow">↳</div>
        <div class="visibleRowError">
          <td class="visibleRowError">
          <xsl:value-of select="/t:TestRun/t:Results/t:UnitTestResult[@testId=$testId]/t:Output/t:ErrorInfo/t:StackTrace" />
            </td>
        </div>
      </div>      
      <div id="{generate-id($testId)}ResultFiles" class="hiddenRow">
        <div id="exceptionErrorArrow">↳</div>
        <div class="visibleRowError">
          <td class="visibleRowError">
            <xsl:for-each select="/t:TestRun/t:Results/t:UnitTestResult[@testId=$testId]/t:ResultFiles/t:ResultFile" >
              <xsl:value-of select="@path" />
              <br/>
            </xsl:for-each>
          </td>
        </div>
      </div>
      <div id="{generate-id($testId)}MessageErrorInfo" class="hiddenRow">
        <div id="exceptionErrorArrow">↳</div>
        <div class="visibleRowError">
          <td class="visibleRowError">
            <xsl:value-of select="/t:TestRun/t:Results/t:UnitTestResult[@testId=$testId]/t:Output/t:ErrorInfo/t:Message" />
          </td>
        </div>
      </div>
    </xsl:for-each>
  </xsl:template>
  <xsl:template name="tClassDetails">
    <xsl:param name="testId" />
    <xsl:param name="testDescription" />
    <xsl:for-each select="/t:TestRun/t:Results/t:UnitTestResult[@testId=$testId]">
      <div class="divTableRow">
        <div class="divTableHead"><xsl:value-of select="trxreport:GetShortDateTime(@startTime)" /></div>
        <div class="divTableCellID"><xsl:value-of select="/t:TestRun/t:TestDefinitions/t:UnitTest[@id=$testId]/t:TcmInformation/@testCaseId" /></div>
        <div class="divTableCellTName"><xsl:value-of select="@testName" /></div>
        <div class="divTableCellStatus">
          <xsl:call-template name="tStatus">
            <xsl:with-param name="testId" select="$testId" />
          </xsl:call-template>
        </div>
        <div class="divTableCellMessage">
          <div class="Message">
            <xsl:call-template name="debugInfo">
              <xsl:with-param name="testId" select="$testId" />
            </xsl:call-template>
          </div>
        </div>
        <div class="divTableCellMachine"><xsl:value-of select="@computerName" /></div>
        <div class="divTableCellDuration"><xsl:value-of select="trxreport:ToExactTimeDefinition(@duration)" /></div>
      </div>
      
      <div id="{generate-id($testId)}Stacktrace" class="hiddenRow">
        <div id="exceptionErrorArrow">↳</div>
        <div class="visibleRowError">
          <td class="visibleRowError">
          <xsl:value-of select="/t:TestRun/t:Results/t:UnitTestResult[@testId=$testId]/t:Output/t:ErrorInfo/t:StackTrace" />
            </td>
        </div>
      </div>      
      <div id="{generate-id($testId)}ResultFiles" class="hiddenRow">
        <div id="exceptionErrorArrow">↳</div>
        <div class="visibleRowError">
          <td class="visibleRowError">
            <xsl:for-each select="/t:TestRun/t:Results/t:UnitTestResult[@testId=$testId]/t:ResultFiles/t:ResultFile" >
              <xsl:value-of select="@path" />
              <br/>
            </xsl:for-each>
          </td>
        </div>
      </div>
      <div id="{generate-id($testId)}MessageErrorInfo" class="hiddenRow">
        <div id="exceptionErrorArrow">↳</div>
        <div class="visibleRowError">
          <td class="visibleRowError">
            <xsl:value-of select="/t:TestRun/t:Results/t:UnitTestResult[@testId=$testId]/t:Output/t:ErrorInfo/t:Message" />
          </td>
        </div>
      </div>
      
    </xsl:for-each>
  </xsl:template>
  <xsl:template name="imageExtractor">
    <xsl:param name="testId" />
    <xsl:for-each select="/t:TestRun/t:Results/t:UnitTestResult[@testId=$testId]/t:Output">
      <xsl:variable name="StdOut" select="trxreport:ExtractImageUrl(t:StdOut)"/>
      <xsl:variable name="MessageErrorInfo" select="trxreport:ExtractImageUrl(t:ErrorInfo/t:Message)"/>
      <xsl:variable name="MessageErrorStacktrace" select="trxreport:ExtractImageUrl(t:ErrorInfo/t:StackTrace)"/>
      <xsl:variable name="StdErr" select="trxreport:ExtractImageUrl(t:ResultFiles)"/>
      
      <xsl:choose>
        <xsl:when test="$StdOut">
          <div class="atachmentImage" onclick="show('floatingImageBackground');updateFloatingImage('{$StdOut}');"></div>
        </xsl:when>
        <xsl:when test="$MessageErrorInfo">
          <div class="atachmentImage" onclick="show('floatingImageBackground');updateFloatingImage('{$MessageErrorInfo}');"></div>
        </xsl:when>
        <xsl:when test="$MessageErrorStacktrace">
          <div class="atachmentImage" onclick="show('floatingImageBackground');updateFloatingImage('{$MessageErrorStacktrace}');"></div>
        </xsl:when>        
        <!--<xsl:when test="$StdErr">
          <div class="atachmentImage" onclick="show('floatingImageBackground');updateFloatingImage('{$StdErr}');"></div>
        </xsl:when>-->
      </xsl:choose>
    </xsl:for-each>
  </xsl:template>
  <xsl:template name="debugInfo">
    <xsl:param name="testId" />
    <xsl:for-each select="/t:TestRun/t:Results/t:UnitTestResult[@testId=$testId]/t:Output">
      <xsl:variable name="MessageErrorStacktrace" select="t:ErrorInfo/t:StackTrace"/>      
      <xsl:if test="$MessageErrorStacktrace">
          <a style="padding-left: 5px;" id="{generate-id($testId)}StacktraceToggle" href="javascript:ShowHide('{generate-id($testId)}Stacktrace','{generate-id($testId)}StacktraceToggle','Stacktrace','Stacktrace');">Stacktrace</a>
      </xsl:if>
      
      <xsl:variable name="MessageErrorInfo" select="t:ErrorInfo/t:Message"/>
      <xsl:if test="$MessageErrorInfo">
        <td id="{generate-id($testId)}MessageErrorInfo" class="hiddenRow">
        <a style="padding-left: 5px;"  id="{generate-id($testId)}MessageErrorInfoToggle" href="javascript:ShowHide('{generate-id($testId)}MessageErrorInfo','{generate-id($testId)}MessageErrorInfoToggle','Error Msg','Hide Msg');">Error Msg</a>
          </td>
      </xsl:if>

      <xsl:for-each select="/t:TestRun/t:Results/t:UnitTestResult[@testId=$testId]/t:ResultFiles">
        <xsl:variable name="ResultFiles" select="t:ResultFile"/>
        <xsl:if test="$ResultFiles">
          <td id="{generate-id($testId)}ResultFiles" class="hiddenRow">
            <a style="padding-left: 5px;" id="{generate-id($testId)}ResultFileInfoToggle" href="javascript:ShowHide('{generate-id($testId)}ResultFiles','{generate-id($testId)}ResultFileInfoToggle','Files','Hide Files');">Files</a>
          </td>
        </xsl:if>
      </xsl:for-each>
      <!--<xsl:variable name="MessageErrorInfo" select="t:ErrorInfo/t:Message"/>
      <xsl:if test="$MessageErrorInfo">
        <xsl:value-of select="$MessageErrorInfo"/>
        <br/>
      </xsl:if>-->

    </xsl:for-each>
  </xsl:template>
</xsl:stylesheet>
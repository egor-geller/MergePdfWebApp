using MergePdfWebApp.Models;
using Microsoft.Reporting.WebForms.Internal.Soap.ReportingServices2005.Execution;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Services.Protocols;

namespace MergePdfWebApp.Controllers
{
    public class ValuesController : ApiController
    {
        const string ERROR_WITH_PARAMS = "Error with given params";
        private static Logger logger = LogManager.GetCurrentClassLogger();
        //50005 = 'HTTP://TAMLOGFIN/REPORTSERVER?/TAM/Hazmana-Tubin&GoremYozem=9998&Shana=2022&Numerator=4&MsMahadura=1&StatusHazmana=2&SugTofes=2&SumIsBig=1&ShanaTaktzivit=2022'
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public IHttpActionResult Post([FromBody] Details details)
        {
            logger.Info("---- MergePdfWebApp::Post() ----");

            string enviroment = details.enviroment;
            string fileName = details.fileName;
            string fullRsUrl = details.fullRsUrl;//.ToLower();
            string fileUrl = details.fileUrl;
            string rsName = details.rsName.ToLower();
            if (null != rsName)
            {
                rsName = rsName.ToLower();
            }
            string SqlServerName = ""; // "tamlogfin";
            string SqlDbName = ""; // "lgdata";
            string reportPath = "";
            string mifratLink = details.mifratLink;

            logger.Info("MergePdfWebApp:Post::enviroment:" + enviroment);
            logger.Info("MergePdfWebApp:Post::fileName:" + fileName);
            logger.Info("MergePdfWebApp:Post::fullRsUrl:" + fullRsUrl);
            logger.Info("MergePdfWebApp:Post::fileUrl:" + fileUrl);
            logger.Info("MergePdfWebApp:Post::rsName:" + rsName);
            logger.Info("MergePdfWebApp:Post::SqlServerName:" + SqlServerName);
            logger.Info("MergePdfWebApp:Post::SqlDbName:" + SqlDbName);
            logger.Info("MergePdfWebApp:Post::reportPath:" + reportPath);
            logger.Info("MergePdfWebApp:Post::mifratLink:" + mifratLink);



            ReportExecutionService rs = new ReportExecutionService();
            rs.Credentials = System.Net.CredentialCache.DefaultCredentials;
            logger.Info("switch (" + enviroment + ")");
            switch (enviroment)
            {
                case "SAPIENS":
                    SqlServerName = "tamlogfin";
                    SqlDbName = "lgdata";
                    reportPath = @"/TAM";
                    rs.Url = "http://tamlogfin/reportserver/ReportExecution2005.asmx";
                    break;
                case "TADEV":
                    SqlServerName = "sql08\\devop";
                    SqlDbName = "db917";
                    reportPath = @"/CHOSHEN";
                    rs.Url = "http://SQLRS03/reportserver/ReportExecution2005.asmx";
                    break;
                case "TAHAD":
                    SqlServerName = "sql08\\testop";
                    SqlDbName = "db917";
                    reportPath = @"/CHOSHEN";
                    rs.Url = "http://SQLRS03/reportserver/ReportExecution2005.asmx";
                    break;
                case "TAPPR":
                    SqlServerName = "sql08\\preprodop";
                    SqlDbName = "db917";
                    reportPath = @"/CHOSHEN";
                    rs.Url = "http://SQLRS03/reportserver/ReportExecution2005.asmx";
                    break;
                case "TAPROD":
                    SqlServerName = "SQL09";
                    SqlDbName = "db917";
                    reportPath = @"/CHOSHEN";
                    rs.Url = "http://SQLRS03/reportserver/ReportExecution2005.asmx";
                    break;
                default:
                    SqlServerName = "tamlogfin";
                    SqlDbName = "lgdata";
                    reportPath = @"/TAM";
                    rs.Url = "http://tamlogfin/reportserver/ReportExecution2005.asmx";
                    break;
            }

            //var tempParams = "GoremYozem=9998&Shana=2022&Numerator=4&MsMahadura=1&StatusHazmana=2&SugTofes=2&SumIsBig=1&ShanaTaktzivit=2022"; 
            // "GOREM=9998&SHANA=2022&NUM=8&MAHADURA=1&HADPASA=2&SignType=1";
            var tempParams = fullRsUrl;
            logger.Info("MergePdfWebApp:Post::tempParams:" + tempParams);
            //tempParams = fullRsUrl.Substring(fullRsUrl.IndexOf(rsName) + rsName.Length + 1, fullRsUrl.IndexOf("&rc:")- (fullRsUrl.IndexOf(rsName) + rsName.Length + 1));

            //var result = tempParams.Trim('{', '}').Split('&').Select(x => x.Split('=')).ToDictionary(x => x[0], x => int.Parse(x[1]));
            //if (null == result)
            //{
            //    logger.Error("Post:result==null");
            //}
            //else
            //{
            //    logger.Info("Post:result.Count:" + result.Count);
            //}
            //sasa^

            //Dictionary<string, string> resDict1 = tempParams.Trim('{', '}').Split('&').Select(x => x.Split('=')).ToDictionary(x => x[0], x => x[1]);
            string[] parameter_lines = tempParams.Trim('{', '}').Split('&');
            if (parameter_lines == null || parameter_lines.Length < 1)
            {
                logger.Error("MergePdfWebApp:Post:: parameter_lines null or < 1.");
            }
            Dictionary<string, string> resDict = new Dictionary<string, string>();
            for (int k = 0; k < parameter_lines.Length; ++k)
            {
                string[] x = parameter_lines[k].Split('=');
                if (x.Length > 1)
                {
                    resDict.Add(x[0], x[1]);
                }
                else
                {
                    logger.Error($"MergePdfWebApp:Post:: parameter_lines[{k}] =  {parameter_lines[k]}");
                    continue;
                }
            }
            int result_count = resDict.Count;
            ParameterValue[] parameters = new ParameterValue[result_count];//sasa
            //ParameterValue[] parameters = new ParameterValue[result.Count];
            int i = 0;
            foreach (KeyValuePair<string, string> entry in resDict)
            {
                parameters[i] = new ParameterValue();
                parameters[i].Name = entry.Key;
                parameters[i++].Value = entry.Value;
                try
                {
                    logger.Info("Post:Name:" + entry.Key + "; Value:" + entry.Value);
                }
                catch (Exception e)
                {
                    logger.Error("MergePdfWebApp:Post::message: " + e.Message + "; StackTrace" + e.StackTrace);
                }

            }
            // Console.WriteLine(string.Join("\r\n", result.Select(x => x.Key + " : " + x.Value)));

            /**/

            //            ReportExecutionService rs = new ReportExecutionService();
            //            rs.Credentials = System.Net.CredentialCache.DefaultCredentials;

            //   rs.Url = "http://tamlogfin/reportserver/ReportExecution2005.asmx";

            // Render arguments  

            byte[] result2 = null;
            //string reportPath = @"/TAM/Hazmana-Tubin";
            //string reportPath = @"HTTP://TAMLOGFIN/REPORTSERVER?/TAM/Hazmana-Tubin";
            reportPath += rsName;// @"Hazmana-Tubin";
            logger.Info("Post:reportPath:" + reportPath);

            string format = "PDF";
            string historyID = null;
            string devInfo = @"<DeviceInfo><Toolbar>False</Toolbar></DeviceInfo>";
            DataSourceCredentials[] credentials = null;
            string showHideToggle = null;
            string encoding;
            string mimeType;
            string extension;
            Warning[] warnings = null;
            ParameterValue[] reportHistoryParameters = null;
            string[] streamIDs = null;

            ExecutionInfo execInfo = new ExecutionInfo();
            ExecutionHeader execHeader = new ExecutionHeader();

            try
            {
                logger.Info("MergePdfWebApp:Post::rs.ExecutionHeaderValue = execHeader;");
                rs.ExecutionHeaderValue = execHeader;

                execInfo = rs.LoadReport(reportPath, historyID);
                //xecInfo = rs.LoadReport("/TAM/Shiryun", null);

                rs.SetExecutionParameters(parameters, "en-us");
                String SessionId = rs.ExecutionHeaderValue.ExecutionID;

                logger.Info("MergePdfWebApp:Post::SessionID:" + rs.ExecutionHeaderValue.ExecutionID);
            }

            catch (Exception e)
            {
                logger.Error("MergePdfWebApp:Post::message: " + e.Message + "; StackTrace" + e.StackTrace);
            }
            try
            {
                logger.Info("MergePdfWebApp:Post::rs.Render");
                logger.Info($"MergePdfWebApp:Post::rs.Render({format},{devInfo}");
                result2 = rs.Render(format, devInfo, out extension, out encoding, out mimeType, out warnings, out streamIDs);
                logger.Info($"MergePdfWebApp:Post::rs.Render(... {extension},{encoding},{mimeType},{warnings}");
                if (null == streamIDs)
                {
                    logger.Info("MergePdfWebApp:Post::streamIDs == null");
                }
                else
                {
                    try
                    {
                        logger.Info("MergePdfWebApp:Post::streamIDs.Length" + streamIDs.Length);
                    }
                    catch { }
                }
                execInfo = rs.GetExecutionInfo();

                logger.Info("MergePdfWebApp:Post::Execution date and time:" + execInfo.ExecutionDateTime);

            }
            catch (SoapException e)
            {
                logger.Error("MergePdfWebApp:Post::message: " + e.Message + "; StackTrace" + e.StackTrace);
                //logger.Log(LogLevel.Info, "line 149 " + ex.Message);
                //Console.WriteLine(ex.Detail.OuterXml);
            }

            // Write the contents of the report to an MHTML file.  
            try
            {
                //\\TAMLOGFIN\C$\TEMP\FILES\UPLOADS\HESKEM\
                //FileStream stream = File.Create("report.pdf", result2.Length);
                //FileStream stream = File.Create("C:/temp/rsAndMifrat.pdf", result2.Length);
                //FileStream stream = File.Create(@"\\TAMLOGFIN\C$\TEMP\FILES\UPLOADS\HESKEM\report.pdf", result.Length);

                //FileStream stream = File.Create(@"\\TAMLOGFIN\C$\TEMP\FILES\UPLOADS\HESKEM\" + fileName + ".pdf", result2.Length);
                //FileStream stream = File.Create(@"\\TAMLOGFIN\C$\TEMP\FILES\UPLOADS\HAZMANA_TUBIN\" + fileName + ".pdf", result2.Length);
                string pdffile = fileUrl + fileName + ".pdf";
                if (null == result2)
                {
                    logger.Info($"MergePdfWebApp:Post::File.Create({pdffile}, NULL)");
                }
                else
                {
                    logger.Info($"MergePdfWebApp:Post::File.Create({pdffile},{result2.Length})");
                }
                
                FileStream stream = File.Create(pdffile, result2.Length);

                logger.Info($"MergePdfWebApp:Post::File '{pdffile}' created.");
                stream.Write(result2, 0, result2.Length);
                logger.Info("MergePdfWebApp:Post::Result written to the file.");
                stream.Close();
            }
            catch (Exception e)
            {
                logger.Error("MergePdfWebApp:Post::message: " + e.Message + "; StackTrace" + e.StackTrace);
                //logger.Log(LogLevel.Info, "line 195 " + e.Message);
                //Console.WriteLine(e.Message);
            }
            /**/
            try
            {
                ConcatFiles concatFiles = new ConcatFiles();
                //string rsFile = @"\\TAMLOGFIN\C$\TEMP\FILES\UPLOADS\HESKEM\" + fileName + ".pdf";
                string rsFile = fileUrl + fileName + ".pdf";
                logger.Info($"MergePdfWebApp:Post::rsFile: {rsFile}");
                //string mifratFile = @"\\TAMLOGFIN\C$\TEMP\FILES\UPLOADS\HESKEM\" + fileName + ".pdf";
                string mifratFile = mifratLink;
                logger.Info($"MergePdfWebApp:Post::mifratFile: {mifratFile}");

                if (mifratFile.ToLower().Contains(".pdf"))
                {
                    logger.Info($"MergePdfWebApp:Post::concatFiles.MergePDF({rsFile}, {mifratFile}, {fileUrl}) start");
                    concatFiles.MergePDF(rsFile, mifratFile, fileUrl);
                    logger.Info($"MergePdfWebApp:Post::concatFiles.MergePDF(...) end");
                }

            }
            catch (Exception e)
            {
                logger.Error("MergePdfWebApp:Post::message: " + e.Message + "; StackTrace" + e.StackTrace);
                //logger.Log(LogLevel.Info, "line 212 " + ex.Message);
                //Console.WriteLine(ex.Message);
            }
            logger.Info("End of the Post(). return OK");
            return StatusCode(HttpStatusCode.OK);
        }

        // PUT api/values/5
        public IHttpActionResult Put(int id, [FromBody] JObject jsonData)
        {
            logger.Info("---- MergePdfWebApp::Put() ----");
            // string param1, param2, param3, param4, param5;
            string startUrl, endUrl, fullUrl;
            string fileName;

            string tempLink, rsWithParams, rsName, rsParameters, env;
            tempLink = "HTTP://TAMLOGFIN/REPORTSERVER?/TAM/Asmachtaot&JR_NO=11&RC:TOOLBAR=FALSE&RS:COMMAND=RENDER&RS:CLEARSESSION=TRUE";

            //tempLink = "HTTP://SQLRS03/REPORTSERVER/PAGES/REPORTVIEWER.ASPX?/CHOSHEN/BakashaTubin&GoremYozem=121&Shana=2022&Numerator=1&SugHadpasa=1&SapakNo=695&RC:TOOLBAR=FALSE&RS:COMMAND=RENDER&RS:CLEARSESSION=TRUE";
            //tempLink = rsData.json.param1;
            //var details = JObject.Parse(tempLink);
            if (null == jsonData)
            {
                logger.Error("MergePdfWebApp:Put::jsonData == null");
            }
            dynamic json = jsonData;
            tempLink = json.rsData;
            fileName = json.fileName;
            startUrl = json.startUrl;
            endUrl = json.endUrl;
            fullUrl = json.fullUrl;
            tempLink = fullUrl.ToUpper();

            logger.Info("MergePdfWebApp:Put::tempLink:" + tempLink);
            logger.Info("MergePdfWebApp:Put::fileName:" + fileName);
            logger.Info("MergePdfWebApp:Put::startUrl:" + startUrl);
            logger.Info("MergePdfWebApp:Put::endUrl:" + endUrl);
            logger.Info("MergePdfWebApp:Put::fullUrl:" + fullUrl);
            logger.Info("MergePdfWebApp:Put::tempLink:" + tempLink);



            //tempLink = tempLink.ToUpper();

            /*
            dynamic json = jsonData;
            param1 = json.rsName;
            param1 = json.param1;
            param2 = json.param2;
            param3 = json.param3;
            param4 = json.param4;
            param5 = json.param5;
            */
            try
            {
                env = tempLink.Contains("HTTP://TAMLOGFIN") ? "TAM" : "CHOSHEN";
                logger.Info("MergePdfWebApp:Put::env:" + env);
                if (env == "TAM")
                {
                    rsWithParams = tempLink.Substring(tempLink.IndexOf("?/TAM/") + 6);
                }
                else
                {
                    rsWithParams = tempLink.Substring(tempLink.IndexOf("?/CHOSHEN/") + 10);
                }
                rsName = rsWithParams.Substring(0, rsWithParams.IndexOf('&'));
                logger.Info("MergePdfWebApp:Put::rsName:" + rsName);
                rsParameters = rsWithParams.Substring(rsWithParams.IndexOf('&') + 1, rsWithParams.IndexOf("&RC:") - (rsWithParams.IndexOf('&') + 1));
                logger.Info("MergePdfWebApp:Put::rsParameters:" + rsParameters);

            }
            catch (Exception e)
            {
                logger.Error("MergePdfWebApp:Put::HttpStatusCode.BadRequest, ERROR_WITH_PARAMS. Message: " + e.Message + "; StackTrace" + e.StackTrace);
                return Content(HttpStatusCode.BadRequest, ERROR_WITH_PARAMS);
            }
            ReportExecutionService rs = new ReportExecutionService();
            rs.Credentials = System.Net.CredentialCache.DefaultCredentials;
            //  rs.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
            //ReportingService rs = new ReportingService();
            //rs.Credentials = System.Net.CredentialCache.DefaultCredentials;

            rs.Url = "http://tamlogfin/reportserver/ReportExecution2005.asmx";
            logger.Info("MergePdfWebApp:Put::rs.Url:" + rs.Url);
            // Render arguments  
            byte[] result = null;
            string reportPath = @"/TAM/Shiryun";
            string format = "PDF";
            string historyID = null;
            string devInfo = @"<DeviceInfo><Toolbar>False</Toolbar></DeviceInfo>";

            // Prepare report parameter.  
            ParameterValue[] parameters = new ParameterValue[5];
            parameters[0] = new ParameterValue();
            parameters[0].Name = "GoremYozem";
            parameters[0].Value = "9998";
            parameters[1] = new ParameterValue();
            parameters[1].Name = "Shana";
            parameters[1].Value = "2022"; // June  
            parameters[2] = new ParameterValue();
            parameters[2].Name = "Numerator";
            parameters[2].Value = "2";
            parameters[3] = new ParameterValue();
            parameters[3].Name = "SugHadpasa";
            parameters[3].Value = "1";
            parameters[4] = new ParameterValue();
            parameters[4].Name = "ShanaTaktzivit";
            parameters[4].Value = "2022";
            /*
            parameters[2] = new ParameterValue();
            parameters[2].Name = "IsCahash";
            parameters[2].Value = "1";
            */
            DataSourceCredentials[] credentials = null;
            string showHideToggle = null;
            string encoding;
            string mimeType;
            string extension;
            Warning[] warnings = null;
            ParameterValue[] reportHistoryParameters = null;
            string[] streamIDs = null;

            ExecutionInfo execInfo = new ExecutionInfo();
            ExecutionHeader execHeader = new ExecutionHeader();

            try
            {
                logger.Info("MergePdfWebApp:Put:rs.ExecutionHeaderValue = execHeader;");
                rs.ExecutionHeaderValue = execHeader;

                logger.Info($"MergePdfWebApp:Put:rs.LoadReport({reportPath}, {historyID})");
                execInfo = rs.LoadReport(reportPath, historyID);
                //xecInfo = rs.LoadReport("/TAM/Shiryun", null);

                rs.SetExecutionParameters(parameters, "en-us");
                String SessionId = rs.ExecutionHeaderValue.ExecutionID;
                logger.Info($"MergePdfWebApp:Put:SessionId:{SessionId}");
                //logger.Info("SessionID: {0}", rs.ExecutionHeaderValue.ExecutionID);
            }

            catch (Exception e)
            {
                logger.Error("MergePdfWebApp:Put::Message: " + e.Message + "; StackTrace" + e.StackTrace);
                //logger.Log(LogLevel.Info, "line 134 " + e.Message);
                Console.WriteLine(e.Message);
            }

            try
            {
                logger.Info($"rs.Render({format}, {devInfo},");
                result = rs.Render(format, devInfo, out extension, out encoding, out mimeType, out warnings, out streamIDs);
                logger.Info($"rs.Render(..., {extension} , {encoding},{mimeType}, {warnings}, out streamIDs);");
                if (null == streamIDs)
                {
                    logger.Error("MergePdfWebApp:Put::streamIDs == null");
                }

                execInfo = rs.GetExecutionInfo();

                logger.Info("MergePdfWebApp:Put::Execution date and time: {0}", execInfo.ExecutionDateTime);

            }
            catch (SoapException e)
            {
                logger.Error("MergePdfWebApp:Put::Message: " + e.Message + "; StackTrace" + e.StackTrace);
                //logger.Log(LogLevel.Info, "line 149 " + ex.Message);
                Console.WriteLine(e.Detail.OuterXml);
            }
            // Write the contents of the report to an MHTML file.  
            try
            {
                //\\TAMLOGFIN\C$\TEMP\FILES\UPLOADS\HESKEM\
                //FileStream stream = File.Create("report.pdf", result.Length);
                //FileStream stream = File.Create(@"\\TAMLOGFIN\C$\TEMP\FILES\UPLOADS\HESKEM\report.pdf", result.Length);
                if (null == result)
                {
                    logger.Error("MergePdfWebApp:Put::result == null;438");
                }
                string pdffile = @"\\TAMLOGFIN\C$\TEMP\FILES\UPLOADS\HESKEM\" + fileName + ".pdf";
                logger.Info($"MergePdfWebApp:Put::File.Create({pdffile}, {result.Length});");
                FileStream stream = File.Create(pdffile, result.Length);

                logger.Info("MergePdfWebApp:Put::File created.");
                stream.Write(result, 0, result.Length);


                logger.Info("MergePdfWebApp:Put::Result written to the file.");
                stream.Close();
            }
            catch (Exception e)
            {
                logger.Error("MergePdfWebApp:Put::Message: " + e.Message + "; StackTrace" + e.StackTrace);
                Console.WriteLine(e.Message);
            }
            logger.Info("MergePdfWebApp:Put::return OK");
            return StatusCode(HttpStatusCode.OK);
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}

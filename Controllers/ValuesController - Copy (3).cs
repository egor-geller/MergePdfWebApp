using MergePdfWebApp.Models;
using Microsoft.Reporting.WebForms.Internal.Soap.ReportingServices2005.Execution;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
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
        private static Logger logger = LogManager.GetCurrentClassLogger();
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
            string SqlServerName = "";  // "tamlogfin";
            string SqlDbName = "";      // "lgdata";
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
            rs.Credentials = CredentialCache.DefaultCredentials;
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
                    rs.Url = "http://sql08/ReportServer_DEVOP/ReportExecution2005.asmx";
                    break;
                case "TAHAD":
                    SqlServerName = "sql08\\testop";
                    SqlDbName = "db917";
                    reportPath = @"/CHOSHEN";
                    rs.Url = "HTTP://SQL08/REPORTSERVER_PREPRODOP/ReportExecution2005.asmx";

                    break;
                case "TAPPR":
                    SqlServerName = "sql08\\preprodop";
                    SqlDbName = "db917";
                    reportPath = @"/CHOSHEN";
                    rs.Url = "HTTP://SQL08/REPORTSERVER_PREPRODOP/ReportExecution2005.asmx";
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
            string env_url = Utils.ReadSetting(enviroment);

            logger.Info($"enviroment:{enviroment}, rs.Url:[{rs.Url}]");
            var tempParams = fullRsUrl;

            logger.Info("MergePdfWebApp:Post::tempParams:" + tempParams);
            //fullRsUrl parser
            List<ParameterValue> parameters = GetParameters(tempParams);
            // Render arguments  
            reportPath += rsName;// @"Hazmana-Tubin";
            logger.Info("Post:reportPath !!!:" + reportPath);

            ExecutionInfo execInfo = new ExecutionInfo();
            ExecutionHeader execHeader = new ExecutionHeader();
            string rsFile = fileUrl + fileName + ".pdf";
            string mifratFile = mifratLink;
            logger.Info($"rsFile:[{rsFile}]");
            logger.Info($"mifratFile:[{mifratFile}]");
            try
            {
                ExecutionInfo executionInfo = rs.LoadReport(reportPath, null);
                rs.SetExecutionParameters(parameters.ToArray(), "en-US");

                string deviceInfo = "<DeviceInfo><Toolbar>False</Toolbar></DeviceInfo>";
                string mimeType;
                string encoding;
                string extention;
                Warning[] warning;
                string[] streamId;
                var result = rs.Render("PDF", deviceInfo, out extention, out mimeType, out encoding, out warning, out streamId);
                logger.Info($"MergePdfWebApp:Post::rs.Render(PDF, {deviceInfo}, {extention}, {mimeType}, {encoding}, warning, streamId)");
                if (null == result)
                {
                    logger.Error("MergePdfWebApp:rs.Render returns NULL");
                }
                else
                {
                    logger.Info($"MergePdfWebApp:rs.Render result.Length:{result.Length}");
                }
                File.WriteAllBytes(rsFile, result);
                logger.Info("MergePdfWebApp:Post::File.WriteAllBytes completed without exception");
            }
            catch (Exception e)
            {
                logger.Error("MergePdfWebApp:Post::message: " + e.Message + "; StackTrace" + e.StackTrace);
            }
            try
            {
                ConcatFiles concatFiles = new ConcatFiles();
                concatFiles.MergePDF(rsFile, mifratFile, fileUrl);
                logger.Info("MergePdfWebApp:Post::MergePDF completed without exception");

                ///Must be deleted
                //if (File.Exists(rsFile))
                //{
                //    File.Delete(rsFile);
                //    logger.Info($"MergePdfWebApp:Post::File.Delete({rsFile}) completed");
                //}
            }
            catch (Exception e)
            {
                logger.Error("MergePdfWebApp:Post::MergePDF: " + e.Message + "; StackTrace" + e.StackTrace);
            }
            logger.Info("End of the Post(). return OK");
            return StatusCode(HttpStatusCode.OK);
        }
        // PUT api/values/5
        public IHttpActionResult Put(int id, [FromBody] JObject jsonData)
        {
            logger.Info("---- MergePdfWebApp::Put() ----");
            logger.Info("MergePdfWebApp:Put::return OK");
            return StatusCode(HttpStatusCode.OK);
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }

        private List<ParameterValue> GetParameters(string Params)
        {
            List<ParameterValue> parameters = new List<ParameterValue>();
            string[] parameter_lines = Params.Trim('{', '}').Split('&');
            if (parameter_lines == null || parameter_lines.Length < 1)
            {
                logger.Error("MergePdfWebApp:GetParameters:: split ERROR.");
                return null;
            }
            logger.Info($"MergePdfWebApp:GetParameters parameter_lines.Length:{parameter_lines.Length}");
            for (int k = 0; k < parameter_lines.Length; ++k)
            {
                string[] x = parameter_lines[k].Split('=');
                if (x.Length > 1)
                {
                    string name = x[0];
                    string value = x[1];
                    logger.Info($"parameters name:{name}, value:{value}");
                    parameters.Add(new ParameterValue { Name = name, Value = value });
                }
                else
                {
                    Console.WriteLine($"MergePdfWebApp:Post:: parameter_lines[{k}] =  {parameter_lines[k]}");
                    continue;
                }
            }
            logger.Info($"MergePdfWebApp:GetParameters parameters.Count:{parameters.Count}");
            return parameters;
        }


    }//class
}

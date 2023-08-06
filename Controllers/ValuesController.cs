using MergePdfWebApp.Models;
using Microsoft.Reporting.WebForms.Internal.Soap.ReportingServices2005.Execution;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Http;

namespace MergePdfWebApp.Controllers
{
    public class ValuesController : ApiController
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return id != 0 ? id.ToString() : "value";
        }

        // POST api/values
        public IHttpActionResult Post([FromBody] Details details)
        {
            logger.Info("        ");
            logger.Info("---- MergePdfWebApp::Post() ----");

            string enviroment = details.Enviroment;
            string fileName = details.FileName;
            string fullRsUrl = details.FullRsUrl;
            string fileUrl = details.FileUrl;
            string rsName = details.RsName.ToLower();
            if (null != rsName)
            {
                rsName = rsName.ToLower();
            }

            logger.Info("MergePdfWebApp:Post::enviroment:" + enviroment);
            logger.Info("MergePdfWebApp:Post::fileName:" + fileName);
            logger.Info("MergePdfWebApp:Post::fullRsUrl:" + fullRsUrl);
            logger.Info("MergePdfWebApp:Post::fileUrl:" + fileUrl);
            logger.Info("MergePdfWebApp:Post::rsName:" + rsName);

            string reportPath;
            ReportExecutionService rs = new ReportExecutionService();
            rs.Credentials = CredentialCache.DefaultCredentials;
            logger.Info("switch (" + enviroment + ")");
            switch (enviroment)
            {
                case "SAPIENS":
                    reportPath = @"/TAM";
                    break;
                case "TAHAD":
                    reportPath = @"/CHOSHEN_HADRACHA";
                    break;
                case "TADEV":
                case "TAPPR":
                case "TAPROD":
                    reportPath = @"/CHOSHEN";
                    break;
                default:
                    logger.Error($"No case for environment:{enviroment}");
                    return StatusCode(HttpStatusCode.BadRequest);
            }
            string env_url = Utils.ReadSetting(enviroment);
            if (null == env_url)
            {
                logger.Error($"No Url for environment:{enviroment}");
                return StatusCode(HttpStatusCode.BadRequest);
            }
            rs.Url = env_url;
            logger.Info($"enviroment:{enviroment}, rs.Url:[{rs.Url}]");
            rs.ExecutionHeaderValue = new ExecutionHeader();
            var tempParams = fullRsUrl;

            logger.Info("MergePdfWebApp:Post::tempParams:" + tempParams);
            List<ParameterValue> parameters = GetParameters(tempParams);

            // Render arguments  
            reportPath += rsName;// @"Hazmana-Tubin";
            logger.Info("Post:reportPath:" + reportPath);

            string rsFile = fileUrl + fileName + ".pdf";
            logger.Info($"rsFile:[{rsFile}]");
            try
            {
                ExecutionInfo executionInfo = rs.LoadReport(reportPath, null);
                rs.SetExecutionParameters(parameters.ToArray(), "en-US");

                string deviceInfo = "<DeviceInfo><Toolbar>False</Toolbar></DeviceInfo>";
                var result = rs.Render("PDF", deviceInfo, out string extention, out string mimeType, out string encoding, out Warning[] warning, out string[] streamId);
                logger.Info($"MergePdfWebApp:Post::rs.Render(PDF, {deviceInfo}, {extention}, {mimeType}, {encoding}, warning, streamId)");
                logger.Info($"MergePdfWebApp:Post::Execution date and time: {executionInfo.ExecutionDateTime}");

                if (null == result)
                {
                    logger.Error("MergePdfWebApp:rs.Render returns NULL");
                }
                else
                {
                    logger.Info($"MergePdfWebApp:rs.Render result.Length:{result.Length}");
                }
                if (null != warning)
                {
                    for (int k = 0; k < warning.Length; ++k)
                    {
                        logger.Error($"MergePdfWebApp:Post::rs.Render Warning[{k}]:{warning[k].Message}");
                    }
                }
                ///                
                /// Download rsFile file
                ///                
                File.WriteAllBytes(rsFile, result);
                logger.Info("MergePdfWebApp:Post::File.WriteAllBytes() completed without exception");

                ConcatFiles concatFiles = new ConcatFiles();
                concatFiles.MergePDF(rsFile, parameters, enviroment, fileUrl);
                logger.Info("MergePdfWebApp:Post::MergePDF completed without exception");
            }
            catch (Exception e)
            {
                logger.Error("MergePdfWebApp:Post::ERROR MergePDF: " + e.Message + "; StackTrace" + e.StackTrace);
                return StatusCode(HttpStatusCode.InternalServerError);
            }
            logger.Info("End of the Post(). return OK");
            return StatusCode(HttpStatusCode.OK);
        }
        // PUT api/values/5
        public IHttpActionResult Put(int id, [FromBody] JObject jsonData)
        {
            logger.Info("\n---- MergePdfWebApp::Put() ----");
            logger.Info("MergePdfWebApp:Put::Not Implemented");
            return StatusCode(HttpStatusCode.NotImplemented);
        }

        // DELETE api/values/5
        public IHttpActionResult Delete(int id)
        {
            logger.Info("\n---- MergePdfWebApp::Delete() ----");
            logger.Info("MergePdfWebApp:Delete::Not Implemented");
            return StatusCode(HttpStatusCode.NotImplemented);
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
                    parameters.Add(new ParameterValue { Name = name, Value = value });
                    logger.Info($"parameters name:{name}, value:{value}");
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

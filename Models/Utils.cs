using NLog;
using System.Configuration;

namespace MergePdfWebApp.Models
{
    public class Utils
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public static string ReadSetting(string key)
        {
            string result = null;
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                result = appSettings[key] ?? "Not Found";
            }
            catch (ConfigurationErrorsException)
            {
                logger.Error($"Error reading app settings key:{key}");
            }
            return result;
        }
    }//public class Utils
}
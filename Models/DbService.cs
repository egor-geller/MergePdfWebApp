using NLog;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace MergePdfWebApp.Models
{
    public class DbService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly string GET_IMG_OF_PRODUCT_SQL = "SELECT " +
            "CONCAT(RTRIM(LTRIM(IMG.[FILE_PATH])), RTRIM(LTRIM(IMG.[ENTITY_KEY])), '-', RTRIM(LTRIM(IMG.[FILE_NAME]))) AS LINK " +
            "FROM [TBHZSHR_HAZMASHURA] SHURA " +
            "JOIN [TBPARIT_PARIT] PARIT ON " +
            "PARIT.[MAKAT] = SHURA.[MAKAT] " +
            "JOIN [GN_ENTITY_FILES] IMG " +
            "ON IMG.[ENTITY_KEY] LIKE LTRIM(RTRIM(PARIT.[MAKAT])) + '%' " +
            "JOIN [dbo].[GN_PARAMETERS] B ON " +
            "B.[GN_MODULE_ID] = IMG.[GN_MODULE_ID] " +
            "AND [GN_PRAMS_CODE] = 'PLS' " +
            "AND IMG.[GN_MODULE_ID] = 'LG' " +
            "AND IMG.[ENTITY_CODE] = 'KPT' " +
            "WHERE [GR_YZ_HAZMANA] = @goremYozem " +
            "AND [SHANA_HAZMANA] = @shana " +
            "AND [NU_HAZMANA] = @nuHazmana " +
            "AND [MS_MAHADURA_HZMANA] = @msMahadura " +
            "AND IMG.[FILE_PATH] != ''";

        private static readonly string GET_MIFRAT_SQL = "SELECT " +
            "CONCAT(RTRIM(LTRIM(IMG.[FILE_PATH])), RTRIM(LTRIM(IMG.[ENTITY_KEY])), '-', RTRIM(LTRIM(IMG.[FILE_NAME]))) AS FULLFILE " +
            "FROM [DBO].[TBHZSHR_HAZMASHURA] SHURA " +
            "JOIN [DBO].[TBPARIT_PARIT] PARIT " +
            "ON PARIT.[MAKAT] = SHURA.[MAKAT] " +
            "JOIN [DBO].[GN_ENTITY_FILES] IMG " +
            "ON IMG.[GN_MODULE_ID] = 'LG' " +
            "AND IMG.[ENTITY_CODE] = 'KMT' " +
            "AND IMG.[ENTITY_KEY] LIKE RTRIM(LTRIM(SHURA.[MAKAT])) + '%' " +
            "WHERE SHURA.[GR_YZ_HAZMANA] = @goremYozem " +
            "AND SHURA.[SHANA_HAZMANA] = @shana " +
            "AND SHURA.[NU_HAZMANA] = @nuHazmana " +
            "AND SHURA.[MS_MAHADURA_HZMANA] = @msMahadura " +
            "AND IMG.[FILE_PATH] != ''";

        private static readonly string SHEM_PARIT_SQL = "SELECT " +
            "NCHAR(8237) + REVERSE(RTRIM(SHEM_PARIT)) + NCHAR(8236) AS SHEM " +
            "FROM TBPARIT_PARIT " +
            "WHERE MAKAT = @makat";

        DbService() { }
        private static readonly object lock_in = new object();
        private static DbService instance = null;
        public static DbService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lock_in)
                    {
                        if (instance == null)
                        {
                            instance = new DbService();
                        }
                    }
                }
                return instance;
            }
        }

        public List<string> GetImagesOfProducts(string goremYozem, string shana, string nuHazmana, string msMahadura, string serverEnvironment)
        {
            List<string> imgs = new List<string>();

            if (AreParamsEmpty(goremYozem, shana, nuHazmana, msMahadura, serverEnvironment))
            {
                return null;
            }

            string connectionString = ConnString(serverEnvironment);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                Logger.Info("DbService::GetImagesOfProducts:using (SqlConnection connection = new SqlConnection(connectionString)");

                using (SqlCommand command = new SqlCommand(GET_IMG_OF_PRODUCT_SQL, connection))
                {
                    command.Parameters.AddWithValue("@goremYozem", goremYozem);
                    command.Parameters.AddWithValue("@shana", shana);
                    command.Parameters.AddWithValue("@nuHazmana", nuHazmana);
                    command.Parameters.AddWithValue("@msMahadura", msMahadura);

                    try
                    {
                        connection.Open();
                        var reader = command.ExecuteReader();
                        Logger.Info($"DbService::GetImagesOfProducts: start reading img urls");
                        while (reader.Read())
                        {
                            string url = (string)reader["LINK"];
                            imgs.Add(url);
                            Logger.Info($"DbService::GetImagesOfProducts:img:{url}");
                        }
                        Logger.Info($"DbService::GetImagesOfProducts: Successfully read img urls: count = {imgs.Count}");
                        return imgs;
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"DbService::GetImagesOfProducts::EXCEPTION: {e.Message};\n\t{e.StackTrace}");
                        return null;
                    }
                }
            }
        }

        public List<string> GetMifratim(string goremYozem, string shana, string nuHazmana, string msMahadura, string serverEnvironment)
        {
            List<string> mifratim = new List<string>();

            if (AreParamsEmpty(goremYozem, shana, nuHazmana, msMahadura, serverEnvironment))
            {
                return null;
            }

            string connectionString = ConnString(serverEnvironment);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                Logger.Info("DbService::GetMifratim:using (SqlConnection connection = new SqlConnection(connectionString)");

                using (SqlCommand command = new SqlCommand(GET_MIFRAT_SQL, connection))
                {
                    command.Parameters.AddWithValue("@goremYozem", goremYozem);
                    command.Parameters.AddWithValue("@shana", shana);
                    command.Parameters.AddWithValue("@nuHazmana", nuHazmana);
                    command.Parameters.AddWithValue("@msMahadura", msMahadura);

                    try
                    {
                        connection.Open();
                        var reader = command.ExecuteReader();
                        Logger.Info($"DbService::GetMifratim: start reading mifrat urls");
                        while (reader.Read())
                        {
                            string url = (string)reader["FULLFILE"];
                            mifratim.Add(url);
                            Logger.Info($"DbService::GetMifratim:mifrat:{url}");
                        }
                        Logger.Info($"DbService::GetMifratim: Successfully read mifrat urls: count = {mifratim.Count}");
                        return mifratim;
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"DbService::GetMifratim::EXCEPTION: {e.Message};\n\t{e.StackTrace}");
                        return null;
                    }
                }
            }
        }

        public string GetShemParit(string makat, string serverEnvironment)
        {
            if (string.IsNullOrEmpty(makat) || string.IsNullOrEmpty(serverEnvironment))
            {
                return null;
            }

            string connectionString = ConnString(serverEnvironment);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                Logger.Info("DbService::GetShemParit:using (SqlConnection connection = new SqlConnection(connectionString)");

                using (SqlCommand command = new SqlCommand(SHEM_PARIT_SQL, connection))
                {
                    command.Parameters.AddWithValue("@makat", makat);

                    try
                    {
                        connection.Open();
                        var reader = command.ExecuteReader();
                        string shemParit = string.Empty;
                        Logger.Info($"DbService::GetShemParit: start reading Shem parit with makat: {makat}");
                        if (!reader.Read())
                        {
                            Logger.Error($"DbService::GetShemParit::EXCEPTION: No records were returned.");
                            return null;
                        }
                        shemParit = reader.GetString(0);
                        Logger.Info($"DbService::GetShemParit: Successfully read Shem parit");
                        return shemParit;
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"DbService::GetShemParit::EXCEPTION: {e.Message};\n\t{e.StackTrace}");
                        return null;
                    }
                }
            }
        }

        private string ConnString(string serverEnvironment)
        {
            string[] dbServer = CalcEnviroment(serverEnvironment);
            Logger.Info($"DbService::dbServer - SqlServerName: {dbServer[0]}, SqlDbName: {dbServer[1]}");

            string connectionString = "Data Source=" + dbServer[0] + ";Initial Catalog=" + dbServer[1] + ";Integrated Security=true";
            Logger.Info("DbService::connectionString: " + connectionString);

            return connectionString;
        }

        private bool AreParamsEmpty(params string[] x)
        {
            string[] paramNames = new string[] { "GoremYozem", "Shana", "NuHazmana", "MsMahadura", "ServerEnv" };

            foreach (string s in x)
            {
                if (string.IsNullOrEmpty(s))
                {
                    Logger.Warn("DbService::Params:: goremYozem|shana|nuHazmana|msMahadura|serverEnvironment is null or empty");
                    return true;
                }
            }

            for (var i = 0; i < x.Length; i++)
            {
                Logger.Info($"DbService::params: {paramNames[i]} : {x[i]}");
            }

            return false;
        }

        private string[] CalcEnviroment(string serverEnv)
        {
            string[] dbServer = new string[2];
            switch (serverEnv)
            {
                case "TADEV":
                    dbServer[0] = @"sql08\devop";
                    dbServer[1] = "db917";
                    break;
                case "TAHAD":
                    dbServer[0] = @"sql08\testop";
                    dbServer[1] = "db917";
                    break;
                case "TAPPR":
                    dbServer[0] = @"sql08\preprodop";
                    dbServer[1] = "db917";
                    break;
                case "TAEY":
                    dbServer[0] = @"sql08\preprodop";
                    dbServer[1] = "db917_tst";
                    break;
                case "TAPROD":
                    dbServer[0] = "SQL09";
                    dbServer[1] = "db917";
                    break;
                default:
                    dbServer[0] = "tamlogfin";
                    dbServer[1] = "lgdata";
                    break;
            }
            return dbServer;
        }
    }
}

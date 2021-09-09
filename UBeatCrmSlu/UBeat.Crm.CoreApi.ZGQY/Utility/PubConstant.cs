
using Microsoft.Extensions.Configuration;
using System;
namespace IRCS.DBUtility
{
    
    public class PubConstant
    {        
        /// <summary>
        /// 获取连接字符串
        /// </summary>
        public static string ConnectionString
        {           
            get 
            {
                try
                {
                    string _connectionString = AppConfigurtaionServices.configuration.GetSection("Appsettings:ConnectionString").Get<string>();
                    return _connectionString;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public static string ConnectionString_MySQL
        {
            get
            {
                try
                {
                    string _connectionString = AppConfigurtaionServices.configuration.GetSection("Appsettings:ConnectionString_MYSQL").Get<string>();
                    return _connectionString;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public static string ConnectionString_NpgSQL
        {
            get
            {

                string _connectionString = AppConfigurtaionServices.configuration.GetSection("Appsettings:ConnectionString_NpgSQL").Get<string>();
                return _connectionString;
            }
        }

    }
}
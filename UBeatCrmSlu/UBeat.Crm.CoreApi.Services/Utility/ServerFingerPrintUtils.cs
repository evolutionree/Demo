using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.BaseSys;

namespace UBeat.Crm.CoreApi.Services.Utility
{
    public class ServerFingerPrintUtils
    {
        private static readonly object lockObj = new object();
        /// <summary>
        /// 单例实例
        /// </summary>
        public static ServerFingerPrintUtils instance = null;
        private ServerFingerPrintInfo _currentFingerPrint;
        /// <summary>
        /// 记录当前的服务指纹信息
        /// </summary>
        public ServerFingerPrintInfo CurrentFingerPrint { get { return _currentFingerPrint; } }
        private static Logger _logger = NLog.LogManager.GetLogger("UBeat.Crm.CoreApi.DomainModel.BaseSys.ServerFingerPrintUtils");

        /// <summary>
        /// 单例获取函数
        /// </summary>
        /// <returns></returns>
        public static ServerFingerPrintUtils getInstance()
        {
            lock (lockObj)
            {
                if (instance == null) instance = new ServerFingerPrintUtils();
                return instance;
            }
        }
        /// <summary>
        /// 检查并生成服务器指纹信息
        /// 先读取文件中的指纹信息
        /// 然后获取服务的指纹信息
        /// 如果文件存在指纹信息，则需要对比指纹信息的正确性，如果不正确，抛出异常，如果正确，则返回文件中的指纹信息
        /// 如果文件不存在指纹信息，则生成服务器指纹信息，并保存到本地文件中，且返回此文件的指纹信息
        /// </summary>
        /// <returns></returns>
        public ServerFingerPrintInfo checkAndAddFingerPrint()
        {
            ServerFingerPrintInfo filePrint = this.LoadFromFile();
            ServerFingerPrintInfo serverPrint = this.GetFromServer();
            if (serverPrint == null) throw new Exception("无法获取服务器指纹信息");
            if (filePrint == null)
            {
                Guid ServerGroupId = Guid.NewGuid();
                Guid ServerId = Guid.NewGuid();
                serverPrint.ServerId = ServerId;
                serverPrint.ServerGroupId = ServerGroupId;
                this.saveFingerPrint(serverPrint);
                _currentFingerPrint = serverPrint; ;
                _logger.Trace("全新的服务器，生成新的服务器群组Id：" + ServerGroupId.ToString() + " , 服务器id:" + ServerId.ToString());
                return serverPrint;
            }
            else
            {
                if (filePrint.MachineName != serverPrint.MachineName
                    || filePrint.OsType != serverPrint.OsType
                    || filePrint.WorkPath != serverPrint.WorkPath
                    || filePrint.ServerUrl != serverPrint.ServerUrl)
                {
                    _logger.Error("检查服务指纹失败");
                    throw new Exception("检查服务指纹失败");
                }
                _currentFingerPrint = filePrint;
                return filePrint;
            }

        }



        /// <summary>
        /// 从本地文件中获取服务器指纹信息
        /// </summary>
        /// <returns></returns>
        private ServerFingerPrintInfo LoadFromFile()
        {
            string tmp = "";
            System.IO.StreamReader reader = null;
            try
            {
                string filepath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "serverfinger.json");
                reader = new StreamReader(filepath);
                tmp = reader.ReadToEnd();
                ServerFingerPrintInfo ret = Newtonsoft.Json.JsonConvert.DeserializeObject<ServerFingerPrintInfo>(tmp);
                return ret;
            }
            catch (Exception ex)
            {
                _logger.Trace(ex.Message);
            }
            finally
            {
                if (reader != null)
                {
                    try
                    {
                        reader.Close();
                    }
                    catch (Exception e) { }
                }
            }
            return null;
        }
        /// <summary>
        /// 把指纹信息存入本地
        /// </summary>
        /// <param name="info"></param>
        private void saveFingerPrint(ServerFingerPrintInfo info)
        {
            System.IO.StreamWriter wr = null;
            try
            {
                string filepath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "serverfinger.json");
                wr = new StreamWriter(filepath);
                wr.Write(Newtonsoft.Json.JsonConvert.SerializeObject(info));

            }
            catch (Exception ex)
            {
                _logger.Trace(ex.Message);
            }
            finally
            {
                try
                {
                    if (wr != null)
                    {
                        wr.Close();
                    }
                    wr = null;
                }
                catch (Exception ex)
                {
                }
            }
        }
        /// <summary>
        /// 收集当前服务实例的相关信息，以便生成新的服务器指纹
        /// </summary>
        /// <returns></returns>
        private ServerFingerPrintInfo GetFromServer()
        {
            ServerFingerPrintInfo ret = new ServerFingerPrintInfo();
            try
            {
                ret.OsType = System.Environment.OSVersion.Platform.ToString();
                ret.MachineName = System.Environment.MachineName;
                ret.WorkPath = System.IO.Directory.GetCurrentDirectory();
                var config = new ConfigurationBuilder()
                     .SetBasePath(Directory.GetCurrentDirectory())
                     .AddJsonFile("hosting.json")
                     .Build();
                ret.ServerUrl = config.GetValue<string>("urls");

            }
            catch (Exception ex)
            {
            }
            return ret;
        }
    }
}

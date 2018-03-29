using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace UBeat.Crm.CoreApi.Utility
{
    /// <summary>
    /// 用于记录服务器指纹，服务器指纹（或者称为服务指纹）是简单的标记不同的服务器（服务），
    /// 主要是服务于定时事务的识别上。
    /// 当有若干的服务器（服务）同时连接一个数据库。
    /// 当服务器检测到一个事务需要被激活（实例化）的时候，会先写入当前服务器的指纹信息（GUID),实例结束后会清除指纹信息。
    /// 别的服务器检测到需要激活，但已经被别的服务器激活了（有指纹信息），则跳过此定时事务。
    /// 当服务器（服务）关闭重启后，启动时检查数据库中已经包含此服务器指纹的定时事务，清除这些标记
    /// </summary>
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
        public ServerFingerPrintInfo CurrentFingerPrint { get { return _currentFingerPrint; }}
        private static Logger _logger = NLog.LogManager.GetLogger("UBeat.Crm.CoreApi.Utility.ServerFingerPrintUtils");

        /// <summary>
        /// 单例获取函数
        /// </summary>
        /// <returns></returns>
        public static ServerFingerPrintUtils getInstance() {
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
        public ServerFingerPrintInfo checkAndAddFingerPrint() {
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
            else {
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
            finally {
                if (reader != null) {
                    try
                    {
                        reader.Close();  } catch (Exception e) { }
                }
            }
            return null;
        }
        /// <summary>
        /// 把指纹信息存入本地
        /// </summary>
        /// <param name="info"></param>
        private void saveFingerPrint(ServerFingerPrintInfo info) {
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
            finally {
                try
                {
                    if (wr != null) {
                        wr.Close();
                    }
                    wr = null;
                }
                catch (Exception ex) {
                }
            }
        }
        /// <summary>
        /// 收集当前服务实例的相关信息，以便生成新的服务器指纹
        /// </summary>
        /// <returns></returns>
        private ServerFingerPrintInfo GetFromServer() {
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
                ret.ServerUrl =config.GetValue<string>("urls");

            }
            catch (Exception ex) {
            }
            return ret;
        }
    }
    public class ServerFingerPrintInfo {
        public Guid ServerGroupId { get; set; }
        public Guid ServerId { get; set; }
        public string WorkPath { get; set; }
        public string OsType { get; set; }
        public string MachineName { get; set; }
        public string ServerUrl { get; set; }
    }
}

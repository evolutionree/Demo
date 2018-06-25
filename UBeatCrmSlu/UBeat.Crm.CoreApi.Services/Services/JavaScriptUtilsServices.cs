using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.IRepository;

namespace UBeat.Crm.CoreApi.Services.Services
{
    /// <summary>
    /// 用于JavaScript引擎的所有服务提供的功能
    /// 慢慢会增加功能
    /// </summary>
    public class JavaScriptUtilsServices:EntityBaseServices
    {
        private readonly IQRCodeRepository _qRCodeRepository;
        public JavaScriptUtilsServices(IQRCodeRepository qRCodeRepository)  {
            _qRCodeRepository = qRCodeRepository;
        }
        public List<Dictionary<string, object>> DbExecute(string strSQL) {
            try
            {
                return _qRCodeRepository.ExecuteSQL(strSQL, 1);
            }
            catch (Exception ex) {
                return new List<Dictionary<string, object>>();
            }

        }
        public void Log(string message) {
            Console.WriteLine(message);
        }
        public void LogObject(object obj) {
            Console.WriteLine(obj);
        }
    }
}

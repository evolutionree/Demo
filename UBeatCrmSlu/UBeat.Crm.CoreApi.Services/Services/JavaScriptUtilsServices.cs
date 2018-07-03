using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.Message;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models.Message;

namespace UBeat.Crm.CoreApi.Services.Services
{
    /// <summary>
    /// 用于JavaScript引擎的所有服务提供的功能
    /// 慢慢会增加功能
    /// </summary>
    public class JavaScriptUtilsServices:EntityBaseServices
    {
        private readonly IQRCodeRepository _qRCodeRepository;
        private MessageServices _messageService = null;
        public JavaScriptUtilsServices(IQRCodeRepository qRCodeRepository)  {
            _qRCodeRepository = qRCodeRepository;
        }
        public MessageServices MessageService
        {
            get
            {
                if (_messageService == null)
                    _messageService = new MessageServices(CacheService);
                return _messageService;
            }
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
        public void AddMessage(string typeid , string entityid, string businessid, int msggroupid, int msgstyletype,
                    string msgtitle, string msgtitletip, string msgcontent, string receivers)
        {
            MessageParameter param = new MessageParameter();
            param.Receivers = new Dictionary<DomainModel.Message.MessageUserType, List<int>>();
            List<int> ires = new List<int>();
            string[] tmpuser = receivers.Split(',');
            foreach (string u in tmpuser) {
                int t = 0;
                if (int.TryParse(u, out t)) {
                    ires.Add(t);
                }
            }
            param.Receivers.Add(MessageUserType.SpecificUser, ires);
            param.TypeId = Guid.Parse(typeid);
            param.EntityId = Guid.Parse(entityid);
            param.BusinessId = Guid.Parse(businessid);
            MessageGroupType groupType = (MessageGroupType)msggroupid;
            MessageStyleType messageStyleType = (MessageStyleType)msgstyletype;
            Dictionary<string, object> msgparam = new Dictionary<string, object>();
            MessageService.WriteMessageWithoutTemplate(null, param, 0, groupType, messageStyleType, msgtitle, msgcontent, msgparam);
            //return Guid.Empty;
        }
        public void Log(string message) {
            Console.WriteLine(message);
        }
        public void LogObject(object obj) {
            Console.WriteLine(obj);
        }
    }
}

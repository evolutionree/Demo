using AutoMapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Message;
using UBeat.Crm.CoreApi.DomainModel.Notice;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Message;
using UBeat.Crm.CoreApi.Services.Models.Notice;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class NoticeServices : EntityBaseServices
    {

        private readonly INoticeRepository _noticeRepository;
        private readonly IMapper _mapper;

        public NoticeServices(IMapper mapper, INoticeRepository noticeRepository)
        {
            _noticeRepository = noticeRepository;
            _mapper = mapper;
        }

        public OutputResult<object> NoticeQuery(NoticeListModel notice, int userNumber)
        {
            var entity = _mapper.Map<NoticeListModel, NoticeListMapper>(notice);
            return new OutputResult<object>(_noticeRepository.NoticeQuery(entity, userNumber));
        }
        public OutputResult<object> NoticeMobQuery(NoticeListModel notice, int userNumber)
        {
            var entity = _mapper.Map<NoticeListModel, NoticeListMapper>(notice);
            return new OutputResult<object>(_noticeRepository.NoticeMobQuery(entity, userNumber));
        }
        public OutputResult<object> NoticeSendRecordQuery(NoticeSendRecordModel notice, int userNumber)
        {
            var entity = _mapper.Map<NoticeSendRecordModel, NoticeSendRecordMapper>(notice);

            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }

            return new OutputResult<object>(_noticeRepository.NoticeSendRecordQuery(entity, userNumber));
        }
        public OutputResult<object> NoticeVersionHistoryQuery(NoticeListModel notice, int userNumber)
        {
            var entity = _mapper.Map<NoticeListModel, NoticeListMapper>(notice);
            return new OutputResult<object>(_noticeRepository.NoticeVersionHistoryQuery(entity, userNumber));
        }
        public OutputResult<object> NoticeInfoQuery(NoticeListModel notice, int userNumber)
        {
            var entity = _mapper.Map<NoticeListModel, NoticeListMapper>(notice);
            return new OutputResult<object>(_noticeRepository.NoticeInfoQuery(entity, userNumber));
        }

        public OutputResult<object> InsertNotice(NoticeModel notice, int userNumber)
        {
            var entity = _mapper.Map<NoticeModel, NoticeMapper>(notice);

            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            return ExcuteInsertAction((transaction, arg, userData) =>
            {                    //验证通过后，插入数据
                var result = _noticeRepository.InsertNotice(transaction, entity, userNumber);
                if (!userData.HasDataAccess(transaction, RoutePath, entity.EntityId, DeviceClassic, new List<Guid>() { Guid.Parse(result.Id) }, "noticeid"))
                {
                    throw new Exception("您没有权限新增该实体数据");
                }

                return HandleResult(result);
            }, entity, entity.EntityId, userNumber);

        }

        public OutputResult<object> UpdateNotice(NoticeModel notice, int userNumber)
        {
            var entity = _mapper.Map<NoticeModel, NoticeMapper>(notice);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }

            return ExcuteUpdateAction((transaction, arg, userData) =>
            {                    //验证通过后，插入数据
                var result = _noticeRepository.UpdateNotice(transaction, entity, userNumber);

                return HandleResult(result);
            }, entity, entity.EntityId, userNumber, new List<Guid>() { Guid.Parse(entity.NoticeId) }, "noticeid");
        }

        public OutputResult<object> DisabledNotice(NoticeDisabledModel notice, int userNumber)
        {
            var entity = _mapper.Map<NoticeDisabledModel, NoticeDisabledMapper>(notice);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            List<Guid> lstGuid = new List<Guid>();
            foreach (var tmp in entity.NoticeIds.Split(','))
            {
                lstGuid.Add(Guid.Parse(tmp));
            }
            return ExcuteDeleteAction((transaction, arg, userData) =>
            {                    //验证通过后，插入数据
                var result = _noticeRepository.DisabledNotice(transaction, entity, userNumber);

                return HandleResult(result);
            }, entity, entity.EntityId, userNumber, lstGuid, "noticeid");
        }
        public OutputResult<object> UpdateNoticeReadFlag(NoticeReadFlagModel notice, int userNumber)
        {
            var entity = _mapper.Map<NoticeReadFlagModel, NoticeReadFlagMapper>(notice);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            return HandleResult(_noticeRepository.UpdateNoticeReadFlag(entity, userNumber));
        }
        public OutputResult<object> SendNoticeToUser(NoticeReceiverModel noticeReceiver, int userNumber)
        {
            var entity = _mapper.Map<NoticeReceiverModel, NoticeReceiverMapper>(noticeReceiver);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            bool success = false;
            var ids = entity.NoticeId.Split(',').Select(m=>new Guid(m)).ToList();
            var res = ExcuteUpdateAction((transaction, arg, userData) =>
             {                    //验证通过后，插入数据
                 var result = _noticeRepository.SendNoticeToUser(transaction, entity, userNumber);
                 success = result.Flag == 1;
                 return HandleResult(result);
             }, entity, entity.EntityId, userNumber, ids, "noticeid");
            if (success)
            {
                Task.Run(() =>
                {
                    foreach (var noticeId in ids)
                    {
                        WriteMessage(noticeId, userNumber);
                    }
                });
            }

            return res;
        }

        private void WriteMessage(Guid noticeId, int userNumber)
        {

            var noticeInfo = _noticeRepository.NoticeInfoQuery(new NoticeListMapper() { NoticeId = noticeId.ToString() }, userNumber);
            var msgParamData = new Dictionary<string, object>();
            msgParamData.Add("headimg", noticeInfo["headimg"]);
            msgParamData.Add("recversion", noticeInfo["recversion"]);

            var msg = new MessageParameter();
            msg.EntityId = new Guid("00000000-0000-0000-0000-000000000002");
            msg.TypeId = new Guid("00000000-0000-0000-0000-000000000002");
            msg.RelBusinessId = Guid.Empty;
            msg.RelEntityId = Guid.Empty;
            msg.BusinessId = noticeId;
            msg.ParamData = JsonConvert.SerializeObject(msgParamData);
            msg.FuncCode = "NoticeAdd";
            var receivers = new Dictionary<MessageUserType, List<int>>();
            var reciversInfo = _noticeRepository.GetNoticeReceivers(noticeId);
            if (reciversInfo != null && reciversInfo.Count > 0)
            {
                var mesReceivers = MessageService.GetMessageRecevers(msg.EntityId, msg.BusinessId);
                List<int> recevers = new List<int>();
                if (mesReceivers != null)
                {
                    foreach (var m in reciversInfo)
                    {
                        if (!mesReceivers.Exists(a => a.UserId == m.UserId))
                        {
                            recevers.Add(m.UserId);
                        }
                    }
                }
                else recevers = reciversInfo.Select(m => m.UserId).ToList();

                receivers.Add(MessageUserType.SpecificUser, recevers);
            }
            msg.Receivers = receivers;

            var users = new List<int>();
            users.Add(userNumber);

            var userInfos = MessageService.GetUserInfoList(users.Distinct().ToList());
            var operatorInfo = userInfos.Find(m => m.UserId == userNumber);
            var paramData = new Dictionary<string, string>();
            paramData.Add("operator", operatorInfo.UserName);
            var noticetitle = noticeInfo["noticetitle"].ToString();
            var headremark = noticeInfo["headremark"].ToString();
            paramData.Add("noticetitle", string.IsNullOrEmpty(noticetitle) ? "公告通知" : noticetitle);
            paramData.Add("headremark", string.IsNullOrEmpty(headremark) ? "新的公告通知" : headremark);

            msg.TemplateKeyValue = paramData;

            MessageService.WriteMessageAsyn(msg, userNumber);


        }
    }
}

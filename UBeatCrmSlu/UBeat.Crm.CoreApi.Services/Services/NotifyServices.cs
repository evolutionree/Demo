using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using UBeat.Crm.CoreApi.DomainModel.BasicData;
using UBeat.Crm.CoreApi.DomainModel.Notice;
using UBeat.Crm.CoreApi.DomainModel.Notify;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.BasicData;
using UBeat.Crm.CoreApi.Services.Models.Notify;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Version;
using UBeat.Crm.CoreApi.Services.Models.Message;
using UBeat.Crm.CoreApi.DomainModel.Message;
using System.Threading.Tasks;
using System.Linq;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class NotifyServices : BasicBaseServices
    {
        private readonly INotifyRepository _notifyRepository;
        private readonly IMapper _mapper;

        public NotifyServices(IMapper mapper, INotifyRepository notifyRepository)
        {
            _notifyRepository = notifyRepository;
            _mapper = mapper;
        }

        public OutputResult<object> FetchMessage(NotifyFetchModel versionModel, int userNumber)
        {
            var versionEntity = _mapper.Map<NotifyFetchModel, NotifyFetchMessageMapper>(versionModel);
            if (versionEntity == null || !versionEntity.IsValid())
            {
                return HandleValid(versionEntity);
            }

            var result = _notifyRepository.FetchMessage(versionEntity, userNumber);
            return new OutputResult<object>(result);
        }

        public OutputResult<object> WriteReadStatus(NotifyReadModel readModel, int userNumber)
        {
            if (string.IsNullOrWhiteSpace(readModel?.MsgIds))
            {
                return ShowError<object>("消息ID不能为空");
            }

            var result = _notifyRepository.WriteReadStatus(readModel.MsgIds, userNumber);
            return new OutputResult<object>(result);
        }
        public OutputResult<object> GetMessageList(PageParamModel data, int userNumber)
        {

            return ExcuteAction((transaction, arg, userData) =>
            {
                var pageParam = new PageParam { PageIndex = data.PageIndex, PageSize = data.PageSize };
                if (!pageParam.IsValid())
                {
                    return HandleValid(pageParam);
                }

                var result = _notifyRepository.GetMessageList(transaction, pageParam, data.MsgType, userNumber);
                return new OutputResult<object>(result);
            }, data, userNumber);
        }
       
        public OutputResult<object> WriteMessage(NotifyEntity readModel, int userNumber)
        {

            var result = _notifyRepository.WriteMessage(readModel, true, userNumber);
            IncreaseDataVersion(DataVersionType.MsgData, null);
            return new OutputResult<object>(result);
        }

        //分页获取消息
        public OutputResult<object> GetPageMessageList(PageMsgsParameter param, int userNumber)
        {
            if (param == null)
            {
                return ShowError<object>("参数不能为空");
            }
            var result = MessageService.GetMessageList(userNumber, param.PageIndex, param.PageSize, param.EntityId, param.BusinessId, param.MsgGroupIds, param.MsgStyleTypes);

            Task.Run(() =>
            {
                UpdateMessageStatus(result.DataList, userNumber);
            });

            return new OutputResult<object>(result);
        }
        //增量获取消息
        public OutputResult<object> GetIncrementMessageList(IncrementMsgsParameter param, int userNumber)
        {
            if (param == null)
            {
                return ShowError<object>("参数不能为空");
            }
            var incrementPage = new IncrementPageParameter(param.RecVersion, param.Direction, param.PageSize);
            var result = MessageService.GetMessageList(incrementPage, userNumber, param.EntityId, param.BusinessId, param.MsgGroupIds, param.MsgStyleTypes);

            Task.Run(() =>
            {
                UpdateMessageStatus(result.DataList, userNumber);
            });

            return new OutputResult<object>(result);
        }
        //更新消息状态
        private void UpdateMessageStatus(List<MessageInfo> dataList, int userNumber)
        {
            List<MsgWriteBackInfo> msgWriteBackInfoList = new List<MsgWriteBackInfo>();
            foreach (var mdata in dataList)
            {
                if(mdata.ReadStatus != 0)
                {
                    continue;
                }
                if (mdata.MsgStyleType == MessageStyleType.SystemNotice || mdata.MsgStyleType == MessageStyleType.RedmindNotSkip)
                {
                    msgWriteBackInfoList.Add(new MsgWriteBackInfo(mdata.MsgId, 2));
                }
                else msgWriteBackInfoList.Add(new MsgWriteBackInfo(mdata.MsgId, 1));
            }
            MessageService.UpdateMessageStatus(msgWriteBackInfoList, userNumber);
        }
        //回写消息
        public OutputResult<object> MessageWriteBack(List<Guid> messageids, int userNumber)
        {
            MessageService.MessageWriteBack(messageids, userNumber);
            IncreaseDataVersion(DataVersionType.MsgData, null);
            return new OutputResult<object>("OK");
        }

        public OutputResult<object> WriteMessage(MessageParameter data, int userNumber)
        {
            MessageService.WriteMessage( null,data, userNumber);
            return new OutputResult<object>("OK");
        }

        public OutputResult<object> WriteMessageAsyn(MessageParameter data, int userNumber)
        {
            MessageService.WriteMessageAsyn( data, userNumber);
            return new OutputResult<object>("OK");
        }

        //统计未读消息数量
        public OutputResult<object> StatisticUnreadMessage(List<MessageGroupType> msgGroupIds, int userNumber)
        {
            var result = MessageService.StatisticUnreadMessage(msgGroupIds, userNumber);

            return new OutputResult<object>(result);
        }
        //修改消息状态 => 已读
        public OutputResult<object> MessageRead(MsgStuausParameter parameter, int userNumber)
        {
            var readstatus = (int)MsgStatus.Read; //状态修改已读
            MessageService.UpdateMessageStutas(parameter.MsgGroupId, null, readstatus, userNumber);
            return new OutputResult<object>() { Status = 0, Message = "操作成功" };
        }
        //删除消息
        public OutputResult<object> DeleteMessage(MsgStuausParameter paras, int userNumber)
        {
            var readstatus = (int)MsgStatus.Del; //状态修改删除
            List<Guid> msgIds = new List<Guid>();
            if (!string.IsNullOrEmpty(paras.MessageIds))
            {
                var ids = paras.MessageIds.Split(',');
                for (int i = 0; i < ids.Count(); i++)
                {
                    msgIds.Add(new Guid(ids[i]));
                }
            }
            MessageService.UpdateMessageStutas(paras.MsgGroupId, msgIds, readstatus, userNumber);
            return new OutputResult<object>() { Status = 0, Message = "操作成功" };
        }

        public OutputResult<object> GetDynamicsUnMsg(UnHandleMsgModel data, int userNumber)
        {
            return MessageService.GetDynamicsUnMsg(data, userNumber);
              
        }
        public OutputResult<object> GetWorkFlowsMsg(UnHandleMsgModel data, int userNumber)
        {
            return MessageService.GetWorkFlowsMsg(data, userNumber);
              
        }
        public OutputResult<object> GetMessageCount(int userNumber)
        {
            return MessageService.GetMessageCount(userNumber);

        }
    }
}

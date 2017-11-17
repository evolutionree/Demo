using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;
using UBeat.Crm.CoreApi.DomainModel.Dynamics;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Dynamics;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class DynamicServices : EntityBaseServices
    {
        IDynamicRepository _repository;
        private readonly IDynamicEntityRepository _dynamicEntityRepository;
        private readonly IEntityProRepository _entityProRepository;

        public DynamicServices(IDynamicRepository repository, IEntityProRepository entityProRepository, IDynamicEntityRepository dynamicEntityRepository)
        {
            _repository = repository;
            _dynamicEntityRepository = dynamicEntityRepository;
            _entityProRepository = entityProRepository;
        }

        public OutputResult<object> SaveDynamicAbstract(SaveDynamicAbstractModel data, int userId)
        {
            DynamicAbstractInsert crmData = new DynamicAbstractInsert()
            {
                TypeID = data.Typeid.HasValue ? data.Typeid.Value : Guid.Empty,
                EntityID = data.Entityid,
                Fieldids = data.Fieldids,
                UserNo = userId,
            };
            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }
            return HandleResult(_repository.InsertDynamicAbstract(crmData));
        }

        public OutputResult<object> SelectDynamicAbstract(SelectDynamicAbstractModel data, int userId)
        {
            DynamicAbstractSelect crmData = new DynamicAbstractSelect()
            {
                TypeID = data.TypeID.HasValue ? data.TypeID.Value : Guid.Empty,
                EntityID = data.EntityID,
                UserNo = userId
            };
            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }
            return new OutputResult<object>(_repository.SelectDynamicAbstract(crmData));
        }

        public OutputResult<object> AddDynamic(AddDynamicModel data, int userId)
        {
            DynamicInsert crmData = new DynamicInsert()
            {
                Content = data.Content,
                UserNo = userId,
            };
            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }
            var dbStandardResult = _repository.InsertDynamic(crmData);
            return HandleResult(dbStandardResult);
        }

        public OutputResult<object> DeleteDynamic(DeleteDynamicModel data, int userId)
        {
            DynamicDelete crmData = new DynamicDelete()
            {
                DynamicId = data.DynamicId,
                UserNo = userId,
            };
            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }
            var dbStandardResult = _repository.DeleteDynamic(crmData);
            return HandleResult(dbStandardResult);
        }
        public OutputResult<object> SelectDynamicList(SelectDynamicListModel data, int userId, AnalyseHeader header)
        {
            var pageParam = new PageParam { PageIndex = data.PageIndex, PageSize = data.PageSize };
            if (!pageParam.IsValid())
            {
                return HandleValid(pageParam);
            }
            bool isWebRequest = (header != null && header.Device != null) ? header.Device.ToLower().Equals("web") : true;
            DynamicSelect crmData = new DynamicSelect()
            {
                EntityId = data.EntityId.GetValueOrDefault(),
                Businessid = data.Businessid.GetValueOrDefault(),
                DynamicTypes = data.DynamicTypes,
                UserNo = userId,
                RecVersion = data.RecVersion,
                CommentRecVersion = data.CommentVersion,
                PraiseRecVersion = data.PraiseVersion,
                RecStatus = isWebRequest ? 1 : -1,
            };

            return new OutputResult<object>(_repository.SelectDynamic(pageParam, crmData));
        }
        public OutputResult<object> SelectDynamic(SelectDynamicModel data, int userId)
        {
            if (data == null || data.DynamicId == null)
            {
                return ShowError<object>("参数错误");
            }
            var dynamicid = Guid.Empty;
            if (!Guid.TryParse(data.DynamicId.ToString(), out dynamicid))
            {
                return ShowError<object>("DynamicId 必须为UUID格式");
            }

            return new OutputResult<object>(_repository.SelectDynamic(dynamicid, userId));
        }

        public OutputResult<object> GetDynamicInfoList(DynamicListModel data, int userId)
        {
            DynamicListParameter param = new DynamicListParameter()
            {
                EntityId = data.EntityId.GetValueOrDefault(),
                Businessid = data.Businessid.GetValueOrDefault(),
                DynamicTypes = data.DynamicTypes
            };
            object result = null;
            //增量
            if (data.RequestType == 0)
            {
                var incrementPage = new IncrementPageParameter(data.RecVersion, data.Direction, data.PageSize);
                result = _repository.GetDynamicInfoList(param, incrementPage, userId);

            }
            else//分页
            {
                result = _repository.GetDynamicInfoList(param, data.PageIndex, data.PageSize, userId);
            }


            return new OutputResult<object>(result);
        }


        public OutputResult<object> GetDynamicInfo(SelectDynamicModel data, int userId)
        {
            if (data == null || data.DynamicId == null)
            {
                return ShowError<object>("参数错误");
            }
            var dynamicid = Guid.Empty;
            if (!Guid.TryParse(data.DynamicId.ToString(), out dynamicid))
            {
                return ShowError<object>("DynamicId 必须为UUID格式");
            }

            return new OutputResult<object>(_repository.GetDynamicInfo(dynamicid));
        }
        public OutputResult<object> AddDynamicComments_old(AddDynamicCommentsModel data, int userId)
        {
            DynamicCommentsInsert crmData = new DynamicCommentsInsert()
            {
                DynamicId = data.DynamicId,
                PCommentsid = data.PCommentsid.GetValueOrDefault(),
                Comments = data.Comments,
                UserNo = userId,
            };
            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }
            return HandleResult(_repository.AddDynamicComments(crmData));
        }

        public OutputResult<object> AddDynamicComments(AddDynamicCommentsModel data, int userId)
        {
            if (data.DynamicId == Guid.Empty)
                return ShowError<object>("DynamicId不可为空");
            var crmData = new DynamicCommentParameter()
            {
                DynamicId = data.DynamicId,
                PcommentsId = data.PCommentsid.GetValueOrDefault(),
                Comments = data.Comments
            };
            var dynamic = _repository.GetDynamicInfo(data.DynamicId);
            if (dynamic == null)
            {
                return ShowError<object>("动态数据不存在");
            }

            var newid = _repository.AddDynamicComments(crmData, userId);

            if (newid != Guid.Empty)
            {
                Task.Run(() =>
                {
                    DynamicEntityDetailtMapper detailMapper = new DynamicEntityDetailtMapper()
                    {
                        EntityId = dynamic.RelEntityId,
                        RecId = dynamic.RelBusinessId,
                        NeedPower = 0
                    };
                    //日报和周计划
                    if (dynamic.EntityId == new Guid("601cb738-a829-4a7b-a3d9-f8914a5d90f2") || 
                        dynamic.EntityId == new Guid("0b81d536-3817-4cbc-b882-bc3e935db845") ||
                        dynamic.EntityId == new Guid("fcc648ae-8817-48b7-b1d7-49ed4c24316b"))
                    {
                        //周总结
                        if (dynamic.EntityId != new Guid("fcc648ae-8817-48b7-b1d7-49ed4c24316b"))
                        {
                            detailMapper.EntityId = dynamic.EntityId;
                            detailMapper.RecId = dynamic.BusinessId;
                        }
                       
                        var detail = _dynamicEntityRepository.Detail(detailMapper, userId);
                        var members = MessageService.GetEntityMember(detail as Dictionary<string, object>);
                        DateTime reportdate = DateTime.Parse(detail["reportdate"].ToString());
                        var tempdata = JsonConvert.SerializeObject(dynamic.Tempdata);
                        //编辑操作的消息
                        var entityInfotemp = _entityProRepository.GetEntityInfo(dynamic.TypeId);
                        var entityMsg = MessageService.GetDailyMsgParameter(reportdate,entityInfotemp, dynamic.BusinessId, dynamic.RelBusinessId, "EntityDynamicComment", userId, members, null, tempdata);
                        if (entityMsg.TemplateKeyValue.ContainsKey("commentinfo"))
                        {
                            entityMsg.TemplateKeyValue["commentinfo"] = data.Comments;
                        }
                        else entityMsg.TemplateKeyValue.Add("commentinfo", data.Comments);
                        MessageService.WriteMessageAsyn(entityMsg, userId);

                    }
                    else
                    {
                        var detail = _dynamicEntityRepository.Detail(detailMapper, userId);
                        var members = MessageService.GetEntityMember(detail as Dictionary<string, object>);
                        var tempdata = JsonConvert.SerializeObject(dynamic.Tempdata);
                        //编辑操作的消息
                        var entityInfotemp = _entityProRepository.GetEntityInfo(dynamic.TypeId);
                        var entityMsg = MessageService.GetEntityMsgParameter(entityInfotemp, dynamic.BusinessId, dynamic.RelBusinessId, "EntityDynamicComment", userId, members, null, tempdata);
                        if(entityMsg.TemplateKeyValue.ContainsKey("commentinfo"))
                        {
                            entityMsg.TemplateKeyValue["commentinfo"] = data.Comments;
                        }
                        else entityMsg.TemplateKeyValue.Add("commentinfo", data.Comments);
                        MessageService.WriteMessageAsyn(entityMsg, userId);
                    } 
                    
                });
            }
            return new OutputResult<object>(newid);
        }

        public OutputResult<object> DeleteDynamicComments(DeleteDynamicCommentsModel data, int userId)
        {
            return HandleResult(_repository.DeleteDynamicComments(data.Commentsid, userId));
        }

        public OutputResult<object> AddDynamicPraise_old(DynamicPraiseModel data, int userId)
        {
            DynamicPraiseMapper crmData = new DynamicPraiseMapper()
            {
                DynamicId = data.DynamicId,
                UserNo = userId
            };
            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }
            return HandleResult(_repository.AddDynamicPraise(crmData));
        }

        public OutputResult<object> AddDynamicPraise(DynamicPraiseModel data, int userId)
        {

            if (data.DynamicId == Guid.Empty)
                return ShowError<object>("DynamicId不可为空");
            var dynamic = _repository.GetDynamicInfo(data.DynamicId);
            if (dynamic == null)
            {
                return ShowError<object>("动态数据不存在");
            }
            var res = _repository.AddDynamicPraise(data.DynamicId, userId);
            if (res)
            {
                Task.Run(() =>
                {
                    DynamicEntityDetailtMapper detailMapper = new DynamicEntityDetailtMapper()
                    {
                        EntityId = dynamic.RelEntityId,
                        RecId = dynamic.RelBusinessId,
                        NeedPower = 0
                    };
                    var detail = _dynamicEntityRepository.Detail(detailMapper, userId);
                    var members = MessageService.GetEntityMember(detail as Dictionary<string, object>);
                    var tempdata = JsonConvert.SerializeObject(dynamic.Tempdata);
                    //编辑操作的消息
                    var entityInfotemp = _entityProRepository.GetEntityInfo(dynamic.TypeId);
                    var entityMsg = MessageService.GetEntityMsgParameter(entityInfotemp, dynamic.BusinessId, dynamic.RelBusinessId, "EntityDynamicPrase", userId, members, null, tempdata);
                    MessageService.WriteMessageAsyn(entityMsg, userId);
                });

            }
            return new OutputResult<object>();
        }

        public OutputResult<object> DeleteDynamicPraise(DynamicPraiseModel data, int userId)
        {
            DynamicPraiseMapper crmData = new DynamicPraiseMapper()
            {
                DynamicId = data.DynamicId,
                UserNo = userId
            };
            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }
            return HandleResult(_repository.DeleteDynamicPraise(crmData));
        }
    }
}

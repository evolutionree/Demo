using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.Vocation;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Vocation;
using Newtonsoft.Json;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;
using System.Linq;
using Newtonsoft.Json.Linq;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models.Rule;
using UBeat.Crm.CoreApi.DomainModel.Version;
using AutoMapper;
using UBeat.Crm.CoreApi.Core.Utility;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class VocationServices : EntityBaseServices
    {
        private IVocationRepository _repository;
        private readonly IEntityProRepository _entityProRepository;
        private IMapper _mapper;
        public VocationServices(IVocationRepository repository, IEntityProRepository entityProRepository, IMapper mapper)
        {
            _repository = repository;
            _entityProRepository = entityProRepository;
            _mapper = mapper;
        }


        /// <summary>
        /// 添加职能
        /// </summary>
        /// <param name="body"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> AddVocation(VocationSaveModel body, int userNumber)
        {
            string VocationName = "";
            MultiLanguageUtils.GetDefaultLanguageValue(body.VocationName,body.VocationName_Lang, out VocationName);
            var crmData = new VocationAdd()
            {
                VocationName = VocationName,
                Description = body.Description,
                VocationName_Lang = body.VocationName_Lang

            };

            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }

            return HandleResult(_repository.AddVocation(crmData, userNumber));
        }

        /// <summary>
        /// 添加职能
        /// </summary>
        /// <param name="body"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> AddCopyVocation(CopyVocationSaveModel body, int userNumber)
        {

            var entity = _mapper.Map<CopyVocationSaveModel, CopyVocationAdd>(body);
            if (!entity.IsValid())
            {
                return HandleValid(entity);
            }

            return HandleResult(_repository.AddCopyVocation(entity, userNumber));
        }
        /// <summary>
        /// 编辑职能
        /// </summary>
        /// <param name="body"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> EditVocation(VocationSaveModel body, int userNumber)
        {
            string vocationName = "";
            MultiLanguageUtils.GetDefaultLanguageValue(body.VocationName, body.VocationName_Lang, out vocationName);
            var crmData = new VocationEdit()
            {
                VocationId = body.VocationId.Value,
                VocationName = vocationName,
                Description = body.Description,
                VocationName_Lang = body.VocationName_Lang
            };

            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }

            return HandleResult(_repository.EditVocation(crmData, userNumber));
        }


        /// <summary>
        /// 删除职能
        /// </summary>
        /// <param name="body"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> DeleteVocation(VocationDeleteModel body, int userNumber)
        {
            var crmData = new VocationDelete()
            {
                VocationIds = body.VocationIds

            };

            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }
            var res = HandleResult(_repository.DeleteVocation(crmData, userNumber));
            var users = _repository.GetVocationUsers(body.VocationIds);
            if (users != null && users.Count > 0)
            {
                IncreaseDataVersion(DataVersionType.PowerData, users.Select(m => m.UserId).ToList());
                RemoveUserDataCache(users.Select(m => m.UserId).ToList());
            }
            return res;
        }



        /// <summary>
        /// 获取职能列表
        /// </summary>
        /// <param name="body"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public dynamic GetVocations(VocationSelectModel body, int userNumber)
        {
            PageParam page = new PageParam()
            {
                PageIndex = body.PageIndex,
                PageSize = body.PageSize
            };

            if (!page.IsValid())
            {
                return HandleValid(page);
            }

            return new OutputResult<object>(_repository.GetVocations(page, body.VocationName, userNumber));
        }




        /// <summary>
        /// 根据职能id,获取功能列表
        /// </summary>
        /// <param name="body"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public dynamic GetFunctionsByVocationId(VocationFunctionSelectModel body, int userNumber)
        {
            var crmData = new VocationFunctionSelect()
            {
                FuncId = body.FuncId,
                VocationId = body.VocationId,
                Direction = body.Direction
            };

            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }

            return new OutputResult<object>(_repository.GetFunctionsByVocationId(crmData));
        }



        /// <summary>
        /// 编辑职能下的功能
        /// </summary>
        /// <param name="body"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> EditVocationFunctions(VocationFunctionEditModel body, int userNumber)
        {

            //把请求数据转为数据库方便处理的数据格式
            var listItems = new List<FunctionItem>();
            foreach (var item in body.FunctionJson)
            {
                if (item.FunctionCode == "Menu" || item.FunctionCode == "Dynamic" || item.FunctionCode == "TabDynamic")
                {
                    var functionIds = body.FunctionJson.Where(x => x.ParentId == item.FunctionId).Select(x => x.FunctionId).ToArray();
                    var count = listItems.Where(x => x.FunctionId == item.ParentId).Count();
                    if (count < 1)
                    {
                        listItems.Add(new FunctionItem()
                        {
                            FunctionId = item.ParentId,
                            relationValue = string.Join(",", functionIds)
                        });
                    }
                }
                else
                {
                    listItems.Add(new FunctionItem()
                    {
                        FunctionId = item.FunctionId,
                        relationValue = item.FunctionCode
                    });
                }
            }

            var serializerSettings = new JsonSerializerSettings()
            {
                ContractResolver = new LowercaseContractResolver()
            };

            var crmData = new VocationFunctionEdit()
            {
                VocationId = body.VocationId,
                FunctionJson = JsonConvert.SerializeObject(listItems, serializerSettings)
            };


            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }

            var result = HandleResult(_repository.EditVocationFunctions(crmData, userNumber));
            if (result.Status == 0)
            {
                GetCommonCacheData(userNumber, true);
                var users = _repository.GetVocationUsers(body.VocationId);
                if (users != null && users.Count > 0)
                {
                    IncreaseDataVersion(DataVersionType.PowerData, users.Select(m => m.UserId).ToList());
                    RemoveUserDataCache(users.Select(m => m.UserId).ToList());

                }
            }
            return result;
        }



        /// <summary>
        /// 添加功能下的规则
        /// </summary>
        /// <param name="body"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        //public OutputResult<object> AddFunctionRule(FunctionRuleAddModel body, int userNumber)
        //{
        //    var crmData = new FunctionRuleAdd()
        //    {
        //        VocationId = body.VocationId,
        //        FunctionId = body.FunctionId,
        //        Rule = JsonConvert.SerializeObject(body.Rule),
        //        RuleItem = JsonConvert.SerializeObject(body.RuleItems),
        //        RuleSet = JsonConvert.SerializeObject(body.RuleSet)
        //    };

        //    if (!crmData.IsValid())
        //    {
        //        return HandleValid(crmData);
        //    }

        //    return HandleResult(_repository.AddFunctionRule(crmData, userNumber));
        //}



        /// <summary>
        /// 编辑功能下的规则
        /// </summary>
        /// <param name="body"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        //public OutputResult<object> EditFunctionRule(FunctionRuleEditModel body, int userNumber)
        //{
        //    var crmData = new FunctionRuleEdit()
        //    {
        //        Rule = body.Rule,
        //        RuleItem = body.RuleItem,
        //        RuleSet = body.RuleSet
        //    };

        //    if (!crmData.IsValid())
        //    {
        //        return HandleValid(crmData);
        //    }

        //    return HandleResult(_repository.EditFunctionRule(crmData, userNumber));
        //}


        /// <summary>
        /// 获取功能下的规则
        /// </summary>
        /// <param name="body"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public dynamic GetFunctionRule(FunctionRuleSelectModel body, int userNumber)
        {
            var crmData = new FunctionRuleSelect()
            {
                VocationId = body.VocationId,
                FunctionId = body.FunctionId,
                EntityId = body.EntityId
            };

            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }

            var ruleList = _repository.GetFunctionRule(crmData, userNumber);
            var obj = ruleList.GroupBy(t => new
            {
                t.FunctionId,
                t.VocationId,
                t.RuleId,
                t.RuleName,
                t.RuleSet,
            }).Select(group => new VocationRuleInfoModel
            {
                RuleId = group.Key.RuleId,
                VocationId = group.Key.VocationId,
                FunctionId = group.Key.FunctionId,
                RuleName = group.Key.RuleName,
                RuleItems = group.Select(t => new RuleItemInfoModel
                {
                    ItemId = t.ItemId,
                    ItemName = t.ItemName,
                    FieldId = t.FieldId,
                    Operate = t.Operate,
                    UseType = t.UseType,
                    RuleData = t.RuleData,
                    RuleType = t.RuleType
                }).ToList(),
                RuleSet = new RuleSetInfoModel
                {
                    RuleSet = group.Key.RuleSet
                }
            }).ToList();
            return new OutputResult<object>(obj);
        }


        public OutputResult<object> GetUserFunctions(UserFunctionModel body, int userNumber)
        {
            List<Dictionary<string, object>> resutl = new List<Dictionary<string, object>>();
            var userData = GetUserData(userNumber);
            if (userData != null && userData.Vocations != null)
            {

                foreach (var m in userData.Vocations)
                {
                    var functions = m.Functions.Where(a => a.EntityId == body.EntityId && a.DeviceType == (int)DeviceClassic);
                    foreach (var f in functions)
                    {
                        if (!resutl.Exists(a => (Guid)a["funcid"] == f.FuncId && (Guid)a["entityid"] == f.EntityId))
                        {
                            var func = new Dictionary<string, object>();
                            func.Add("funcid", f.FuncId);
                            func.Add("relationvalue", f.RelationValue);
                            func.Add("funccode", f.Funccode);
                            func.Add("funcname", f.FuncName);
                            func.Add("entityid", f.EntityId);
                            resutl.Add(func);
                        }
                    }
                }
            }
            return new OutputResult<object>(resutl);
        }


        /// <summary>
        /// 获取职能下的用户
        /// </summary>
        /// <param name="body"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public dynamic GetVocationUser(VocationUserSelectModel body, int userNumber)
        {
            var crmData = new VocationUserSelect()
            {
                VocationId = body.VocationId,
                DeptId = body.DeptId,
                UserName = body.UserName
            };

            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }

            PageParam page = new PageParam()
            {
                PageIndex = body.PageIndex,
                PageSize = body.PageSize
            };

            if (!page.IsValid())
            {
                return HandleValid(page);
            }

            return new OutputResult<object>(_repository.GetVocationUser(page, crmData, userNumber));
        }





        /// <summary>
        /// 删除职能下的用户
        /// </summary>
        /// <param name="body"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> DeleteVocationUser(VocationUserDeleteModel body, int userNumber)
        {
            var crmData = new VocationUserDelete()
            {
                VocationId = body.VocationId,
                UserIds = body.UserIds
            };

            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }
            var res = HandleResult(_repository.DeleteVocationUser(crmData, userNumber));
            var users = _repository.GetVocationUsers(body.VocationId);
            if (users != null && users.Count > 0)
            {
                IncreaseDataVersion(DataVersionType.PowerData, users.Select(m => m.UserId).ToList());
                RemoveUserDataCache(users.Select(m => m.UserId).ToList());
            }
            return res;
        }



        /// <summary>
        /// 根据用户的职能，获取某个用户可用的功能列表
        /// </summary>
        /// <param name="body"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public dynamic GetUserFunctions(UserFunctionSelectModel body, int userNumber)
        {
            var crmData = new UserFunctionSelect()
            {
                UserNumber = body.UserNumber,
                DeviceType = body.DeviceType,
                Version = body.Version,

            };

            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }

            return new OutputResult<object>(_repository.GetUserFunctions(crmData));
        }



        /// <summary>
        /// 添加功能
        /// </summary>
        /// <param name="body"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> AddFunction(FunctionAddModel body, int userNumber)
        {
            if (body.FuncId.HasValue)
            {

                var crmData = new FunctionItemEdit()
                {
                    FuncId = body.FuncId.Value,
                    FuncName = body.FuncName,
                    FuncCode = body.FuncCode
                };

                if (!crmData.IsValid())
                {
                    return HandleValid(crmData);
                }
                return HandleResult(_repository.EditFunction(crmData, userNumber));

            }
            else
            {
                var crmData = new FunctionAdd()
                {
                    TopFuncId = body.TopFuncId,
                    FuncName = body.FuncName,
                    FuncCode = body.FuncCode,
                    EntityId = body.EntityId,
                    DeviceType = body.DeviceType
                };

                if (!crmData.IsValid())
                {
                    return HandleValid(crmData);
                }

                return HandleResult(_repository.AddFunction(crmData, userNumber));
            }
        }

        /// <summary>
        /// 编辑功能
        /// </summary>
        /// <param name="body"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> EditFunction(FunctionItemEditModel body, int userNumber)
        {
            var crmData = new FunctionItemEdit()
            {
                FuncId = body.FuncId,
                FuncName = body.FuncName,
                FuncCode = body.FuncCode
            };

            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }

            return HandleResult(_repository.EditFunction(crmData, userNumber));
        }



        /// <summary>
        /// 删除功能
        /// </summary>
        /// <param name="body"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> DeleteFunction(FunctionItemDeleteModel body, int userNumber)
        {
            var crmData = new FunctionItemDelete()
            {
                FuncId = body.FuncId
            };

            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }

            return HandleResult(_repository.DeleteFunction(crmData, userNumber));
        }





        /// <summary>
        /// 根据职能树
        /// </summary>
        /// <param name="body"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public dynamic GetFunctionTree(FunctionTreeSelectModel body, int userNumber)
        {
            var crmData = new FunctionTreeSelect()
            {
                TopFuncId = body.TopFuncId,
                Direction = body.Direction
            };

            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }

            return new OutputResult<object>(_repository.GetFunctionTree(crmData));
        }





        //end of class
    }
}



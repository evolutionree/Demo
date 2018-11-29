using AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;
using UBeat.Crm.CoreApi.DomainModel.Dynamics;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;
using UBeat.Crm.CoreApi.DomainModel.Notify;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Account;
using UBeat.Crm.CoreApi.Services.Models.Customer;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using UBeat.Crm.CoreApi.Services.Models.Message;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class CustomerServices : BasicBaseServices
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IMapper _mapper;
        private readonly IDynamicEntityRepository _dynamicEntityRepository;
        private readonly IDynamicRepository _dynamicRepository;
        private readonly IDocumentsRepository _documentsRepository;
        private readonly INotifyRepository _notifyRepository;
        private readonly IEntityProRepository _entityProRepository;
        private readonly IDataSourceRepository _dataSourceRepository;

        readonly Guid custEntityId = new Guid("f9db9d79-e94b-4678-a5cc-aa6e281c1246");


        public CustomerServices(IMapper mapper, ICustomerRepository customerRepository, IDynamicEntityRepository dynamicEntityRepository, IDynamicRepository dynamicRepository, IDocumentsRepository documentsRepository, INotifyRepository notifyRepository, IEntityProRepository entityProRepository, IDataSourceRepository dataSourceRepository)
        {
            _customerRepository = customerRepository;
            _mapper = mapper;
            _dynamicEntityRepository = dynamicEntityRepository;
            _dynamicRepository = dynamicRepository;
            _documentsRepository = documentsRepository;
            _notifyRepository = notifyRepository;
            _entityProRepository = entityProRepository;
            _dataSourceRepository = dataSourceRepository;
        }

        public OutputResult<object> QueryCustRelate(Guid custId, int usernumber)
        {
            return ExcuteAction((transaction, arg, userData) =>
            {
                // string rulesql = userData.RuleSqlFormat(RoutePath, custEntityId, DeviceClassic);

                var dyn = _customerRepository.QueryCustRelate(transaction, custId);
                return new OutputResult<object>(dyn);
            }, custId, usernumber);
        }

        //获取待合并的客户列表
        public OutputResult<object> GetMergeCustomerList(MergeCustListModel custModel, int usernumber)
        {
            return ExcuteAction((transaction, arg, userData) =>
            {
                //只有不处于审批中的客户可以合并
                string wheresql = userData.RuleSqlFormat(RoutePath, custEntityId, DeviceClassic);

                var sqlParameters = new List<DbParameter>();
                // sqlParameters.Add(new NpgsqlParameter(item.Key, item.Value));
                var allList = _customerRepository.GetMergeCustomerList(transaction, wheresql,custModel.SearchKey, sqlParameters.ToArray(), custModel.PageIndex, custModel.PageSize);

                return new OutputResult<object>(allList);

            }, custModel, usernumber);


        }



        #region --合并客户--
        //合并客户
        public OutputResult<object> MergeCustomer(CustMergeModel custModel, UserInfo userinfo)
        {

            if (custModel == null || custModel.CustIds == null || custModel.MainCustId == null || custModel.MainCustId == Guid.Empty)
                return ShowError<object>("参数错误");
            else if (custModel.CustIds.Count <= 0)
                return ShowError<object>("被合并的客户必须大于一个");

            if (_customerRepository.IsWorkFlowCustomer(custModel.CustIds, userinfo.UserId))
            {
                return ShowError<object>("被合并的客户不能是审批中的数据");
            }

            string mainCustName = null;
            List<string> mergecustName = null;
            List<string> mesReciver = null;
            DateTime reconlive = DateTime.MinValue;
            int custstatus = 1;
            List<string> viewusers = GetViewUsers(custModel, userinfo.UserId, custEntityId, out mainCustName, out mergecustName, out mesReciver, out reconlive, out custstatus);

            var relateEntitys = _entityProRepository.GetRelateEntityList(custEntityId, userinfo.UserId);

            var custFields = _dynamicEntityRepository.GetDataSourceEntityFields();
            var dataSource = _dataSourceRepository.GetEntityDataSources(custEntityId);


            var result = ExcuteAction((transaction, arg, userData) =>
            {
                //判断是否有合并权限
                if (!userData.HasDataAccess(transaction, RoutePath, custEntityId, DeviceClassic, custModel.CustIds))
                {
                    throw new Exception("被合并客户数据不符合Rule");
                }

                //只有不处于审批中的客户可以合并
                int usernumber = userData.UserId;
                foreach (var mergecustId in custModel.CustIds)
                {

                    //1.查找客户实体的所有关联动态实体，遍历对应的业务表，更新recrelateid为主客户id
                    var myrelateEntitys = relateEntitys.Where(m => m.ModelType == DomainModel.EntityPro.EntityModelType.Dynamic).ToList();
                    foreach (var m in myrelateEntitys)
                    {
                        var recordResult = _customerRepository.UpdateCustomerRelateDynamicEntity(transaction, custModel.MainCustId, custModel.CustIds, m.EntityTableName, usernumber);
                        if (recordResult == false)
                        {
                            throw new Exception("合并关联动态实体数据失败");
                        }
                    }
                    //2.查找所有关联了所属客户控件的实体，遍历对应的业务表，更新被合并的客户id为主客户id

                    foreach (var source in dataSource)
                    {
                        foreach (var field in custFields)
                        {
                            var tableName = field.EntityTableName;
                            //如果字段配置包含了客户数据源的id时
                            if (field.FieldConfig.Contains(source.DataSrcId.ToString()))
                            {
                                foreach (var cid in custModel.CustIds)
                                {
                                    var tempData = _customerRepository.GetCustomerRelateField(transaction, tableName, field.FieldName, cid);
                                    foreach (var temp in tempData)
                                    {
                                        if (temp.Data == null)
                                            continue;
                                        var fieldValue = JObject.Parse(temp.Data);
                                        List<string> ids = new List<string>();
                                        List<string> names = new List<string>();
                                        if (fieldValue["id"] != null)
                                        {
                                            ids = fieldValue["id"].ToString().Split(',').ToList();
                                            names = fieldValue["name"].ToString().Split(',').ToList();
                                            int index = ids.IndexOf(cid.ToString());
                                            ids.RemoveAt(index);
                                            names.RemoveAt(index);
                                        }
                                        ids.Add(custModel.MainCustId.ToString());
                                        names.Add(mainCustName);
                                        fieldValue["id"] = string.Join(",", ids.Distinct());
                                        fieldValue["name"] = string.Join(",", names.Distinct());
                                        var r = _customerRepository.UpdateCustomerRelateField(transaction, tableName, field.FieldName, fieldValue.ToString(), temp.RecId, usernumber);
                                        if (r == false)
                                        {
                                            throw new Exception("合并关联的客户数据失败");
                                        }
                                    }
                                }

                            }
                        }
                    }
                    //3.修改被合并客户关联的所有动态表中的业务id
                    var resulttemp = _dynamicRepository.MergeDynamic(transaction, custEntityId, custModel.MainCustId, custModel.CustIds, usernumber);
                    if (resulttemp == false)
                    {
                        throw new Exception("合并关联的动态数据失败");
                    }
                    //4.修改被合并客户关联的所有文档表中的业务id
                    resulttemp = _documentsRepository.MergeEntityDocument(transaction, custEntityId, custModel.MainCustId, custModel.CustIds, usernumber);
                    if (resulttemp == false)
                    {
                        throw new Exception("合并关联的文档数据失败");
                    }
                    //5.修改被合并客户关联的所有流程数据的业务id（暂不处理）

                    //6.合并客户信息的相关人
                    if (viewusers.Count > 0)
                    {
                        Dictionary<string, object> updateFileds = new Dictionary<string, object>();
                        updateFileds.Add("recupdator", usernumber);
                        updateFileds.Add("recupdated", DateTime.Now);
                        if (reconlive > DateTime.MinValue)
                            updateFileds.Add("reconlive", DateTime.Now);
                        updateFileds.Add("viewusers", string.Join(",", viewusers));
                        updateFileds.Add("custstatus", custstatus);

                        var updateViewusers = _customerRepository.UpdateCustomer(transaction, custModel.MainCustId, updateFileds, usernumber);
                        if (updateViewusers == false)
                        {
                            throw new Exception("合并相关人字段失败");
                        }
                    }

                    //7.删除被合并的客户
                    resulttemp = _customerRepository.DeleteBeMergeCustomer(transaction, custModel.CustIds, usernumber);
                    if (resulttemp == false)
                    {
                        throw new Exception("移除被合并客户失败");
                    }
                }
                return new OutputResult<object>(null);

            }, custModel, userinfo.UserId);

            SendMessage(custModel.MainCustId, userinfo, mainCustName, mergecustName);

            return result;

        }


        private void SendMessage(Guid bussinessId, UserInfo userinfo, string mainCustName, List<string> mergecustName)
        {
            Task.Run(() =>
            {
                try
                {
                    DynamicEntityDetailtMapper mainDetailMapper = new DynamicEntityDetailtMapper()
                    {
                        EntityId = custEntityId,
                        RecId = bussinessId,
                        NeedPower = 0
                    };
                    var mainCustDetail = _dynamicEntityRepository.Detail(mainDetailMapper, userinfo.UserId);

                    var relentityid = Guid.Empty;
                    var typeid = mainCustDetail.ContainsKey("rectype") ? Guid.Parse(mainCustDetail["rectype"].ToString()) : custEntityId;
                    var newMembers = MessageService.GetEntityMember(mainCustDetail as Dictionary<string, object>);
                    var msg = new MessageParameter();
                    msg.EntityId = custEntityId;
                    msg.TypeId = typeid;
                    msg.RelBusinessId = Guid.Empty;
                    msg.RelEntityId = Guid.Empty;
                    msg.BusinessId = bussinessId;
                    msg.ParamData = null;
                    msg.FuncCode = "CustomerMerge";

                    msg.Receivers = MessageService.GetEntityMessageReceivers(newMembers, null);

                    var paramData = new Dictionary<string, string>();
                    paramData.Add("operator", userinfo.UserName);
                    paramData.Add("mergecustnames", string.Format("{0}、{1}", mainCustName, string.Join("、", mergecustName)));
                    paramData.Add("newcustname", mainCustName);

                    msg.TemplateKeyValue = paramData;

                    MessageService.WriteMessageAsyn(msg, userinfo.UserId);
                }
                catch (Exception ex)
                {

                }
            });
        }



        /// <summary>
        /// 计算合并后的相关字段数据值，被合并的客户负责人应该归为客户的相关人
        /// </summary>
        /// <param name="custModel"></param>
        /// <param name="userNumber"></param>
        /// <param name="custEntityId"></param>
        /// <returns></returns>
        private List<string> GetViewUsers(CustMergeModel custModel, int userNumber, Guid custEntityId, out string mainCustName, out List<string> mergecustName, out List<string> mesReciver, out DateTime onlivetime, out int custstatus)
        {
            //获取被合并客户的所有相关人
            var viewusers = new List<string>();
            DynamicEntityDetailtListMapper detailMapper = new DynamicEntityDetailtListMapper()
            {
                EntityId = custEntityId,
                RecIds = string.Join(",", custModel.CustIds),
                NeedPower = 0
            };

            var mergecustDetail = _dynamicEntityRepository.DetailList(detailMapper, userNumber);
            mergecustName = new List<string>();
            DateTime reconlive = DateTime.MinValue;
            DateTime reconlivetemp = DateTime.MinValue;
            DateTime reccreated = DateTime.MinValue;
            int _custstatus = 1;

            foreach (var mergecustData in mergecustDetail)
            {

                if (mergecustData == null)
                    throw new Exception("待合并客户不存在");
                if (mergecustData.ContainsKey("viewusers") && mergecustData["viewusers"] != null)
                {
                    viewusers.AddRange(mergecustData["viewusers"].ToString().Split(','));
                }
                // //被合并的客户负责人应该归为客户的相关人
                if (mergecustData.ContainsKey("recmanager") && mergecustData["recmanager"] != null)
                {
                    viewusers.Add(mergecustData["recmanager"].ToString());
                }
                object rectemp = null;
                mergecustData.TryGetValue("recname", out rectemp);
                if (rectemp != null)
                    mergecustName.Add(rectemp.ToString());

                if (mergecustData.ContainsKey("reconlive") && mergecustData["reconlive"] != null)
                {
                    DateTime.TryParse(mergecustData["reconlive"].ToString(), out reconlivetemp);
                    DateTime.TryParse(mergecustData["reccreated"].ToString(), out reccreated);
                    if ((reconlivetemp - reccreated).TotalMinutes > 1 && reconlive < reconlivetemp)
                    {
                        reconlive = reconlivetemp;
                    }
                }
                object custstatustemp = 1;
                mergecustData.TryGetValue("custstatus", out custstatustemp);
                if ((int)custstatustemp > _custstatus)
                    _custstatus = (int)custstatustemp;

            }
            DynamicEntityDetailtMapper mainDetailMapper = new DynamicEntityDetailtMapper()
            {
                EntityId = custEntityId,
                RecId = custModel.MainCustId,
                NeedPower = 0
            };
            var mainCustDetail = _dynamicEntityRepository.Detail(mainDetailMapper, userNumber);
            if (mainCustDetail == null)
                throw new Exception("主客户不存在");
            object temp = null;
            mainCustDetail.TryGetValue("recname", out temp);
            if (temp == null)
                throw new Exception("缺少客户名称字段");
            mainCustName = temp.ToString();
            if (mainCustDetail.ContainsKey("viewusers") && mainCustDetail["viewusers"] != null)
            {
                viewusers.AddRange(mainCustDetail["viewusers"].ToString().Split(','));
            }
            if (mainCustDetail.ContainsKey("reconlive") && mainCustDetail["reconlive"] != null)
            {
                DateTime.TryParse(mainCustDetail["reconlive"].ToString(), out reconlivetemp);
                DateTime.TryParse(mainCustDetail["reccreated"].ToString(), out reccreated);
                if ((reconlivetemp - reccreated).TotalMinutes > 1 && reconlive < reconlivetemp)
                {
                    reconlive = reconlivetemp;
                }
            }
            onlivetime = reconlive;


            viewusers = viewusers.Distinct().ToList();

            mesReciver = new List<string>();
            object recmanager = null;
            mainCustDetail.TryGetValue("recmanager", out recmanager);
            if (temp != null)
                mesReciver.Add(recmanager.ToString());

            mesReciver.AddRange(viewusers);
            mesReciver = mesReciver.Distinct().ToList();

            object mytemp = 1;
            mainCustDetail.TryGetValue("custstatus", out mytemp);
            if (mytemp == null)
            {
                _custstatus = 1;
            }
            else {
                if ((int)mytemp > _custstatus)
                    _custstatus = (int)mytemp;
            }

            custstatus = _custstatus;
            return viewusers;
        }
        #endregion

        /// <summary>
        /// 用于在标准动态实体保存中回调的方法，包括新增保存，编辑保存，删除三个操作
        /// 新增保存，如果线索字段不为空，则反写对应线索的状态，且检测是否需要增加联系人
        /// 编辑保存，如果线索字段不为空，则检测是否需要新增联系人
        /// 删除客户，如果
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="param"></param>
        /// <param name="trackback"></param>
        public void CustomerSaveCallback(DbTransaction transaction, Dictionary<string, object> param, List<Dictionary<string, object>> trackback,int userNum,string userName) {
            if (param == null) return;
            if (param.ContainsKey("recid") == false || param.ContainsKey("entityid") == false || param.ContainsKey("opertype") ==false ) { return; }
            string recid = param["recid"].ToString();
            string entityid = param["entityid"].ToString();
            OperatType opertype = (OperatType)param["opertype"];

            string saleclueid = _customerRepository.getSaleClueIdFromCustomer(transaction, recid, 0);
            if (saleclueid == null || saleclueid.Length == 0) return;
            if (opertype == OperatType.Insert)
            {
                //反写销售线索
                _customerRepository.rewriteSaleClue(transaction, saleclueid, 0);
                //尝试新增联系人
                TryAndSaveContract(transaction, saleclueid, recid,userNum,userName);
            }
            else if (opertype == OperatType.Update)
            {
                TryAndSaveContract(transaction, saleclueid, recid, userNum, userName);
            }
            else if (opertype == OperatType.Delete) {

            } 
        }
        public void TryAndSaveContract(DbTransaction transaction, string saleclueid ,string custid,int userNum,string userName)
        {
            EntityTransferServices transferService = (EntityTransferServices)dynamicCreateService(typeof(EntityTransferServices).FullName, true);
            EntityTransferActionModel model = new EntityTransferActionModel();
            model.RuleId = "e09c381e-538f-4e3c-bc6d-7f7dd88d0a22";
            model.SrcEntityId = "db330ae1-b78c-4e39-bbb5-cc3c4a0c2e3b";
            model.SrcRecId = saleclueid;
            try
            {
                if (this._customerRepository.checkNeedAddContact(transaction,saleclueid,custid) == false) return;
                Dictionary<string, IDictionary<string, object>> transferData = transferService.CommonTransferBill(header, model, userNum, userName, new Dictionary<string, object>(), new Dictionary<string, object>(), transaction);

            }
            catch (Exception ex) {
            }


        }

        public OutputResult<object> SelectTodayIndex(int userNumber)
        {
            var data = _customerRepository.SelectTodayIndex(userNumber);
            return new OutputResult<object>(data);
        }

        public OutputResult<object> SelectDaily(int userNumber)
        {
            return ExcuteAction((transaction, arg, userData) =>
            {
                Dictionary<string, object> result = new Dictionary<string, object>();
                #region 获取当日拜访计划客户
                var beginDate = DateTime.Now.ToString("yyyy-MM-dd");
                var endDate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
                var customer = _customerRepository.SelectCustomerOfVisitPlan(beginDate, endDate, userNumber, transaction);
                #endregion
                result.Add("VisitSum", customer.Count());
                result.Add("data", customer);
                #region 查询客户拜访小结
                var data = _customerRepository.SelectTodayIndex(userNumber);
                #endregion
                result.Add("SaleSum", data["todaysale"]);
                return new OutputResult<object>(result);
            }, userNumber, userNumber);
        }

        #region 客户分配
        public OutputResult<object> DistributionCustomer(DistributionCustomerParam entity, int userid)
        {
            DbTransaction tran = null;
            var result = _customerRepository.DistributionCustomer(entity.Recids, entity.UserId, userid, tran);
            if (result)
                return new OutputResult<object>(null, "分配成功");
            else
                return new OutputResult<object>(null, "分配失败", 1);
        }
        #endregion
    }
}

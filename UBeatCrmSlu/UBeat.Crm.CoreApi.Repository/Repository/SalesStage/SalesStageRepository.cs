using Dapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.SalesStage;
using UBeat.Crm.CoreApi.DomainModel.Utility;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;
using System.Data.Common;

namespace UBeat.Crm.CoreApi.Repository.Repository.SalesStage
{
    public class SalesStageRepository : RepositoryBase, ISalesStageRepository
    {
        #region 销售阶段设定
        public Dictionary<string, List<IDictionary<string, object>>> SalesStageQuery(SalesstageTypeMapper entity, int userNumber)
        {
            var procName =
                "SELECT crm_func_salesstage_list(@salesstagetypeid,@userno,@foradmin)";

            var dataNames = new List<string> { "SalesStage" };
            var param = new
            {
                SalesstageTypeId = entity.SalesstageTypeId,
                UserNo = userNumber,
                ForAdmin=entity.ForAdmin
            };

            Dictionary<string, List<IDictionary<string, object>>> result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        public OperateResult InsertSalesStage(SaveSalesStageMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_stage_setting_add(@stagename,@winrate,@typeid,@userno,@stagename_lang)
            ";
            var param = new DynamicParameters();
            param.Add("stagename", entity.StageName);
            param.Add("winrate", entity.WinRate);
            param.Add("userno", userNumber);
            param.Add("typeid", entity.SalesStageTypeId);
            param.Add("stagename_lang", JsonConvert.SerializeObject(entity.StageName_Lang));
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult UpdateSalesStage(SaveSalesStageMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_stage_setting_edit(@salesstageid,@stagename,@winrate,@userno,@stagename_lang)
            ";
            var param = new DynamicParameters();
            param.Add("salesstageid", entity.SalesStageId);
            param.Add("stagename", entity.StageName);
            param.Add("winrate", entity.WinRate);
            param.Add("userno", userNumber);
            param.Add("stagename_lang", JsonConvert.SerializeObject(entity.StageName_Lang));
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult DisabledSalesStage(DisabledSalesStageMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * from crm_func_salesstage_disabled(@salesstageid, @status, @userno)";
            var param = new DynamicParameters();
            param.Add("salesstageid", entity.SalesStageId);
            param.Add("status", entity.RecStatus );
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult OrderBySalesStage(OrderBySalesStageMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_salesstage_orderby(@salesstageids,@userno)
            ";
            OperateResult result = new OperateResult();

            var param = new DynamicParameters();
            param.Add("salesstageids", entity.SalesStageIds);
            param.Add("userno", userNumber);
            return DataBaseHelper.QuerySingle<OperateResult>(sql, param);

        }


        public OperateResult OpentHighSetting(OpenHighSettingMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_salesstage_highsetting_save(@isopenhighsetting,@typeid,@userno)
            ";
            OperateResult result = new OperateResult();

            var param = new DynamicParameters();
            param.Add("isopenhighsetting", entity.IsOpenHighSetting);
            param.Add("typeid", entity.TypeId);
            param.Add("userno", userNumber);
            return DataBaseHelper.QuerySingle<OperateResult>(sql, param);

        }
        public int GetHighSetting(string TypeId, int userNumber) {
            int ret = 0;
            try
            {
                string sql = @"Select isopenhighsetting from crm_sys_salesstage_type_setting where salesstagetypeid=@salesstagetypeid";
                var param = new DynamicParameters();
                param.Add("salesstagetypeid", Guid.Parse( TypeId));
                List<IDictionary<string, object>> rs = DataBaseHelper.Query(sql, param);
                if (rs  != null && rs.Count>0)
                {
                    ret = (int)rs[0]["isopenhighsetting"];
                    return ret;
                }
            }
            catch (Exception ex) {
            }
            try
            {
                string sql = @"insert into crm_sys_salesstage_type_setting(salesstagetypeid,isopenhighsetting,reccreator,recupdator) values(@salesstagetypeid,@isopenhighsetting,@reccreator,@recupdator)";
                var param = new DynamicParameters();
                param.Add("salesstagetypeid", Guid.Parse(TypeId));
                param.Add("isopenhighsetting", 0);
                param.Add("reccreator", userNumber);
                param.Add("recupdator", userNumber);
                DataBaseHelper.ExecuteNonQuery(sql, param);
                return 0;
            }
            catch (Exception ex) {
            }
            return 0; 
        }

        #endregion

        #region 销售阶段事件
        public Dictionary<string, List<IDictionary<string, object>>> SalesStageSettingQuery(SalesStageSetLstMapper entity, int userNumber)
        {
            var procName =
                "SELECT crm_func_salesstage_set_list(@salesstageid,@userno)";

            var dataNames = new List<string> { "SalesStageEvent", "SalesStageOppInfo", "SalesStageDynEntity" };

            var param = new DynamicParameters();
            param.Add("salesstageid", entity.SalesStageId);
            param.Add("userno", userNumber);

            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }
        public OperateResult InsertSalesStageEventSetting(AddSalesStageEventSetMapper entity, int userNumber)
        {
            var procName =
                "SELECT * FROM crm_func_salesstage_event_set_add(@salesstageid,@eventname,@isneedupfile,@userno)";


            var param = new DynamicParameters();
            param.Add("salesstageid", entity.SalesStageId);
            param.Add("eventname", entity.EventName);
            param.Add("isneedupfile", entity.IsNeedUpFile);
            param.Add("userno", userNumber);

            var result = DataBaseHelper.QuerySingle<OperateResult>(procName, param, CommandType.Text);
            return result;
        }
        public OperateResult UpdateSalesStageEventSetting(EditSalesStageEventSetMapper entity, int userNumber)
        {
            var procName =
                "SELECT * FROM crm_func_salesstage_event_set_edit(@eventsetid,@salesstageid,@eventname,@isneedupfile,@userno)";


            var param = new DynamicParameters();
            param.Add("eventsetid", entity.EventSetId);
            param.Add("salesstageid", entity.SalesStageId);
            param.Add("eventname", entity.EventName);
            param.Add("isneedupfile", entity.IsNeedUpFile);
            param.Add("userno", userNumber);

            var result = DataBaseHelper.QuerySingle<OperateResult>(procName, param, CommandType.Text);
            return result;
        }
        public OperateResult DisabledSalesStageEventSetting(DisabledSalesStageEventSetMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * from crm_func_salesstage_event_set_disabled(@eventsetid, @userno)";
            var param = new DynamicParameters();
            param.Add("eventsetid", entity.EventSetId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }
        #endregion

        #region  销售阶段商机关键信息
        //public Dictionary<string, List<IDictionary<string, object>>> SalesStageOppInfoSettingQuery(SalesStageOppInfoSetLstMapper entity, int userNumber)
        //{
        //    var procName =
        //        "SELECT crm_func_salesstage_oppinfo_set_list(@salesstageid,@userno)";

        //    var dataNames = new List<string> { "SalesStageOppInfo" };
        //    var param = new DynamicParameters();
        //    param.Add("salesstageid", entity.SalesStageId);
        //    param.Add("userno", userNumber);

        //    var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
        //    return result;
        //}
        public Dictionary<string, List<IDictionary<string, object>>> SalesStageInfoFieldsQuery(SalesStageOppInfoFieldsMapper entity, int userNumber)
        {
            var procName =
                "SELECT * from  crm_func_salesstage_info_field_list(@salesstagetypeid,@entityid,@salesstageid,@userno)";

            var dataNames = new List<string> { "SalesStageOppInfo", "SalesStageOppInfoVi" };
            var param = new DynamicParameters();
            param.Add("salesstageid", entity.SalesStageId);
            param.Add("entityid", entity.EntityId);
            param.Add("salesstagetypeid", entity.SalesStageTypeId);
            param.Add("userno", userNumber);

            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }
        public OperateResult SaveSalesStageOppInfoSetting(SaveSalesStageOppInfoSetMapper entity, int userNumber)
        {
            var procName =
                "SELECT * FROM crm_func_salesstage_info_set_save(@salesstageid,@entityid,@salesstagetypeid,@fieldids,@userno)";


            var param = new DynamicParameters();
            param.Add("salesstageid", entity.SalesStageId);
            param.Add("entityid", entity.EntityId);
            param.Add("salesstagetypeid", "");//函数中会重新计算
            param.Add("fieldids", entity.FieldIds);
            param.Add("userno", userNumber);

            var result = DataBaseHelper.QuerySingle<OperateResult>(procName, param, CommandType.Text);
            return result;
        }
        #endregion

        #region 关联动态实体    
        public Dictionary<string, List<IDictionary<string, object>>> SalesStageRelEntityQuery(int userNumber)
        {
            var procName =
                "SELECT crm_func_salesstage_relentity_list(@userno)";

            var dataNames = new List<string> { "SalesStageRelEntity" };
            var param = new
            {
                UserNo = userNumber
            };

            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }
        public OperateResult InsertSalesStageDynEntitySetting(AddSalesStageDynEntitySetMapper entity, int userNumber)
        {
            var procName =
                "SELECT * FROM crm_func_salesstage_dynentity_set_add(@salesstageid,@relentityid,@userno)";


            var param = new DynamicParameters();
            param.Add("salesstageid", entity.SalesStageId);
            param.Add("relentityid", entity.RelEntityId);
            param.Add("userno", userNumber);

            var result = DataBaseHelper.QuerySingle<OperateResult>(procName, param, CommandType.Text);
            return result;
        }

        public OperateResult DeleteSalesStageDynEntitySetting(DelSalesStageDynEntitySetMapper entity, int userNumber)
        {
            var procName =
                "SELECT * FROM crm_func_salesstage_dynentity_set_delete(@dynentityid,@userno)";


            var param = new DynamicParameters();
            param.Add("dynentityid", entity.DynEntityId);
            param.Add("userno", userNumber);

            var result = DataBaseHelper.QuerySingle<OperateResult>(procName, param, CommandType.Text);
            return result;
        }
        #endregion

        #region  销售阶段推进步骤接口
        public Dictionary<string, List<IDictionary<string, object>>> SalesStageStepInfoQuery(SalesStageStepInfoMapper entity, int userNumber)
        {
            var procName = "  SELECT  crm_func_salesstage_step_info(@recid,@salesstageid,@salesstagetypeid, @userno) ";
            var param = new DynamicParameters();
            param.Add("recid", entity.RecId);
            param.Add("salesstageid", entity.SalesStageId);
            param.Add("salesstagetypeid", entity.SalesStageTypeId);
            param.Add("userno", userNumber);
            var dataNames = new List<string> { "EventSet", "OppInfoSet", "DynamicEntitySet", "DynamicValCursor", "SalesStageStatus" };
            return DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
        }
        public OperateResult CheckAllowPushSalesStage(SaveSalesStageStepInfoMapper entity, int userNumber)
        {
            var procName =
                "SELECT * FROM  crm_func_salesstage_nextstep_check(@recid,@typeid,@salesstageids,@relrecid,@userno)";
            var param = new DynamicParameters();
            param.Add("recid", entity.RecId);
            param.Add("typeid", entity.TypeId);
            param.Add("salesstageids", entity.SalesStageIds);
            param.Add("relrecid", entity.RelRecId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(procName, param, CommandType.Text);//判断是否开启高级设置
            return result;
        }
        public OperateResult CheckAllowReturnSalesStage(ReturnSalesStageStepInfoMapper entity, int userNumber)
        {
            var procName =
                "SELECT * FROM  crm_func_salesstage_returnback_check(@recid,@salesstageid,@typeid,@userno)";
            var param = new DynamicParameters();
            param.Add("recid", entity.RecId);
            param.Add("salesstageid", entity.SalesStageId);
            param.Add("typeid", entity.TypeId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(procName, param, CommandType.Text);//判断是否开启高级设置
            return result;
        }

        public OperateResult SaveSalesStageEvent(SaveSalesStageStepInfoMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_salesstage_event_save(@isweb,@recid,@salesstageid,@eventset,@userno)
            ";
            var eventJson = JsonHelper.ToJson(entity.Event);
            var param = new DynamicParameters();
            param.Add("isweb", entity.IsWeb);
            param.Add("recid", entity.RecId);
            param.Add("salesstageid", entity.SalesStageId);
            param.Add("eventset", eventJson);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult SaveSalesStageOppInfo(SaveSalesStageStepInfoMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_salesstage_info_save(@recid,@salesstageid,@oppinfoset,@userno)
            ";
            //       var oppinfoJson =JsonHelper.ToJson(entity.Info);
            var param = new DynamicParameters();
            param.Add("recid", entity.RecId);
            param.Add("salesstageid", entity.SalesStageId);
            //         param.Add("oppinfoset", oppinfoJson);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }
        public OperateResult SaveSalesStageInfo(SaveSalesStageStepInfoMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_salesstage_save(@isweb,@typeid,@recid,@salesstageid,@event,@userno)
            ";
            var eventJson = JsonHelper.ToJson(entity.Event);
            var param = new DynamicParameters();
            param.Add("isweb", entity.IsWeb);
            param.Add("typeid", entity.TypeId);
            param.Add("recid", entity.RecId);
            param.Add("salesstageid", entity.SalesStageId);
            param.Add("event", eventJson);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }
        public OperateResult UpdateOpportunityStatus(UpdateOpportunityStatusMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_opp_salesstage_status_update(@recid,@salesstageid,@userno)
            ";
            var param = new DynamicParameters();
            param.Add("recid", entity.RecId);
            param.Add("salesstageid", entity.SalesStageId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult SaveSalesStageDynEntity(SaveDynEntityMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_salesstage_dynentity_save(@recid,@salesstageid,@dynrecid,@userno)
            ";
            var param = new DynamicParameters();
            param.Add("recid", entity.RecId);
            param.Add("salesstageid", entity.SalesStageId);
            param.Add("dynrecid", entity.DynRecId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }
        public string ReturnSalesStageDynentityId(string recId, string salesStageId, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_salesstage_return_dynentity_id(@recid,@salesstageid,@userno)
            ";
            var param = new DynamicParameters();
            param.Add("recid", recId);
            param.Add("salesstageid", salesStageId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<string>(sql, param);
            return result;
        }
        public OperateResult SaveLoseOrderInfo(LoseOrderMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_salesstage_loseorder_save(@loseorderid,@opportunityid,@losereason,@reasonsupplement,@userno)
            ";

            var param = new DynamicParameters();
            param.Add("loseorderid", entity.LoseOrderId);
            param.Add("opportunityid", entity.OpportunityId);
            param.Add("losereason", entity.LoseReason);
            param.Add("reasonsupplement", entity.ReasonSupplement);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult SaveWinOrderInfo(WinOrderMapper entity, int userNumber)
        {
            var sql = @"
        SELECT* FROM crm_func_salesstage_winorder_save(@winorderid, @opportunityid, @incometype, @signedate, @remark, @userno)
            ";

            var param = new DynamicParameters();
            param.Add("winorderid", entity.WinOrderId);
            param.Add("opportunityid", entity.OpportunityId);
            param.Add("incometype", entity.IncomeType);
            param.Add("signedate", entity.SigneDate);
            param.Add("remark", entity.Remark);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }
        public Dictionary<string, List<IDictionary<string, object>>> LoseOrderInfoQuery(OrderInfoMapper entity, int userNumber)
        {
            var procName = @"
                SELECT   crm_func_loseorder_info(@opportunityid,@userno)
            ";
            var dataNames = new List<string> { "LoseOrderInfo" };
            var param = new DynamicParameters();
            param.Add("opportunityid", entity.OpportunityId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        public Dictionary<string, List<IDictionary<string, object>>> WinOrderInfoQuery(OrderInfoMapper entity, int userNumber)
        {
            var procName = @"
                SELECT   crm_func_winorder_info(@opportunityid,@userno)
            ";
            var dataNames = new List<string> { "WinOrderInfo" };
            var param = new DynamicParameters();
            param.Add("opportunityid", entity.OpportunityId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }



        public OperateResult SalesStageRestart(SalesStageRestartMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * from   crm_func_salesstage_restart(@recid,@typeid,@userno)
            ";

            var param = new DynamicParameters();
            param.Add("recid", entity.RecId);
            param.Add("typeid", entity.TypeId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public string ReturnEntityId(string typeId, int userNumber)
        {
            var sql = @"
        SELECT* FROM crm_sys_return_entity_id(@typeid, @userno)
            ";
            var param = new DynamicParameters();
            param.Add("typeid", typeId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<string>(sql, param);
            return result;
        }
        /// <summary>
        /// 根据销售阶段类型id，检测并修复销售阶段中的赢单和输单的表单配置
        /// 如果是高级模式，赢单阶段清空表单，输单固定为输单表单
        /// 如果是非高级模式，赢单固定为赢单表单，输单固定为输单的表单配置
        /// </summary>
        /// <param name="typeId"></param>
        /// <param name="userNumber"></param>
        public void CheckSaleStageDynamicFormSetting(string typeId, int userNumber,DbTransaction transaction = null)
        {
            string Std_LoseForm_EntityId = "aa3222ac-767b-4bbf-8c0e-deae42760f05";
            string Std_WinForm_EntityId = "d08792f6-c323-455c-b6f1-61bacb2923ca";
            try
            {
                string cmdText = string.Format(@"select t.salesstagetypeid ,t.isopenhighsetting,
	                                                    a.salesstageid ,a.stagename,
	                                                    b.dynentitysetid,b.relentityid
                                                    from 
		                                                    crm_sys_salesstage_type_setting t 
			                                                    inner join crm_sys_salesstage_setting  a on a.salesstagetypeid  = t.salesstagetypeid
			                                                    left outer join crm_sys_salesstage_dynentity_setting b on a.salesstageid = b.salesstageid 
                                                    where t.salesstagetypeid = '{0}'::uuid
                                                    order by a.recorder ", typeId);
                List<Dictionary<string, object>> data = ExecuteQuery(cmdText, new DbParameter[] { }, transaction);
                if (data == null || data.Count == 0) return;
                int isopenhighsetting = 0;
                if (data[0]["isopenhighsetting"] == null) return;
                int.TryParse(data[0]["isopenhighsetting"].ToString(), out isopenhighsetting);
                string winstageid = "";
                string windyentitysetid = "";
                string winentityid = "";
                string losestageid = "";
                string losedyentitysetid = "";
                string loseentityid = "";
                for (int i = 0; i < data.Count; i++) {
                    if (data[i]["stagename"] != null) {
                        if (data[i]["stagename"].ToString() == "赢单")
                        {
                            if (data[i]["salesstageid"] != null) {
                                winstageid = data[i]["salesstageid"].ToString();
                            }
                            if (data[i]["dynentitysetid"] != null)
                            {
                                windyentitysetid = data[i]["dynentitysetid"].ToString();
                            }
                            if (data[i]["relentityid"] != null)
                            {
                                winentityid = data[i]["relentityid"].ToString();
                            }
                        }
                        else if(data[i]["stagename"].ToString() == "输单") {
                            if (data[i]["salesstageid"] != null)
                            {
                                losestageid = data[i]["salesstageid"].ToString();
                            }
                            if (data[i]["dynentitysetid"] != null)
                            {
                                losedyentitysetid = data[i]["dynentitysetid"].ToString();
                            }
                            if (data[i]["relentityid"] != null)
                            {
                                loseentityid = data[i]["relentityid"].ToString();
                            }
                        }
                    }
                }
                if (winstageid.Length == 0 || losestageid.Length == 0) return;

                if (isopenhighsetting == 1)
                {
                    if (windyentitysetid.Length > 0)
                    {
                        //删除赢单表单的关联
                        cmdText = string.Format(@"delete from crm_sys_salesstage_dynentity_setting where dynentitysetid='{0}'::uuid", windyentitysetid);
                        ExecuteNonQuery(cmdText, new DbParameter[] { }, transaction);
                    }
                }
                else {
                    if (windyentitysetid.Length > 0)
                    {
                        if (Std_WinForm_EntityId != winentityid) {

                            cmdText = string.Format("update crm_sys_salesstage_dynentity_setting set relentityid='{0}'::uuid where dynentitysetid='{1}'::uuid", Std_WinForm_EntityId, winstageid);
                            ExecuteNonQuery(cmdText, new DbParameter[] { }, transaction);
                        }
                    }
                    else {
                        //插入赢单配置
                        cmdText = string.Format(@"INSERT INTO crm_sys_salesstage_dynentity_setting(
                                                relentityid, salesstageid, recorder, recstatus, reccreator, recupdator, reccreated, recupdated) 
                                                values('{0}'::uuid,'{1}'::uuid,0,1,{2},{2},now(),now())", Std_WinForm_EntityId, winstageid, userNumber);
                        ExecuteNonQuery(cmdText, new DbParameter[] { }, transaction);
                    }
                   
                }
                //处理输单表单
                if (losedyentitysetid == null)
                {
                    //插入输单表单设置
                    cmdText = string.Format(@"INSERT INTO crm_sys_salesstage_dynentity_setting(
                                                relentityid, salesstageid, recorder, recstatus, reccreator, recupdator, reccreated, recupdated) 
                                                values('{0}'::uuid,'{1}'::uuid,0,1,{2},{2},now(),now())", Std_LoseForm_EntityId, losestageid, userNumber);
                    ExecuteNonQuery(cmdText, new DbParameter[] { }, transaction);
                }
                else {
                    if (Std_LoseForm_EntityId != loseentityid) {
                        //更新输单表单
                        cmdText = string.Format("update crm_sys_salesstage_dynentity_setting set relentityid='{0}'::uuid where dynentitysetid='{1}'::uuid ", Std_LoseForm_EntityId, losedyentitysetid);
                        ExecuteNonQuery(cmdText, new DbParameter[] { }, transaction);
                    }
                }
            }
            catch (Exception ex) {
            }
        }

        public List<string> queryDynamicRecIdsFromOppId(string oppid, int userNumber, DbTransaction transaction = null)
        {
            try
            {
                string cmdText = string.Format(@"select dynrecid::text  FROM crm_sys_salesstage_dynentity  where recid ='{0}'::uuid ", oppid);
                List<Dictionary<string, object>> retList = ExecuteQuery(cmdText, new DbParameter[] { }, transaction);
                List<string> ret = new List<string>();
                foreach (Dictionary<string, object> item in retList) {
                    string id = item["dynrecid"].ToString();
                    ret.Add(id);
                }
                return ret;
            }
            catch (Exception ex) {

            }
            return new List<string>();
        }

        public int checkHasOppInStageID(string stageid, int userNum, DbTransaction transaction = null) {
            try
            {
                string cmdText = string.Format(@"select count(*) totalcount
                                                    from crm_sys_opportunity 
                                                    where recstatus =1 
                                                    and recstageid = '{0}'::uuid", stageid);
                object obj = ExecuteScalar(cmdText, new DbParameter[] { }, transaction);
                if (obj == null) return 0;
                return int.Parse(obj.ToString());
            }
            catch (Exception ex) {

            }
            return 0;
        }

        #endregion
    }
}

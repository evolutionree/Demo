using System;
using System.Collections.Generic;
using System.Data;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Reminder;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;
using System.Linq;
using Newtonsoft.Json;
using Dapper;

namespace UBeat.Crm.CoreApi.Repository.Repository.Reminder
{
    public class ReminderRepository : IReminderRepository
    {

        /// <summary>
        /// 获取系统提醒列表
        /// </summary>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public dynamic GetReminderSettingList(int userNumber)
        {
            var executeSql = "select * from crm_func_reminder_setting_select(@userno)";
            var args = new
            {
                UserNo = userNumber,
            };

            return DataBaseHelper.QueryStoredProcCursor(executeSql, args, CommandType.Text);
        }

        /// <summary>
        /// 获取提醒设置子选项
        /// </summary>
        /// <param name="settingId"></param>
        /// <returns></returns>
        public List<IDictionary<string, object>> getReminderSettingItems(int dicTypeid)
        {
            var executeSql = "select a.dictypeid,b.dicid,b.dataval,c.id itemid" +
                " from crm_sys_dictionary_type a " +
                "inner join crm_sys_dictionary b on a.dictypeid=b.dictypeid " +
                "left join crm_sys_reminder_setitem c on c.dicid = b.dicid " +
                "where a.dictypeid =@dicTypeid and a.recstatus = 1  order by a.recorder ";
            var args = new
            {
                dicTypeid = dicTypeid
            };
            return DataBaseHelper.Query(executeSql, args);
        }


        /// <summary>
        /// 更新系统提醒
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumber"></param>
        public void UpdateDocumentFolder(IList<ReminerSettingInsert> data, int userNumber)
        {
            var executeSql = @"select * from crm_func_reminder_setting_update(
                               @configid,@configname,@recstatus,@checkday,@cronstring,@configval,@userno)";

            List<dynamic> args = new List<dynamic>();
            foreach (var item in data)
            {
                args.Add(new
                {
                    ConfigId = item.Id.ToString(),
                    ConfigName = item.Name,
                    RecStatus = item.RecStatus,
                    CheckDay = item.CheckDay,
                    Cronstring = item.CronString,
                    ConfigVal = item.ConfigVal,
                    UserNo = userNumber.ToString(),
                });
            }

            DataBaseHelper.ExecuteNonQuery(executeSql, args);
        }
        /// <summary>
        /// 保存系统提醒子项设置列表
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumber"></param>
        public void UpdateReminderItem(List<ReminderItemInsert> data, IList<int> typeIdList)
        {
            foreach (var typeId in typeIdList)
            {
                //清除历史记录
                string delSql = "delete from  crm_sys_reminder_setitem a where a.dictypeid=@dictypeid";
                List<dynamic> delArgs = new List<dynamic>();
                delArgs.Add(new
                {
                    dicTypeid = typeId
                });
                DataBaseHelper.ExecuteNonQuery(delSql, delArgs);
            }
            foreach (var item in data)
            {
                List<dynamic> args = new List<dynamic>();
                //重新插入记录
                string inertSql = "insert into crm_sys_reminder_setitem VALUES(uuid_generate_v4(),@dicId,@dicTypeid)";
                args.Add(new
                {
                    dicId = item.dicId,
                    dicTypeid = item.dicTypeId
                });
                DataBaseHelper.ExecuteNonQuery(inertSql, args);
            }
        }

        /// <summary>
        /// 添加自定义提醒
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public OperateResult AddCustomReminder(ReminerEventInsert data)
        {
            var executeSql = @"SELECT * FROM crm_func_reminder_event_insert(@eventname,@entityregid,@title,@checkday,
                                                                           @sendtime,@type,@expandfieldid,@param,
                                                                           @content,@usercolumn,@remindtype,@timeformat,@userno)";
            var args = new
            {
                EventName = data.EventName,
                EntityRegId = data.EntityId,
                Title = data.Title,
                CheckDay = data.CheckDay,
                SendTime = data.SendTime,
                Type = data.Type,
                ExpandFieldId = data.ExpandFieldId,
                Param = data.Params,
                Content = data.Content,
                UserColumn = data.UserColumn,
                RemindType = data.RemindType,
                TimeFormat = data.TimeFormat,
                UserNo = data.UserNumber
            };

            var result = DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
            return result;
        }


        /// <summary>
        /// 更新自定义提醒
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public OperateResult UpdateCustomReminder(ReminerEventUpdate data)
        {
            var executeSql = @"SELECT * FROM crm_func_reminder_event_update(@eventid,@eventname,@title,@checkday, 
                                                                            @sendtime,@expandfieldid,
                                                                            @param,@content,@timeformat,@userno)";
            var args = new
            {
                EventId = data.EventId,
                EventName = data.EventName,
                Title = data.Title,
                CheckDay = data.CheckDay,
                SendTime = data.SendTime,
                ExpandFieldId = data.ExpandFieldId,
                Param = data.Params,
                Content = data.Content,
                TimeFormat = data.TimeFormat,
                UserNo = data.UserNumber
            };


            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);

        }


        /// <summary>
        /// 删除自定义提醒
        /// </summary>
        /// <param name="eventids"></param>
        /// <param name="usernumber"></param>
        /// <returns></returns>
        public OperateResult DeleteCustomReminder(List<string> eventids, int usernumber)
        {
            var executeSql = @"SELECT * FROM crm_func_reminder_event_delete(@eventids,@userno)";
            var args = new
            {
                EventIds = string.Join(",", eventids),
                UserNo = usernumber.ToString()
            };

            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }


        /// <summary>
        /// 启用，停用自定义提醒
        /// </summary>
        /// <param name="eventids"></param>
        /// <param name="status"></param>
        /// <param name="usernumber"></param>
        public void SetCustomReminderEnable(List<string> eventids, int status, int usernumber)
        {
            var executeSql = "SELECT * FROM crm_func_reminder_event_setstatus(@eventids,@status,@userno)";

            var args = new
            {
                EventIds = string.Join(",", eventids),
                Status = status,
                UserNo = usernumber
            };


            DataBaseHelper.ExecuteNonQuery(executeSql, args);
        }


        /// <summary>
        /// 获取一条自定义提醒的内容
        /// </summary>
        /// <param name="eventid"></param>
        /// <param name="usernumber"></param>
        /// <returns></returns>
        public List<IDictionary<string, object>> CustomReminderInfo(string eventid, int usernumber)
        {
            var executeSql = "select * from  crm_func_reminder_event_select(@eventid,@userno)";

            var args = new
            {
                EventId = eventid,
                UserNo = usernumber
            };

            var result = DataBaseHelper.QueryStoredProcCursor(executeSql, args, CommandType.Text);
            return result;

        }

        /// <summary>
        /// 获取全部自定义提醒
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="usernumber"></param>
        /// <returns></returns>
        public List<IDictionary<string, object>> ReminderMessageList(int pageIndex, int pageSize, int usernumber)
        {
            var executeSql = "select * from  crm_func_reminder_event_page(@pageindex,@pagesize,@userno)";

            var args = new
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                UserNo = usernumber
            };

            return DataBaseHelper.QueryStoredProcCursor(executeSql, args, CommandType.Text);

        }



        #region 职能提醒

        public dynamic GetReminderList(PageParam page, ReminderListMapper data, int userNumber)
        {

            var executeSql = "select * from crm_func_reminder_select(@rectype,@recstatus,@remindername,@userno,@pageindex,@pagesize)";
            var args = new
            {
                rectype = data.RecType,
                recstatus = data.RecStatus,
                remindername = data.ReminderName,
                UserNo = userNumber,
                pageindex = page.PageIndex,
                pagesize = page.PageSize
            };



            var result = DataBaseHelper.QueryStoredProcCursor(executeSql, args, CommandType.Text);
            var dataNames = new List<string> { "datacursor", "pagecursor" };
            var dataResult = DataBaseHelper.QueryStoredProcCursor(executeSql, dataNames, args, CommandType.Text);
            return dataResult;
        }


        public OperateResult SaveReminder(ReminderSaveMapper data, int usernumber)
        {
            string strSql = string.Empty;


      


            // if reminderid is not null, then update else insert
            if (data.ReminderId.HasValue)
            {
                var oldReminder = GetReminderById(data.ReminderId.Value, usernumber);

                if (oldReminder.IsRepeat != data.IsRepeat || oldReminder.CronString != data.CronString || oldReminder.RecStatus != data.RecStatus)
                {
                    strSql = @" UPDATE crm_sys_reminder
                            SET remindername=@remindername,
                                isrepeat=@isrepeat,
                                repeattype=@repeattype,
                                cronstring=@cronstring,
                                recstatus=@recstatus,
                                remark=@remark,
                                recupdator=@recupdator,
                                recupdated=now(),
                                recversion=nextval('crm_sys_reminder_version_id_sequence'::regclass),
                                remindername_lang=@remindername_lang::jsonb
                            WHERE recid=@reminderid; ";
                }
                else
                {
                    //如果只是更新了提醒名称,就不需要更新recversion
                    strSql = @" UPDATE crm_sys_reminder
                            SET remindername=@remindername,
                                isrepeat=@isrepeat,
                                repeattype=@repeattype,
                                cronstring=@cronstring,
                                recstatus=@recstatus,
                                remark=@remark,
                                recupdator=@recupdator,
                                recupdated=now(),
                                remindername_lang=@remindername_lang::jsonb
                            WHERE recid=@reminderid; ";
                }

            }
            else
            {
                strSql = @"INSERT INTO crm_sys_reminder(recid,remindername,entityid,isrepeat,repeattype,cronstring,recstatus,remark,reccreator,recupdator,rectype,remindername_lang) 
                           VALUES(@reminderid,@remindername,@entityid,@isrepeat,@repeattype,@cronstring,@recstatus,@remark,@reccreator,@recupdator,@rectype,@remindername_lang::jsonb); ";
            }


            if (data.ReminderId == null)
            {

                data.ReminderId = Guid.NewGuid();
            }


            var param = new
            {
                reminderid = data.ReminderId,
                remindername = data.ReminderName,
                entityid = data.EndityId,
                isrepeat = data.IsRepeat,
                repeattype = data.RepeatType,
                cronstring = data.CronString,
                recstatus = data.RecStatus,
                remark = data.Remark,
                reccreator = usernumber,
                recupdator = usernumber,
                rectype = data.RecType,
                remindername_lang = JsonConvert.SerializeObject( data.ReminderName_Lang)
            };

            bool isSuccess = false;
            int result = DataBaseHelper.ExecuteNonQuery(strSql, param, CommandType.Text);
            if (result > 0)
            {
                isSuccess = true;
            }
            else
            {
                isSuccess = false;
            }

            return new OperateResult()
            {
                Id = data.ReminderId.ToString(),
                Flag = isSuccess ? 1 : 0,
            };

        }





        public IDictionary<string, object> GetReminder(ReminderSelectMapper data, int usernumber)
        {
            var strSql = @" SELECT  remindername,
                                    entityid
                                    isrepeat,
                                    repeattype,
                                    cronstring,
                                    recstatus,
                                    remark
                            FROM crm_sys_reminder
                            WHERE recid=@reminderid ";

            var param = new
            {
                reminderid = data.ReminderId
            };

            var resultList = DataBaseHelper.Query(strSql, param);
            IDictionary<string, object> resultSingle = new Dictionary<string, object>();
            if (resultList != null && resultList.Count > 0)
            {
                resultSingle = resultList[0];
            }

            return resultSingle;
        }



        public OperateResult SaveReminderRule(ReminderSaveRuleMapper data, RuleInsertMapper ruleData, int usernumber)
        {

            var executeSql = @"SELECT * FROM crm_func_reminder_rule_save(@reminderid,@rule,@ruleitem,@ruleset,@rulerelation,
                                                                         @hasperson,@ispersonfixed,@hasdepartment,@isdepartmentfixed,@receiver,
                                                                         @title,@content,@updatefield,@contentparam,@receiverrange,@userno)";

            var args = new
            {
                ReminderId = data.ReminderId,

                Rule = ruleData.Rule,
                RuleItem = ruleData.RuleItem,
                RuleSet = ruleData.RuleSet,
                RuleRelation = ruleData.RuleRelation,

                HasPerson = data.HasPerson,
                IsPersonFixed = data.IsPersonFixed,
                HasDepartment = data.HasDepartment,
                IsDepartmentFixed = data.IsDepartmentFixed,
                Receiver = data.Receiver,

                Title = data.Title,
                Content = data.Content,

                UpdateField = data.UpdateField,
                ContentParam = data.ContentParam,
                ReceiverRange = data.ReceiverRange,

                UserNo = usernumber
            };

            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }



        public List<ReminderRuleDetailMapper> GetReminderRule(ReminderSelectMapper body, int usernumber)
        {
            var executeSql = "select * from  crm_func_reminder_rule_select(@reminderid,@userno)";
            var args = new
            {
                reminderid = body.ReminderId,
                UserNo = usernumber
            };

            var dataResult = DataBaseHelper.QueryStoredProcCursor<ReminderRuleDetailMapper>(executeSql, args, CommandType.Text);
            return dataResult;
        }


        public OperateResult DisableReminder(ReminderDisableMapper data, int usernumber)
        {
            var executeSql = @"SELECT * FROM crm_func_reminder_disabled_save(@reminderid,@entityrecid,@receiverid,@reminderstatus,@userno)";
            var args = new
            {
                reminderid = data.ReminderId,
                entityrecid = data.EntityRecId,
                receiverid = usernumber,
                reminderstatus = data.ReminderStatus,
                UserNo = usernumber
            };

            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }


        //select* from 
        //select* from crm_sys_reminder_receiver_user
        //select* from crm_sys_reminder_receiver_department

        public List<ReminderRecieverUserMapper> GetReminderReceiverUser(ReminderSelectMapper body, int usernumber)
        {
            var executeSql = "select * from crm_sys_reminder_receiver_user where reminderid=@reminderid";
            var args = new
            {
                reminderid = body.ReminderId,
            };

            var dataResult = DataBaseHelper.Query<ReminderRecieverUserMapper>(executeSql, args, CommandType.Text);
            return dataResult;
        }


        public List<ReminderRecieverDepartmentMapper> GetReminderReceiverDepartment(ReminderSelectMapper body, int usernumber)
        {
            var executeSql = "select * from crm_sys_reminder_receiver_department where reminderid=@reminderid";
            var args = new
            {
                reminderid = body.ReminderId,
            };

            var dataResult = DataBaseHelper.Query<ReminderRecieverDepartmentMapper>(executeSql, args, CommandType.Text);
            return dataResult;
        }



        public List<ReminderRecycleRuleMapper> GetReminderRecycleRule(ReminderSelectMapper body, int usernumber)
        {
            var executeSql = "select * from  crm_sys_reminder_recycle_rule where reminderid=@reminderid";
            var args = new
            {
                reminderid = body.ReminderId,
            };

            var dataResult = DataBaseHelper.Query<ReminderRecycleRuleMapper>(executeSql, args, CommandType.Text);
            return dataResult;
        }


        public ReminderMapper GetReminderById(Guid id, int usernumber)
        {
            var executeSql = "select * from  crm_sys_reminder where recid=@reminderid";
            var args = new
            {
                reminderid = id,
            };

            return DataBaseHelper.Query<ReminderMapper>(executeSql, args, CommandType.Text).FirstOrDefault();

        }

		#endregion

		public List<ReminderMapper> GetAllReminder()
		{
			var executeSql = "select * from  crm_sys_reminder where recstatus = 1;";
			var args = new
			{
			};

			return DataBaseHelper.Query<ReminderMapper>(executeSql, args, CommandType.Text);

		}

		public List<IDictionary<string, object>> CallFunction(string functionName, string eventId, int userNumber)
		{
			var result = new List<IDictionary<string, object>>(); 
			var executeSql = $"select * from {functionName}(@eventid,@userno)";
				 
			var args = new
			{
				EventId = eventId,
				UserNo = userNumber
			}; 
			result = DataBaseHelper.QueryStoredProcCursor(executeSql, args, CommandType.Text);
				 
			return result;
		}

        public dynamic getSubscribeMsgList()
        {
            var sql = string.Format(@" 
			with Tmsg_reg as ( 
			select * from crm_plu_messagerecord 
			where msgstatus = 2
			), 
			TUser as (
				select DISTINCT u.userid,d.deptid,u.username from crm_sys_userinfo u
				inner join crm_sys_account_userinfo_relate r on u.userid = r.userid
				inner join crm_sys_account a on r.accountid = a.accountid
				inner join crm_sys_department d on d.deptid = r.deptid
				where u.recstatus = 1 and a.recstatus = 1 and d.recstatus = 1
			)

			select reg.*, 
            (relentity->>'id') entityid 
            from Tmsg_reg reg
			inner join TUser u on reg.msgreceiver::integer = u.userid;");

            var param = new DynamicParameters();

            var result = DataBaseHelper.Query(sql, param, CommandType.Text);
            return result;
        }

        public int UpdateSubscribeMsg(List<Guid> recIds)
        {
            var result = 0;

            if (recIds.Count > 0)
            {
                var updateSql = string.Format("update crm_plu_messagerecord set msgstatus = 1, msgsendtime = now() where recid in('{0}');", string.Join("','", recIds));
                var updateParam = new DynamicParameters();

                result = DataBaseHelper.ExecuteNonQuery(updateSql, updateParam, CommandType.Text);
            }

            return result;
        }

        public dynamic GetRecByEntityIdRecCode(Guid entityId, string recCode)
        {
            dynamic result = null;
            var selectSql = "select entitytable from crm_sys_entity where entityid = @entityId limit 1;";

            var selectParam = new DynamicParameters();
            selectParam.Add("entityId", entityId);
            var tableName = DataBaseHelper.ExecuteScalar<string>(selectSql, selectParam);
            if (!string.IsNullOrEmpty(tableName))
            {
                selectSql = string.Format("select * from  {0} where reccode = @recCode limit 1;", tableName);
                selectParam = new DynamicParameters();
                selectParam.Add("recCode", recCode);

                return DataBaseHelper.Query<dynamic>(selectSql, selectParam, CommandType.Text);
            }

            return result;
        }
    }
}



using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Account;
using UBeat.Crm.CoreApi.Services.Models.Department;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.Services.Models.Reminder;
using UBeat.Crm.CoreApi.DomainModel.Reminder;
using UBeat.Crm.CoreApi.Utility;
using UBeat.Crm.CoreApi.Services.Models.EntityPro;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DomainModel;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Authorize(ActiveAuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class ReminderController : BaseController
    {
        private readonly ReminderServices _service;
        private readonly RuleTranslatorServices _ruleService;

        private readonly string enterpriseNo;
		private readonly ScheduleTypeEnum isNeedSchedule;
		public ReminderController(ReminderServices service, RuleTranslatorServices ruleService) : base(service)
        {
            _service = service;
            _ruleService = ruleService;

            //获取作业调度的配置
            IConfigurationRoot config = ServiceLocator.Current.GetInstance<IConfigurationRoot>();
            var _config = config.GetSection("ScheduleSetting").Get<ScheduleSetting>();
            enterpriseNo = _config.EnterpriseNo;
			isNeedSchedule = _config.IsNeedSchedule;
		}


        /// <summary>
        /// 获取系统提醒设置列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("configlist")]
        public OutputResult<object> ConfigList()
        {
            return _service.GetRemainderSettingList(UserId);
        }

        /// <summary>
        /// 保存系统提醒设置列表
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>

        [HttpPost]
        [Route("saveconfig")]
        public OutputResult<object> SaveConfig([FromBody] ReminderSettingEditModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            return _service.UpdateRemainderSetting(body, UserId);
        }


        /// <summary>
        /// 保存系统提醒子项设置列表
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>

        [HttpPost]
        [Route("saveitemconfig")]
        public OutputResult<object> SaveItemConfig([FromBody] ReminderItemSettingEditModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            return _service.UpdateRemainderItemSetting(body);
        }


        /// <summary>
        ///  添加自定义提醒
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [Route("add")]
        [HttpPost]
        public OutputResult<object> AddCustomReminder([FromBody] ReminderEventAddModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            var result = _service.AddCustomReminder(body, UserId);
            if (result.Status == 0)
            {
				if(isNeedSchedule == ScheduleTypeEnum.ScheduleTypeNeed)
				{
					//新增成功了，注册调度服务
					var model = ScheduleServices.CreateCustomReminder(ScheduleServices.CustomerTipsJobName, body.Title,
						enterpriseNo, result.DataBody.ToString(), "");

					ScheduleServices.AddSchedule(model);
				} 
            }

            return result;
        }



        /// <summary>
        /// 编辑自定义提醒
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [Route("edit")]
        [HttpPost]
        public OutputResult<object> UpdateCustomReminder([FromBody] ReminderEventEditModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");

            return _service.UpdateCustomReminder(body, UserId);


        }


        /// <summary>
        /// 删除自定义提醒
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [Route("delete")]
        [HttpPost]
        public OutputResult<object> DeleteCustomReminder([FromBody] ReminderEventDeleteModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            if (body.EventIds == null) return ResponseError<object>("eventid不能为空");


            var result = _service.DeleteCustomReminder(body.EventIds, UserId);
            if (result.Status == 0)
            {
				if (isNeedSchedule == ScheduleTypeEnum.ScheduleTypeNeed)
				{
					var modelList = new List<TaskJobFullNameModel>();
					foreach (var eventid in body.EventIds)
					{
						var model = ScheduleServices.Creator(ScheduleServices.CustomerTipsJobName, enterpriseNo, eventid, UserId.ToString());
						modelList.Add(model);
					}
					ScheduleServices.DelScheduleWithFullName(modelList);
				} 
            }

            return result;
        }


        /// <summary>
        ///   启用、停用自定义提醒
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [Route("setstatus")]
        [HttpPost]
        public OutputResult<object> SetCustomReminderEnable([FromBody] ReminderEventSetStatussModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            if (body.EventIds == null) return ResponseError<object>("eventid不能为空");

            _service.SetCustomReminderEnable(body.EventIds, body.Status, UserId);

			if (isNeedSchedule == ScheduleTypeEnum.ScheduleTypeNeed)
			{
				var modelList = new List<TaskJobFullNameModel>();
				foreach (var eventid in body.EventIds)
				{
					var model = ScheduleServices.Creator(ScheduleServices.CustomerTipsJobName, enterpriseNo, eventid, UserId.ToString());
					modelList.Add(model);
				}

				// 如果status=1,是重启job,如果status=0,是暂停job
				if (body.Status == 0)
				{
					ScheduleServices.ResumeJobsWithFullName(modelList);
				}
				else
				{
					ScheduleServices.StopJobsWithFullName(modelList);
				}
			} 

            return new OutputResult<object>();
        }


        /// <summary>
        /// 立即执行job
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>

        [Route("activate")]
        [HttpPost]
        public OutputResult<object> ExecuteTaskNow([FromBody] ReminderEventActivateModel body)
        {

            if (body == null) return ResponseError<object>("参数格式错误");
            if (body.RemindId == null) return ResponseError<object>("RemindId 不能为空");

            return _service.ExecuteTaskNow(body, enterpriseNo, UserId);
        }


        /// <summary>
        /// 获取自定义提醒详情
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [Route("getsingle")]
        [HttpPost]
        public OutputResult<object> GetCustomReminderInfo([FromBody] ReminderEventGetModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            if (body.EventId == null) return ResponseError<object>("EventId 不能为空");
            return new OutputResult<object>(_service.CustomReminderInfo(body.EventId.ToString(), UserId));
        }



        /// <summary>
        /// 获取自定义提醒列表
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [Route("getlist")]
        [HttpPost]
        public OutputResult<object> GetCustomReminderList([FromBody] ReminderEventListModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            return _service.ReminderMessageList(body.PageIndex, body.PageSize, UserId);
        }


        #region  智能提醒

        //获取智能提醒列表
        [Route("listreminder")]
        [HttpPost]
        public OutputResult<object> GetReminderList([FromBody] ReminderListModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            return _service.GetReminderList(body, UserId);

        }


        //保存智能提醒，包括新增和编辑
        [HttpPost]
        [Route("savereminder")]
        public OutputResult<object> SaveReminder([FromBody] ReminderSaveModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");

            if (body.IsRepeat && string.IsNullOrEmpty(body.CronString))
            {
                return ResponseError<object>("重复频次不能为空");
            }

            return _service.SaveReminder(body, enterpriseNo, UserId);
        }


        //获取智能提醒详情
        [HttpPost]
        [Route("getreminder")]
        public OutputResult<object> GetReminder([FromBody] ReminderSelectModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            return _service.GetReminder(body, UserId);
        }


        //保存智能提醒规则, 包括新增和编辑
        [HttpPost]
        [Route("savereminderrule")]
        public OutputResult<object> SaveReminderRule([FromBody] ReminderSaveRuleModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");


            var ruleBody = new RuleSaveModel()
            {
                EntityId = body.EntityId,
                TypeId = body.TypeId,
                Id = body.Id,
                RoleId = body.RoleId,
                MenuName = body.MenuName,
                RuleId = body.RuleId,
                RuleName = body.RuleName,
                RelEntityId = body.RelEntityId,
                Rulesql = body.Rulesql,
                Rule = body.Rule,
                RuleItems = body.RuleItems,
                RuleSet = body.RuleSet
            };


            //获取处理后的规则
            var ruleMapper = _ruleService.GetRuleSaveMapper(ruleBody, UserId);

            return _service.SaveReminderRule(body, ruleMapper, UserId);
        }


        //获取智能提醒规则
        [HttpPost]
        [Route("getreminderrule")]
        public OutputResult<object> GetReminderRule([FromBody] ReminderSelectModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            return _service.GetReminderRule(body, UserId);
        }



        //不再次接收提醒
        [HttpPost]
        [Route("disablereminder")]
        public OutputResult<object> DisableReminder([FromBody] ReminderDisableModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            return _service.DisableReminder(body, UserId);
        }


		#endregion

		#region 提醒和回收
		[HttpPost]
		[Route("autoreminder")]
		public OutputResult<object> AutoReminder([FromBody] ReminderDisableModel body)
		{
			if (body == null) return ResponseError<object>("参数格式错误");
			return new OutputResult<object>(_service.AutoReminder());
		}
        #endregion

        #region 消息发送记录表
        [HttpPost]
        [Route("sendmsg")]
        public OutputResult<object> SendMsg([FromBody] object body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            return new OutputResult<object>(_service.SendMsg());
        }
        #endregion
    }
}












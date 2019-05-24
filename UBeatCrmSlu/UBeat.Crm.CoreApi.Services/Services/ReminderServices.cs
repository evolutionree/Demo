using DocumentFormat.OpenXml.VariantTypes;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Reminder;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Reminder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UBeat.Crm.CoreApi.Services.Models.Rule;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.Services.Utility;
using UBeat.Crm.CoreApi.Services.Models.Message;
using UBeat.Crm.CoreApi.DomainModel.Message;
using Microsoft.Extensions.Configuration;

namespace UBeat.Crm.CoreApi.Services.Services
{
	public class ReminderServices : BaseServices
	{
		IReminderRepository _repository;
		/// private readonly Dictionary<DocumentType, string> staticEntityIdDic = null;

		private readonly ScheduleTypeEnum isNeedSchedule;
		public ReminderServices(IReminderRepository repository)
		{
			_repository = repository;

			//获取作业调度的配置
			IConfigurationRoot config = ServiceLocator.Current.GetInstance<IConfigurationRoot>();
			var _config = config.GetSection("ScheduleSetting").Get<ScheduleSetting>();
			isNeedSchedule = _config.IsNeedSchedule;
		}
		 
		public OutputResult<object> GetRemainderSettingList(int userNumber)
		{
			List<IDictionary<string, object>> list = new List<IDictionary<string, object>>(_repository.GetReminderSettingList(userNumber));
			foreach (var item in list)
			{
				if (item["dictypeid"] != null)
				{
					//表示存在子选项
					int dictypeid = int.Parse(item["dictypeid"].ToString());
					List<IDictionary<string, object>> itemList = _repository.getReminderSettingItems(dictypeid);
					item.Add("items", itemList);
				}
			}
			return new OutputResult<object>(list);
		}

		public OutputResult<object> UpdateRemainderSetting(ReminderSettingEditModel body, int userNumber)
		{
			var data = new List<ReminerSettingInsert>();
			foreach (var item in body.ConfigList)
			{
				var crmData = new ReminerSettingInsert()
				{
					Id = item.Id,
					Name = item.Name,
					RecStatus = item.RecStatus,
					CheckDay = item.CheckDay,
					CronString = item.CronString,
					ConfigVal = item.ConfigVal
				};
				if (!crmData.IsValid())
				{
					return HandleValid(crmData);
				}
				data.Add(crmData);
			}

			_repository.UpdateDocumentFolder(data, userNumber);
			return new OutputResult<object>();
		}

		public OutputResult<object> UpdateRemainderItemSetting(ReminderItemSettingEditModel body)
		{
			var newList = new List<ReminderItemInsert>();
			foreach (var item in body.ConfigList)
			{
				var crmData = new ReminderItemInsert()
				{
					id = item.id,
					dicId = item.dicId,
					dicTypeId = item.dicTypeId.Value
				};
				if (!crmData.IsValid())
				{
					return HandleValid(crmData);
				}
				newList.Add(crmData);
			}

			_repository.UpdateReminderItem(newList, body.setTypeList);
			return new OutputResult<object>();
		}

		public OutputResult<object> AddCustomReminder(ReminderEventAddModel data, int userNumber)
		{
			var crmData = new ReminerEventInsert()
			{
				EntityId = data.EntityId.ToString(),
				EventName = data.EventName,
				Title = data.Title,
				CheckDay = data.CheckDay,
				SendTime = data.SendTime,
				Type = data.Rectype,
				ExpandFieldId = data.ExpandFieldId.ToString(),
				Params = data.Param,
				UserNumber = userNumber,
				Content = data.ReminderContent,
				UserColumn = data.UserColumn,
				RemindType = data.RemindType,
				TimeFormat = data.TimeFormat,
			};

			if (!crmData.IsValid())
			{
				return HandleValid(crmData);
			}

			return HandleResult(_repository.AddCustomReminder(crmData));

		}

		public OutputResult<object> UpdateCustomReminder(ReminderEventEditModel data, int userNumber)
		{
			var crmData = new ReminerEventUpdate()
			{
				EventId = data.EventId.ToString(),
				EventName = data.EventName,
				Title = data.Title,
				CheckDay = data.CheckDay,
				SendTime = data.SendTime,
				ExpandFieldId = data.ExpandFieldId.ToString(),
				Params = data.Param,
				UserNumber = userNumber,
				Content = data.ReminderContent,
				TimeFormat = data.TimeFormat,

			};

			if (!crmData.IsValid())
			{
				return HandleValid(crmData);
			}

			return HandleResult(_repository.UpdateCustomReminder(crmData));
		}

		public OutputResult<object> DeleteCustomReminder(List<string> eventids, int usernumber)
		{
			return HandleResult(_repository.DeleteCustomReminder(eventids, usernumber));
		}


		public void SetCustomReminderEnable(List<string> eventids, int status, int usernumber)
		{
			_repository.SetCustomReminderEnable(eventids, status, usernumber);

		}

		public List<IDictionary<string, object>> CustomReminderInfo(string eventid, int usernumber)
		{

			return _repository.CustomReminderInfo(eventid, usernumber);
		}

		public OutputResult<object> ReminderMessageList(int pageIndex, int pageSize, int usernumber)
		{
			var pageParam = new PageParam { PageIndex = pageIndex, PageSize = pageSize };
			if (!pageParam.IsValid())
			{
				return HandleValid(pageParam);
			}

			return new OutputResult<object>(_repository.ReminderMessageList(pageIndex, pageSize, usernumber));
		}

		public OutputResult<object> GetReminderList(ReminderListModel body, int usernumber)
		{
			var crmData = new ReminderListMapper()
			{
				ReminderName = body.ReminderName,
				RecStatus = body.RecStatus,
				RecType = body.RecType
			};

			PageParam pageData = new PageParam()
			{
				PageIndex = body.PageIndex,
				PageSize = body.PageSize
			};

			return new OutputResult<object>(_repository.GetReminderList(pageData, crmData, usernumber));

		}

		public OutputResult<object> SaveReminder_old(ReminderSaveModel body, string enterpriseNo, int usernumber)
		{
			var crmData = new ReminderSaveMapper()
			{
				ReminderId = body.ReminderId,
				RecType = body.RecType,
				ReminderName = body.ReminderName,
				EndityId = body.EntityId,
				IsRepeat = body.IsRepeat,
				RecStatus = body.RecStatus,
				RepeatType = body.RepeatType,
				CronString = body.CronString,
				Remark = body.Remark
			};


			//读取更新之前的数据
			ReminderMapper oldData = new ReminderMapper();
			if (body.ReminderId.HasValue)
			{
				oldData = _repository.GetReminderById(body.ReminderId.Value, usernumber);
			}
			else
			{
				oldData.RecVersion = 0;
			}

			//写数据库
			var result = _repository.SaveReminder(crmData, usernumber);

			var newData = _repository.GetReminderById(Guid.Parse(result.Id), usernumber);
			var newVersion = newData.RecVersion;
			enterpriseNo = newData.RecVersion.ToString();

			//写数据库成功,然后操作schedule
			if (result.Flag == 1)
			{
				if(isNeedSchedule == ScheduleTypeEnum.ScheduleTypeNeed)
				{
					//拼接cron字符串
					string cronString = GetCronString(body);

					//智能提醒
					if (body.RecType == 0)
					{
						if (body.IsRepeat)
						{
							if (body.ReminderId == null)
							{
								if (body.RecStatus == 1)
								{
									//新增成功了，注册调度服务
									var model = ScheduleServices.CreateCustomReminder(ScheduleServices.CustomerTipsJobName, body.ReminderName, newData.RecVersion.ToString(), result.Id, cronString);
									ScheduleServices.AddSchedule(model);
								}
							}
							else
							{
								//获取旧数据,用来比较是否数据有改变
								if (oldData.RecStatus == body.RecStatus && oldData.IsRepeat == body.IsRepeat && oldData.RepeatType == body.RepeatType && body.CronString == oldData.CronString)
								{
									//数据没有改变，只更新数据库数据，不需要更新schedule
								}
								else
								{
									var modelList = new List<TaskJobFullNameModel>();
									var oldVersion = oldData.RecVersion;
									//var model = ScheduleServices.Creator(ScheduleServices.CustomerTipsJobName, enterpriseNo, body.ReminderId.ToString(), usernumber.ToString());
									var model = ScheduleServices.Creator(ScheduleServices.CustomerTipsJobName, oldVersion.ToString(), body.ReminderId.ToString(), usernumber.ToString());
									modelList.Add(model);

									//请求数据是启用状态
									if (body.RecStatus == 1)
									{
										if (oldData.IsRepeat == body.IsRepeat && oldData.RepeatType == body.RepeatType && body.CronString == oldData.CronString)
										{
											// 重启 reminder
											ScheduleServices.ResumeJobsWithFullName(modelList);
										}
										else
										{
											// 删除 old reminder
											var removeResult = ScheduleServices.DelScheduleWithFullName(modelList);

											if (removeResult.IsSucc)
											{
												// 新增 reminder
												//var newModel = ScheduleServices.CreateCustomReminder(ScheduleServices.CustomerTipsJobName, body.ReminderName, enterpriseNo, result.Id, cronString);
												var newModel = ScheduleServices.CreateCustomReminder(ScheduleServices.CustomerTipsJobName, body.ReminderName, newVersion.ToString(), result.Id, cronString);
												ScheduleServices.AddSchedule(newModel);
											}
										}
									}
									else
									{
										//老数据是启用状态
										if (oldData.RecStatus == 1)
										{
											// 暂停reminder
											ScheduleServices.StopJobsWithFullName(modelList);
										}
									}
								}
							}
						}
						else
						{
							var modelList = new List<TaskJobFullNameModel>();
							var model = ScheduleServices.Creator(ScheduleServices.CustomerTipsJobName, oldData.RecVersion.ToString(), body.ReminderId.ToString(), usernumber.ToString());
							modelList.Add(model);

							//如果启用
							if (body.RecStatus == 1)
							{
								//修改老数据
								if (body.ReminderId.HasValue)
								{
									//时间改变了
									if (oldData.CronString != newData.CronString)
									{
										// 删除 old reminder
										ScheduleServices.DelScheduleWithFullName(modelList);

										// 新增 reminder
										var newModel = ScheduleServices.CreateCustomReminder(ScheduleServices.CustomerTipsJobName, body.ReminderName, newData.RecVersion.ToString(), result.Id, cronString);
										ScheduleServices.AddSchedule(newModel);

									}
									else
									{
										//如果老数据是暂停状态
										if (oldData.RecStatus == 0)
										{

											// 重启 reminder
											ScheduleServices.ResumeJobsWithFullName(modelList);

										}
									}
								}
								else
								{
									// 新增 reminder
									var newModel = ScheduleServices.CreateCustomReminder(ScheduleServices.CustomerTipsJobName, body.ReminderName, newData.RecVersion.ToString(), result.Id, cronString);
									ScheduleServices.AddSchedule(newModel);
								}


							}
							else
							{
								//不启用
								//老数据是启用状态
								if (body.ReminderId.HasValue && oldData.RecStatus == 1)
								{
									// 暂停reminder
									ScheduleServices.StopJobsWithFullName(modelList);
								}
							}
						}

					}
					else //处理回收机制的执行逻辑
					{

						var modelList = new List<TaskJobFullNameModel>();
						var model = ScheduleServices.Creator(ScheduleServices.CustomerTipsJobName, oldData.RecVersion.ToString(), body.ReminderId.ToString(), usernumber.ToString());
						modelList.Add(model);

						//回收机制
						//启用状态
						if (body.RecStatus == 1)
						{
							// 删除 old reminder
							ScheduleServices.DelScheduleWithFullName(modelList);

							// 新增 reminder
							// 每天早晨 8点 执行一次
							if (string.IsNullOrEmpty(cronString))
							{
								cronString = "0 0 8 * * ?";
							}
							// string cronString = "0 0 8 * * ?";
							//前端要穿类型为1，然后再传时分秒
							var newModel = ScheduleServices.CreateCustomReminder(ScheduleServices.CustomerTipsJobName, body.ReminderName, newData.RecVersion.ToString(), result.Id, cronString);
							ScheduleServices.AddSchedule(newModel);
						}
						else
						{
							//老数据是启用状态，新数据是停用状态
							if (oldData.RecStatus == 1)
							{
								// 暂停reminder
								ScheduleServices.StopJobsWithFullName(modelList);

							}
						}
					}
				}  
			}

			return new OutputResult<object>(result);

		}

		private enum ReminderStatus
		{

			Disable = 0,
			Enable = 1
		}

		public OutputResult<object> SaveReminder_new(ReminderSaveModel body, string enterpriseNo, int usernumber)
		{
			var crmData = new ReminderSaveMapper()
			{
				ReminderId = body.ReminderId,
				RecType = body.RecType,
				ReminderName = body.ReminderName,
				EndityId = body.EntityId,
				IsRepeat = body.IsRepeat,
				RecStatus = body.RecStatus,
				RepeatType = body.RepeatType,
				CronString = body.CronString,
				Remark = body.Remark
			};


			//读取更新之前的数据
			ReminderMapper oldData = new ReminderMapper();
			if (body.ReminderId.HasValue)
			{
				oldData = _repository.GetReminderById(body.ReminderId.Value, usernumber);
			}

			//写数据库
			var result = _repository.SaveReminder(crmData, usernumber);
			var newData = _repository.GetReminderById(Guid.Parse(result.Id), usernumber);

			//保存数据成功
			//因为每一次更新,job name 都会发生改变,所以只能删除，新增，不能暂停或者重启
			//TODO:需要修改schedule的API
			if (result.Flag == 1)
			{
				if(isNeedSchedule == ScheduleTypeEnum.ScheduleTypeNeed)
				{
					//拼接cron字符串
					string cronString = GetCronString(body);

					var modelList = new List<TaskJobFullNameModel>();
					var model = ScheduleServices.Creator(ScheduleServices.CustomerTipsJobName, enterpriseNo, body.ReminderId.ToString(), usernumber.ToString());
					modelList.Add(model);

					//编辑数据
					if (body.ReminderId.HasValue)
					{
						//数据发生变化
						if (!(oldData.RecStatus == body.RecStatus && body.CronString == oldData.CronString && oldData.IsRepeat == body.IsRepeat))
						{
							//数据有变化,是启用状态
							if (body.RecStatus == (int)ReminderStatus.Enable)
							{
								//数据发生了变化

								// 删除老数据 reminder
								var deleteResult = ScheduleServices.DeleteJob(model);


								// 新增 reminder
								var newModel = ScheduleServices.CreateCustomReminder(ScheduleServices.CustomerTipsJobName, body.ReminderName, enterpriseNo, result.Id, cronString);
								ScheduleServices.AddSchedule(newModel);


							}
							else
							{
								//数据是禁用状态
								if (oldData.RecStatus == (int)ReminderStatus.Enable)
								{
									// 删除老数据 reminder
									ScheduleServices.DelScheduleWithFullName(modelList);
								}
							}
						}
					}
					else
					{
						//新增数据,如果数据是启用状体
						if (body.RecStatus == (int)ReminderStatus.Enable)
						{
							//如果启用,调用schedule add
							var newModel = ScheduleServices.CreateCustomReminder(ScheduleServices.CustomerTipsJobName, body.ReminderName, enterpriseNo, result.Id, cronString);
							ScheduleServices.AddSchedule(newModel);
						}
					}
				} 
			}

			return new OutputResult<object>(result);
		}

		public OutputResult<object> SaveReminder(ReminderSaveModel body, string enterpriseNo, int usernumber)
		{
			string reminderName = body.ReminderName;
			string tmp = MultiLanguageUtils.GetDefaultLanguageValue(body.ReminderName_Lang);
			if (tmp != null) reminderName = tmp;
			var crmData = new ReminderSaveMapper()
			{
				ReminderId = body.ReminderId,
				RecType = body.RecType,
				ReminderName = reminderName,
				EndityId = body.EntityId,
				IsRepeat = body.IsRepeat,
				RecStatus = body.RecStatus,
				RepeatType = body.RepeatType,
				CronString = body.CronString,
				Remark = body.Remark,
				ReminderName_Lang = body.ReminderName_Lang
			};


			//读取更新之前的数据
			ReminderMapper oldData = new ReminderMapper();
			if (body.ReminderId.HasValue)
			{
				oldData = _repository.GetReminderById(body.ReminderId.Value, usernumber);
			}

			//写数据库
			var result = _repository.SaveReminder(crmData, usernumber);
			var newData = _repository.GetReminderById(Guid.Parse(result.Id), usernumber);

			//保存数据成功
			//因为每一次更新,job name 都会发生改变,所以只能删除，新增，不能暂停或者重启
			//TODO:需要修改schedule的API
			if (result.Flag == 1)
			{
				if(isNeedSchedule == ScheduleTypeEnum.ScheduleTypeNeed)
				{
					//拼接cron字符串
					string cronString = GetCronString(body);

					var modelList = new List<TaskJobFullNameModel>();
					var model = ScheduleServices.Creator(ScheduleServices.CustomerTipsJobName, oldData.RecVersion.ToString(), body.ReminderId.ToString(), usernumber.ToString());
					modelList.Add(model);

					//编辑数据
					if (body.ReminderId.HasValue)
					{
						//数据发生变化
						if (!(oldData.RecStatus == body.RecStatus && body.CronString == oldData.CronString && oldData.IsRepeat == body.IsRepeat))
						{
							//数据有变化,是启用状态
							if (body.RecStatus == (int)ReminderStatus.Enable)
							{
								//数据发生了变化

								// 删除老数据 reminder
								ScheduleServices.DelScheduleWithFullName(modelList);

								// 新增 reminder
								var newModel = ScheduleServices.CreateCustomReminder(ScheduleServices.CustomerTipsJobName, body.ReminderName, newData.RecVersion.ToString(), result.Id, cronString);
								ScheduleServices.AddSchedule(newModel);


							}
							else
							{
								//数据是禁用状态
								if (oldData.RecStatus == (int)ReminderStatus.Enable)
								{
									// 删除老数据 reminder
									ScheduleServices.DelScheduleWithFullName(modelList);
								}
							}
						}
					}
					else
					{
						//新增数据,如果数据是启用状体
						if (body.RecStatus == (int)ReminderStatus.Enable)
						{
							//如果启用,调用schedule add
							var newModel = ScheduleServices.CreateCustomReminder(ScheduleServices.CustomerTipsJobName, body.ReminderName, newData.RecVersion.ToString(), result.Id, cronString);
							ScheduleServices.AddSchedule(newModel);
						}
					}
				} 
			}

			return new OutputResult<object>(result);
		}
		public OutputResult<object> GetReminder(ReminderSelectModel body, int usernumber)
		{

			var crmData = new ReminderSelectMapper()
			{
				ReminderId = body.ReminderId
			};

			return new OutputResult<object>(_repository.GetReminder(crmData, usernumber));
		}

		public enum ReceiverType
		{
			/// <summary>
			/// 固定的人
			/// </summary>
			FixedPerson = 0,

			/// <summary>
			/// 表单中人
			/// </summary>
			FieldPerson = 1,

			/// <summary>
			/// 固定部门
			/// </summary>
			FixedDepartment = 2,

			/// <summary>
			/// 表单中部门
			/// </summary>
			FieldDepartment = 3,
		}

		public OutputResult<object> SaveReminderRule(ReminderSaveRuleModel body, RuleInsertMapper ruleMapper, int usernumber)
		{

			bool hasPerson = false;
			bool hasDepartment = false;
			bool isFixedPerson = false;
			bool isFixedDepartment = false;
			if (body.Receiver != null && body.Receiver.Count > 0)
			{

				//是否选择的了人
				int fixedPerson = body.Receiver.Where(x => x.ItemType == (int)ReceiverType.FixedPerson).Count();
				int personField = body.Receiver.Where(x => x.ItemType == (int)ReceiverType.FieldPerson).Count();
				if (fixedPerson > 0 || personField > 0)
				{
					hasPerson = true;
				}

				//是否是固定的人
				if (fixedPerson > 0)
				{
					isFixedPerson = true;
				}


				//是否选择了部门
				int fixedDepartment = body.Receiver.Where(x => x.ItemType == (int)ReceiverType.FixedDepartment).Count();
				int DepartmentField = body.Receiver.Where(x => x.ItemType == (int)ReceiverType.FieldDepartment).Count();
				if (fixedDepartment > 0 || DepartmentField > 0)
				{
					hasDepartment = true;
				}

				//是否是固定的部门
				if (fixedDepartment > 0)
				{
					isFixedDepartment = true;
				}

			}

			//设置json序列化为小写
			var serializerSettings = new JsonSerializerSettings()
			{
				ContractResolver = new LowercaseContractResolver()
			};


			//序列化回收规则
			string updataField = string.Empty;
			if (body.UpdateField != null && body.UpdateField.Count() > 0)
			{
				updataField = JsonConvert.SerializeObject(body.UpdateField, serializerSettings);
			}



			var crmData = new ReminderSaveRuleMapper()
			{
				HasPerson = hasPerson,
				IsPersonFixed = isFixedPerson,
				HasDepartment = hasDepartment,
				IsDepartmentFixed = isFixedDepartment,

				ReminderId = body.ReminderId,
				Title = body.Title,
				Content = body.Content,
				Receiver = JsonConvert.SerializeObject(body.Receiver, serializerSettings),
				UpdateField = updataField,
				ContentParam = body.ContentParam,
				ReceiverRange = body.ReceiverRange
			};

			return new OutputResult<object>(_repository.SaveReminderRule(crmData, ruleMapper, usernumber));
		}

		public OutputResult<object> GetReminderRule(ReminderSelectModel body, int usernumber)
		{
			var crmData = new ReminderSelectMapper()
			{
				ReminderId = body.ReminderId
			};


			var _receiverUser = _repository.GetReminderReceiverUser(crmData, usernumber);
			var _receiverDepartment = _repository.GetReminderReceiverDepartment(crmData, usernumber);
			var _recycleRule = _repository.GetReminderRecycleRule(crmData, usernumber);


			var receiver = new List<ReminderReceiverModel>();
			if (_receiverUser != null && _receiverUser.Count() > 0)
			{
				foreach (var item in _receiverUser)
				{
					if (item.IsEntityField)
					{
						receiver.Add(new ReminderReceiverModel()
						{
							ItemType = (int)ReceiverType.FieldPerson,
							UserField = item.FieldId
						});

					}
					else
					{
						receiver.Add(new ReminderReceiverModel()
						{
							ItemType = (int)ReceiverType.FixedPerson,
							UserId = item.UserId
						});
					}
				}
			}


			if (_receiverDepartment != null && _receiverDepartment.Count() > 0)
			{
				foreach (var item in _receiverDepartment)
				{

					if (item.IsEntityField)
					{
						receiver.Add(new ReminderReceiverModel()
						{
							ItemType = (int)ReceiverType.FieldDepartment,
							DepartmentField = item.FieldId,
						});
					}
					else
					{
						receiver.Add(new ReminderReceiverModel()
						{
							ItemType = (int)ReceiverType.FixedDepartment,
							DepartmentId = item.DepartmentId,
						});
					}
				}
			}

			var updateField = new List<ReminderRecycleRuleModel>();
			if (_recycleRule != null && _recycleRule.Count() > 0)
			{

				foreach (var item in _recycleRule)
				{
					updateField.Add(new ReminderRecycleRuleModel()
					{
						FieldId = item.FieldId,
						FieldValue = item.FieldValue

					});
				}
			}

			var infoList = _repository.GetReminderRule(crmData, usernumber);
			var _entityId = infoList.FirstOrDefault().EntityId;
			var _hasPerson = infoList.FirstOrDefault().HasPerson;
			var _isPersonFixed = infoList.FirstOrDefault().IsPersonFixed;
			var _hasDepartment = infoList.FirstOrDefault().HasDepartment;
			var _isDepartmentFixed = infoList.FirstOrDefault().IsDepartmentFixed;
			var _receiverRange = infoList.FirstOrDefault().ReceiverRange;


			var _title = infoList.FirstOrDefault().TemplateTitle;
			var _content = "##" + infoList.FirstOrDefault().TemplateContent + "##";
			var _contentParam = infoList.FirstOrDefault().ContentParam;


			if (_entityId == Guid.Empty.ToString())
			{
				_entityId = null;
			}

			var result = infoList.GroupBy(t => new
			{
				t.RuleId,
				t.RuleName,
				t.RuleSet
			}).Select(group => new ReminderDetailModel
			{
				EntityId = _entityId,
				RuleId = group.Key.RuleId,
				RuleName = group.Key.RuleName,
				RuleItems = group.Select(t => new RuleItemInfoModel
				{
					ItemId = t.ItemId,
					ItemName = t.ItemName,
					FieldId = t.FieldId,
					Operate = t.Operate,
					UseType = t.UseType,
					RuleData = t.RuleData,
					RuleType = t.RuleType,
					EntityId = t.EntityId,

				}).ToList(),
				RuleSet = new RuleSetInfoModel
				{
					RuleSet = group.Key.RuleSet
				},
				HasPerson = _hasPerson,
				IsPersonFixed = _isPersonFixed,
				HasDepartment = _hasDepartment,
				IsDepartmentFixed = _isDepartmentFixed,
				Receiver = receiver,
				UpdateField = updateField,
				Title = _title,
				Content = _content,
				ContentParam = _contentParam,
				ReceiverRange = _receiverRange,


			}).ToList().FirstOrDefault();


			return new OutputResult<object>(result);
		}

		public OutputResult<object> DisableReminder(ReminderDisableModel body, int usernumber)
		{
			var crmData = new ReminderDisableMapper()
			{
				ReminderId = body.ReminderId,
				EntityRecId = body.EntityRecId,
				ReminderStatus = body.ReminderStatus
			};

			return new OutputResult<object>(_repository.DisableReminder(crmData, usernumber));
		}

		public bool AutoReminder()
		{
			//1.获取提醒列表
			//2.解析提醒规则
			//3.调用提醒方法获取待提醒的数据列表
			//4.推送消息 
			var userNumber = 1; 
			List<ReminderMapper> list = _repository.GetAllReminder();
			DateTime now = DateTime.Now.AddMinutes(-1);
			Logger.Info(string.Concat("AutoReminder->now:", now));
			foreach (var reminder in list)
			{
				ReminderSaveModel model = new ReminderSaveModel();
				model.IsRepeat = reminder.IsRepeat;
				model.RepeatType = reminder.RepeatType;
				model.CronString = reminder.CronString;
				model.CronString = this.GetCronString(model);

				if (TriggerCronbCheckUtils.Match(now, model.CronString))
				{
					reminderMsg(userNumber, reminder);
				}
			}

			return true;
		}

		private void reminderMsg(int userNumber, ReminderMapper reminder)
		{
			var dataList = _repository.CallFunction(reminder.FunctionName, reminder.RecId.ToString(), userNumber);
			if (dataList != null && dataList.Count > 0)
			{
				var ListDic = new Dictionary<string, List<IDictionary<string, object>>>();
				for (int i = 0; i < dataList.Count; i++)
				{
					var recId = string.Concat(dataList[i]["recid"]);
					if (!ListDic.ContainsKey(recId))
						ListDic.Add(recId, new List<IDictionary<string, object>>() { dataList[i] });
					else
						ListDic[recId].Add(dataList[i]);
				}

				foreach (var item in ListDic)
				{
					var dataItem = item.Value.FirstOrDefault();
					if (dataItem != null)
					{
						var receiverIntList = new List<int>();
						foreach (var recItem in item.Value)
						{
							var rec = Convert.ToInt32(string.Concat(recItem["receiver"]));
							receiverIntList.Add(rec);
						}
						var receiveDic = new Dictionary<MessageUserType, List<int>>();
						if (receiverIntList.Count > 0)
							receiveDic.Add(MessageUserType.SpecificUser, receiverIntList);

						if (receiveDic.Count > 0)
						{
							var templateKeyValue = new Dictionary<string, string>();
							templateKeyValue.Add("title", string.Concat(dataItem["title"]));
							templateKeyValue.Add("content", string.Concat(dataItem["content"]));
							templateKeyValue.Add("pushcontent", string.Concat(dataItem["pushcontent"]));

							var relEntityId = Guid.Empty;
							Guid.TryParse(string.Concat(dataItem["relentityid"]), out relEntityId);

							var relBusinessId = Guid.Empty;
							Guid.TryParse(string.Concat(dataItem["relrecid"]), out relBusinessId);

							var data = new MessageParameter
							{
								FuncCode = string.Concat(dataItem["funcode"]),
								EntityId = Guid.Parse(string.Concat(dataItem["entityid"])),
								TypeId = Guid.Parse(string.Concat(dataItem["typeid"])),
								BusinessId = Guid.Parse(string.Concat(dataItem["recid"])),
								RelEntityId = relEntityId,
								RelBusinessId = relBusinessId,
								ParamData = string.Concat(dataItem["msgparam"]),
								TemplateKeyValue = templateKeyValue,
								Receivers = receiveDic,
								CopyUsers = null
							};

							MessageService.WriteMessageAsyn(data, userNumber);
						}
					}
				}
			}
		}

		public string GetCronString_old(ReminderSaveModel body)
		{

			string cronString = string.Empty;
			if (body.IsRepeat)
			{
				//05:03:02

				string hour = string.Empty;
				string minute = string.Empty;
				string second = string.Empty;

				string day = string.Empty;
				string month = string.Empty;

				string[] arrayFull = new string[2];

				if (!string.IsNullOrEmpty(body.CronString))
				{
					arrayFull = body.CronString.Split(',');

					string[] arrayTime = new string[3];
					arrayTime = arrayFull[1].Split(':');

					if (arrayTime.Length == 3)
					{
						hour = arrayTime[0].TrimStart('0');
						minute = arrayTime[1].TrimStart('0');
						second = arrayTime[2].TrimStart('0');
					}
				}

				//日 0， 周 1， 月 2 ，年 3 ，自定义 4
				switch (body.RepeatType)
				{
					case 0: // 05:03:02
						cronString = string.Format("{0} {1} {2} * * ?", second, minute, hour);
						break;
					case 1:// 1,05:03:02
						string week = arrayFull[0];
						cronString = string.Format("{0} {1} {2} ? * {3}", second, minute, hour, week);
						break;
					case 2://28,05:03:02
						day = arrayFull[0];
						cronString = string.Format("{0} {1} {2} {3} * ?", second, minute, hour, day);
						break;
					case 3://2017-8-31,05:03:02
						string[] arrayData = arrayFull[0].Split('-');
						if (arrayData.Length == 3)
						{
							month = arrayData[1].Trim('0');
							day = arrayData[2].Trim('0');
						}
						cronString = string.Format("{0} {1} {2} {3} {4} ?", second, minute, hour, day, month);
						break;

					default:
						break;
				}
			}
			else
			{

				//如果没有选择重复频次，就立即执行
				var _now = DateTime.Now;
				_now = _now.AddDays(1);
				var _year = _now.Year;
				var _month = _now.Month;
				var _day = _now.Day;
				cronString = string.Format("0 0 4 {0} {1} ? {2}", _day, _month, _year); //第二天凌晨4点
			}


			return cronString;

		}

		public string GetCronString(ReminderSaveModel body)
		{

			string cronString = string.Empty;

			//05:03:02

			string hour = string.Empty;
			string minute = string.Empty;
			string second = string.Empty;

			string day = string.Empty;
			string month = string.Empty;
			string year = string.Empty;

			string[] arrayFull = new string[2];

			if (!string.IsNullOrEmpty(body.CronString))
			{
				arrayFull = body.CronString.Split(',');

				string[] arrayTime = new string[3];
				if (arrayFull.Length > 1)
				{
					arrayTime = arrayFull[1].Split(':');
				}
				else
				{
					arrayTime = arrayFull[0].Split(':');
				}

				if (arrayTime.Length == 3)
				{
					hour = arrayTime[0].TrimStart('0');
					minute = arrayTime[1].TrimStart('0');
					second = arrayTime[2].TrimStart('0');
				}

				if (string.IsNullOrEmpty(hour))
				{
					hour = "0";
				}

				if (string.IsNullOrEmpty(minute))
				{
					minute = "0";
				}

				if (string.IsNullOrEmpty(second))
				{
					second = "0";
				}

				//有年月日数据
				if (!string.IsNullOrEmpty(arrayFull[0]) && arrayFull[0].Contains('-'))
				{

					string[] arrayData = arrayFull[0].Split('-');
					if (arrayData.Length == 3)
					{
						year = arrayData[0].TrimStart('0');
						month = arrayData[1].Trim('0');
						day = arrayData[2].Trim('0');
					}
				}



				if (body.IsRepeat)
				{
					//日 0， 周 1， 月 2 ，年 3 ，自定义 4
					switch (body.RepeatType)
					{
						case 0: // 05:03:02  0 42 10 * * ?
							cronString = string.Format(" {0} {1} {2} * * ?", second, minute, hour);
							break;
						case 1:// 1,05:03:02   0 5 11 ? * 6
							string week = arrayFull[0];
							int weekInt = int.Parse(week);
							weekInt = weekInt + 1;
							cronString = string.Format("{0} {1} {2} ? * {3}", second, minute, hour, weekInt);
							break;
						case 2://28,05:03:02     //0 10 11 1 * ?
							day = arrayFull[0];
							cronString = string.Format("{0} {1} {2} {3} * ?", second, minute, hour, day);
							break;
						case 3://2017-8-31,05:03:02
							cronString = string.Format("{0} {1} {2} {3} {4} ?", second, minute, hour, day, month);
							break;

						default:
							break;
					}
				}
				else
				{
					cronString = string.Format("{0} {1} {2} {3} {4} ? {5}", second, minute, hour, day, month, year);
				}
			}
			else
			{

				if (body.RecType == 0)
				{
					//前段数据为空,第二天凌晨4点
					var _now = DateTime.Now;
					_now = _now.AddDays(1);
					var _year = _now.Year;
					var _month = _now.Month;
					var _day = _now.Day;
					cronString = string.Format("0 0 8 {0} {1} ? {2}", _day, _month, _year);
				}
				else
				{

					cronString = "0 0 8 * * ?";
				}

			}


			return cronString;

		}

		public string GetCronString(bool isRepeat, int repeatType)
		{
			string cronString = string.Empty;
			if (isRepeat)
			{
				//日 0， 周 1， 月 2 ，年 3 ，自定义 4
				switch (repeatType)
				{
					case 0:
						cronString = "0 0 4 * * ?"; //每天凌晨4点
						break;
					case 1:
						cronString = "0 0 4 ? * 1"; //每周一凌晨4点
						break;
					case 2:
						cronString = "0 0 4 1 * ?"; //每月1号凌晨4点
						break;
					case 3:
						cronString = "0 0 4 1 1 ?"; //每年1月1号凌晨4点
						break;
					default:
						break;
				}
			}
			else
			{
				var _now = DateTime.Now;
				_now = _now.AddDays(1);
				var _year = _now.Year;
				var _month = _now.Month;
				var _day = _now.Day;
				cronString = string.Format("0 0 4 {0} {1} ? {2}", _day, _month, _year); //第二天凌晨4点
			}
			return cronString;

		}

		public OutputResult<object> ExecuteTaskNow(ReminderEventActivateModel body, string enterpriseNo, int usernumber)
		{
			bool isSuccess = false;

			ReminderMapper _reminder = _repository.GetReminderById(Guid.Parse(body.RemindId), usernumber);
			if (_reminder != null)
			{
				if(_reminder.RecStatus == 0)
					return new OutputResult<object>(null, "停用的数据不能进行立即提醒操作", 1);

				if (isNeedSchedule == ScheduleTypeEnum.ScheduleTypeNeed)
				{
					var model = ScheduleServices.Creator(ScheduleServices.CustomerTipsJobName, _reminder.RecVersion.ToString(), body.RemindId, usernumber.ToString());
					var result = ScheduleServices.ExecuteNow(model);

					if (result.IsSucc)
					{

						isSuccess = true;
					}
					else
					{
						isSuccess = false;
					}
				}
				else
				{ 
					reminderMsg(usernumber, _reminder);
					isSuccess = true;
				}
			}
			 
			if (isSuccess)
			{
				return new OutputResult<object>(null, null, 0);
			}
			else
			{
				return new OutputResult<object>(null, null, 1);
			}

		}

	}
}

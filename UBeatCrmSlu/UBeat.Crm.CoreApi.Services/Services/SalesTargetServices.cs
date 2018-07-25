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
using UBeat.Crm.CoreApi.DomainModel.SalesTarget;
using UBeat.Crm.CoreApi.Services.Models.SalesTarget;
using UBeat.Crm.CoreApi.IRepository;
using AutoMapper;
using UBeat.Crm.CoreApi.Services.Models.Rule;
using UBeat.Crm.CoreApi.DomainMapper.Rule;
using UBeat.Crm.CoreApi.Services.Models.Excels;
using UBeat.Crm.CoreApi.Services.Utility.ExcelUtility;
using Microsoft.Extensions.Caching.Memory;
using UBeat.Crm.CoreApi.Core.Utility;

namespace UBeat.Crm.CoreApi.Services.Services
{
	public class SalesTargetServices : BasicBaseServices
	{
		private IMemoryCache _cache;
		private string taskDataId_prefix = "exceltask_";
		private ISalesTargetRepository _repository;
		private ExcelServices _excelServices;
		public SalesTargetServices(ISalesTargetRepository repository, IMemoryCache memoryCache, ExcelServices excelServices)
		{
			_repository = repository;
			_cache = memoryCache;
			_excelServices = excelServices;
		}


		/// <summary>
		/// 保存销售目标
		/// </summary>
		/// <param name="body"></param>
		/// <param name="userNumber"></param>
		/// <returns></returns>
		public OutputResult<object> AddSalesTarget(SalesTargetSaveModel body, int userNumber)
		{
			var crmData = new SalesTargetInsertMapper()
			{
				Year = body.Year,
				JanCount = body.JanCount,
				FebCount = body.FebCount,
				MarCount = body.MarCount,
				AprCount = body.AprCount,
				MayCount = body.MayCount,
				JunCount = body.JunCount,
				JulCount = body.JulCount,
				AugCount = body.AugCount,
				SepCount = body.SepCount,
				OctCount = body.OctCount,
				NovCount = body.NovCount,
				DecCount = body.DecCount,
				UserId = body.UserId,
				DepartmentId = body.DepartmentId,
				IsGroupTarget = body.IsGroupTarget,
				NormTypeId = body.NormTypeId
			};


			if (!crmData.IsValid())
			{
				return HandleValid(crmData);
			}

			return HandleResult(_repository.InsertSalesTarget(crmData, userNumber));
		}




		/// <summary>
		/// 编辑销售目标
		/// </summary>
		/// <param name="body"></param>
		/// <param name="userNumber"></param>
		/// <returns></returns>
		public OutputResult<object> EditSalesTarget(SalesTargetSaveModel body, int userNumber)
		{
			var crmData = new SalesTargetEditMapper()
			{
				JanCount = body.JanCount,
				FebCount = body.FebCount,
				MarCount = body.MarCount,
				AprCount = body.AprCount,
				MayCount = body.MayCount,
				JunCount = body.JunCount,
				JulCount = body.JulCount,
				AugCount = body.AugCount,
				SepCount = body.SepCount,
				OctCount = body.OctCount,
				NovCount = body.NovCount,
				DecCount = body.DecCount
			};

			if (!crmData.IsValid())
			{
				return HandleValid(crmData);
			}

			return HandleResult(_repository.EditSalesTarget(crmData, userNumber));
		}



		/// <summary>
		/// 获取销售目标分页列表
		/// </summary>
		/// <param name="body"></param>
		/// <param name="userNumber"></param>
		/// <returns></returns>
		public dynamic GetSalesTargets(SalesTargetSelectModel body, int userNumber)
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


			var crmData = new SalesTargetSelectMapper()
			{
				Year = body.Year,
				NormTypeId = body.NormTypeId,
				DepartmentId = body.DepartmentId,
				SearchName = body.SearchName
			};

			if (!crmData.IsValid())
			{
				return HandleValid(crmData);

			}

			return new OutputResult<object>(_repository.GetSalesTargets(page, crmData, userNumber));
		}




		/// <summary>
		/// 获取销售目标明细
		/// </summary>
		/// <param name="body"></param>
		/// <param name="userNumber"></param>
		/// <returns></returns>
		public dynamic GetSalesTargetDetail(SalesTargetSelectDetailModel body, int userNumber)
		{
			var crmData = new SalesTargetSelectDetailMapper()
			{
				DepartmentId = body.DepartmentId,
				UserId = body.UserId,
				NormTypeId = body.NormTypeId,
				IsGroupTarget = body.IsGroupTarget,
				Year = body.Year
			};

			if (!crmData.IsValid())
			{
				return HandleValid(crmData);
			}


			return new OutputResult<object>(_repository.GetSalesTargetDetail(crmData, userNumber));
		}



		/// <summary>
		/// 设置销售目标开始月份
		/// </summary>
		/// <param name="body"></param>
		/// <param name="userNumber"></param>
		/// <returns></returns>
		public OutputResult<object> SetBeginMoth(SalesTargetSetBeginMothModel body, int userNumber)
		{
			var crmData = new SalesTargetSetBeginMothMapper()
			{
				BeginYear = body.BeginDate.Year,
				BeginMonth = body.BeginDate.Month,
				DepartmentId = body.DepartmentId,
				UserId = body.UserId
			};

			if (!crmData.IsValid())
			{
				return HandleValid(crmData);
			}

			return HandleResult(_repository.SetBeginMoth(crmData, userNumber));



		}


		/// <summary>
		/// 保存销售指标
		/// </summary>
		/// <param name="body"></param>
		/// <param name="userNumber"></param>
		/// <returns></returns>
		public OutputResult<object> AddSalesTargetNormType(SaleTargetNormTypeSaveModel body, int userNumber)
		{
			if (!body.Id.HasValue)
			{
				body.Id = Guid.Empty;
			}
            string tmpName = MultiLanguageUtils.GetDefaultLanguageValue(body.NormTypeName_Lang);
            if (tmpName != null) body.Name = tmpName;
            string name = body.Name;
            var crmData = new SalesTargetNormTypeMapper()
            {
                Id = body.Id,
                Name = name,
                EntityId = Guid.Empty,
                FieldName = string.Empty,
                CaculateType = 0,
                NormTypeName_Lang = body.NormTypeName_Lang
            };

			if (!crmData.IsValid())
			{
				return HandleValid(crmData);
			}
            if (body.Name == null || body.Name.Trim().Length == 0) {
                return new OutputResult<object>(null, "指标名称不能为空", -1);
            }
			return HandleResult(_repository.InsertSalesTargetNormType(crmData, userNumber));
		}




		/// <summary>
		/// 删除销售指标
		/// </summary>
		/// <param name="body"></param>
		/// <param name="userNumber"></param>
		/// <returns></returns>
		public OutputResult<object> DeleteSalesTargetNormType(SaleTargetNormTypeDeleteModel body, int userNumber)
		{
			var crmData = new SalesTargetNormTypeDeleteMapper()
			{
				Id = body.Id,
			};

			if (!crmData.IsValid())
			{
				return HandleValid(crmData);
			}

			return HandleResult(_repository.DeleteSalesTargetNormType(crmData, userNumber));
		}



		/// <summary>
		/// 获取销售指标列表
		/// </summary>
		/// <param name="body"></param>
		/// <param name="userNumber"></param>
		/// <returns></returns>
		public dynamic GetNormTypeList()
		{
			return new OutputResult<object>(_repository.GetTargetNormTypeList());
		}



		//转换数据库中的规则为前段可以使用的规则
		public OutputResult<object> GetSalesTargetNormRule(SaleTargetNormRuleDetailModel entityModel, int userId)
		{
			var crmData = new SalesTargetNormTypeDetailMapper()
			{
				Id = entityModel.Id,
			};

			var infoList = _repository.GetSalesTargetNormDetail(crmData, userId);

			var _entityId = infoList.FirstOrDefault().EntityId;
			if (_entityId == Guid.Empty.ToString())
			{
				_entityId = null;
			}

            var obj = infoList.GroupBy(t => new
            {
                t.RuleId,
                t.RuleName,
                t.RuleSet
            }).Select(group => new RoleRuleInfoModel
            {
                EntityId = _entityId,
                FieldName = infoList.FirstOrDefault().FieldName,
                BizDateFieldName = infoList.FirstOrDefault().BizDateFieldName,
				CaculateType = infoList.FirstOrDefault().CaculateType,
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
					RuleType = t.RuleType
				}).ToList(),
				RuleSet = new RuleSetInfoModel
				{
					RuleSet = group.Key.RuleSet
				}
			}).ToList();
			return new OutputResult<object>(obj);
		}




		/// <summary>
		/// 获取实体列表
		/// </summary>
		/// <param name="body"></param>
		/// <param name="userNumber"></param>
		/// <returns></returns>
		public dynamic GetEntityList()
		{
			return new OutputResult<object>(_repository.GetEntityList());
		}
		public dynamic GetSalesTargetDept(int userNumber)
		{
			return new OutputResult<object>(_repository.GetSalesTargetDept(userNumber));
		}

		public dynamic GetEntityFields(SaleTargetEntityFieldSelect body)
		{
			return new OutputResult<object>(_repository.GetEntityFields(body.EntityId,body.FieldType));
		}

		public OutputResult<object> ImportData(ImportDataModel formData, int userno, dynamic deptList, dynamic userList, dynamic targetTypeList)
		{
			//获取公共缓存数据
			var commonData = GetCommonCacheData(userno);
			//获取个人用户数据
			UserData userData = GetUserData(userno);

			//判断该接口是否有职能控制，只控制有职能控制的接口，其他接口不处理功能权限判断
			if (commonData.TotalFunctions.Exists(a => a.RoutePath != null && a.RoutePath.Trim().Trim('/').Equals(RoutePath)))
			{
				if (!userData.HasFunction(RoutePath, Guid.Empty, DeviceClassic))
				{
					return ShowError<object>("对不起，您没有该功能的权限");
				}
			}

			if (formData == null || formData.Data == null)
			{
				return ShowError<object>("参数错误");
			}
			if (formData.Data.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
			{
				return ShowError<object>("请上传有效的excel文件，且版本为2007及以上");
			}
			if (string.IsNullOrEmpty(formData.Key))
			{
				return ShowError<object>("Key不可为空");
			}
			string taskid = Guid.NewGuid().ToString();
			string taskName = System.IO.Path.GetFileNameWithoutExtension(formData.Data.FileName);

			var sheetDefineOk = new List<SheetDefine>();
			var sheetDefineTmp = GeneralDynamicTemplate_Import(targetTypeList);

			var sheetName = "销售目标";
			string filename = null;
			var sheetDefine = _excelServices.GetSheetDefine(formData.Key, out filename);
			if (sheetDefine.Count() > 0)
			{
				SheetTemplate templateDefine = sheetDefine.Where(i => i.SheetName == sheetName).FirstOrDefault() as SheetTemplate;
				foreach (SimpleSheetTemplate simple in sheetDefineTmp)
				{
					SheetTemplate item = new SheetTemplate();
					item.SheetName = simple.SheetName;

					item.ExecuteSQL = templateDefine.ExecuteSQL;
					item.DefaultDataSql = templateDefine.DefaultDataSql;
					item.IsStoredProcCursor = templateDefine.IsStoredProcCursor;

					item.StylesheetXml = templateDefine.StylesheetXml;
					item.HeadersTemplate = templateDefine.HeadersTemplate;
					item.ColumnMap = templateDefine.ColumnMap;
					item.ColumnsOuterXml = templateDefine.ColumnsOuterXml;

					sheetDefineOk.Add(item);
				}
			}

			//解析Excel的数据
			var sheetDatas = OXSExcelReader.ReadExcelList(formData.Data.OpenReadStream(), sheetDefineOk);

			var taskData = new TaskDataModel();

			taskData.TaskName = taskName;
			taskData.FormDataKey = formData.Key;
			taskData.OperateType = formData.OperateType;

			Dictionary<string, object> parameters = new Dictionary<string, object>();
			if (formData.DefaultParameters != null && formData.DefaultParameters.Count > 0)
			{
				parameters = formData.DefaultParameters;
			}
			if (!parameters.ContainsKey("importtype"))
			{
				parameters.Add("importtype", (int)taskData.OperateType);
			}

			taskData.DefaultParameters = parameters;
			taskData.UserNo = userno;
			taskData.SheetDefines = sheetDefineOk;
			taskData.Datas = sheetDatas;
			//写入任务的缓存数据
			_cache.Set(taskDataId_prefix + taskid, taskData, new DateTimeOffset(DateTime.Now.AddDays(3)));
			//TaskStart(taskid, taskData);
			return new OutputResult<object>(new { taskid = taskid });
		}

		/// <summary>
		/// 导入的模版 包含数据
		/// </summary>
		/// <param name="deptList"></param>
		/// <param name="userList"></param>
		/// <param name="targetTypeList"></param>
		/// <returns></returns>
		public List<SheetDefine> GeneralDynamicTemplate_ImportData(Dictionary<string, object> targetDic, dynamic deptList, dynamic userList, dynamic targetTypeList)
		{
			List<SheetDefine> defines = new List<SheetDefine>();

			try
			{
				int curYear = DateTime.Now.Year;
				var queryResult = targetTypeList.DataBody as Dictionary<string, List<IDictionary<string, object>>>;
				var pageData = queryResult["datacursor"];
				foreach (var typeModel in pageData)
				{
					var categoryname = string.Concat(typeModel["normtypename"]);

					List<SimpleHeader> headers = new List<SimpleHeader>();
					headers.Add(new SimpleHeader() { FieldName = "itemname", HeaderText = "名称", IsNotEmpty = true, Width = 150, FieldType = FieldType.Text });
					headers.Add(new SimpleHeader() { FieldName = "targettype", HeaderText = "指标类型", IsNotEmpty = true, Width = 150, FieldType = FieldType.Text });
					headers.Add(new SimpleHeader() { FieldName = "isgrouptarget", HeaderText = "目标类型", IsNotEmpty = true, Width = 150, FieldType = FieldType.Text });
					headers.Add(new SimpleHeader() { FieldName = "year", HeaderText = "年份", IsNotEmpty = true, Width = 150, FieldType = FieldType.NumberInt });
					headers.Add(new SimpleHeader() { FieldName = "yeartarget", HeaderText = "年度目标", IsNotEmpty = true, Width = 150, FieldType = FieldType.NumberInt });
					#region 12月
					headers.Add(new SimpleHeader() { FieldName = "jancount", HeaderText = "1月", IsNotEmpty = false, Width = 150, FieldType = FieldType.NumberInt });
					headers.Add(new SimpleHeader() { FieldName = "febcount", HeaderText = "2月", IsNotEmpty = false, Width = 150, FieldType = FieldType.NumberInt });
					headers.Add(new SimpleHeader() { FieldName = "marcount", HeaderText = "3月", IsNotEmpty = false, Width = 150, FieldType = FieldType.NumberInt });
					headers.Add(new SimpleHeader() { FieldName = "aprcount", HeaderText = "4月", IsNotEmpty = false, Width = 150, FieldType = FieldType.NumberInt });
					headers.Add(new SimpleHeader() { FieldName = "maycount", HeaderText = "5月", IsNotEmpty = false, Width = 150, FieldType = FieldType.NumberInt });
					headers.Add(new SimpleHeader() { FieldName = "juncount", HeaderText = "6月", IsNotEmpty = false, Width = 150, FieldType = FieldType.NumberInt });
					headers.Add(new SimpleHeader() { FieldName = "julcount", HeaderText = "7月", IsNotEmpty = false, Width = 150, FieldType = FieldType.NumberInt });
					headers.Add(new SimpleHeader() { FieldName = "augcount", HeaderText = "8月", IsNotEmpty = false, Width = 150, FieldType = FieldType.NumberInt });
					headers.Add(new SimpleHeader() { FieldName = "sepcount", HeaderText = "9月", IsNotEmpty = false, Width = 150, FieldType = FieldType.NumberInt });
					headers.Add(new SimpleHeader() { FieldName = "octcount", HeaderText = "10月", IsNotEmpty = false, Width = 150, FieldType = FieldType.NumberInt });
					headers.Add(new SimpleHeader() { FieldName = "novcount", HeaderText = "11月", IsNotEmpty = false, Width = 150, FieldType = FieldType.NumberInt });
					headers.Add(new SimpleHeader() { FieldName = "deccount", HeaderText = "12月", IsNotEmpty = false, Width = 150, FieldType = FieldType.NumberInt });
					#endregion
					List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();

					var queryResultDept = deptList.DataBody as Dictionary<string, List<IDictionary<string, object>>>;
					var pageDataDept = queryResultDept["datacursor"];

					#region 定义变量
					var dataKey = "";

					var year = "";
					var normtypeId = "";
					var deptid = "";
					var userid = "";
					IDictionary<string, object> targetData;

					var yeartarget = "0";
					var jancount = "0";
					var febcount = "0";
					var marcount = "0";
					var aprcount = "0";
					var maycount = "0";
					var juncount = "0";
					var julcount = "0";
					var augcount = "0";
					var sepcount = "0";
					var octcount = "0";
					var novcount = "0";
					var deccount = "0";
					#endregion

					foreach (var item in pageDataDept)
					{
						year = curYear.ToString();
						normtypeId = string.Concat(typeModel["normtypeid"]);
						deptid = string.Concat(item["deptid"]);

						dataKey = string.Concat(year, ",", normtypeId, ",", deptid);
						if (targetDic.Keys.Contains(dataKey))
						{
							targetData = targetDic[dataKey] as IDictionary<string, object>;
							if (targetData != null)
							{
								#region 赋值数据
								yeartarget = string.Concat(targetData["yearcount"]);
								jancount = string.Concat(targetData["jancount"]);
								febcount = string.Concat(targetData["febcount"]);
								marcount = string.Concat(targetData["marcount"]);
								aprcount = string.Concat(targetData["aprcount"]);
								maycount = string.Concat(targetData["maycount"]);
								juncount = string.Concat(targetData["juncount"]);
								julcount = string.Concat(targetData["julcount"]);
								augcount = string.Concat(targetData["augcount"]);
								sepcount = string.Concat(targetData["sepcount"]);
								octcount = string.Concat(targetData["octcount"]);
								novcount = string.Concat(targetData["novcount"]);
								deccount = string.Concat(targetData["deccount"]);
								#endregion
							}
						}

						var dic = new Dictionary<string, object>();

						dic.Add("itemname", string.Concat(item["namepath"]));
						dic.Add("targettype", categoryname);
						dic.Add("isgrouptarget", "团队目标");
						dic.Add("year", curYear.ToString());
						dic.Add("yeartarget", yeartarget);
						#region 12月
						dic.Add("jancount", jancount);
						dic.Add("febcount", febcount);
						dic.Add("marcount", marcount);
						dic.Add("aprcount", aprcount);
						dic.Add("maycount", maycount);
						dic.Add("juncount", juncount);
						dic.Add("julcount", julcount);
						dic.Add("augcount", augcount);
						dic.Add("sepcount", sepcount);
						dic.Add("octcount", octcount);
						dic.Add("novcount", novcount);
						dic.Add("deccount", deccount);
						#endregion
						rows.Add(dic);

						var queryResultUser = userList.DataBody as Dictionary<string, List<IDictionary<string, object>>>;
						var pageDataUser = queryResultUser["PageData"];
						foreach (var u in pageDataUser)
						{
							var deptId = string.Concat(u["deptid"]);
							if (deptId == string.Concat(item["deptid"]))
							{
								year = curYear.ToString();
								normtypeId = string.Concat(typeModel["normtypeid"]);
								userid = string.Concat(u["userid"]);

								dataKey = string.Concat(year, ",", normtypeId, ",", userid);
								if (targetDic.Keys.Contains(dataKey))
								{
									targetData = targetDic[dataKey] as IDictionary<string, object>;
									if (targetData != null)
									{
										#region 赋值数据
										yeartarget = string.Concat(targetData["yearcount"]);
										jancount = string.Concat(targetData["jancount"]);
										febcount = string.Concat(targetData["febcount"]);
										marcount = string.Concat(targetData["marcount"]);
										aprcount = string.Concat(targetData["aprcount"]);
										maycount = string.Concat(targetData["maycount"]);
										juncount = string.Concat(targetData["juncount"]);
										julcount = string.Concat(targetData["julcount"]);
										augcount = string.Concat(targetData["augcount"]);
										sepcount = string.Concat(targetData["sepcount"]);
										octcount = string.Concat(targetData["octcount"]);
										novcount = string.Concat(targetData["novcount"]);
										deccount = string.Concat(targetData["deccount"]);
										#endregion
									}
								}

								var dicUser = new Dictionary<string, object>();
								dicUser.Add("itemname", string.Concat(u["username"]));
								dicUser.Add("targettype", categoryname);
								dicUser.Add("isgrouptarget", "人员目标");
								dicUser.Add("year", curYear.ToString());
								dicUser.Add("yeartarget", yeartarget);
								#region 12月
								dicUser.Add("jancount", jancount);
								dicUser.Add("febcount", febcount);
								dicUser.Add("marcount", marcount);
								dicUser.Add("aprcount", aprcount);
								dicUser.Add("maycount", maycount);
								dicUser.Add("juncount", juncount);
								dicUser.Add("julcount", julcount);
								dicUser.Add("augcount", augcount);
								dicUser.Add("sepcount", sepcount);
								dicUser.Add("octcount", octcount);
								dicUser.Add("novcount", novcount);
								dicUser.Add("deccount", deccount);
								#endregion
								rows.Add(dicUser);
							}
						}
					}

					defines.Add(new SimpleSheetTemplate()
					{
						SheetName = categoryname,
						Headers = headers,
						DataObject = rows
					});
				}
			}
			catch (Exception ex)
			{
				throw new Exception("生成动态模板失败", ex);
			}
			return defines;
		}

		/// <summary>
		/// 导入的模版 不包含数据
		/// </summary>
		/// <param name="targetTypeList"></param>
		/// <returns></returns>
		public List<SheetDefine> GeneralDynamicTemplate_Import(dynamic targetTypeList)
		{
			List<SheetDefine> defines = new List<SheetDefine>();

			try
			{
				int curYear = DateTime.Now.Year;
				var queryResult = targetTypeList.DataBody as Dictionary<string, List<IDictionary<string, object>>>;
				var pageData = queryResult["datacursor"];
				foreach (var typeModel in pageData)
				{
					var categoryname = typeModel["normtypename"].ToString();

					List<SimpleHeader> headers = new List<SimpleHeader>();
					headers.Add(new SimpleHeader() { FieldName = "itemname", HeaderText = "名称", IsNotEmpty = true, Width = 150, FieldType = FieldType.Text });
					headers.Add(new SimpleHeader() { FieldName = "targettype", HeaderText = "指标类型", IsNotEmpty = true, Width = 150, FieldType = FieldType.Text });
					headers.Add(new SimpleHeader() { FieldName = "isgrouptarget", HeaderText = "目标类型", IsNotEmpty = true, Width = 150, FieldType = FieldType.Text });
					headers.Add(new SimpleHeader() { FieldName = "year", HeaderText = "年份", IsNotEmpty = true, Width = 150, FieldType = FieldType.NumberInt });
					headers.Add(new SimpleHeader() { FieldName = "yeartarget", HeaderText = "年度目标", IsNotEmpty = true, Width = 150, FieldType = FieldType.NumberInt });
					#region 12月
					headers.Add(new SimpleHeader() { FieldName = "jancount", HeaderText = "1月", IsNotEmpty = false, Width = 150, FieldType = FieldType.NumberInt });
					headers.Add(new SimpleHeader() { FieldName = "febcount", HeaderText = "2月", IsNotEmpty = false, Width = 150, FieldType = FieldType.NumberInt });
					headers.Add(new SimpleHeader() { FieldName = "marcount", HeaderText = "3月", IsNotEmpty = false, Width = 150, FieldType = FieldType.NumberInt });
					headers.Add(new SimpleHeader() { FieldName = "aprcount", HeaderText = "4月", IsNotEmpty = false, Width = 150, FieldType = FieldType.NumberInt });
					headers.Add(new SimpleHeader() { FieldName = "maycount", HeaderText = "5月", IsNotEmpty = false, Width = 150, FieldType = FieldType.NumberInt });
					headers.Add(new SimpleHeader() { FieldName = "juncount", HeaderText = "6月", IsNotEmpty = false, Width = 150, FieldType = FieldType.NumberInt });
					headers.Add(new SimpleHeader() { FieldName = "julcount", HeaderText = "7月", IsNotEmpty = false, Width = 150, FieldType = FieldType.NumberInt });
					headers.Add(new SimpleHeader() { FieldName = "augcount", HeaderText = "8月", IsNotEmpty = false, Width = 150, FieldType = FieldType.NumberInt });
					headers.Add(new SimpleHeader() { FieldName = "sepcount", HeaderText = "9月", IsNotEmpty = false, Width = 150, FieldType = FieldType.NumberInt });
					headers.Add(new SimpleHeader() { FieldName = "octcount", HeaderText = "10月", IsNotEmpty = false, Width = 150, FieldType = FieldType.NumberInt });
					headers.Add(new SimpleHeader() { FieldName = "novcount", HeaderText = "11月", IsNotEmpty = false, Width = 150, FieldType = FieldType.NumberInt });
					headers.Add(new SimpleHeader() { FieldName = "deccount", HeaderText = "12月", IsNotEmpty = false, Width = 150, FieldType = FieldType.NumberInt });
					#endregion

					List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();

					defines.Add(new SimpleSheetTemplate()
					{
						SheetName = categoryname,
						Headers = headers,
						DataObject = rows
					});
				}
			}
			catch (Exception ex)
			{
				throw new Exception("生成动态模板失败", ex);
			}
			return defines;
		}

		public dynamic GetYearSalesTarget(YearSaleTargetSetlectModel body, int userNumber)
		{

			return new OutputResult<object>(_repository.GetSubDepartmentAndUserYearSalesTarget(body.Id, body.IsGroup, body.Year, body.NormTypeId, userNumber));
		}

		public OutputResult<object> SaveYearSalesTarget(List<YearSalesTargetSaveModel> body, int userNumber)
		{
			//获取公共缓存数据
			var commonData = GetCommonCacheData(userNumber);
			//获取个人用户数据
			UserData userData = GetUserData(userNumber);


			//判断该接口是否有职能控制，只控制有职能控制的接口，其他接口不处理功能权限判断
			if (commonData.TotalFunctions.Exists(a => a.RoutePath != null && a.RoutePath.Trim().Trim('/').Equals(RoutePath)))
			{
				if (!userData.HasFunction(RoutePath, Guid.Empty, DeviceClassic))
				{
					return ShowError<object>("对不起，您没有该功能的权限");
				}

			}


			OperateResult result = new OperateResult();

			var allList = new List<YearSalesTargetSaveMapper>();

			//判断数据是否ok
			foreach (var item in body)
			{
				var crmData = new YearSalesTargetSaveMapper()
				{
					Id = item.Id,
					IsGroup = item.IsGroup,
					YearCount = item.YearCount,
					Year = item.Year,
					NormTypeId = item.NormTypeId,
					DepartmentId = item.DepartmentId,
				};

				if (!crmData.IsValid())
				{
					return HandleValid(crmData);
				}

				result = _repository.SaveYearSalesTarget(crmData, userNumber);

				// allList.Add(crmData);
			}
			/*
						var insertList = new List<YearSalesTargetSaveMapper>();
						var updateList = new List<YearSalesTargetSaveMapper>();

						foreach (var item in allList)
						{
							var isGroup = false;
							if (item.IsGroup == 1)
							{
								isGroup = true;
							}
							var isExists = _repository.IsSalesTargetExists(item.Id, item.Year, isGroup, item.NormTypeId, userNumber);
							if (isExists)
							{
								//更新数据
								_repository.UpdateSalesTarget(item.Id, item.Year, isGroup, item.NormTypeId, item.YearCount, userNumber);

							}
							else
							{
								//新增一条数据
								_repository.InsertSalesTarget(item.Id, item.Year, isGroup, item.NormTypeId, item.YearCount, userNumber);
							}


						}

						OperateResult resule = new OperateResult()
						{
							Flag = 1,
							Msg = "保存销售目标成功"
						};
						*/
			return HandleResult(result);
		}

		public dynamic GetEntityFields(Guid departmentId, int userNumber)
		{
			return new OutputResult<object>(_repository.GetSubDepartmentAndUser(departmentId, userNumber));

		}

		/// <summary>
		/// 保存销售目标
		/// </summary>
		/// <param name="body"></param>
		/// <param name="userNumber"></param>
		/// <returns></returns>
		public OutputResult<object> SaveSalesTarget(SalesTargetSaveModel body, int userNumber)
		{

			//获取公共缓存数据
			var commonData = GetCommonCacheData(userNumber);
			//获取个人用户数据
			UserData userData = GetUserData(userNumber);


			//判断该接口是否有职能控制，只控制有职能控制的接口，其他接口不处理功能权限判断
			if (commonData.TotalFunctions.Exists(a => a.RoutePath != null && a.RoutePath.Trim().Trim('/').Equals(RoutePath)))
			{
				if (!userData.HasFunction(RoutePath, Guid.Empty, DeviceClassic))
				{
					return ShowError<object>("对不起，您没有该功能的权限");
				}

			}

			var crmData = new SalesTargetInsertMapper()
			{
				Year = body.Year,
				JanCount = body.JanCount,
				FebCount = body.FebCount,
				MarCount = body.MarCount,
				AprCount = body.AprCount,
				MayCount = body.MayCount,
				JunCount = body.JunCount,
				JulCount = body.JulCount,
				AugCount = body.AugCount,
				SepCount = body.SepCount,
				OctCount = body.OctCount,
				NovCount = body.NovCount,
				DecCount = body.DecCount,
				UserId = body.UserId,
				DepartmentId = body.DepartmentId,
				IsGroupTarget = body.IsGroupTarget,
				NormTypeId = body.NormTypeId
			};


			if (!crmData.IsValid())
			{
				return HandleValid(crmData);
			}

			return HandleResult(_repository.SaveSalesTarget(crmData, userNumber));
		}

		public ExportModel ExportData(YearSaleTargetSetlectModel data)
		{
			string filename = null;
			var sheetDefine = new List<SheetDefine>();
			var sheets = new List<ExportSheetData>();

			//TODO

			return new ExportModel()
			{
				FileName = filename == null ? null : filename.Replace("模板", ""),
				ExcelFile = OXSExcelWriter.GenerateExcel(sheets)
			};
		}


		public bool IsSalesTargetExists(string id, int year, bool isGroup, Guid normTypeId, int userNumber)
		{
			return _repository.IsSalesTargetExists(id, year, isGroup, normTypeId, userNumber);
		}

		public Dictionary<string, object> GetTargetDic()
		{
			Dictionary<string, object> dic = new Dictionary<string, object>();

			var year = "";
			var normtypeId = "";
			var isgroupTarget = false;
			var deptid = "";
			var userid = "";
			List<IDictionary<string,object>> dicList = _repository.GetCurYearALlTargetList(DateTime.Now.Year);
			foreach (var item in dicList)
			{
				year = string.Concat(item["year"]);
				normtypeId = string.Concat(item["normtypeid"]);
				isgroupTarget = bool.Parse(string.Concat(item["isgrouptarget"]));
				deptid = string.Concat(item["departmentid"]);
				userid = string.Concat(item["userid"]);

				var key = "";
				if (isgroupTarget)
				{
					key = string.Concat(year, ",", normtypeId, ",", deptid);
					if(!dic.ContainsKey(key))
						dic.Add(key, item);
				}
				else
				{
					key = string.Concat(year, ",", normtypeId, ",", userid);
					if (!dic.ContainsKey(key))
						dic.Add(key, item); 
				}
			}

			return dic;
		}
	}
}

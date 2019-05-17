using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;
using UBeat.Crm.CoreApi.DomainModel.Excels;
using UBeat.Crm.CoreApi.DomainModel.Message;
using UBeat.Crm.CoreApi.DomainModel.Utility;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using UBeat.Crm.CoreApi.Services.Models.Excels;
using UBeat.Crm.CoreApi.Services.Models.Message;
using UBeat.Crm.CoreApi.Services.Utility;
using UBeat.Crm.CoreApi.Services.Utility.ExcelUtility;
using UBeat.Crm.CoreApi.Utility;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class ExcelServices : BasicBaseServices
    {

        IExcelRepository _repository;
        FileServices _fileServices;
        DynamicEntityServices _entityServices;
        NotifyServices _notifyServices;
        private readonly IDynamicEntityRepository _dynamicEntityRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IDataSourceRepository _dataSourceRepository;
        private readonly IEntityProRepository _entityProRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly ICustomerRepository _customerRepository;

        object lockObj = new object();
        object tasklockObj = new object();
        private IMemoryCache _cache;
        private string taskList_prefix = "excelTaskList_";
        private string taskDataId_prefix = "exceltask_";


        //static Dictionary<int, ProgressModel> taskCache = new Dictionary<int, ProgressModel>();

        public ExcelServices(IExcelRepository repository, IDynamicEntityRepository dynamicEntityRepository, IDepartmentRepository departmentRepository, IDataSourceRepository dataSourceRepository, IEntityProRepository entityProRepository, DynamicEntityServices entityServices, IMemoryCache memoryCache, NotifyServices notifyServices, IRoleRepository roleRepository, ICustomerRepository customerRepository)
        {
            _repository = repository;
            _dynamicEntityRepository = dynamicEntityRepository;
            _departmentRepository = departmentRepository;
            _dataSourceRepository = dataSourceRepository;
            _entityProRepository = entityProRepository;
            _entityServices = entityServices;
            _notifyServices = notifyServices;
            _roleRepository = roleRepository;
            _customerRepository = customerRepository;
            _fileServices = new FileServices();
            _cache = memoryCache;
        }

        public OutputResult<object> AddExcel(AddExcelModel data, int userId)
        {
            string templateContent = null;
            string excelName = null;
            if (data == null)
            {
                return ShowError<object>("参数错误");
            }
            if (data.ExcelTemplate != null)
            {
                if (data.ExcelTemplate.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    return ShowError<object>("please upload a valid excel file of version 2007 and above");
                }
                var templates = OXSExcelReader.ReadExcelTemplate(data.ExcelTemplate.OpenReadStream());
                templateContent = JsonConvert.SerializeObject(templates);
                excelName = data.ExcelTemplate.FileName;
            }

            var crmData = new AddExcelDomainModel()
            {
                ExcelTemplateId = data.ExcelTemplateId.HasValue ? data.ExcelTemplateId.Value : Guid.Empty,
                BusinessName = data.BusinessName,
                Entityid = data.Entityid.HasValue ? data.Entityid.Value : Guid.Empty,
                FuncName = data.FuncName,
                Remark = data.Remark,
                ExcelName = excelName,
                TemplateContent = templateContent,
                UserNo = userId
            };
            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }
            return HandleResult(_repository.AddExcel(crmData));

        }

        public OutputResult<object> DeleteExcel(DeleteExcelModel data, int userId)
        {
            if (data == null)
            {
                return ShowError<object>("参数错误");
            }

            var crmData = new DeleteExcelDomainModel()
            {
                RecId = data.RecId,
                UserNo = userId
            };
            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }
            return HandleResult(_repository.DeleteExcel(crmData));
        }

        public OutputResult<object> SelectExcels(ExcelSelectModel data, int userId)
        {
            if (data == null)
            {
                return ShowError<object>("参数错误");
            }
            var pageParam = new PageParam { PageIndex = data.PageIndex, PageSize = data.PageSize };
            if (!pageParam.IsValid())
            {
                return HandleValid(pageParam);
            }

            ExcelSelectDomainModel crmData = new ExcelSelectDomainModel()
            {
                UserNo = userId,
                Entityid = data.Entityid
            };

            return new OutputResult<object>(_repository.SelectExcels(pageParam, crmData));
        }

        public OutputResult<object> GetTaskList(int usernumber, List<string> taskids)
        {

            string cacheKey = taskList_prefix + usernumber;
            var taskList = new List<ProgressModel>();
            if (!_cache.TryGetValue(cacheKey, out taskList))
            {
                taskList = new List<ProgressModel>();
            }
            taskList = taskList.Where(m => (m.TotalRowsCount > m.DealRowsCount) || (taskids != null && taskids.Contains(m.TaskId))).ToList();
            return new OutputResult<object>(taskList);
        }

        /// <summary>
        /// 生成嵌套表格导入模板（新增+编辑）
        /// </summary>
        /// <param name="mainEntityId"></param>
        /// <param name="mainTypeId"></param>
        /// <param name="detailEntityId"></param>
        /// <returns></returns>
        public ExportModel GenerateDetailImportTemplate(Guid mainEntityId, Guid mainTypeId, Guid detailEntityId)
        {
            List<SheetDefine> defines = this.GeneralDetailTemplate_Import(detailEntityId, mainTypeId, ExcelOperateType.ImportAdd, 1);
            var sheetsdata = new List<ExportSheetData>();


            foreach (var m in defines)
            {
                var tiprow = new Dictionary<string, object>();//字段说明的提示数据
                var simpleTemp = m as SimpleSheetTemplate;
                var typeFields = simpleTemp.DataObject as List<DynamicEntityDataFieldMapper>;
                foreach (var header in simpleTemp.Headers)
                {
                    var field = typeFields.Find(o => o.FieldName == header.FieldName);
                    tiprow.Add(field.FieldName, GetFieldTip(field));
                }
                var sheetDataTemp = new ExportSheetData() { SheetDefines = m };
                sheetDataTemp.DataRows.Add(tiprow);
                sheetsdata.Add(sheetDataTemp);
            }
            var entityname = _repository.GetEntityName(mainEntityId);
            return new ExportModel()
            {
                FileName = entityname == null ? null : string.Format("{0}导入.xlsx", entityname),
                ExcelFile = OXSExcelWriter.GenerateExcel(sheetsdata)
            };
        }

        public OutputResult<object> TaskStart(string taskid, IServiceProvider serviceProvider)
        {
            var taskData = _cache.Get<TaskDataModel>(taskDataId_prefix + taskid);
            if (taskData == null)
                return ShowError<object>("任务不存在");
            TaskStart(taskid, taskData, serviceProvider);
            _cache.Remove(taskDataId_prefix + taskid);
            return new OutputResult<object>("任务已启动");
        }



        private void TaskStart(string taskid, TaskDataModel taskData, IServiceProvider serviceProvider)
        {
            if (taskData == null)
                return;
            string cacheKey = taskList_prefix + taskData.UserNo;
            var taskList = new List<ProgressModel>();
            if (!_cache.TryGetValue(cacheKey, out taskList))
            {
                taskList = new List<ProgressModel>();
            }


            var task = Task.Run(() =>
            {
                var progress = new ProgressModel();
                progress.TaskId = taskid;
                progress.TaskName = taskData.TaskName;
                progress.TotalRowsCount = taskData.Datas.Sum(o => o.DataRows.Count);
                lock (tasklockObj)
                {
                    taskList.Insert(0, progress);
                    _cache.Set(cacheKey, taskList, new DateTimeOffset(DateTime.Now.AddDays(3)));

                }


                var resultData = new List<ExportSheetData>();
                bool hasError = false;
                //针对每个sheet的数据进行上传操作
                foreach (var m in taskData.Datas)
                {
                    SheetDefine define = taskData.SheetDefines.Where(t => t.SheetName.Equals(m.SheetName)).FirstOrDefault();
                    var error = new ExportSheetData()
                    {
                        SheetDefines = define.Clone(),
                    };

                    var success = new ExportSheetData()
                    {
                        SheetDefines = define.Clone(),
                    };
                    error.SheetDefines.SheetName = define.SheetName + "_失败数据";
                    success.SheetDefines.SheetName = define.SheetName + "_成功数据";
                    resultData.Add(success);

                    if (define is SheetTemplate && string.IsNullOrEmpty(m.ExecuteSQL))
                    {
                        error.DataRows = m.DataRows;
                        error.RowErrors.Add("当前Sheet对应的模板缺乏导入的ExecuteSQL定义");
                        resultData.Add(error);
                        progress.DealRowsCount += m.DataRows.Count;
                        progress.ErrorRowsCount += m.DataRows.Count;
                        continue;
                    }
                    #region 开始整理数据，把嵌套实体合并到一个地方
                    bool hasTableField = false;
                    if (define is MultiHeaderSheetTemplate)
                    {
                        foreach (var headeritem in ((MultiHeaderSheetTemplate)define).Headers)
                        {
                            if (headeritem.SubHeaders != null && headeritem.SubHeaders.Count > 0)
                            {
                                hasTableField = true;
                                break;
                            }
                        }
                    }

                    /**
                     *需要考虑三种模式：
                     * 1、冗余模式
                     * 2、首行模式
                     * 3、合并模式
                     * 4、混合模式
                     **/
                    MultiHeaderSheetTemplate realDefine = null;
                    if (hasTableField)
                    {
                        //只有包含嵌套表格的导入才需要重新整理数据
                        List<Dictionary<string, object>> RealDatas = new List<Dictionary<string, object>>();
                        int totalRowCount = m.DataRows.Count;
                        Dictionary<string, object> lastRealRowData = null;
                        realDefine = define as MultiHeaderSheetTemplate;
                        for (int i = 0; i < totalRowCount; i++)
                        {
                            bool isNewRow = false;
                            Dictionary<string, object> curRow = m.DataRows[i];
                            foreach (var header in realDefine.Headers)
                            {
                                if (header.SubHeaders == null || header.SubHeaders.Count == 0)
                                {
                                    if (lastRealRowData == null)
                                    {
                                        isNewRow = true;
                                        break;
                                    }
                                    if (curRow[header.FieldName] != null && (curRow[header.FieldName].ToString() != "" && curRow[header.FieldName].ToString() != "0")
                                          && (lastRealRowData[header.FieldName] == null || curRow[header.FieldName].ToString() != lastRealRowData[header.FieldName].ToString()))
                                    {
                                        isNewRow = true;
                                        break;
                                    }
                                }
                            }
                            if (isNewRow)
                            {
                                if (lastRealRowData != null)
                                {
                                    RealDatas.Add(lastRealRowData);
                                }
                                lastRealRowData = new Dictionary<string, object>();
                                foreach (var header in realDefine.Headers)
                                {
                                    if (header.SubHeaders == null || header.SubHeaders.Count == 0)
                                    {
                                        lastRealRowData[header.FieldName] = curRow[header.FieldName];
                                    }
                                    else
                                    {
                                        lastRealRowData[header.FieldName] = new List<Dictionary<string, object>>();

                                    }
                                }
                            }
                            foreach (var header in realDefine.Headers)
                            {
                                if (header.SubHeaders != null && header.SubHeaders.Count > 0)
                                {
                                    Dictionary<string, object> subRow = new Dictionary<string, object>();
                                    bool hasNoEmptyField = false;
                                    foreach (var subHeader in header.SubHeaders)
                                    {
                                        string fieldname = header.FieldName + "." + subHeader.FieldName;
                                        if (curRow.ContainsKey(fieldname))
                                        {
                                            subRow.Add(subHeader.FieldName, curRow[fieldname]);
                                            if (curRow[fieldname] != null && curRow[fieldname].ToString().Length > 0)
                                            {
                                                hasNoEmptyField = true;
                                            }
                                        }
                                        else
                                        {
                                            subRow.Add(subHeader.FieldName, "");
                                        }
                                    }
                                    if (hasNoEmptyField)
                                    {
                                        List<DynamicEntityDataFieldMapper> subfields = (List<DynamicEntityDataFieldMapper>)(((Dictionary<string, object>)realDefine.SubDataObject)[header.FieldName]);
                                        DynamicEntityDataFieldMapper firstField = subfields.FirstOrDefault();
                                        Dictionary<string, object> newRowInfo = new Dictionary<string, object>();
                                        newRowInfo["TypeId"] = firstField.TypeId;
                                        newRowInfo["FieldData"] = subRow;

                                        ((List<Dictionary<string, object>>)lastRealRowData[header.FieldName]).Add(newRowInfo);
                                    }
                                }
                            }
                        }
                        if (lastRealRowData != null)
                        {
                            RealDatas.Add(lastRealRowData);
                        }
                        m.DataRows = RealDatas;

                    }
                    progress.TotalRowsCount = taskData.Datas.Sum(o => o.DataRows.Count);
                    #endregion
                    var taskSuccessRows = new List<Dictionary<string, object>>();
                    var taskErrorRows = new List<Dictionary<string, object>>();
                    var taskErrorTips = new List<string>();
                    var taskSuccessTips = new List<string>();
                    var excelDataList = new List<Dictionary<string, object>>();
                    try
                    {
                        foreach (var onerow in m.DataRows)
                        {

                            var tempRow = new Dictionary<string, object>(onerow);
                            //测试上传进度接口时，暂停3s
                            //Thread.Sleep(10000);
                            //1、数据校验
                            string errorMsg = string.Empty;
                            bool issucces = false;
                            if (ValidationRowData(define, taskData.OperateType, tempRow, taskData.UserNo, out errorMsg))
                            {
                                using (var conn = GetDbConnect())
                                {
                                    conn.Open();
                                    var tran = conn.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);

                                    try
                                    {
                                        //2、验证成功，执行导入
                                        //动态实体方式导入
                                        if (define is MultiHeaderSheetTemplate)
                                        {
                                            var entityid = new Guid(taskData.FormDataKey);
                                            issucces = DynamicImport(tran, entityid, define, taskData.OperateType, tempRow, taskData.UserNo, out errorMsg);
                                        }
                                        //模板方式导入
                                        else if (define is SheetTemplate)
                                        {
                                            var taskdata = new ImportRowDomainModel()
                                            {
                                                DataRow = tempRow,
                                                Sql = m.ExecuteSQL,//新增导入和覆盖导入由自定义的sql决定
                                                DefaultParameters = taskData.DefaultParameters,
                                                UserNo = taskData.UserNo,
                                                OperateType = (int)taskData.OperateType
                                            };
                                            var sqlresult = _repository.ImportRowData(tran, taskdata);
                                            issucces = sqlresult.Flag == 1;
                                            errorMsg = sqlresult.Msg;
                                        }
                                        else
                                        {
                                            issucces = false;
                                            errorMsg = "系统错误，模板类型不存在";
                                        }

                                        tran.Commit();
                                    }
                                    catch (Exception ex)
                                    {
                                        tran.Rollback();
                                        issucces = false;
                                        errorMsg = "导入异常：" + ex.Message;
                                    }
                                    finally
                                    {
                                        conn.Close();
                                        conn.Dispose();
                                    }
                                }
                            }
                            if (issucces)
                            {
                                if (hasTableField && realDefine != null
                                    && realDefine.DataObject != null && realDefine.DataObject is List<DynamicEntityDataFieldMapper>)
                                {
                                    ConstructImportResult(taskSuccessRows, taskSuccessTips, onerow, tempRow, realDefine, errorMsg, "导入成功，{0}");
                                }
                                else
                                {
                                    taskSuccessRows.Add(onerow);
                                    taskSuccessTips.Add(string.Format("导入成功，{0}", errorMsg));
                                }
                                excelDataList.Add(tempRow);
                            }
                            else
                            {
                                if (hasTableField && realDefine != null
                                    && realDefine.DataObject != null && realDefine.DataObject is List<DynamicEntityDataFieldMapper>)
                                {
                                    ConstructImportResult(taskErrorRows, taskErrorTips, onerow, tempRow, realDefine, errorMsg, "导入失败，{0}");
                                }
                                else
                                {
                                    taskErrorRows.Add(onerow);
                                    taskErrorTips.Add(string.Format("导入失败，{0}", errorMsg));
                                }
                            }
                            progress.DealRowsCount += 1;
                            if (!issucces)
                                progress.ErrorRowsCount += 1;
                            onerow["task_isdealed"] = 1;
                        }
                        if (taskData.TaskName.Contains("毛利"))
                            BackWriteBudgetOfJx(excelDataList, taskData.TaskName);
                    }
                    catch (Exception ex)
                    {
                        var temp = m.DataRows.Where<Dictionary<string, object>>(item => item.ContainsKey("task_isdealed") == false || item["task_isdealed"] == null || item["task_isdealed"].ToString() != "1").ToList();
                        foreach (var eRow in temp)
                        {
                            if (hasTableField && realDefine != null
                                    && realDefine.DataObject != null && realDefine.DataObject is List<DynamicEntityDataFieldMapper>)
                            {
                                ConstructImportResult(taskErrorRows, taskErrorTips, eRow, null, realDefine, ex.Message, "导入出现异常：{0}");
                                //没有经过ValidRow的数据，要自行处理

                            }
                            else
                            {
                                taskErrorRows.Add(eRow);
                                taskErrorTips.Add(string.Format("导入出现异常：{0}", ex.Message)); ;
                            }
                        }
                        progress.DealRowsCount += temp.Count;
                        progress.ErrorRowsCount += temp.Count;
                    }
                    finally
                    {
                        if (taskSuccessRows.Count > 0)
                        {
                            success.DataRows.AddRange(taskSuccessRows);
                            success.RowErrors.AddRange(taskSuccessTips);
                        }
                        if (taskErrorRows.Count > 0)
                        {
                            error.DataRows.AddRange(taskErrorRows);
                            error.RowErrors.AddRange(taskErrorTips);
                        }
                    }
                    if (error.DataRows.Count > 0)
                    {
                        resultData.Add(error);
                        hasError = true;
                    }
                }
                if (resultData.Count > 0)
                {
                    //生成错误提示的Excel文档
                    var errorExcleBytes = OXSExcelWriter.GenerateExcel(resultData);
                    var fileID = Guid.NewGuid().ToString();
                    //上传文档到文件服务器
                    progress.ResultFileId = _fileServices.UploadFile(null, fileID, string.Format("{0}导入结果.xlsx", taskData.TaskName), errorExcleBytes);

                }
                //var taskDataTemp = _cache.Get(taskDataId_prefix + taskid) as TaskDataModel;
                if (taskData != null)
                {

                    Guid entityid = Guid.Empty;
                    Guid.TryParse(taskData.FormDataKey, out entityid);
                    SendMessage(entityid, progress, hasError, taskData.UserNo);

                }
            });

        }

        private void ConstructImportResult(List<Dictionary<string, object>> taskRow, List<string> taskTips,
                            Dictionary<string, object> onerow, Dictionary<string, object> tempRow,
                            MultiHeaderSheetTemplate realDefine, string errorMsg,
                            string ResultFormatString)
        {
            int maxTableRow = 1;
            if (tempRow == null)
            {
                tempRow = new Dictionary<string, object>(onerow);
                foreach (DynamicEntityDataFieldMapper fieldType in (List<DynamicEntityDataFieldMapper>)realDefine.DataObject)
                {
                    if (fieldType.ControlType == (int)DynamicProtocolControlType.LinkeTable
                         && tempRow.ContainsKey(fieldType.FieldName) && tempRow[fieldType.FieldName] != null)
                    {
                        List<Dictionary<string, object>> newList = new List<Dictionary<string, object>>();
                        tempRow["org_data_" + fieldType.FieldName] = newList;
                        foreach (Dictionary<string, object> subRow in (List<Dictionary<string, object>>)tempRow[fieldType.FieldName])
                        {
                            if (subRow.ContainsKey("FieldData") && subRow["FieldData"] != null)
                            {
                                newList.Add((Dictionary<string, object>)subRow["FieldData"]);
                            }
                        }
                    }
                }
            }
            foreach (DynamicEntityDataFieldMapper fieldType in (List<DynamicEntityDataFieldMapper>)realDefine.DataObject)
            {
                if (fieldType.ControlType == (int)DynamicProtocolControlType.LinkeTable)
                {
                    if (tempRow.ContainsKey("org_data_" + fieldType.FieldName) && tempRow["org_data_" + fieldType.FieldName] != null)
                    {
                        int thisLen = ((List<Dictionary<string, object>>)tempRow["org_data_" + fieldType.FieldName]).Count;
                        if (thisLen > maxTableRow) maxTableRow = thisLen;
                    }
                    else if (tempRow.ContainsKey(fieldType.FieldName) && tempRow[fieldType.FieldName] != null)
                    {
                        tempRow["org_data_" + fieldType.FieldName] = new List<Dictionary<string, object>>();
                        foreach (Dictionary<string, object> rowDataInfo in (List<Dictionary<string, object>>)tempRow[fieldType.FieldName])
                        {
                            if (rowDataInfo.ContainsKey("FieldData"))
                            {
                                ((List<Dictionary<string, object>>)tempRow["org_data_" + fieldType.FieldName]).Add(new Dictionary<string, object>((Dictionary<string, object>)rowDataInfo["FieldData"]));
                            }
                            else
                            {
                                ((List<Dictionary<string, object>>)tempRow["org_data_" + fieldType.FieldName]).Add(new Dictionary<string, object>());
                            }
                        }
                        int thisLen = ((List<Dictionary<string, object>>)tempRow["org_data_" + fieldType.FieldName]).Count;
                        if (thisLen > maxTableRow) maxTableRow = thisLen;
                    }
                }
            }
            for (int tmpi = 0; tmpi < maxTableRow; tmpi++)
            {
                Dictionary<string, object> thisRow = new Dictionary<string, object>();
                foreach (DynamicEntityDataFieldMapper fieldType in (List<DynamicEntityDataFieldMapper>)realDefine.DataObject)
                {
                    if (fieldType.ControlType == (int)DynamicProtocolControlType.LinkeTable)
                    {
                        if (tempRow.ContainsKey("org_data_" + fieldType.FieldName) && tempRow["org_data_" + fieldType.FieldName] != null)
                        {
                            if (((List<Dictionary<string, object>>)tempRow["org_data_" + fieldType.FieldName]).Count <= tmpi)
                            {
                                continue;
                            }
                            Dictionary<string, object> subRowData = ((List<Dictionary<string, object>>)tempRow["org_data_" + fieldType.FieldName])[tmpi];
                            if (realDefine.SubDataObject != null && realDefine.SubDataObject is Dictionary<string, object>
                                    && ((Dictionary<string, object>)realDefine.SubDataObject).ContainsKey(fieldType.FieldName))
                            {
                                List<DynamicEntityDataFieldMapper> thisSubTypes = (List<DynamicEntityDataFieldMapper>)((Dictionary<string, object>)realDefine.SubDataObject)[fieldType.FieldName];
                                foreach (DynamicEntityDataFieldMapper subFieldType in thisSubTypes)
                                {
                                    if (subRowData.ContainsKey(subFieldType.FieldName))
                                    {
                                        thisRow[fieldType.FieldName + "." + subFieldType.FieldName] = subRowData[subFieldType.FieldName];
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (tmpi == 0 && onerow.ContainsKey(fieldType.FieldName))
                            thisRow[fieldType.FieldName] = onerow[fieldType.FieldName];
                    }

                }
                taskRow.Add(thisRow);
                if (tmpi == 0)
                    taskTips.Add(string.Format(ResultFormatString, errorMsg));
                else
                    taskTips.Add("");

            }
        }

        private void SendMessage(Guid entityid, ProgressModel progress, bool hasError, int userId)
        {
            Task.Run(() =>
            {
                try
                {

                    var msg = new MessageParameter();
                    msg.EntityId = entityid;
                    msg.TypeId = Guid.Empty;
                    msg.RelBusinessId = Guid.Empty;
                    msg.RelEntityId = Guid.Empty;
                    msg.BusinessId = Guid.Empty;
                    msg.ParamData = JsonConvert.SerializeObject(progress);
                    msg.FuncCode = "ImportRedmind";

                    msg.Receivers.Add(MessageUserType.SpecificUser, new List<int>() { userId });

                    var paramData = new Dictionary<string, string>();

                    paramData.Add("title", "导入任务完成");
                    paramData.Add("content", string.Format("导入结果：{0}", hasError ? "存在错误" : "成功导入"));
                    paramData.Add("pushcontent", string.Format("您的{0}任务已完成，导入结果：{0}", progress.TaskName, hasError ? "存在错误" : "成功导入"));

                    msg.TemplateKeyValue = paramData;

                    MessageService.WriteMessageAsyn(msg, userId);
                }
                catch (Exception ex)
                {

                }
            });
        }
        private void SendExportMessage(Guid entityid, ProgressModel progress, bool hasError, int userId)
        {
            Task.Run(() =>
            {
                try
                {

                    var msg = new MessageParameter();
                    msg.EntityId = entityid;
                    msg.TypeId = Guid.Empty;
                    msg.RelBusinessId = Guid.Empty;
                    msg.RelEntityId = Guid.Empty;
                    msg.BusinessId = Guid.Empty;
                    msg.ParamData = JsonConvert.SerializeObject(progress);
                    msg.FuncCode = "ExportRedmind";

                    msg.Receivers.Add(MessageUserType.SpecificUser, new List<int>() { userId });

                    var paramData = new Dictionary<string, string>();

                    paramData.Add("title", "导出任务完成");
                    paramData.Add("content", string.Format("成功导出"));
                    paramData.Add("pushcontent", string.Format("成功导出"));

                    msg.TemplateKeyValue = paramData;

                    MessageService.WriteMessageAsyn(msg, userId);
                }
                catch (Exception ex)
                {

                }
            });
        }


        #region --导入Excel模板--
        /// <summary>
        /// 导入Excel模板
        /// </summary>
        /// <param name="formData"></param>
        /// <param name="userId"></param>
        /// <returns>模板id</returns> 
        public OutputResult<object> ImportTemplate(ImportTemplateModel formData, int userId)
        {
            if (formData == null || formData.Data == null)
            {
                return ShowError<object>("参数错误");
            }
            if (formData.Data.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                return ShowError<object>("请上传有效的excel文件，且版本为2007及以上");
            }
            Guid tempid = Guid.Empty;
            if (!string.IsNullOrEmpty(formData.TemplateId) && !Guid.TryParse(formData.TemplateId, out tempid))
            {
                return ShowError<object>("TemplateId必须为UUID格式的数据");
            }
            try
            {
                var templates = OXSExcelReader.ReadExcelTemplate(formData.Data.OpenReadStream());
                var crmdata = new ExcelTemplateModel();
                crmdata.UserNo = userId;
                crmdata.ExcelTemplateId = tempid;
                crmdata.ExcelName = formData.Data.FileName;
                crmdata.TemplateContent = JsonConvert.SerializeObject(templates);

                return HandleResult(_repository.SaveExcelTemplate(crmdata));
            }
            catch (Exception ex)
            {
                return ShowError<object>(ex.ToString());
            }
        }


        #endregion

        #region --导出Excel模板-- 
        public ExportModel ExportTemplate(string funcname, int userId)
        {
            string filename = null;

            var templates = GetExcelTemplate(funcname, out filename);
            if (templates == null)
                throw new Exception("模板数据解析错误，请重新上传模板");
            return new ExportModel()
            {
                ExcelFile = OXSExcelWriter.GenerateExcelTemplate(templates),
                FileName = filename
            };
        }
        #endregion

        #region --导入数据--
        public OutputResult<object> ImportData(ImportDataModel formData, int userno)
        {
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

            string filename = null;
            var sheetDefine = new List<SheetDefine>();
            string taskName;
            if (formData.TemplateType == TemplateType.FixedTemplate)//获取对应的模板
            {
                sheetDefine = GetSheetDefine(formData.Key, out filename);
                taskName = System.IO.Path.GetFileNameWithoutExtension(formData.Data.FileName);
            }
            else
            {
                Guid entityid = Guid.Empty;
                if (!Guid.TryParse(formData.Key, out entityid))
                    return ShowError<object>("实体id必须是guid类型");
                sheetDefine = GeneralDynamicTemplate(entityid, null, formData.OperateType, ExportDataColumnSourceEnum.WEB_Standard, userno);

                taskName = _repository.GetEntityName(entityid);
            }

            //解析Excel的数据
            var sheetDatas = OXSExcelReader.ReadExcelList(formData.Data.OpenReadStream(), sheetDefine);

            var taskData = new TaskDataModel();

            taskData.TaskName = taskName;
            taskData.FormDataKey = formData.Key;
            taskData.OperateType = formData.OperateType;
            taskData.DefaultParameters = formData.DefaultParameters;
            taskData.UserNo = userno;
            taskData.SheetDefines = sheetDefine;
            taskData.Datas = sheetDatas;
            //写入任务的缓存数据
            _cache.Set(taskDataId_prefix + taskid, taskData, new DateTimeOffset(DateTime.Now.AddDays(3)));
            //TaskStart(taskid, taskData);
            return new OutputResult<object>(new { taskid = taskid });

        }

        #region --动态方式导入--  private bool DynamicImport(define, operateType,  onerow, userno, out errorMsg)
        /// <summary>
        /// 动态方式导入
        /// </summary>
        /// <returns></returns>
        private bool DynamicImport(DbTransaction tran, Guid entityid, SheetDefine define, ExcelOperateType operateType, Dictionary<string, object> onerow, int userno, out string errorMsg)
        {
            bool issucces = true;
            errorMsg = "";
            Guid typeId = Guid.Empty;
            var sheetTemplate = define as MultiHeaderSheetTemplate;
            if (sheetTemplate.DataObject is List<DynamicEntityDataFieldMapper>)
            {
                var typeFields = sheetTemplate.DataObject as List<DynamicEntityDataFieldMapper>;
                typeId = typeFields.FirstOrDefault().TypeId;

            }
            Dictionary<string, object> extRow = new Dictionary<string, object>();
            Guid existRecordid = Guid.Empty;
            //客户查重条件从客户基础资料中查询D
            if (entityid.ToString() == "f9db9d79-e94b-4678-a5cc-aa6e281c1246")
            {
                var tempdata = _customerRepository.IsCustomerExist(onerow);
                if (tempdata.Count > 1 && operateType == ExcelOperateType.ImportUpdate)
                {
                    errorMsg = "被引用的客户不允许覆盖导入,";
                }
                else if (tempdata.Count == 1)
                {
                    existRecordid = tempdata.FirstOrDefault().Custid;
                }
            }
            else if (entityid.ToString() == "e450bfd7-ff17-4b29-a2db-7ddaf1e79342")
            {
                //联系人，需要增加拼音字段
                if (onerow.ContainsKey("recname"))
                {
                    var name = onerow["recname"] as string;
                    var pinYin = PinYinConvert.ToChinese(name, true);
                    if (!string.IsNullOrWhiteSpace(pinYin))
                    {
                        onerow.Add("namepinyin", pinYin);
                    }
                }
            }
            else
            {
                existRecordid = _dynamicEntityRepository.DynamicEntityExist(entityid, onerow, Guid.Empty);
            }
            OperateResult oResult = null;

            onerow.Add("field_by_import_type", operateType);
            onerow.Add("field_by_import", 1);//新增导入操作标识符，数据库函数通过该键值判断是否导入操作，再通过 crm_sys_entity_import_func_event 表读取验证规则
                                             //导入前执行特殊规则验证



            //实体动态新增导入
            if (operateType == ExcelOperateType.ImportAdd)
            {
                //记录不存在，则新增
                if (existRecordid == Guid.Empty)
                {
                    oResult = _dynamicEntityRepository.DynamicAdd(tran, typeId, onerow, null, userno);
                    errorMsg = "新增导入,";
                }
                else
                {
                    issucces = false;
                    errorMsg = "记录已存在";
                }
            }
            //实体动态覆盖导入
            else if (operateType == ExcelOperateType.ImportUpdate)
            {
                //记录存在，覆盖数据
                if (existRecordid != Guid.Empty)
                {
                    oResult = _dynamicEntityRepository.DynamicEdit(tran, typeId, existRecordid, onerow, userno);
                    errorMsg = "覆盖导入,";
                }
                else
                {
                    //插入新数据
                    oResult = _dynamicEntityRepository.DynamicAdd(tran, typeId, onerow, null, userno);
                    errorMsg = "新增导入,";
                }
            }
            //判断sql执行是否成功
            if (oResult != null)
            {
                issucces = oResult.Flag == 1;
                errorMsg += oResult.Msg;
                errorMsg.TrimEnd(',');

            }
            return issucces;
        }
        #endregion

        #endregion

        #region --导出数据--
        /// <summary>
        /// 导出数据
        /// </summary>
        public ExportModel ExportData(ExportDataModel data, int userId)
        {
            Thread.CurrentThread.Priority = ThreadPriority.Lowest;
            if (data.NestTableList == null) data.NestTableList = new List<string>();
            var userData = HasFunctionAccess(data.UserId, data.EntityId);


            string filename = null;
            var sheetDefine = new List<SheetDefine>();
            if (data.TemplateType == TemplateType.FixedTemplate)//获取对应的模板
                sheetDefine = GetSheetDefine(data.FuncName, out filename);
            else
            {

                if (data.DynamicModel == null)
                    throw new Exception("DynamicQuery必须有值");
                sheetDefine = GeneralDynamicTemplate(data.DynamicModel.EntityId, data.NestTableList, ExcelOperateType.Export, data.ColumnSource, data.UserId);
            }
            string entityname = "";
            var sheets = new List<ExportSheetData>();
            foreach (var m in sheetDefine)
            {
                var sheetdata = new ExportSheetData();
                var pageData = new List<Dictionary<string, object>>();
                if (m is SheetTemplate)//固定模板方式导出
                {
                    var crmData = new ExportDataDomainModel()
                    {
                        IsStoredProcCursor = m.IsStoredProcCursor == 1,
                        QueryParameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(data.QueryParameters),
                        Sql = m.ExecuteSQL,
                        UserNo = data.UserId,
                        RuleSql = userData.RuleSqlFormat(RoutePath, Guid.Empty, DeviceClassic)
                    };
                    pageData = _repository.ExportData(crmData);
                }
                else //动态实体配置方式导出
                {
                    var template = m as MultiHeaderSheetTemplate;
                    if (template.Headers.Count == 0)
                    {
                        throw new Exception("请设置web列表显示字段");
                    }
                    if (data.DynamicModel == null)
                    {
                        throw new Exception("实体类型数据导出，需要传DynamicModel参数");
                    }
                    entityname = _repository.GetEntityName(data.DynamicModel.EntityId);
                    filename = string.Format("{0}导出数据.xlsx", entityname);
                    #region 获取并处理主表数据
                    var isAdvance = data.DynamicModel.IsAdvanceQuery == 1;
                    var dataList = _entityServices.DataList2(data.DynamicModel, isAdvance, data.UserId);
                    var queryResult = dataList.DataBody as Dictionary<string, List<Dictionary<string, object>>>;
                    var pageDataTemp = queryResult["PageData"];
                    var tempFields = GetExportSpecFields(template.Headers);
                    var AllExportFields = GetExportFields(template.Headers);
                    bool HasDetailTable = false;
                    List<MergeCellInfo> MergeList = new List<MergeCellInfo>();
                    foreach (var item in tempFields)
                    {
                        foreach (var mdata in pageDataTemp)
                        {
                            if (mdata.ContainsKey(item.FieldName) == false) continue;//第一层忽略表格内容字段
                            switch (item.FieldType)
                            {
                                case FieldType.TimeDate:
                                    {
                                        var values = mdata[item.FieldName];
                                        DateTime dt;
                                        if (values != null && DateTime.TryParse(values.ToString(), out dt))
                                        {
                                            mdata[item.FieldName] = dt.ToString("yyyy-MM-dd");
                                        }
                                    }
                                    break;
                                case FieldType.Image:
                                    {
                                        var values = mdata[item.FieldName];
                                        if (values != null)
                                        {
                                            var urlArray = values.ToString().Split(',');
                                            StringBuilder urlContent = new StringBuilder();
                                            int rowheight = 0;
                                            foreach (var str in urlArray)
                                            {
                                                urlContent.Append(string.Format(_fileServices.UrlConfig.ReadUrl, str));
                                                urlContent.Append("\r\n");
                                                rowheight += 25;
                                            }
                                            mdata[item.FieldName] = new OXSExcelDataCell(urlContent.ToString(), rowheight);
                                            template.Headers.FirstOrDefault(o => o.FieldName.Equals(item.FieldName)).Width = 700;
                                        }
                                    }
                                    break;
                                case FieldType.Address:
                                case FieldType.reference:
                                    if (mdata.ContainsKey(item.FieldName + "_name"))
                                    {
                                        var _namevalues = mdata[item.FieldName + "_name"];
                                        if (mdata.ContainsKey(item.FieldName))
                                            mdata[item.FieldName] = _namevalues;
                                        else mdata.Add(item.FieldName, _namevalues);
                                    }
                                    break;
                            }


                        }
                    }

                    // pageData = new List<Dictionary<string, object>>();
                    foreach (var item1 in pageDataTemp)
                    {

                        var dic = new Dictionary<string, object>();
                        foreach (var item2 in item1)
                        {
                            if (dic.ContainsKey(item2.Key))
                            {
                                dic[item2.Key] = item2.Value;
                            }
                            else
                            {
                                dic.Add(item2.Key, item2.Value);
                            }
                        }
                        pageData.Add(dic);
                    }
                    #endregion
                    #region 开始处理嵌套表格数据问题
                    Dictionary<string, Dictionary<string, Dictionary<string, object>>> allSubTableDict = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>();
                    var typeVisibleFields = _entityProRepository.FieldWebVisibleQuery(data.DynamicModel.EntityId.ToString(), data.UserId);
                    if (!typeVisibleFields.ContainsKey("FieldVisible"))
                        throw new Exception("获取实体显示字段接口报错，缺少FieldVisible参数的结果集");
                    var typeFields = typeVisibleFields["FieldVisible"];
                    Dictionary<string, List<Dictionary<string, object>>> list = new Dictionary<string, List<Dictionary<string, object>>>();
                    foreach (var field in typeFields)
                    {
                        if (!field.ContainsKey("displayname") || !field.ContainsKey("fieldname") || !field.ContainsKey("controltype"))
                            throw new Exception("获取实体显示字段接口缺少必要参数");
                        if (field["displayname"] == null || field["fieldname"] == null || field["controltype"] == null)
                            throw new Exception("获取实体显示字段接口必要参数数据不允许为空");
                        var displayname = field["displayname"].ToString();
                        var fieldname = field["fieldname"].ToString();
                        var controltype = field["controltype"].ToString();
                        if (int.Parse(controltype) == (int)DynamicProtocolControlType.LinkeTable
                            && data.NestTableList.Exists((string s) => s.Equals(fieldname)))
                        {
                            var sFieldConfig = field["fieldconfig"].ToString();
                            DynamicProtocolFieldConfig fieldConfigInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<DynamicProtocolFieldConfig>(sFieldConfig);
                            if (fieldConfigInfo != null && fieldConfigInfo.EntityId != null && fieldConfigInfo.EntityId.Equals(Guid.Empty) == false)
                            {
                                HasDetailTable = true;
                                List<Guid> MainIds = new List<Guid>();//如果涉及嵌套表格，这个变量是记录所有主实体的id，虽然性能查了一点，但不是绝对的问题
                                foreach (var item1 in pageDataTemp)//这个遍历不合并到上面的遍历的原因是因为代码的可阅读性，虽然降低了一点点可以忽略的性能。
                                {
                                    if (item1.ContainsKey(fieldname) && item1[fieldname] != null)
                                    {
                                        string tmp = item1[fieldname].ToString();
                                        string[] ids = tmp.Split(",");
                                        foreach (string id in ids)
                                        {
                                            Guid tmpid = Guid.Empty;
                                            if (Guid.TryParse(id, out tmpid))
                                            {
                                                MainIds.Add(tmpid);
                                            }
                                        }


                                    }
                                }
                                //确实是需要输出的嵌套表格
                                DynamicEntityListModel query = new DynamicEntityListModel()
                                {
                                    EntityId = fieldConfigInfo.EntityId,
                                    ViewType = (int)DynamicProtocolViewType.Web,
                                    SearchData = new Dictionary<string, object>(),
                                    ExtraData = new Dictionary<string, object>(),
                                    SearchDataXOR = new Dictionary<string, object>(),
                                    PageIndex = 1,
                                    PageSize = 1024 * 1024,
                                    IsAdvanceQuery = 1,
                                    MainIds = MainIds,
                                    NeedPower = 0
                                };
                                var sub_dataList = _entityServices.DataList2(query, true, data.UserId);
                                var sub_queryResult = sub_dataList.DataBody as Dictionary<string, List<Dictionary<string, object>>>;
                                var sub_pageDataTemp = sub_queryResult["PageData"];
                                foreach (var item in tempFields)
                                {
                                    if (item.FieldName.StartsWith(fieldname + ".") == false) continue;
                                    foreach (var mdata in sub_pageDataTemp)
                                    {

                                        string thisfieldname = item.FieldName.Substring(fieldname.Length + 1);
                                        if (mdata.ContainsKey(thisfieldname) == false) continue;//第一层忽略表格内容字段
                                        switch (item.FieldType)
                                        {
                                            case FieldType.TimeDate:
                                                {
                                                    var values = mdata[thisfieldname];
                                                    DateTime dt;
                                                    if (values != null && DateTime.TryParse(values.ToString(), out dt))
                                                    {
                                                        mdata[thisfieldname] = dt.ToString("yyyy-MM-dd");
                                                    }
                                                }
                                                break;
                                            case FieldType.Image:
                                                {
                                                    var values = mdata[thisfieldname];
                                                    if (values != null)
                                                    {
                                                        var urlArray = values.ToString().Split(',');
                                                        StringBuilder urlContent = new StringBuilder();
                                                        int rowheight = 0;
                                                        foreach (var str in urlArray)
                                                        {
                                                            urlContent.Append(string.Format(_fileServices.UrlConfig.ReadUrl, str));
                                                            urlContent.Append("\r\n");
                                                            rowheight += 25;
                                                        }
                                                        mdata[thisfieldname] = new OXSExcelDataCell(urlContent.ToString(), rowheight);
                                                        template.Headers.FirstOrDefault(o => o.FieldName.Equals(item.FieldName)).Width = 700;
                                                    }
                                                }
                                                break;
                                            case FieldType.Address:
                                            case FieldType.reference:
                                                if (mdata.ContainsKey(thisfieldname + "_name"))
                                                {
                                                    var _namevalues = mdata[thisfieldname + "_name"];
                                                    if (mdata.ContainsKey(thisfieldname))
                                                        mdata[thisfieldname] = _namevalues;
                                                    else mdata.Add(thisfieldname, _namevalues);
                                                }
                                                break;

                                        }


                                    }
                                }
                                //
                                var sub_pageData = new Dictionary<string, Dictionary<string, object>>();
                                foreach (var item1 in sub_pageDataTemp)
                                {

                                    var dic = new Dictionary<string, object>();
                                    foreach (var item2 in item1)
                                    {

                                        dic.Add(item2.Key, item2.Value);
                                    }
                                    if (dic.ContainsKey("recid") && dic["recid"] != null)
                                    {

                                        sub_pageData.Add(dic["recid"].ToString(), dic);
                                    }
                                }
                                allSubTableDict.Add(fieldname, sub_pageData);

                            }
                        }
                    }
                    #endregion

                    #region 把主表的数据与每个嵌套实体的数据合并起来
                    var new_PageData = new List<Dictionary<string, object>>();
                    int curRow = 3;
                    foreach (Dictionary<string, object> itemData in pageData)
                    {
                        int maxRow = 1;
                        Dictionary<string, List<Dictionary<string, object>>> thisSubTableData = new Dictionary<string, List<Dictionary<string, object>>>();
                        foreach (string fieldname in data.NestTableList)
                        {
                            List<Dictionary<string, object>> detailList = new List<Dictionary<string, object>>();
                            if (itemData.ContainsKey(fieldname) && itemData[fieldname] != null && allSubTableDict.ContainsKey(fieldname))
                            {
                                string details = itemData[fieldname].ToString();
                                if (details != null && details.Length > 0)
                                {
                                    Dictionary<string, Dictionary<string, object>> orgSubTableData = allSubTableDict[fieldname];
                                    string[] subids = details.Split(',');
                                    foreach (string id in subids)
                                    {
                                        if (orgSubTableData.ContainsKey(id))
                                        {
                                            detailList.Add(orgSubTableData[id]);
                                        }
                                    }
                                    if (detailList.Count > maxRow) maxRow = detailList.Count;
                                }
                            }
                            thisSubTableData.Add(fieldname, detailList);
                        }
                        if (data.RowMode == ExportDataRowModeEnum.MergeRow && maxRow > 1)
                        {
                            int curCol = 0;
                            foreach (var field in AllExportFields)
                            {

                                if (field.FieldName.IndexOf('.') < 0)
                                {
                                    MergeCellInfo mergeCellInfo = new MergeCellInfo()
                                    {
                                        FromColIndex = curCol,
                                        FromRowIndex = curRow,
                                        RowCount = maxRow,
                                        ColCount = 1
                                    };
                                    MergeList.Add(mergeCellInfo);
                                }
                                curCol++;
                            }
                        }
                        for (int i = 0; i < maxRow; i++)
                        {
                            curRow++;
                            Dictionary<string, object> newData = null;
                            if (data.RowMode == ExportDataRowModeEnum.FullFill)
                            {
                                newData = copyDictionary(itemData);
                            }
                            else if (data.RowMode == ExportDataRowModeEnum.KeepEmpty)
                            {
                                if (i == 0)
                                {
                                    newData = copyDictionary(itemData);
                                }
                                else
                                {
                                    newData = new Dictionary<string, object>();
                                }
                            }
                            else
                            {
                                if (i == 0)
                                {
                                    newData = copyDictionary(itemData);
                                }
                                else
                                {
                                    newData = new Dictionary<string, object>();
                                }
                            }
                            foreach (string key in thisSubTableData.Keys)
                            {
                                List<Dictionary<string, object>> subList = thisSubTableData[key];
                                if (subList.Count > i)
                                {
                                    Dictionary<string, object> subItem = subList[i];
                                    appendDictionary(subItem, newData, key + ".");
                                }
                            }
                            new_PageData.Add(newData);
                        }
                    }
                    pageData = new_PageData;
                    #endregion
                    sheetdata.MergeList = MergeList;
                }
                sheetdata.DataRows = pageData;
                sheetdata.SheetDefines = m;
                sheets.Add(sheetdata);
            }
            int newUserId = userId;
            if (sheets[0].DataRows.Count > 1000 && data.CanChange2Asynch == false)
            {
                ThreadPool.QueueUserWorkItem(delegate (object dataparam)
                {
                    ExportModel exportModel = new ExportModel()
                    {
                        FileName = filename == null ? null : filename.Replace("模板", ""),
                        ExcelFile = OXSExcelWriter.GenerateExcel(sheets)
                    };
                    ProgressModel progress = new ProgressModel() { };
                    var fileID = Guid.NewGuid().ToString();
                    //上传文档到文件服务器
                    progress.ResultFileId = _fileServices.UploadFile(null, fileID, "导出结果.xlsx", exportModel.ExcelFile);

                    SendExportMessage(Guid.Empty, progress, false, newUserId);
                });
                return new ExportModel()
                {
                    FileName = "异步处理",
                    Message = string.Format("共有{0}条记录需要导出，预计耗时{1}分钟，导出成功后会以消息方式通知您！", sheets[0].DataRows.Count, sheets[0].DataRows.Count / 5000),
                    IsAysnc = true,
                    ExcelFile = new byte[] { }
                };

            }
            else
            {
                return new ExportModel()
                {
                    FileName = filename == null ? null : filename.Replace("模板", ""),
                    ExcelFile = OXSExcelWriter.GenerateExcel(sheets)
                };
            }

        }
        public List<MultiHeader> GetExportSpecFields(List<MultiHeader> headers)
        {
            List<MultiHeader> ret = new List<MultiHeader>();
            foreach (MultiHeader o in headers)
            {
                if (o.FieldType == FieldType.Image
                || o.FieldType == FieldType.Address
                || o.FieldType == FieldType.reference
                || o.FieldType == FieldType.TimeDate)
                {
                    ret.Add(o);
                }
                if (o.SubHeaders != null && o.SubHeaders.Count > 0)
                {
                    ret.AddRange(GetExportSpecFields(o.SubHeaders));
                }
            }

            return ret;
        }
        public List<MultiHeader> GetExportFields(List<MultiHeader> headers)
        {
            List<MultiHeader> ret = new List<MultiHeader>();
            foreach (MultiHeader o in headers)
            {
                if (o.SubHeaders != null && o.SubHeaders.Count > 0)
                {
                    ret.AddRange(GetExportFields(o.SubHeaders));
                }
                else
                {
                    ret.Add(o);
                }
            }

            return ret;
        }
        public Dictionary<string, object> copyDictionary(Dictionary<string, object> orgDict)
        {
            if (orgDict == null) return null;
            Dictionary<string, object> retDict = new Dictionary<string, object>();
            foreach (string key in orgDict.Keys)
            {
                retDict.Add(key, orgDict[key]);
            }
            return retDict;
        }
        public void appendDictionary(Dictionary<string, object> orgDict, Dictionary<string, object> resultDict, string prefix)
        {
            foreach (string key in orgDict.Keys)
            {
                resultDict.Add(prefix + key, orgDict[key]);
            }
        }
        #endregion

        #region --生成导入模板--
        /// <summary>
        /// 生成固定模板的导入模板
        /// </summary>
        /// <param name="funcname"></param>
        /// <returns></returns>
        public ExportModel GenerateImportTemplate(string funcname, int userId)
        {
            string filename = null;

            var templates = GetExcelTemplate(funcname, out filename);
            if (templates == null)
                throw new Exception("模板数据解析错误，请重新上传模板");

            Dictionary<string, List<Dictionary<string, object>>> sheetRows = new Dictionary<string, List<Dictionary<string, object>>>();
            foreach (var item in templates.DataSheets)
            {
                if (!string.IsNullOrEmpty(item.DefaultDataSql))
                {
                    List<Dictionary<string, object>> defaultRows = new List<Dictionary<string, object>>();

                    var crmData = new ExportDataDomainModel()
                    {
                        IsStoredProcCursor = item.IsStoredProcCursor == 1,
                        QueryParameters = null,
                        Sql = item.DefaultDataSql,
                        UserNo = userId
                    };
                    var defaultRowsTmp = _repository.ExportData(crmData);

                    foreach (var row in defaultRowsTmp)
                    {
                        defaultRows.Add(row as Dictionary<string, object>);
                    }

                    sheetRows.Add(item.SheetName, defaultRows);
                }
            }

            return new ExportModel()
            {
                ExcelFile = OXSExcelWriter.GenerateImportTemplate(templates, sheetRows),
                FileName = filename
            };
        }

        /// <summary>
        /// 生成动态模板的导入模板
        /// </summary>
        /// <param name="funcname"></param>
        /// <returns></returns>
        public ExportModel GenerateImportTemplate(Guid entityid, int userNumber)
        {

            var defines = GeneralDynamicTemplate(entityid, null, ExcelOperateType.ImportAdd, ExportDataColumnSourceEnum.WEB_Standard, userNumber);
            var sheetsdata = new List<ExportSheetData>();


            foreach (var m in defines)
            {
                var tiprow = new Dictionary<string, object>();//字段说明的提示数据
                var simpleTemp = m as MultiHeaderSheetTemplate;
                var typeFields = simpleTemp.DataObject as List<DynamicEntityDataFieldMapper>;
                foreach (var header in simpleTemp.Headers)
                {
                    var field = typeFields.Find(o => o.FieldName == header.FieldName);
                    tiprow.Add(field.FieldName, GetFieldTip(field));

                    #region 处理子表
                    if (header.SubHeaders != null && header.SubHeaders.Count > 0)
                    {
                        if (simpleTemp.SubDataObject != null && ((Dictionary<string, object>)simpleTemp.SubDataObject).ContainsKey(header.FieldName))
                        {
                            List<DynamicEntityDataFieldMapper> subTypeFields = (List<DynamicEntityDataFieldMapper>)((Dictionary<string, object>)simpleTemp.SubDataObject)[header.FieldName];
                            if (subTypeFields != null)
                            {
                                foreach (var subHeader in header.SubHeaders)
                                {
                                    var subfield = subTypeFields.Find(o => o.FieldName == subHeader.FieldName);
                                    if (subfield != null)
                                    {
                                        tiprow.Add(field.FieldName + "." + subfield.FieldName, GetFieldTip(subfield));
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }
                var sheetDataTemp = new ExportSheetData() { SheetDefines = m };
                sheetDataTemp.DataRows.Add(tiprow);
                sheetsdata.Add(sheetDataTemp);
            }
            var entityname = _repository.GetEntityName(entityid);
            return new ExportModel()
            {
                FileName = entityname == null ? null : string.Format("{0}导入.xlsx", entityname),
                ExcelFile = OXSExcelWriter.GenerateExcel(sheetsdata)
            };
        }
        //校验字段的数据格式和处理特殊控件
        private string GetFieldTip(DynamicEntityDataFieldMapper typeField)
        {
            string tipcontent = typeField.IsRequire ? "必填" : "非必填";

            switch ((DynamicProtocolControlType)typeField.ControlType)
            {
                case DynamicProtocolControlType.NumberInt://整数
                    tipcontent += ",请输入一个整数";
                    break;
                case DynamicProtocolControlType.NumberDecimal://小数
                    tipcontent += ",小数，最长不超过13个字符";
                    break;
                case DynamicProtocolControlType.TimeDate://日期，年月日
                    tipcontent += ",日期格式需正确";
                    break;
                case DynamicProtocolControlType.TimeStamp:// 日期时间
                    tipcontent += ",日期时间格式需正确";
                    break;
                case DynamicProtocolControlType.PhoneNum://手机号
                    tipcontent += ",手机号格式需正确";
                    break;
                case DynamicProtocolControlType.EmailAddr://邮箱地址
                    tipcontent += ",邮箱地址格式需正确";
                    break;
                case DynamicProtocolControlType.Telephone://电话
                    tipcontent += ",电话格式需正确,如：010-6666666";
                    break;

                case DynamicProtocolControlType.SelectSingle://本地字典单选
                    tipcontent += ",字典单选,必须系统中存在";
                    break;
                case DynamicProtocolControlType.SelectMulti://本地字典多选
                    tipcontent += ",字典多选,必须系统中存在,多个时以','分割";
                    break;

                case DynamicProtocolControlType.AreaRegion://行政区域
                    tipcontent += ",必须为系统中存在的行政区域， 省/省.市/市.区/省.市.区 均可";
                    break;

                case DynamicProtocolControlType.PersonSelectMulti://多选人
                    tipcontent += ",人员姓名（多选）,必须系统中存在,多个时以','分割";
                    break;
                case DynamicProtocolControlType.PersonSelectSingle://单选人
                    tipcontent += ",人员姓名（单选）,必须系统中存在";
                    break;
                case DynamicProtocolControlType.RecManager://负责人
                    tipcontent += ",人员姓名,必须系统中存在";
                    break;
                case DynamicProtocolControlType.DataSourceSingle://数据源单选
                case DynamicProtocolControlType.DataSourceMulti://数据源多选
                    tipcontent += ",必须系统中存在";
                    break;
                case DynamicProtocolControlType.Department://团队组织
                    tipcontent += "请输入团队组织全路径，使用'/'分割";
                    break;
                case DynamicProtocolControlType.Address:


                case DynamicProtocolControlType.Location://定位
                case DynamicProtocolControlType.HeadPhoto://头像
                case DynamicProtocolControlType.AreaGroup://分组
                case DynamicProtocolControlType.TakePhoto://拍照
                case DynamicProtocolControlType.FileAttach://附件
                case DynamicProtocolControlType.LinkeTable://表格控件
                case DynamicProtocolControlType.TreeMulti://树形多选
                case DynamicProtocolControlType.TreeSingle://树形
                case DynamicProtocolControlType.RecId://记录ID
                case DynamicProtocolControlType.RecUpdator://创建人
                case DynamicProtocolControlType.RecCreated://创建时间
                case DynamicProtocolControlType.RecUpdated://更新时间
                case DynamicProtocolControlType.RecAudits://审批状态
                case DynamicProtocolControlType.RecStatus://记录状态
                case DynamicProtocolControlType.RecType://记录类型
                case DynamicProtocolControlType.RecItemid://明细ID
                case DynamicProtocolControlType.RecOnlive://活动时间
                case DynamicProtocolControlType.RecName://记录名称
                    break;
            }
            return tipcontent;
        }
        #endregion

        #region --动态模板导出数据 --
        public ExportModel ExportData(Dictionary<string, string> headers, List<Dictionary<string, object>> rows = null)
        {
            List<ExcelHeader> headersList = new List<ExcelHeader>();
            foreach (var item in headers)
            {
                headersList.Add(new ExcelHeader() { Text = item.Key, FieldName = item.Value, Width = 300 });
            }
            return ExportData(null, headersList, rows);
        }
        public ExportModel ExportData(string sheetName, List<ExcelHeader> headers, List<Dictionary<string, object>> rows = null)
        {
            var columnMap = new List<ColumnMapModel>();
            var headerstemp = new List<SimpleHeader>();
            for (int i = 0; i < headers.Count; i++)
            {
                headerstemp.Add(new SimpleHeader()
                {
                    HeaderText = headers[i].Text,
                    FieldName = headers[i].FieldName,
                    IsNotEmpty = headers[i].IsNotEmpty,
                    Width = headers[i].Width
                });
            }
            var sheetdata = new ExportSheetData();
            sheetdata.SheetDefines = new SimpleSheetTemplate()
            {
                SheetName = sheetName,
                ExecuteSQL = "",
                Headers = headerstemp,
            };
            if (rows != null)
                sheetdata.DataRows = rows;

            return new ExportModel()
            {
                FileName = string.IsNullOrEmpty(sheetName) ? null : string.Format("{0}.xlsx", sheetName),
                ExcelFile = OXSExcelWriter.GenerateExcel(sheetdata)
            };
        }
        #endregion

        #region --动态模板导入数据解析-- +List<Dictionary<string, object>> ImportData(Stream fileStream, List<ExcelHeader> headers)
        public List<Dictionary<string, object>> ImportData(Stream fileStream, List<ExcelHeader> headers)
        {
            var headerstemp = new List<SimpleHeader>();
            for (int i = 0; i < headers.Count; i++)
            {
                headerstemp.Add(new SimpleHeader()
                {
                    HeaderText = headers[i].Text,
                    FieldName = headers[i].FieldName,
                    IsNotEmpty = headers[i].IsNotEmpty,
                    Width = headers[i].Width
                });
            }
            var sheetTemplate = new SimpleSheetTemplate()
            {
                SheetName = "",
                ExecuteSQL = "",
                Headers = headerstemp,
            };

            var data = OXSExcelReader.ReadExcelList(fileStream, sheetTemplate);
            return data == null ? null : data.DataRows;
        }
        #endregion

        #region --生成Excel模板的页签定义--
        public List<SheetDefine> GetSheetDefine(string funcname, out string filename)
        {
            var defines = new List<SheetDefine>();
            var templates = GetExcelTemplate(funcname, out filename);
            if (templates == null)
                throw new Exception("模板数据解析错误，请重新上传模板");
            foreach (var item in templates.DataSheets)
            {
                defines.Add(item);
            }
            return defines;
        }

        private ExcelTemplate GetExcelTemplate(string funcname, out string filename)
        {
            var model = _repository.SelectExcelTemplate(funcname);
            if (model == null)
                throw new Exception("查询funcname数据失败");
            filename = model.ExcelName;
            if (model.Exceltype == 0)//模板生成
            {
                var templates = JsonConvert.DeserializeObject<ExcelTemplate>(model.TemplateContent);
                if (templates == null || templates.DataSheets == null || templates.DataSheets.Count == 0)
                    throw new Exception("模板数据解析错误，请重新上传模板");
                return templates;
            }
            return null;
        }
        #endregion

        #region --生成动态模板--
        private List<SheetDefine> GeneralDynamicTemplate(Guid entityId, List<string> nestTableList, ExcelOperateType operateType, ExportDataColumnSourceEnum columnSource, int userNumber)
        {
            List<SheetDefine> defines = new List<SheetDefine>();
            switch (operateType)
            {
                case ExcelOperateType.ImportAdd:
                case ExcelOperateType.ImportUpdate:
                    defines = GeneralDynamicTemplate_Import(entityId, nestTableList, operateType, userNumber);
                    break;
                default:
                    defines = GeneralDynamicTemplate_Export(entityId, nestTableList, operateType, columnSource, userNumber);
                    break;
            }

            return defines;
        }
        #region --生成动态模板的导出模板定义--
        private List<SheetDefine> GeneralDynamicTemplate_Export(Guid entityId, List<string> nestTableList, ExcelOperateType operateType, ExportDataColumnSourceEnum columnSource, int userNumber)
        {
            List<SheetDefine> defines = new List<SheetDefine>();
            if (nestTableList == null) nestTableList = new List<string>();
            try
            {
                List<IDictionary<string, object>> typeFields = null;
                #region 根据列来源参数，获取主表所需显示的列
                if (columnSource == ExportDataColumnSourceEnum.WEB_Standard)
                {
                    //根据WEB列表标准选项导出
                    Dictionary<string, List<IDictionary<string, object>>> typeVisibleFields = null;
                    typeVisibleFields = _entityProRepository.FieldWebVisibleQuery(entityId.ToString(), userNumber);
                    if (!typeVisibleFields.ContainsKey("FieldVisible"))
                        throw new Exception("获取实体显示字段接口报错，缺少FieldVisible参数的结果集");
                    typeFields = typeVisibleFields["FieldVisible"];
                }
                else if (columnSource == ExportDataColumnSourceEnum.WEB_Personal)
                {
                    //这种情况是根据个人情况导出
                    Dictionary<string, List<IDictionary<string, object>>> typeVisibleFields = null;
                    typeVisibleFields = _entityProRepository.FieldWebVisibleQuery(entityId.ToString(), userNumber);
                    if (!typeVisibleFields.ContainsKey("FieldVisible"))
                        throw new Exception("获取实体显示字段接口报错，缺少FieldVisible参数的结果集");
                    List<IDictionary<string, object>> standardFields = typeVisibleFields["FieldVisible"];
                    Dictionary<string, object> detail = _dynamicEntityRepository.GetPersonalWebListColumnsSetting(entityId, userNumber, null);
                    if (detail != null && detail.ContainsKey("viewconfig") && detail["viewconfig"] != null)
                    {
                        typeFields = new List<IDictionary<string, object>>();
                        WebListPersonalViewSettingInfo view = Newtonsoft.Json.JsonConvert.DeserializeObject<WebListPersonalViewSettingInfo>(detail["viewconfig"].ToString());
                        foreach (WebListPersonalViewColumnSettingInfo column in view.Columns)
                        {
                            //判断是否在标准列表中
                            if (column.IsDisplay != 1) continue;
                            if (standardFields.Exists((IDictionary<string, object> o) => o["fieldid"].ToString().Equals(column.FieldId.ToString())))
                            {
                                IDictionary<string, object> item = standardFields.Where<IDictionary<string, object>>((IDictionary<string, object> o) => o["fieldid"].ToString().Equals(column.FieldId.ToString())).First();
                                if (item != null)
                                    typeFields.Add(item);
                            }
                        }
                        //把没有的补在最后
                        foreach (IDictionary<string, object> item in standardFields)
                        {
                            string fieldid = item["fieldid"].ToString();
                            if (view.Columns.Exists((WebListPersonalViewColumnSettingInfo o) => o.FieldId.ToString().Equals(fieldid))) continue;
                            typeFields.Add(item);
                        }
                    }
                    else
                    {
                        typeFields = standardFields;
                    }
                }
                else if (columnSource == ExportDataColumnSourceEnum.All_Columns)
                {
                    //导出全部字段
                    typeFields = new List<IDictionary<string, object>>();
                    List<DynamicEntityFieldSearch> tmp = _dynamicEntityRepository.GetEntityFields(entityId, userNumber);
                    foreach (DynamicEntityFieldSearch item in tmp)
                    {
                        Dictionary<string, object> newItem = new Dictionary<string, object>();
                        //fieldid,displayname,fieldname,controltype,fieldconfig
                        newItem.Add("fieldid", item.EntityId);
                        newItem.Add("fieldname", item.FieldName);
                        newItem.Add("controltype", item.ControlType);
                        newItem.Add("fieldconfig", item.FieldConfig);
                        newItem.Add("displayname", item.DisplayName);
                        typeFields.Add(newItem);
                    }
                }
                else
                {
                    throw (new Exception("参数异常"));
                }
                #endregion

                var entityInfo = _dynamicEntityRepository.getEntityBaseInfoById(entityId, userNumber);
                if (entityInfo == null)
                    throw new Exception("实体信息不存在");
                var modelType = Convert.ToInt32(entityInfo["modeltype"].ToString());
                List<MultiHeader> headers = new List<MultiHeader>();

                foreach (var field in typeFields)
                {
                    if (!field.ContainsKey("displayname") || !field.ContainsKey("fieldname") || !field.ContainsKey("controltype"))
                        throw new Exception("获取实体显示字段接口缺少必要参数");
                    if (field["displayname"] == null || field["fieldname"] == null || field["controltype"] == null)
                        throw new Exception("获取实体显示字段接口必要参数数据不允许为空");
                    var displayname = field["displayname"].ToString();
                    var fieldname = field["fieldname"].ToString();
                    var controltype = field["controltype"].ToString();

                    FieldType fieldTypetemp = FieldType.Text;
                    if (modelType == 3 && field["fieldname"].ToString() == "recrelateid")//动态实体走别的逻辑
                    {
                        var relField = _dynamicEntityRepository.GetEntityFields(Guid.Parse(entityInfo["relentityid"].ToString()), userNumber).FirstOrDefault(t => t.FieldId == Guid.Parse(entityInfo["relfieldid"].ToString()));
                        displayname = relField.DisplayName;
                        fieldname = entityInfo["relfieldname"].ToString();
                        controltype = relField.ControlType.ToString();
                        ConstructField(controltype, out fieldTypetemp);
                    }
                    else
                    {
                        ConstructField(controltype, out fieldTypetemp);
                    }
                    MultiHeader header = new MultiHeader() { HeaderType = 1, FieldName = fieldname, HeaderText = displayname, Width = 150, FieldType = fieldTypetemp, SubHeaders = new List<MultiHeader>() };
                    headers.Add(header);
                    if (int.Parse(controltype) == (int)EntityFieldControlType.LinkeTable && nestTableList.Exists((string s) => s.Equals(fieldname)))
                    {
                        var fieldConfig = field["fieldconfig"].ToString();
                        DynamicProtocolFieldConfig fieldConfigInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<DynamicProtocolFieldConfig>(fieldConfig);
                        if (fieldConfigInfo.EntityId != null && !fieldConfigInfo.EntityId.Equals(Guid.Empty))
                        {
                            List<IDictionary<string, object>> sub_typeFields = null;
                            if (columnSource == ExportDataColumnSourceEnum.WEB_Personal || columnSource == ExportDataColumnSourceEnum.WEB_Standard)
                            {
                                var sub_typeVisibleFields = _entityProRepository.FieldWebVisibleQuery(fieldConfigInfo.EntityId.ToString(), userNumber);
                                if (!sub_typeVisibleFields.ContainsKey("FieldVisible"))
                                    throw new Exception("获取嵌套实体显示字段接口报错，缺少FieldVisible参数的结果集");
                                var sub_entityInfo = _dynamicEntityRepository.getEntityBaseInfoById(fieldConfigInfo.EntityId, userNumber);
                                if (entityInfo == null)
                                    throw new Exception("嵌套实体信息不存在");
                                sub_typeFields = sub_typeVisibleFields["FieldVisible"];
                            }
                            else
                            {
                                sub_typeFields = new List<IDictionary<string, object>>();
                                List<DynamicEntityFieldSearch> tmp = _dynamicEntityRepository.GetEntityFields(fieldConfigInfo.EntityId, userNumber);
                                foreach (DynamicEntityFieldSearch item in tmp)
                                {
                                    Dictionary<string, object> newItem = new Dictionary<string, object>();
                                    //fieldid,displayname,fieldname,controltype,fieldconfig
                                    newItem.Add("fieldid", item.EntityId);
                                    newItem.Add("fieldname", item.FieldName);
                                    newItem.Add("controltype", item.ControlType);
                                    newItem.Add("fieldconfig", item.FieldConfig);
                                    newItem.Add("displayname", item.DisplayName);
                                    sub_typeFields.Add(newItem);
                                }
                            }

                            header.HeaderType = 0;
                            foreach (var sub_field in sub_typeFields)
                            {
                                if (!sub_field.ContainsKey("displayname") || !sub_field.ContainsKey("fieldname") || !sub_field.ContainsKey("controltype"))
                                    throw new Exception("获取实体显示字段接口缺少必要参数");
                                if (sub_field["displayname"] == null || sub_field["fieldname"] == null || sub_field["controltype"] == null)
                                    throw new Exception("获取实体显示字段接口必要参数数据不允许为空");
                                var sub_displayname = sub_field["displayname"].ToString();
                                var sub_fieldname = sub_field["fieldname"].ToString();
                                var sub_controltype = sub_field["controltype"].ToString();
                                FieldType sub_fieldTypetemp = FieldType.Text;
                                ConstructField(sub_controltype, out sub_fieldTypetemp);
                                sub_fieldname = fieldname + "." + sub_fieldname;
                                MultiHeader sub_header = new MultiHeader() { HeaderType = 1, FieldName = sub_fieldname, HeaderText = sub_displayname, Width = 150, FieldType = sub_fieldTypetemp };
                                header.SubHeaders.Add(sub_header);
                            }
                            //嵌套表格，需要处理嵌套表格内部字段
                        }
                    }
                }
                defines.Add(new MultiHeaderSheetTemplate()
                {
                    SheetName = "Sheet 1",
                    ExecuteSQL = "",
                    Headers = headers,
                    //DataObject = typeFields
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return defines;
        }

        private void ConstructField(string controlType, out FieldType fieldTypetemp)
        {
            fieldTypetemp = FieldType.Text;
            int controltypeInt = 0;
            int.TryParse(controlType, out controltypeInt);
            switch ((DynamicProtocolControlType)controltypeInt)
            {
                case DynamicProtocolControlType.TimeDate:
                    fieldTypetemp = FieldType.TimeDate;
                    break;
                case DynamicProtocolControlType.TakePhoto:
                    fieldTypetemp = FieldType.Image;
                    break;
                case DynamicProtocolControlType.Address:
                    fieldTypetemp = FieldType.Address;
                    break;
                //case DynamicProtocolControlType.FileAttach:
                //    fieldTypetemp = FieldType.File;
                //    break;
                case DynamicProtocolControlType.SelectSingle:
                case DynamicProtocolControlType.SelectMulti:
                case DynamicProtocolControlType.Location:
                case DynamicProtocolControlType.AreaRegion:
                case DynamicProtocolControlType.Department:
                case DynamicProtocolControlType.DataSourceSingle:
                case DynamicProtocolControlType.DataSourceMulti:
                case DynamicProtocolControlType.TreeSingle:
                case DynamicProtocolControlType.PersonSelectSingle:
                case DynamicProtocolControlType.PersonSelectMulti:
                case DynamicProtocolControlType.TreeMulti:
                case DynamicProtocolControlType.RecCreator:
                case DynamicProtocolControlType.RecUpdator:
                case DynamicProtocolControlType.RecManager:
                case DynamicProtocolControlType.RecType:
                case DynamicProtocolControlType.RecAudits:
                case DynamicProtocolControlType.RecStatus:
                case DynamicProtocolControlType.QuoteControl:
                case DynamicProtocolControlType.Product:
                case DynamicProtocolControlType.ProductSet:
                case DynamicProtocolControlType.SalesStage:
                    fieldTypetemp = FieldType.reference;
                    break;
                case DynamicProtocolControlType.RelateControl:
                    fieldTypetemp = FieldType.Related;
                    break;
                case DynamicProtocolControlType.HeadPhoto:
                case DynamicProtocolControlType.AreaGroup:
                case DynamicProtocolControlType.TipText:
                case DynamicProtocolControlType.LinkeTable:
                case DynamicProtocolControlType.FileAttach:
                    break;
                case DynamicProtocolControlType.NumberDecimal:
                    fieldTypetemp = FieldType.NumberDecimal;
                    break;
                case DynamicProtocolControlType.NumberInt:

                    fieldTypetemp = FieldType.NumberInt;
                    break;
                    //case DynamicProtocolControlType.RecId:
                    //case DynamicProtocolControlType.RecItemid:
                    //case DynamicProtocolControlType.RecStatus:
                    //    continue;

            }
        }
        #endregion
        #region --生成动态模板的导入模板定义（For 嵌套表格）--
        private List<SheetDefine> GeneralDetailTemplate_Import(Guid entityId, Guid MainTypeId, ExcelOperateType operateType, int userNumber)
        {
            List<SheetDefine> defines = new List<SheetDefine>();
            EntityTypeQueryMapper entityType = new EntityTypeQueryMapper()
            {
                EntityId = entityId.ToString()
            };
            Guid relTypeId = _dynamicEntityRepository.getGridTypeByMainType(MainTypeId, entityId);
            List<DynamicEntityDataFieldMapper> fields = _entityServices.GetGridTypeFields(relTypeId, entityId, DynamicProtocolOperateType.Add, userNumber);
            try
            {
                List<SimpleHeader> headers = new List<SimpleHeader>();
                foreach (var field in fields)
                {

                    if (!ExportFieldSelect(field, operateType))
                        continue;
                    FieldType tempFieldType = FieldType.Text;
                    //处理特殊类型字段，比如日期类型字段时只获取日期部分
                    switch ((DynamicProtocolControlType)field.ControlType)
                    {
                        case DynamicProtocolControlType.TimeDate:
                            tempFieldType = FieldType.TimeDate;
                            break;
                        case DynamicProtocolControlType.TimeStamp:
                            tempFieldType = FieldType.TimeStamp;
                            break;
                        case DynamicProtocolControlType.NumberDecimal:
                            tempFieldType = FieldType.NumberDecimal;
                            break;
                    }


                    headers.Add(new SimpleHeader() { FieldName = field.FieldName, HeaderText = field.DisplayName, IsNotEmpty = field.IsRequire, Width = 150, FieldType = tempFieldType });
                }
                defines.Add(new SimpleSheetTemplate()
                {
                    SheetName = "Sheet1",
                    ExecuteSQL = "",
                    Headers = headers,
                    DataObject = fields
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return defines;
        }
        #endregion

        #region --生成动态模板的导入模板定义--
        private List<SheetDefine> GeneralDynamicTemplate_Import(Guid entityId, List<string> nestTableList, ExcelOperateType operateType, int userNumber)
        {
            List<SheetDefine> defines = new List<SheetDefine>();
            EntityTypeQueryMapper entityType = new EntityTypeQueryMapper()
            {
                EntityId = entityId.ToString()
            };
            bool isTypeChanged = _entityProRepository.CheckAndNestEntityType(entityType.EntityId, userNumber);
            if (isTypeChanged)
            {
                IncreaseDataVersion(DomainModel.Version.DataVersionType.EntityData);
            }
            var entityTypes = _entityProRepository.EntityTypeQuery(entityType, userNumber).FirstOrDefault().Value;
            try
            {
                foreach (var typeModel in entityTypes)
                {
                    var recstatus = typeModel["recstatus"];
                    if (recstatus == null || int.Parse(recstatus.ToString()) != 1)
                    {
                        continue;
                    }
                    var categoryid = typeModel["categoryid"].ToString();
                    var categoryname = typeModel["categoryname"].ToString();
                    var typeId = new Guid(categoryid);
                    var typeFields = _entityServices.GetTypeFields(typeId, DynamicProtocolOperateType.ImportAdd, userNumber);
                    var subTypes = new Dictionary<string, object>();
                    #region  检查是否存在嵌套表格字段

                    #endregion
                    List<MultiHeader> headers = new List<MultiHeader>();
                    foreach (var field in typeFields)
                    {

                        int rowSpan = 2;
                        int colSpan = 1;
                        List<MultiHeader> SubHeaders = null;
                        if (!ExportFieldSelect(field, operateType))
                            continue;
                        FieldType tempFieldType = FieldType.Text;
                        List<DynamicEntityDataFieldMapper> subtypeFields = null;
                        //处理特殊类型字段，比如日期类型字段时只获取日期部分
                        switch ((DynamicProtocolControlType)field.ControlType)
                        {
                            case DynamicProtocolControlType.TimeDate:
                                tempFieldType = FieldType.TimeDate;
                                break;
                            case DynamicProtocolControlType.TimeStamp:
                                tempFieldType = FieldType.TimeStamp;
                                break;
                            case DynamicProtocolControlType.NumberDecimal:
                                tempFieldType = FieldType.NumberDecimal;
                                break;
                            case DynamicProtocolControlType.LinkeTable:
                                rowSpan = 1;
                                SubHeaders = GeneralTableTemplate_Import(operateType, field, entityId, categoryid, ref colSpan, userNumber, out subtypeFields);
                                if (subtypeFields != null)
                                {
                                    subTypes.Add(field.FieldName, subtypeFields);
                                }
                                break;
                        }


                        headers.Add(new MultiHeader()
                        {
                            FieldName = field.FieldName,
                            HeaderText = field.DisplayName,
                            IsNotEmpty = field.IsRequire,
                            Width = 150,
                            FieldType = tempFieldType,
                            RowSpan = rowSpan,
                            ColSpan = colSpan,
                            HeaderType = (rowSpan == 1 ? 2 : 1),
                            SubHeaders = SubHeaders
                        });
                    }
                    defines.Add(new MultiHeaderSheetTemplate()
                    {
                        SheetName = categoryname,
                        ExecuteSQL = "",
                        Headers = headers,
                        DataObject = typeFields,
                        SubDataObject = subTypes
                    });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return defines;
        }
        private List<MultiHeader> GeneralTableTemplate_Import(ExcelOperateType operateType, DynamicEntityDataFieldMapper fieldInfo, Guid parentEntityId,
            string categoryid, ref int colSpan, int userNumber,
            out List<DynamicEntityDataFieldMapper> typeFields)
        {
            DynamicProtocolFieldConfig fieldConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<DynamicProtocolFieldConfig>(fieldInfo.FieldConfig);
            Guid tableTypeId = this._dynamicEntityRepository.getGridTypeByMainType(Guid.Parse(categoryid), fieldConfig.EntityId);
            typeFields = _entityServices.GetTypeFields(tableTypeId, DynamicProtocolOperateType.ImportAdd, userNumber);
            List<MultiHeader> retHeader = new List<MultiHeader>();
            foreach (var field in typeFields)
            {
                if (!ExportFieldSelect(field, operateType))
                    continue;
                FieldType tempFieldType = FieldType.Text;
                //处理特殊类型字段，比如日期类型字段时只获取日期部分
                switch ((DynamicProtocolControlType)field.ControlType)
                {
                    case DynamicProtocolControlType.TimeDate:
                        tempFieldType = FieldType.TimeDate;
                        break;
                    case DynamicProtocolControlType.TimeStamp:
                        tempFieldType = FieldType.TimeStamp;
                        break;
                    case DynamicProtocolControlType.NumberDecimal:
                        tempFieldType = FieldType.NumberDecimal;
                        break;
                    case DynamicProtocolControlType.LinkeTable:
                        continue;
                }


                retHeader.Add(new MultiHeader()
                {
                    FieldName = field.FieldName,
                    HeaderText = field.DisplayName,
                    IsNotEmpty = field.IsRequire,
                    Width = 150,
                    FieldType = tempFieldType,
                    RowSpan = 1,
                    ColSpan = 1,
                    HeaderType = 1
                });
            }
            return retHeader;
        }
        #endregion

        //过滤不可导出的字段
        private bool ExportFieldSelect(DynamicEntityDataFieldMapper field, ExcelOperateType operateType)
        {
            if (field.IsVisible == false)
            {
                return false;
            }

            bool result = true;
            //只有导入字段需要过滤，导出的字段通过是否可见判断
            if (operateType != ExcelOperateType.Export)
            {
                if (field.IsReadOnly)
                {
                    return false;
                }
                switch ((DynamicProtocolControlType)field.ControlType)
                {
                    case DynamicProtocolControlType.Department://团队组织
                        if (field.FieldType == 1)
                            result = false;
                        break;
                    case DynamicProtocolControlType.Location://定位
                    case DynamicProtocolControlType.HeadPhoto://头像
                    case DynamicProtocolControlType.AreaGroup://分组
                    case DynamicProtocolControlType.TakePhoto://拍照
                    case DynamicProtocolControlType.FileAttach://附件
                    //case DynamicProtocolControlType.LinkeTable://表格控件
                    case DynamicProtocolControlType.TreeMulti://树形多选
                    case DynamicProtocolControlType.TreeSingle://树形
                    case DynamicProtocolControlType.RecId://记录ID
                    case DynamicProtocolControlType.RecUpdator://创建人
                    case DynamicProtocolControlType.RecCreated://创建时间
                    case DynamicProtocolControlType.RecUpdated://更新时间
                    case DynamicProtocolControlType.RecAudits://审批状态
                    case DynamicProtocolControlType.RecStatus://记录状态
                    case DynamicProtocolControlType.RecType://记录类型
                    case DynamicProtocolControlType.RecItemid://明细ID
                    case DynamicProtocolControlType.RecOnlive://活动时间
                        result = false;
                        break;
                }
            }
            return result;
        }
        #endregion

        #region --验证每行数据--
        private bool ValidationRowData(SheetDefine sheetDefine, ExcelOperateType operateType, Dictionary<string, object> rowdata, int userno, out string errorMsg)
        {
            if (sheetDefine is MultiHeaderSheetTemplate)
            {
                var sheetTemplate = sheetDefine as MultiHeaderSheetTemplate;
                return ValidationRowData(sheetTemplate, operateType, rowdata, userno, out errorMsg);
            }
            else
            {
                var sheetTemplate = sheetDefine as SheetTemplate;
                return ValidationRowData(sheetTemplate, rowdata, out errorMsg);
            }
        }

        #region ---动态模板行数据校验---
        /// <summary>
        /// 动态模板行数据校验
        /// </summary>
        private bool ValidationRowData(MultiHeaderSheetTemplate sheetTemplate, ExcelOperateType operateType, Dictionary<string, object> rowdata, int userno, out string errorMsg)
        {
            errorMsg = null;
            bool result = true;
            Guid typeId = Guid.Empty;
            List<DynamicEntityDataFieldMapper> subFields = null;

            if (sheetTemplate.DataObject is List<DynamicEntityDataFieldMapper>)
            {
                var typeFields = sheetTemplate.DataObject as List<DynamicEntityDataFieldMapper>;
                typeId = typeFields.FirstOrDefault().TypeId;

                if (typeFields.Count == 0)
                {
                    result = false;
                    errorMsg = "没有配置字段";
                    return result;
                }
                DynamicProtocolOperateType tempType = DynamicProtocolOperateType.Add;
                switch (operateType)
                {
                    case ExcelOperateType.ImportAdd:
                        tempType = DynamicProtocolOperateType.Add;
                        break;
                    case ExcelOperateType.ImportUpdate:
                        tempType = DynamicProtocolOperateType.Edit;
                        break;
                }

                foreach (var m in typeFields)
                {
                    //字段过滤器，过滤一些只读字段等
                    if (!DynamicProtocolHelper.ValidFieldFilter(m, tempType))
                        continue;

                    object data = null;
                    if (rowdata.TryGetValue(m.FieldName, out data))
                    {
                        //执行实体配置中的数据校验
                        subFields = null;
                        if (m.ControlType == (int)DynamicProtocolControlType.LinkeTable
                            && sheetTemplate.SubDataObject != null && sheetTemplate.SubDataObject is Dictionary<string, object>
                            && ((Dictionary<string, object>)sheetTemplate.SubDataObject).ContainsKey(m.FieldName))
                        {
                            subFields = (List<DynamicEntityDataFieldMapper>)(((Dictionary<string, object>)sheetTemplate.SubDataObject)[m.FieldName]);
                        }
                        var validatResult = DynamicProtocolHelper.ValidFieldConfig(m, data, false, subFields);
                        if (validatResult != null && !validatResult.IsValid)
                        {
                            result = false;
                            errorMsg = validatResult.Tips;
                        }
                    }
                    if (m.IsRequire && (data == null || string.IsNullOrEmpty(data.ToString().Trim())))
                    {
                        result = false;
                        errorMsg = string.Format("缺少必填列:{0}", m.FieldLabel);
                    }
                    else if (data != null && result)
                    {
                        var columnValue = data.ToString().Trim();
                        //处理特殊控件类型的数据以及实体没有配置验证方式的数据校验
                        result = CheckFieldData(sheetTemplate, typeId, m, columnValue, rowdata, userno, out errorMsg);
                    }

                    //如果存在错误，则跳出循环
                    if (result == false)
                        break;

                }
            }
            return result;
        }

        //校验字段的数据格式和处理特殊控件
        public bool CheckFieldData(MultiHeaderSheetTemplate sheetTemplate, Guid typeId, DynamicEntityDataFieldMapper typeField, string columnValue, Dictionary<string, object> rowdata, int userno, out string errorMsg, Dictionary<string, object> fieldFilters = null)
        {
            errorMsg = null;
            List<DynamicEntityDataFieldMapper> subTypes = null;
            switch ((DynamicProtocolControlType)typeField.ControlType)
            {

                case DynamicProtocolControlType.NumberInt://整数
                    if (!string.IsNullOrEmpty(columnValue) && !CommonHelper.IsInt(columnValue))
                    {
                        errorMsg = string.Format("{0}不匹配整数格式", typeField.FieldLabel);
                    }
                    break;
                case DynamicProtocolControlType.NumberDecimal://小数
                    if (!string.IsNullOrEmpty(columnValue) && !CommonHelper.IsNumeric(columnValue))
                    {
                        errorMsg = string.Format("{0}不匹配小数格式", typeField.FieldLabel);
                    }
                    break;
                case DynamicProtocolControlType.TimeDate://日期，年月日
                    if (!string.IsNullOrEmpty(columnValue))
                    {
                        DateTime dt;
                        if (!DateTime.TryParse(columnValue.Trim(), out dt) || !IsTimeDate(string.Format("{0:yyyy-MM-dd}", dt)))
                        {
                            errorMsg = string.Format("{0}不匹配日期格式", typeField.FieldLabel);
                        }
                        else
                        {
                            rowdata[typeField.FieldName] = string.Format("{0:yyyy-MM-dd}", dt);
                        }
                    }
                    break;
                case DynamicProtocolControlType.TimeStamp:// 日期时间
                    if (!string.IsNullOrEmpty(columnValue) && !IsTimeStamp(columnValue))
                    {
                        errorMsg = string.Format("{0}不匹配时间格式", typeField.FieldLabel);
                    }
                    break;
                case DynamicProtocolControlType.PhoneNum://手机号
                    if (!string.IsNullOrEmpty(columnValue) && !IsPhoneNum(columnValue))
                    {
                        errorMsg = string.Format("{0}不匹配手机号码格式", typeField.FieldLabel);
                    }
                    break;
                case DynamicProtocolControlType.EmailAddr://邮箱地址
                    if (!string.IsNullOrEmpty(columnValue) && !IsEmailAddr(columnValue))
                    {
                        errorMsg = string.Format("{0}不匹配邮箱格式", typeField.FieldLabel);
                    }
                    break;
                case DynamicProtocolControlType.Telephone://电话
                                                          //if (!string.IsNullOrEmpty(columnValue) && !IsTelephone(columnValue))
                                                          //{
                                                          //    errorMsg = string.Format("{0}不匹配电话号码格式", typeField.FieldLabel);
                                                          //}
                    break;
                case DynamicProtocolControlType.Address:
                    //if (!string.IsNullOrEmpty(columnValue))
                    {
                        var lat = 0;
                        var lon = 0;
                        var address = new { lat = lat, lon = lon, address = columnValue };
                        rowdata[typeField.FieldName] = JsonHelper.ToJson(address);
                    }
                    break;

                case DynamicProtocolControlType.SelectSingle://本地字典单选
                case DynamicProtocolControlType.SelectMulti://本地字典多选
                    {
                        ValidationDictionary(typeField, columnValue, rowdata, out errorMsg);
                    }
                    break;

                case DynamicProtocolControlType.AreaRegion://行政区域
                    {
                        if (!string.IsNullOrEmpty(columnValue))
                        {
                            var regionid = _repository.GetAreaRegionId(columnValue, out errorMsg);
                            if (string.IsNullOrEmpty(errorMsg))
                            {
                                rowdata[typeField.FieldName] = regionid;
                            }
                            else errorMsg = string.Format("{0}:{1}", typeField.FieldLabel, errorMsg);
                        }
                    }
                    break;

                case DynamicProtocolControlType.PersonSelectMulti://多选人
                    {
                        if (!string.IsNullOrEmpty(columnValue))
                        {
                            List<string> names = columnValue.Split(',').ToList();
                            var userids = _repository.GetRecManagerId(names, out errorMsg);
                            if (string.IsNullOrEmpty(errorMsg))
                            {
                                rowdata[typeField.FieldName] = string.Join(",", userids.ToArray());
                                rowdata[typeField.FieldName + "_name"] = string.Join(",", names.ToArray());
                            }
                            else errorMsg = string.Format("{0}:{1}", typeField.FieldLabel, errorMsg);
                        }
                    }
                    break;
                case DynamicProtocolControlType.PersonSelectSingle://单选人
                case DynamicProtocolControlType.RecManager://负责人
                    {
                        if (!string.IsNullOrEmpty(columnValue))
                        {
                            object useriddata = null;
                            var fieldConfig = JObject.Parse(typeField.FieldConfig);
                            if (fieldConfig["multiple"] != null && fieldConfig["multiple"].ToString() == "1")
                            {
                                List<string> names = columnValue.Split(',').ToList();
                                var userids = _repository.GetRecManagerId(names, out errorMsg);
                                useriddata = string.Join(",", userids.ToArray());
                            }
                            else
                            {
                                useriddata = _repository.GetRecManagerId(columnValue, out errorMsg);
                            }
                            if (string.IsNullOrEmpty(errorMsg))
                            {
                                rowdata[typeField.FieldName] = useriddata;
                                rowdata[typeField.FieldName + "_name"] = columnValue;
                            }
                            else errorMsg = string.Format("{0}:{1}", typeField.FieldLabel, errorMsg);
                        }
                    }
                    break;

                case DynamicProtocolControlType.DataSourceSingle://数据源单选
                case DynamicProtocolControlType.DataSourceMulti://数据源多选
                    {
                        if (!string.IsNullOrEmpty(columnValue))
                        {
                            ValidationDataSource(typeField, columnValue, rowdata, userno, out errorMsg);
                        }
                        else rowdata[typeField.FieldName] = "{}";

                    }
                    break;

                case DynamicProtocolControlType.Product:
                    if (!string.IsNullOrEmpty(columnValue))
                    {
                        var pid = _repository.GetProductId(columnValue, out errorMsg, fieldFilters);
                        if (string.IsNullOrEmpty(errorMsg))
                        {
                            rowdata[typeField.FieldName] = pid;
                            rowdata[typeField.FieldName + "_name"] = columnValue;
                        }
                        else errorMsg = string.Format("{0}:{1}", typeField.FieldLabel, errorMsg);
                    }
                    break;
                case DynamicProtocolControlType.ProductSet:
                    if (!string.IsNullOrEmpty(columnValue))
                    {
                        var pid = _repository.GetProductSeriesId(columnValue, out errorMsg);
                        if (string.IsNullOrEmpty(errorMsg))
                        {
                            rowdata[typeField.FieldName] = pid;
                            rowdata[typeField.FieldName + "_name"] = columnValue;
                        }
                        else errorMsg = string.Format("{0}:{1}", typeField.FieldLabel, errorMsg);
                    }
                    break;

                case DynamicProtocolControlType.Department://团队组织
                    if (!string.IsNullOrEmpty(columnValue) && typeField.FieldType != 1)
                    {
                        ValidationDepartment(typeField, columnValue, rowdata, userno, out errorMsg);

                    }
                    break;
                case DynamicProtocolControlType.SalesStage:
                    if (!string.IsNullOrEmpty(columnValue))
                    {
                        var pid = _repository.GetSalesStageId(typeId, columnValue, out errorMsg);
                        if (string.IsNullOrEmpty(errorMsg))
                        {
                            rowdata[typeField.FieldName] = pid;
                        }
                        else errorMsg = string.Format("{0}:{1}", typeField.FieldLabel, errorMsg);
                    }
                    break;

                case DynamicProtocolControlType.LinkeTable://表格控件

                    rowdata["org_data_" + typeField.FieldName] = new List<Dictionary<string, object>>();
                    if (rowdata.ContainsKey(typeField.FieldName) && rowdata[typeField.FieldName] != null)
                    {

                        List<Dictionary<string, object>> subRowDataList = (List<Dictionary<string, object>>)rowdata[typeField.FieldName];
                        foreach (Dictionary<string, object> rowDataInfo in subRowDataList)
                        {
                            if (rowDataInfo.ContainsKey("FieldData"))
                            {
                                string tmpStr = Newtonsoft.Json.JsonConvert.SerializeObject(rowDataInfo["FieldData"]);
                                ((List<Dictionary<string, object>>)rowdata["org_data_" + typeField.FieldName]).Add(new Dictionary<string, object>(JsonConvert.DeserializeObject<Dictionary<string, object>>(tmpStr)));
                            }
                            else
                            {
                                ((List<Dictionary<string, object>>)rowdata["org_data_" + typeField.FieldName]).Add(new Dictionary<string, object>());
                                return false;
                            }
                        }
                    }
                    if (sheetTemplate != null && sheetTemplate.SubDataObject != null && ((Dictionary<string, object>)sheetTemplate.SubDataObject).ContainsKey(typeField.FieldName))
                    {
                        subTypes = (List<DynamicEntityDataFieldMapper>)(((Dictionary<string, object>)sheetTemplate.SubDataObject)[typeField.FieldName]);
                    }
                    if (subTypes != null)
                    {
                        List<Dictionary<string, object>> subRowDataList = (List<Dictionary<string, object>>)rowdata[typeField.FieldName];
                        foreach (Dictionary<string, object> rowDataInfo in subRowDataList)
                        {
                            Guid subTypeId = Guid.Empty;
                            if (rowDataInfo.ContainsKey("TypeId"))
                            {
                                if (Guid.TryParse(rowDataInfo["TypeId"].ToString(), out subTypeId) == false)
                                {
                                    errorMsg = "嵌套表格的typeid格式异常";
                                    return false;
                                }
                            }
                            else
                            {
                                errorMsg = "嵌套表格的typeid未定义";
                                return false;
                            }
                            if (rowDataInfo.ContainsKey("FieldData"))
                            {
                                Dictionary<string, object> subRow = (Dictionary<string, object>)rowDataInfo["FieldData"];
                                foreach (DynamicEntityDataFieldMapper subTypeField in subTypes)
                                {
                                    if (subRow.ContainsKey(subTypeField.FieldName) && subRow[subTypeField.FieldName] != null)
                                    {
                                        string subColumnValue = subRow[subTypeField.FieldName].ToString();
                                        string subErrorMsg = "";
                                        if (CheckFieldData(sheetTemplate, subTypeId, subTypeField, subColumnValue, subRow, userno, out subErrorMsg) == false)
                                        {
                                            errorMsg = subErrorMsg;
                                            return false;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                errorMsg = "嵌套表格的数据异常";
                                return false;
                            }
                        }

                    }
                    rowdata[typeField.FieldName] = Newtonsoft.Json.JsonConvert.SerializeObject(rowdata[typeField.FieldName]);
                    break;
                case DynamicProtocolControlType.Location://定位
                case DynamicProtocolControlType.HeadPhoto://头像
                case DynamicProtocolControlType.AreaGroup://分组
                case DynamicProtocolControlType.TakePhoto://拍照
                case DynamicProtocolControlType.FileAttach://附件
                case DynamicProtocolControlType.TreeMulti://树形多选
                case DynamicProtocolControlType.TreeSingle://树形
                case DynamicProtocolControlType.RecId://记录ID
                case DynamicProtocolControlType.RecUpdator://创建人
                case DynamicProtocolControlType.RecCreated://创建时间
                case DynamicProtocolControlType.RecUpdated://更新时间
                case DynamicProtocolControlType.RecAudits://审批状态
                case DynamicProtocolControlType.RecStatus://记录状态
                case DynamicProtocolControlType.RecType://记录类型
                case DynamicProtocolControlType.RecItemid://明细ID
                case DynamicProtocolControlType.RecOnlive://活动时间
                case DynamicProtocolControlType.RecName://记录名称
                    break;
            }
            return string.IsNullOrEmpty(errorMsg);
        }
        private bool ValidationDictionary(DynamicEntityDataFieldMapper typeField, string columnValue, Dictionary<string, object> rowdata, out string errorMsg)
        {
            errorMsg = null;
            var fieldConfig = JsonHelper.ToObject<DynamicProtocolFieldConfig>(typeField.FieldConfig);
            if (fieldConfig == null)
            {
                errorMsg = string.Format("{0}配置有误，FieldConfig不能为空", typeField.FieldLabel);
            }
            else
            {
                if (fieldConfig.DataSource == null)
                {
                    errorMsg = string.Format("{0}配置有误，FieldConfig.DataSource不能为空", typeField.FieldLabel);
                    return false;
                }
                int dictypeid = 0;
                if (!int.TryParse(fieldConfig.DataSource.SourceId, out dictypeid))
                {
                    errorMsg = string.Format("{0}配置有误，字典类型不存在", typeField.FieldLabel);
                }
                //如果没填该字段的值，则获取配置中的默认值
                if (string.IsNullOrEmpty(columnValue))
                {
                    //只有特定字段需要设置默认值，其他的为空
                    //rowdata[typeField.FieldName] = fieldConfig.DefaultValue;
                    return true;
                }



                if ((DynamicProtocolControlType)typeField.ControlType == DynamicProtocolControlType.SelectSingle)
                {
                    var dataid = _repository.GetDictionaryDataId(dictypeid, columnValue, out errorMsg);
                    if (string.IsNullOrEmpty(errorMsg))
                    {
                        rowdata[typeField.FieldName] = dataid;
                    }
                    else errorMsg = string.Format("{0}:{1}", typeField.FieldLabel, errorMsg);
                }
                else
                {
                    List<string> dicDatas = columnValue.Split(',').ToList();
                    var dataidList = _repository.GetDictionaryDataId(dictypeid, dicDatas, out errorMsg);
                    if (string.IsNullOrEmpty(errorMsg))
                    {
                        rowdata[typeField.FieldName] = string.Join(",", dataidList.ToArray());
                    }
                    else errorMsg = string.Format("{0}:{1}", typeField.FieldLabel, errorMsg);
                }
            }
            return string.IsNullOrEmpty(errorMsg);
        }
        private bool ValidationDepartment(DynamicEntityDataFieldMapper typeField, string columnValue, Dictionary<string, object> rowdata, int userno, out string errorMsg)
        {
            errorMsg = null;
            var fieldConfig = JsonHelper.ToObject<DynamicProtocolFieldConfig>(typeField.FieldConfig);
            if (fieldConfig == null)
            {
                errorMsg = string.Format("{0}配置有误，FieldConfig不能为空", typeField.FieldLabel);
            }
            else
            {
                //如果没填该字段的值，则获取配置中的默认值
                if (string.IsNullOrEmpty(columnValue))
                {
                    rowdata[typeField.FieldName] = fieldConfig.DefaultValue;
                    return true;
                }
                object datainfo = null;
                if ((DynamicProtocolControlType)typeField.ControlType == DynamicProtocolControlType.Department)
                {
                    if (fieldConfig.Multiple != 1)
                    {
                        var pid = _repository.GetDepartmentId(columnValue, userno, out errorMsg);
                        if (string.IsNullOrEmpty(errorMsg))
                        {
                            rowdata[typeField.FieldName] = pid;
                        }
                        else errorMsg = string.Format("{0}:{1}", typeField.FieldLabel, errorMsg);
                    }
                    else
                    {
                        List<string> dicDatas = columnValue.Split(',').ToList();
                        string id = "";
                        foreach (string dic in dicDatas)
                        {
                            string tmpError = "";
                            var pid = _repository.GetDepartmentId(dic, userno, out tmpError);
                            if (string.IsNullOrEmpty(tmpError))
                            {
                                id = id + "," + pid;
                            }
                            else errorMsg = string.Format("{2},{0}:{1}", typeField.FieldLabel, tmpError, errorMsg);
                        }
                        if (id.Length > 0) id = id.Substring(1);
                        datainfo = id;
                    }
                }
                else
                {
                    //不会进这里来，已经不在使用多选数据源，而是改为单选数据的多选配置
                    /* List<string> dicDatas = columnValue.Split(',').ToList();
                     datainfo = _repository.GetDataSourceMapDataId(ruleSql, dicDatas, out errorMsg);
                     */
                }
                if (string.IsNullOrEmpty(errorMsg))
                {
                    rowdata[typeField.FieldName] = datainfo;
                }
                else errorMsg = string.Format("{0}:{1}", typeField.FieldLabel, errorMsg);
            }
            return string.IsNullOrEmpty(errorMsg);
        }
        //验证处理数据源单选和多选的数据
        private bool ValidationDataSource(DynamicEntityDataFieldMapper typeField, string columnValue, Dictionary<string, object> rowdata, int userno, out string errorMsg)
        {

            errorMsg = null;
            var fieldConfig = JsonHelper.ToObject<DynamicProtocolFieldConfig>(typeField.FieldConfig);
            if (fieldConfig == null)
            {
                errorMsg = string.Format("{0}配置有误，FieldConfig不能为空", typeField.FieldLabel);
            }
            else
            {
                //如果没填该字段的值，则获取配置中的默认值
                if (string.IsNullOrEmpty(columnValue))
                {
                    rowdata[typeField.FieldName] = fieldConfig.DefaultValue;
                    return true;
                }

                if (fieldConfig.DataSource == null)
                {
                    errorMsg = string.Format("{0}配置有误，FieldConfig.DataSource不能为空", typeField.FieldLabel);
                    return false;
                }
                DataSourceDetailMapper datesource = new DataSourceDetailMapper()
                {
                    DatasourceId = fieldConfig.DataSource.SourceId
                };
                var dataSourceDetail = _dataSourceRepository.SelectDataSourceDetail(datesource, userno);
                if (dataSourceDetail == null || dataSourceDetail.Count == 0 || dataSourceDetail.First().Value == null || dataSourceDetail.First().Value.Count == 0)
                {
                    errorMsg = string.Format("{0}配置有误，FieldConfig.DataSource不能为空", typeField.FieldLabel);
                    return false;
                }
                var recDic = dataSourceDetail.First().Value.FirstOrDefault();
                if (!recDic.ContainsKey("rulesql") || recDic["rulesql"] == null || recDic["rulesql"].ToString().Trim() == string.Empty)
                {
                    errorMsg = string.Format("{0}列配置有误，FieldConfig.DataSource中的rulesql不能为空", typeField.FieldLabel);
                    return false;
                }
                var ruleSql = recDic["rulesql"].ToString().Trim();
                ruleSql = ruleSql.Replace(",{querydata}", ",'{querydata}'");
                ruleSql = ruleSql.Replace(",{needpower}", ",0");
                ruleSql = _roleRepository.FormatRoleRule(ruleSql, userno);
                object datainfo = null;
                if ((DynamicProtocolControlType)typeField.ControlType == DynamicProtocolControlType.DataSourceSingle)
                {
                    if (fieldConfig.Multiple != 1)
                    {
                        datainfo = _repository.GetDataSourceMapDataId(ruleSql, columnValue, out errorMsg);
                    }
                    else
                    {
                        List<string> dicDatas = columnValue.Split(',').ToList();
                        datainfo = _repository.GetDataSourceMapDataId(ruleSql, dicDatas, out errorMsg);
                        string id = "";
                        string name = "";
                        List<Dictionary<string, object>> rows = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(Newtonsoft.Json.JsonConvert.SerializeObject(datainfo));

                        foreach (Dictionary<string, object> row in rows)
                        {
                            id = id + "," + (string)row["id"];
                            name = name + "," + (string)row["name"];
                        }
                        if (id.Length > 0) id = id.Substring(1);
                        if (name.Length > 0) name = name.Substring(1);
                        List<string> foundname = name.Split(',').ToList();
                        string unfoundname = string.Join(',', dicDatas.Except(foundname).ToArray());
                        if (unfoundname != null && unfoundname.Length > 0)
                        {
                            errorMsg = string.Format("{0}:{1}", typeField.FieldLabel, "没有找到以下数据:" + unfoundname);
                        }
                        else
                        {
                            Dictionary<string, object> realdata = new Dictionary<string, object>();
                            realdata["id"] = id;
                            realdata["name"] = name;
                            datainfo = realdata;
                        }
                    }
                }
                else
                {
                    //不会进这里来，已经不在使用多选数据源，而是改为单选数据的多选配置
                    /* List<string> dicDatas = columnValue.Split(',').ToList();
                     datainfo = _repository.GetDataSourceMapDataId(ruleSql, dicDatas, out errorMsg);
                     */
                }
                if (string.IsNullOrEmpty(errorMsg))
                {
                    rowdata[typeField.FieldName] = JsonHelper.ToJson(datainfo);
                }
                else errorMsg = string.Format("{0}:{1}", typeField.FieldLabel, errorMsg);
            }
            return string.IsNullOrEmpty(errorMsg);
        }
        #endregion

        #region ---固定模板行数据校验--
        /// <summary>
        /// 固定模板行数据校验
        /// </summary>
        private bool ValidationRowData(SheetTemplate sheetTemplate, Dictionary<string, object> rowdata, out string errorMsg)
        {
            errorMsg = null;
            bool result = true;
            long tmpValue = 0;
            foreach (var m in sheetTemplate.ColumnMap)
            {
                var columnText = OpenXMLExcelHelper.GetColumnName((uint)m.Index);
                if (m.IsNotEmpty)
                {
                    if (!rowdata.ContainsKey(m.FieldName))
                    {
                        result = false;
                        errorMsg = "缺少模板中的必填列";
                    }
                    if (rowdata[m.FieldName] == null || string.IsNullOrEmpty(rowdata[m.FieldName].ToString()))
                    {
                        errorMsg = string.Format("第{0}列为必填列,不可为空", columnText);
                        result = false;
                    }
                }
                var columnValue = rowdata[m.FieldName] != null && !string.IsNullOrEmpty(rowdata[m.FieldName].ToString()) ? rowdata[m.FieldName].ToString().Trim() : "";

                //校验字段的数据格式
                switch (m.FieldType)
                {
                    case FieldType.Address:
                        {
                            var lat = "";
                            var lon = "";
                            var address = new { lat = lat, lon = lon, address = columnValue };
                            rowdata[m.FieldName] = JsonHelper.ToJson(address);
                            break;
                        }

                    case FieldType.EmailAddr:
                        if (!string.IsNullOrEmpty(columnValue) && !IsEmailAddr(columnValue))
                        {
                            errorMsg = string.Format("第{0}列不匹配邮箱格式", columnText);
                            result = false;
                        }
                        break;
                    case FieldType.NumberDecimal:
                        if (!string.IsNullOrEmpty(columnValue) && !CommonHelper.IsNumeric(columnValue))
                        {
                            errorMsg = string.Format("第{0}列不匹配小数格式", columnText);
                            result = false;
                        }
                        break;
                    case FieldType.NumberInt:
                        if (!string.IsNullOrEmpty(columnValue) && !CommonHelper.IsInt(columnValue))
                        {
                            errorMsg = string.Format("第{0}列不匹配整数格式", columnText);
                            result = false;
                        }
                        break;
                    case FieldType.PhoneNum:
                        if (!string.IsNullOrEmpty(columnValue) && !IsPhoneNum(columnValue))
                        {
                            errorMsg = string.Format("第{0}列不匹配手机号码格式", columnText);
                            result = false;
                        }
                        break;
                    case FieldType.Telephone:
                        //if (!string.IsNullOrEmpty(columnValue) && !IsTelephone(columnValue))
                        //{
                        //    errorMsg = string.Format("第{0}列不匹配电话号码格式", columnText);
                        //    result = false;
                        //}
                        break;
                    case FieldType.TimeDate:
                        //if (!string.IsNullOrEmpty(columnValue))
                        //{
                        //	DateTime dt;
                        //	if (!DateTime.TryParse(columnValue.Trim(), out dt) || !IsTimeDate(dt.ToString("d")))
                        //	{
                        //		errorMsg = string.Format("第{0}列不匹配日期格式", columnText);
                        //	}
                        //	else
                        //	{
                        //		rowdata[m.FieldName] = JsonHelper.ToJson(dt.ToString("d"));
                        //	}
                        //}
                        tmpValue = 0;
                        if (long.TryParse(columnValue, out tmpValue))
                        {
                            if (tmpValue > 59)
                                tmpValue -= 1;
                            var tmpDate = new DateTime(1899, 12, 31).AddDays(tmpValue);
                            columnValue = tmpDate.ToString("yyyy-MM-dd");
                            rowdata[m.FieldName] = columnValue;
                            // columnValue = DateTime.FromBinary(tmpValue).ToString("yyyy-MM-dd");
                        }
                        if (!string.IsNullOrEmpty(columnValue) && !IsTimeDate(columnValue.Split(' ')[0]))
                        {
                            errorMsg = string.Format("第{0}列不匹配日期格式", columnText);
                            result = false;
                        }
                        break;
                    case FieldType.TimeStamp:
                        tmpValue = 0;
                        if (long.TryParse(columnValue, out tmpValue))
                        {
                            if (tmpValue > 59)
                                tmpValue -= 1;
                            var tmpDate = new DateTime(1899, 12, 31).AddDays(tmpValue);
                            columnValue = tmpDate.ToString("yyyy-MM-dd hh:mm:ss");
                            rowdata[m.FieldName] = columnValue;
                            // columnValue = DateTime.FromBinary(tmpValue).ToString("yyyy-MM-dd");
                        }
                        if (!string.IsNullOrEmpty(columnValue) && !IsTimeStamp(columnValue))
                        {
                            errorMsg = string.Format("第{0}列不匹配时间格式", columnText);
                            result = false;
                        }
                        break;
                    case FieldType.TextPinyinShort:
                        {
                            string pinyin = PinYinConvert.ToChinese(columnValue, true);
                            rowdata.Add(string.Format("{0}_pinyin", m.FieldName), pinyin);
                            break;
                        }
                    case FieldType.TextPinyin:
                        {
                            string pinyin = PinYinConvert.ToChinese(columnValue, false);
                            rowdata.Add(string.Format("{0}_pinyin", m.FieldName), pinyin);
                            break;
                        }
                    default://默认当字符串处理
                        break;
                }
            }
            return result;
        }
        #endregion
        public object DetailImport(DetailImportParamInfo paramInfo, int userId)
        {
            List<Dictionary<string, object>> retList = new List<Dictionary<string, object>>();

            #region 获取嵌套实体字段信息（根据TypeId，注意转换）
            List<EntityFieldProMapper> FieldList = this._entityProRepository.FieldQuery(paramInfo.DetailEntityId.ToString(), userId);
            #endregion
            #region 解析Excel,并调用前置调用UScript
            List<Dictionary<string, object>> ExcelRowList = OXSExcelReader.ReadExcelFirstSheet(paramInfo.Data.OpenReadStream());
            #endregion
            #region 开始转换,转换过程中注意如果有错误，每行就有标识是否正确（"uk100v7_import_error:'xxx'"）
            Dictionary<string, object> FieldRowDict = null;
            if (ExcelRowList.Count >= 1)
            {
                FieldRowDict = ExcelRowList[0];
                ExcelRowList.RemoveAt(0);
            }
            foreach (Dictionary<string, object> ExcelRowData in ExcelRowList)
            {
                Dictionary<string, object> RealDataRow = new Dictionary<string, object>();
                //开始处理一行
                foreach (string indexKey in ExcelRowData.Keys)
                {
                    string fieldKey = FieldRowDict[indexKey].ToString();
                    EntityFieldProMapper FieldInfo = null;
                    foreach (EntityFieldProMapper field in FieldList)
                    {
                        if (field.FieldLabel.ToLower().Equals(fieldKey.ToLower())
                            || field.FieldName.ToLower().Equals(fieldKey.ToLower()))
                        {
                            FieldInfo = field;
                            break;
                        }
                    }
                    if (FieldInfo != null && ExcelRowData[indexKey] != null)
                    {
                        //处理一列
                        try
                        {
                            string errorMsg = "";
                            DynamicEntityDataFieldMapper f = new DynamicEntityDataFieldMapper()
                            {
                                FieldId = Guid.Parse(FieldInfo.FieldId),
                                FieldName = FieldInfo.FieldName,
                                TypeId = paramInfo.MainTypeId,
                                FieldLabel = FieldInfo.FieldLabel,
                                DisplayName = FieldInfo.DisplayName,
                                ControlType = FieldInfo.ControlType,
                                FieldType = FieldInfo.FieldType,
                                FieldConfig = FieldInfo.FieldConfig
                            };
                            RealDataRow[f.FieldName] = ExcelRowData[indexKey].ToString();
                            if (CheckFieldData(null, paramInfo.MainTypeId, f, ExcelRowData[indexKey].ToString(), RealDataRow, userId, out errorMsg) == false)
                            {
                                if (RealDataRow.ContainsKey("uk100v7_import_error"))
                                {
                                    RealDataRow["uk100v7_import_error"] = RealDataRow["uk100v7_import_error"] + "\r\n" + errorMsg;
                                }
                                else
                                {
                                    RealDataRow["uk100v7_import_error"] = errorMsg;

                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            if (RealDataRow.ContainsKey("uk100v7_import_error") == false)
                            {
                                RealDataRow.Add("uk100v7_import_error", ex.Message);
                            }
                            else
                            {
                                RealDataRow["uk100v7_import_error"] = RealDataRow["uk100v7_import_error"] + "\r\n" + ex.Message;
                            }
                        }
                    }
                }
                retList.Add(RealDataRow);

            }
            #endregion
            #region 调用后置处理UScript
            #endregion
            return retList;
        }
        private bool IsPhoneNum(string value)
        {
            Regex r = new Regex(@"^1\d{10}$");
            return r.IsMatch(value);
        }

        private bool IsTelephone(string value)
        {
            Regex r = new Regex(@"^0\d{2,3}-?\d{7,8}$");
            return r.IsMatch(value);
        }

        private bool IsTimeDate(string value)
        {
            Regex r = new Regex(@"^\d{4}(\-|\/|\.)\d{1,2}\1\d{1,2}$");
            return r.IsMatch(value);
        }
        //
        private bool IsTimeStamp(string value)
        {
            Regex r = new Regex(@"^\d{4}(\-|\/|\.)\d{1,2}\1\d{1,2} ((20|21|22|23|[0-1]?\d):[0-5]?\d:[0-5]?\d)$");
            return r.IsMatch(value);
        }

        private bool IsEmailAddr(string value)
        {
            //Regex r = new Regex(@"^\s*([A-Za-z0-9_-]+(\.\w+)*@(\w+\.)+\w{2,5})\s*$");
            Regex r = new Regex(@"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
            return r.IsMatch(value);
        }
        #endregion

        #region 回写预算
        /// <summary>
        /// 回写预算
        /// </summary>
        private void BackWriteBudgetOfJx(List<Dictionary<string, object>> excelDataList, string TaskName)
        {
            string methodName = "BackWriteBudgetOfJx";
            var type = new Type[] { typeof(List<Dictionary<string, object>>), typeof(string) };
            var param = new object[] { excelDataList, TaskName };
            InvokeBudget(methodName, type, param);
        }

        public void InvokeBudget(string methodName, Type[] types, object[] param)
        {
            string serviceName = "UBeat.Crm.CoreApi.wxChina.Services.JiXin.JxBudgetServices";
            Type type = AssemblyPluginUtils.getInstance().getUKType(serviceName);
            object service = ServiceLocator.Current.GetInstanceWithName(serviceName);
            System.Reflection.MethodInfo methodInfo = null;
            methodInfo = type.GetMethod(methodName, types);
            methodInfo.Invoke(service, param);
        }
        #endregion
    }

}

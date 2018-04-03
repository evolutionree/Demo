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
using UBeat.Crm.CoreApi.DomainModel.Department;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;
using UBeat.Crm.CoreApi.DomainModel.Excels;
using UBeat.Crm.CoreApi.DomainModel.Message;
using UBeat.Crm.CoreApi.DomainModel.Notify;
using UBeat.Crm.CoreApi.DomainModel.Utility;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using UBeat.Crm.CoreApi.Services.Models.Excels;
using UBeat.Crm.CoreApi.Services.Models.Message;
using UBeat.Crm.CoreApi.Services.Utility;
using UBeat.Crm.CoreApi.Services.Utility.ExcelUtility;

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

        public OutputResult<object> TaskStart(string taskid)
        {
            var taskData = _cache.Get<TaskDataModel>(taskDataId_prefix + taskid);
            if (taskData == null)
                return ShowError<object>("任务不存在");
            TaskStart(taskid, taskData);
            _cache.Remove(taskDataId_prefix + taskid);
            return new OutputResult<object>("任务已启动");
        }



        private void TaskStart(string taskid, TaskDataModel taskData)
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
                    //每个线程处理的最大条数
                    var count = 100;
                    //计算线程个数
                    var numThreads = (int)Math.Ceiling((double)m.DataRows.Count / count);
                    //限制最多使用20线程
                    if (numThreads > 10)
                    {
                        numThreads = 10;
                        //此时重新计算每个线程的最大条数
                        count = m.DataRows.Count / numThreads;
                    }
                    var finished = new CountdownEvent(1);
                    for (int i = 0; i < numThreads; i++)
                    {
                        finished.AddCount();
                        var i1 = i;
                        var length = numThreads - 1 == i ? m.DataRows.Count - i * count : count;
                        var rangdata = m.DataRows.GetRange(i * count, length);

                        ThreadPool.QueueUserWorkItem(delegate (object dataparam)
                        {
                            var datarows = dataparam as List<Dictionary<string, object>>;
                            if (datarows == null)
                                return;
                            var taskSuccessRows = new List<Dictionary<string, object>>();
                            var taskErrorRows = new List<Dictionary<string, object>>();
                            var taskErrorTips = new List<string>();
                            var taskSuccessTips = new List<string>();
                            try
                            {
                                foreach (var onerow in datarows)
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
                                                if (define is SimpleSheetTemplate)
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
                                        taskSuccessRows.Add(onerow);
                                        taskSuccessTips.Add(string.Format("导入成功，{0}", errorMsg));
                                    }
                                    else
                                    {
                                        taskErrorRows.Add(onerow);
                                        taskErrorTips.Add(string.Format("导入失败，{0}", errorMsg));
                                    }
                                    //3、记录导入进度
                                    lock (tasklockObj)
                                    {
                                        progress.DealRowsCount += 1;
                                        if (!issucces)
                                            progress.ErrorRowsCount += 1;
                                    }

                                }
                            }
                            catch (Exception ex)
                            {
                                var temp = datarows.Except(taskSuccessRows).Except(taskErrorRows).ToList();
                                foreach (var eRow in temp)
                                {
                                    taskErrorRows.Add(eRow);
                                    taskErrorTips.Add(string.Format("导入出现异常：{0}", ex.Message));
                                }
                                lock (tasklockObj)
                                {
                                    progress.DealRowsCount += temp.Count;
                                    progress.ErrorRowsCount += temp.Count;
                                }
                            }
                            finally
                            {
                                lock (lockObj)
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

                                finished.Signal();
                            }
                        }, rangdata);
                    }
                    finished.Signal();
                    finished.Wait();
                    finished.Dispose();

                    //如果有错误出现，则添加到导出对象中
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
                    //return new OutputResult<object>(fileId, "导入出现错误，错误内容请下载错误提示的Excel文档", 1);
                }
                //var taskDataTemp = _cache.Get(taskDataId_prefix + taskid) as TaskDataModel;
                if (taskData != null)
                {
                    //移除任务的缓存数据
                    // _cache.Remove(taskDataId_prefix + taskid);
                    //lock (lockObj)
                    //{
                    //    taskList.Remove(progress);
                    //}
                    Guid entityid = Guid.Empty;
                    Guid.TryParse(taskData.FormDataKey, out entityid);
                    SendMessage(entityid, progress, hasError, taskData.UserNo);


                    //               NotifyEntity notifyEntity = new NotifyEntity();

                    //notifyEntity.entityid = entityid;
                    //notifyEntity.msgcontent = string.Format("导入结果：{0}", resultData.Count > 1 ? "存在错误" : "成功导入");
                    ////notifyEntity.msgdataid = null;
                    //notifyEntity.msggroupid = (int)MsgEnum.Remind;
                    //notifyEntity.msgparam = JsonConvert.SerializeObject(progress);
                    ////notifyEntity.msgstatus = "";
                    //notifyEntity.msgtitle = "导入任务完成";
                    //notifyEntity.msgtype = (int)MsgTypeEnum.ExportRedmind;
                    //notifyEntity.receiver = taskData.UserNo.ToString();
                    //notifyEntity.sendtime = DateTime.Now;
                    //notifyEntity.userno = taskData.UserNo;

                    //_notifyServices.WriteMessage(notifyEntity, taskData.UserNo);
                }
            });

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
                sheetDefine = GeneralDynamicTemplate(entityid, formData.OperateType, userno);

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
            var sheetTemplate = define as SimpleSheetTemplate;
            if (sheetTemplate.DataObject is List<DynamicEntityDataFieldMapper>)
            {
                var typeFields = sheetTemplate.DataObject as List<DynamicEntityDataFieldMapper>;
                typeId = typeFields.FirstOrDefault().TypeId;

            }

            Guid existRecordid = Guid.Empty;
            //客户查重条件从客户基础资料中查询
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
        public ExportModel ExportData(ExportDataModel data)
        {
            var userData = HasFunctionAccess(data.UserId);


            string filename = null;
            var sheetDefine = new List<SheetDefine>();
            if (data.TemplateType == TemplateType.FixedTemplate)//获取对应的模板
                sheetDefine = GetSheetDefine(data.FuncName, out filename);
            else
            {

                if (data.DynamicModel == null)
                    throw new Exception("DynamicQuery必须有值");
                sheetDefine = GeneralDynamicTemplate(data.DynamicModel.EntityId, ExcelOperateType.Export, data.UserId);
            }

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
                    var template = m as SimpleSheetTemplate;
                    if (template.Headers.Count == 0)
                    {
                        throw new Exception("请设置web列表显示字段");
                    }
                    if (data.DynamicModel == null)
                    {
                        throw new Exception("实体类型数据导出，需要传DynamicModel参数");
                    }
                    var entityname = _repository.GetEntityName(data.DynamicModel.EntityId);
                    filename = string.Format("{0}导出数据.xlsx", entityname);
                    var isAdvance = data.DynamicModel.IsAdvanceQuery == 1;
                    var dataList = _entityServices.DataList2(data.DynamicModel, isAdvance, data.UserId);
                    var queryResult = dataList.DataBody as Dictionary<string, List<Dictionary<string, object>>>;
                    //if (queryResult == null ) queryResult = dataList.DataBody as Dictionary<string, List<Dictionary<string, object>>>;
                    var pageDataTemp = queryResult["PageData"];

                    var tempFields = template.Headers.Where(o => o.FieldType == FieldType.Image
                                                            || o.FieldType == FieldType.Address
                                                            || o.FieldType == FieldType.reference
                                                            || o.FieldType == FieldType.TimeDate
                                                            );
                    if (tempFields.Count() > 0)
                    {
                        foreach (var item in tempFields)
                        {
                            foreach (var mdata in pageDataTemp)
                            {

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
                                    //case FieldType.Related:
                                    //    var entityInfo = _dynamicEntityRepository.getEntityBaseInfoById(data.DynamicModel.EntityId, data.UserId);
                                    //    if (entityInfo != null)
                                    //    {
                                    //        if (Convert.ToInt32(entityInfo["modeltype"].ToString()) == 3)
                                    //        {

                                    //        }
                                    //    }
                                    //    break;
                                }


                            }
                        }
                    }

                    // pageData = new List<Dictionary<string, object>>();
                    foreach (var item1 in pageDataTemp)
                    {
                        var dic = new Dictionary<string, object>();
                        foreach (var item2 in item1)
                        {

                            dic.Add(item2.Key, item2.Value);
                        }
                        pageData.Add(dic);
                    }
                }

                //把IDictionary<string, object>转为Dictionary<string, object>类型


                //foreach (var crmRow in pageData)
                //{

                //	//把IDictionary<string, object>转为Dictionary<string, object>类型
                //	var dic = new Dictionary<string, object>();
                //	foreach (var item in crmRow)
                //	{
                //		dic.Add(item.Key, item.Value);
                //	}
                //	sheetdata.DataRows.Add(dic);
                //}
                sheetdata.DataRows = pageData;

                sheetdata.SheetDefines = m;
                sheets.Add(sheetdata);
            }

            return new ExportModel()
            {
                FileName = filename == null ? null : filename.Replace("模板", ""),
                ExcelFile = OXSExcelWriter.GenerateExcel(sheets)
            };
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

            var defines = GeneralDynamicTemplate(entityid, ExcelOperateType.ImportAdd, userNumber);
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
        private List<SheetDefine> GeneralDynamicTemplate(Guid entityId, ExcelOperateType operateType, int userNumber)
        {
            List<SheetDefine> defines = new List<SheetDefine>();
            switch (operateType)
            {
                case ExcelOperateType.ImportAdd:
                case ExcelOperateType.ImportUpdate:
                    defines = GeneralDynamicTemplate_Import(entityId, operateType, userNumber);
                    break;
                default:
                    defines = GeneralDynamicTemplate_Export(entityId, operateType, userNumber);
                    break;
            }

            return defines;
        }
        #region --生成动态模板的导出模板定义--
        private List<SheetDefine> GeneralDynamicTemplate_Export(Guid entityId, ExcelOperateType operateType, int userNumber)
        {
            List<SheetDefine> defines = new List<SheetDefine>();

            try
            {
                var typeVisibleFields = _entityProRepository.FieldWebVisibleQuery(entityId.ToString(), userNumber);
                if (!typeVisibleFields.ContainsKey("FieldVisible"))
                    throw new Exception("获取实体显示字段接口报错，缺少FieldVisible参数的结果集");
                var entityInfo = _dynamicEntityRepository.getEntityBaseInfoById(entityId, userNumber);
                if (entityInfo == null)
                    throw new Exception("实体信息不存在");
                var modelType = Convert.ToInt32(entityInfo["modeltype"].ToString());
                var typeFields = typeVisibleFields["FieldVisible"];
                List<SimpleHeader> headers = new List<SimpleHeader>();
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
                    headers.Add(new SimpleHeader() { FieldName = fieldname, HeaderText = displayname, Width = 150, FieldType = fieldTypetemp });
                }
                defines.Add(new SimpleSheetTemplate()
                {
                    SheetName = "Sheet 1",
                    ExecuteSQL = "",
                    Headers = headers,
                    //DataObject = typeFields
                });
            }
            catch (Exception ex)
            {
                throw new Exception("生成动态模板失败", ex);
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
                    //case DynamicProtocolControlType.RecId:
                    //case DynamicProtocolControlType.RecItemid:
                    //case DynamicProtocolControlType.RecStatus:
                    //    continue;

            }
        }
        #endregion

        #region --生成动态模板的导入模板定义--
        private List<SheetDefine> GeneralDynamicTemplate_Import(Guid entityId, ExcelOperateType operateType, int userNumber)
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
                    List<SimpleHeader> headers = new List<SimpleHeader>();
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
                        }


                        headers.Add(new SimpleHeader() { FieldName = field.FieldName, HeaderText = field.DisplayName, IsNotEmpty = field.IsRequire, Width = 150, FieldType = tempFieldType });
                    }
                    defines.Add(new SimpleSheetTemplate()
                    {
                        SheetName = categoryname,
                        ExecuteSQL = "",
                        Headers = headers,
                        DataObject = typeFields
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception("生成动态模板失败", ex);
            }
            return defines;
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
            if (sheetDefine is SimpleSheetTemplate)
            {
                var sheetTemplate = sheetDefine as SimpleSheetTemplate;
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
        private bool ValidationRowData(SimpleSheetTemplate sheetTemplate, ExcelOperateType operateType, Dictionary<string, object> rowdata, int userno, out string errorMsg)
        {
            errorMsg = null;
            bool result = true;
            Guid typeId = Guid.Empty;


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
                        var validatResult = DynamicProtocolHelper.ValidFieldConfig(m, data, false);
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
                    else if (data != null)
                    {
                        var columnValue = data.ToString().Trim();
                        //处理特殊控件类型的数据以及实体没有配置验证方式的数据校验
                        result = CheckFieldData(typeId, m, columnValue, rowdata, userno, out errorMsg);
                    }

                    //如果存在错误，则跳出循环
                    if (result == false)
                        break;
                }
            }

            return result;
        }

        //校验字段的数据格式和处理特殊控件
        private bool CheckFieldData(Guid typeId, DynamicEntityDataFieldMapper typeField, string columnValue, Dictionary<string, object> rowdata, int userno, out string errorMsg)
        {
            errorMsg = null;
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
                        var pid = _repository.GetProductId(columnValue, out errorMsg);
                        if (string.IsNullOrEmpty(errorMsg))
                        {
                            rowdata[typeField.FieldName] = pid;
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
                        }
                        else errorMsg = string.Format("{0}:{1}", typeField.FieldLabel, errorMsg);
                    }
                    break;

                case DynamicProtocolControlType.Department://团队组织
                    if (!string.IsNullOrEmpty(columnValue) && typeField.FieldType != 1)
                    {
                        var pid = _repository.GetDepartmentId(columnValue, userno, out errorMsg);
                        if (string.IsNullOrEmpty(errorMsg))
                        {
                            rowdata[typeField.FieldName] = pid;
                        }
                        else errorMsg = string.Format("{0}:{1}", typeField.FieldLabel, errorMsg);
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
                ruleSql = _roleRepository.FormatRoleRule(ruleSql, userno);
                object datainfo = null;
                if ((DynamicProtocolControlType)typeField.ControlType == DynamicProtocolControlType.DataSourceSingle)
                {

                    datainfo = _repository.GetDataSourceMapDataId(ruleSql, columnValue, out errorMsg);
                }
                else
                {
                    List<string> dicDatas = columnValue.Split(',').ToList();
                    datainfo = _repository.GetDataSourceMapDataId(ruleSql, dicDatas, out errorMsg);
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
    }
}

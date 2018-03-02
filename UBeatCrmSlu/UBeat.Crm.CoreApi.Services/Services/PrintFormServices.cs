using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using UBeat.Crm.CoreApi.Services.Models.PrintForm;
using UBeat.Crm.CoreApi.Services.Utility.OpenXMLUtility;
using System.IO;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.DomainModel.PrintForm;
using UBeat.Crm.CoreApi.Services.Models.Account;
using UBeat.Crm.CoreApi.Repository.Repository.Account;
using UBeat.Crm.CoreApi.Core.Utility;
using Microsoft.Extensions.Configuration;
using System.Data;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;
using UBeat.Crm.CoreApi.DomainModel.Account;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;
using Newtonsoft.Json.Linq;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class PrintFormServices : BaseServices
    {
        private readonly IPrintFormRepository _repository;
        private readonly IAccountRepository _accountRepository;
        private readonly IEntityProRepository _entityProRepository;
        private readonly DynamicEntityServices _entityServices;
        //
        FileServices _fileServices;
        public PrintFormServices(IPrintFormRepository repository, IAccountRepository accountRepository, IEntityProRepository entityProRepository, DynamicEntityServices entityServices)
        {
            _repository = repository;
            _fileServices = new FileServices();
            _accountRepository = accountRepository;
            _entityProRepository = entityProRepository;
            _entityServices = entityServices;
        }

        #region ---套打模板管理---
        public OutputResult<object> InsertTemplate(TemplateInfoModel data, int userNumber)
        {
            if (data == null)
                throw new Exception("参数不可为空");
            var model = new CrmSysEntityPrintTemplate()
            {
                EntityId = data.EntityId,
                TemplateName = data.TemplateName,
                TemplateType = data.TemplateType,
                DataSourceType = data.DataSourceType,
                DataSourceFunc = data.DataSourceFunc,
                ExtJs = data.ExtJs,
                FileId = data.FileId,
                RuleId = data.RuleId,
                RuleDesc = data.RuleDesc,
                Description = data.Description,
                RecCreated = DateTime.Now,
                RecUpdated = DateTime.Now,
                RecCreator = userNumber,
                RecUpdator = userNumber
            };
            return new OutputResult<object>(_repository.InsertTemplate(model));
        }

        public OutputResult<object> SetTemplatesStatus(TemplatesStatusModel data, int usernumber)
        {
            if (data == null)
                throw new Exception("参数不可为空");
            if (data.RecIds == null || data.RecIds.Count == 0)
                throw new Exception("参数recids不可为空");
            if (data.RecStatus != 0 && data.RecStatus != 1)
                throw new Exception("参数RecStatus必须为0或者1");
            _repository.SetTemplatesStatus(data.RecIds, data.RecStatus, usernumber);
            return new OutputResult<object>("OK");
        }

        public OutputResult<object> UpdateTemplate(TemplateInfoModel data, int userNumber)
        {
            if (data == null)
                throw new Exception("参数不可为空");
            if (data.RecId.GetValueOrDefault() == Guid.Empty)
                throw new Exception("参数recid不可为空");
            var model = new CrmSysEntityPrintTemplate()
            {
                RecId = data.RecId.GetValueOrDefault(),
                EntityId = data.EntityId,
                TemplateName = data.TemplateName,
                TemplateType = data.TemplateType,
                DataSourceType = data.DataSourceType,
                DataSourceFunc = data.DataSourceFunc,
                ExtJs = data.ExtJs,
                FileId = data.FileId,
                RuleId = data.RuleId,
                RuleDesc = data.RuleDesc,
                Description = data.Description,
                RecCreated = DateTime.Now,
                RecUpdated = DateTime.Now,
                RecCreator = userNumber,
                RecUpdator = userNumber
            };
            _repository.UpdateTemplate(model);
            return new OutputResult<object>("OK");
        }

        public OutputResult<object> GetTemplateList(TemplateListModel data, int userNumber)
        {
            if (data == null)
                throw new Exception("参数不可为空");
            return new OutputResult<object>(_repository.GetTemplateList(data.EntityId, data.RecState));
        }


        #endregion

        public OutputResult<object> PrintEntity(PrintEntityModel data, int usernumber)
        {
            List<byte[]> documentBytes = new List<byte[]>();
            if (data == null)
                throw new Exception("参数不可为空");
            var templateInfo = _repository.GetTemplateInfo(data.TemplateId);
            if (templateInfo == null)
                throw new Exception("输出模板不正确，请重新选择模板");
            if (!templateInfo.FileId.HasValue || templateInfo.FileId.Value == Guid.Empty)
                throw new Exception("未上传输出模板文件，请先上传输出模板文件");

            var fileData = _fileServices.GetFileData(null, templateInfo.FileId.ToString());
            Stream fileStream = new MemoryStream(fileData);
            var excelData = ExcelHelper.ReadExcel(fileStream);
            if (excelData == null || excelData.Sheets == null || excelData.Sheets.Count == 0)
                throw new Exception("输出模板文件解析错误，请检查模板文件格式并上传Office 2007以上版本模板文件");

            IDictionary<string, object> detailData = GetDetailData(data.EntityId, data.RecId, templateInfo, usernumber);
            var fields = _entityProRepository.EntityFieldProQuery(data.EntityId.ToString(), usernumber).FirstOrDefault().Value;
            var userinfo = _accountRepository.GetAccountUserInfo(usernumber);
            foreach (var sheet in excelData.Sheets)
            {
                var newRows = new List<ExcelRowInfo>();
                foreach (var row in sheet.Rows)
                {
                    if (row.RowStatus != RowStatus.Normal) continue;
                    row.RowStatus = RowStatus.Edit;
                    foreach (var cell in row.Cells)
                    {
                        var newRowsTemp = ParsingData(sheet, row, cell, fields, detailData, userinfo);
                        if (newRowsTemp != null && newRowsTemp.Count > 0)
                            newRows.AddRange(newRowsTemp);
                    }
                }
                foreach (var row in newRows)
                {
                    var rowIndex = sheet.Rows.FindLastIndex(m => m.RowIndex == row.RowIndex);
                    sheet.Rows.Insert(rowIndex + 1, row);
                }
            }
            var bytes = ExcelHelper.WrightExcel(excelData);
            //documentBytes.Add(bytes);
            return new OutputResult<object>();
        }

        #region --获取实体详情数据--
        private IDictionary<string, object> GetDetailData(Guid entityId, Guid recId, CrmSysEntityPrintTemplate templateInfo, int usernumber)
        {
            IDictionary<string, object> detailData = null;
            if (templateInfo.DataSourceType == DataSourceType.EntityDetail)
            {
                var paramData = new DynamicEntityDetailtMapper()
                {
                    EntityId = entityId,
                    RecId = recId,
                    NeedPower = 0
                };
                detailData = _entityServices.Detail(paramData, usernumber)["Detail"].FirstOrDefault();
            }
            else if (templateInfo.DataSourceType == DataSourceType.DbFunction)
            {

            }
            else if (templateInfo.DataSourceType == DataSourceType.InternalMethor)
            {

            }

            return detailData;
        }
        #endregion

        #region --解析每行每个单元格的数据--
        private List<ExcelRowInfo> ParsingData(ExcelSheetInfo sheet, ExcelRowInfo row, ExcelCellInfo cell, List<IDictionary<string, object>> fields, IDictionary<string, object> detailData, AccountUserInfo userinfo, List<IDictionary<string, object>> linkTableFields=null, IDictionary<string, object> linkTableDetailData=null)
        {
            var newRows = new List<ExcelRowInfo>();
            //是否IF控制函数
            var newRowsTemp = CheckIFCtlCellValue(sheet, row, cell, fields, detailData, userinfo);
            if (newRowsTemp != null && newRowsTemp.Count > 0)
                newRows.AddRange(newRowsTemp);
            //是否Loop控制函数
            newRowsTemp = CheckLoopCtlCellValue(sheet, row, cell, fields, detailData, userinfo);
            if (newRowsTemp != null && newRowsTemp.Count > 0)
                newRows.AddRange(newRowsTemp);
            //是否值函数
            //解析变量，并用真实值替换变量名
            CheckVariableCellValue(cell, fields, detailData, userinfo);
            //解析嵌套实体变量，并用真实值替换变量名
            if (linkTableFields != null && linkTableDetailData != null && linkTableDetailData.Count > 0)
                ParsingLinkTableVariable(cell, fields, linkTableDetailData, userinfo);

            return newRows;
        }
        #endregion

        #region --解析变量，并用真实值替换变量名--
        /// <summary>
        /// 解析变量，并用真实值替换变量名
        /// </summary>
        /// <param name="cell">当前单元格</param>
        /// <param name="fields">实体字段定义</param>
        /// <param name="detailData">实体详情数据</param>
        /// <param name="linkTableDetailData">实体表格嵌套实体的字段数据，仅在用到表格控件时使用</param>
        /// <param name="userinfo"></param>
        private void CheckVariableCellValue(ExcelCellInfo cell, List<IDictionary<string, object>> fields, IDictionary<string, object> detailData, AccountUserInfo userinfo)
        {
            var formula = ParsingVariable(cell.CellValue, fields, detailData, userinfo);
            if (cell.CellValue != formula)
            {
                cell.IsUpdated = true;
                cell.CellValue = formula;
            }
        }
        private string ParsingVariable(string formulaArg, List<IDictionary<string, object>> fields, IDictionary<string, object> detailData, AccountUserInfo userinfo)
        {
            List<string> fieldList = null;
            var formula = KeywordHelper.ParsingFormula(formulaArg, out fieldList);
            if (fieldList != null && fieldList.Count > 0)
            {
                foreach (var fieldFormat in fieldList)
                {
                    if (KeywordHelper.IsCurUserName(fieldFormat))
                    {
                        formula = formula.Replace(fieldFormat, userinfo.UserName.ToString());
                    }
                    else if (KeywordHelper.IsCurUserId(fieldFormat))
                    {
                        formula = formula.Replace(fieldFormat, userinfo.UserId.ToString());
                    }
                    else if (KeywordHelper.IsCurDate(fieldFormat))
                    {
                        formula = formula.Replace(fieldFormat, DateTime.Now.ToString("yyyy-MM-dd"));
                    }
                    else if (KeywordHelper.IsCurTime(fieldFormat))
                    {
                        formula = formula.Replace(fieldFormat, DateTime.Now.ToString("yyyy-MM-dd HH:mm:SS"));
                    }
                    else if (KeywordHelper.IsCurDeptName(fieldFormat))
                    {
                        formula = formula.Replace(fieldFormat, userinfo.DepartmentName.ToString());
                    }
                    else if (KeywordHelper.IsCurDeptId(fieldFormat))
                    {
                        formula = formula.Replace(fieldFormat, userinfo.DepartmentId.ToString());
                    }
                    else if (KeywordHelper.IsEnterpriseName(fieldFormat))
                    {
                        var enterpriseInfo = _accountRepository.GetEnterpriseInfo();
                        formula = formula.Replace(fieldFormat, enterpriseInfo.EnterpriseName);
                    }
                    else
                    {
                        var fieldnames = KeywordHelper.GetFieldNames(fieldFormat);
                        if (fieldnames.Length == 1)//实体的普通字段
                        {
                            var tempfield = fieldnames[0].Trim();
                            //获取实体字段名称
                            var entityfieldname = fields.Find(m => tempfield.Equals(m["fieldname"]) || tempfield.Equals(m["displayname"]))["fieldname"].ToString();
                            var entityfieldvalue = detailData.ContainsKey(entityfieldname) && detailData[entityfieldname] != null ? detailData[entityfieldname].ToString() : string.Empty;
                            formula = formula.Replace(fieldFormat, entityfieldvalue);
                        }

                    }
                }

            }
            return formula;

        }
        /// <summary>
        /// 解析嵌套表格控件的变量
        /// </summary>
        /// <param name="formulaArg">单元格内的表达式内容</param>
        /// <param name="tableFields">嵌套实体的字段定义</param>
        /// <param name="tableDetailData">嵌套实体的数据详情</param>
        /// <param name="userinfo"></param>
        /// <returns></returns>
        private void ParsingLinkTableVariable(ExcelCellInfo cell, List<IDictionary<string, object>> tableFields, IDictionary<string, object> tableDetailData, AccountUserInfo userinfo)
        {
            var formula = ParsingLinkTableVariable(cell.CellValue, tableFields, tableDetailData, userinfo);
            if (cell.CellValue != formula)
            {
                cell.IsUpdated = true;
                cell.CellValue = formula;
            }
        }
        /// <summary>
        /// 解析嵌套表格控件的变量
        /// </summary>
        /// <param name="formulaArg">单元格内的表达式内容</param>
        /// <param name="tableFields">嵌套实体的字段定义</param>
        /// <param name="tableDetailData">嵌套实体的数据详情</param>
        /// <param name="userinfo"></param>
        /// <returns></returns>
        private string ParsingLinkTableVariable(string formulaArg, List<IDictionary<string, object>> tableFields, IDictionary<string, object> tableDetailData, AccountUserInfo userinfo)
        {
            List<string> fieldList = null;
            var formula = KeywordHelper.ParsingFormula(formulaArg, out fieldList);
            if (fieldList != null && fieldList.Count > 0)
            {
                foreach (var fieldFormat in fieldList)
                {
                    var fieldnames = KeywordHelper.GetFieldNames(fieldFormat);
                    if (fieldnames.Length == 2)//实体表格控件，此时数据由参数linkTableDetailData提供
                    {
                        var tempfield = fieldnames[1].Trim();

                        //获取实体字段名称
                        var entityfieldname = tableFields.Find(m => tempfield.Equals(m["fieldname"]) || tempfield.Equals(m["displayname"]))["fieldname"].ToString();
                        var entityfieldvalue = tableDetailData.ContainsKey(entityfieldname) && tableDetailData[entityfieldname] != null ? tableDetailData[entityfieldname].ToString() : string.Empty;
                        formula = formula.Replace(fieldFormat, entityfieldvalue);
                    }
                }

            }
            return formula;
           
        }
        #endregion

        #region --检查IF代码块的逻辑--
        private List<ExcelRowInfo> CheckIFCtlCellValue(ExcelSheetInfo sheet, ExcelRowInfo row, ExcelCellInfo cell, List<IDictionary<string, object>> fields, IDictionary<string, object> detailData, AccountUserInfo userinfo)
        {
            List<ExcelRowInfo> newRows = new List<ExcelRowInfo>();
            string formula = null;
            int checkRegion = -1;//标识当前检查区域，-1=未进入if 0=if,1=elseif,2=else,3=endif
            if (KeywordHelper.IsKey_IF(cell.CellValue, out formula))
            {
                checkRegion = 0;
                row.RowStatus = RowStatus.Deleted;
                var formulaResult = ParsingVariable(formula, fields, detailData, userinfo);
                DataTable dt = new DataTable();
                var computeResult = dt.Compute(formulaResult, null);
                bool ifValue = false;
                if (computeResult == null || !bool.TryParse(computeResult.ToString(), out ifValue))
                {
                    throw new Exception("IF函数条件格式错误");
                }
                var rowIndex = sheet.Rows.IndexOf(row);
                for (int i = rowIndex + 1; i < sheet.Rows.Count; i++)
                {
                    if (sheet.Rows[i].RowStatus != RowStatus.Normal) continue;
                    sheet.Rows[i].RowStatus = RowStatus.Edit;
                    foreach (var celltemp in sheet.Rows[i].Cells)
                    {
                        //判断同层的elseif逻辑
                        if (KeywordHelper.IsKey_ElseIF(celltemp.CellValue, out formula))
                        {
                            checkRegion = 1;
                            sheet.Rows[i].RowStatus = RowStatus.Deleted;
                            var formulaResultTemp = ParsingVariable(formula, fields, detailData, userinfo);
                            DataTable dtTemp = new DataTable();
                            var computeResultTemp = dtTemp.Compute(formulaResultTemp, null);
                            bool ifValueTemp = false;
                            if (computeResultTemp == null || !bool.TryParse(computeResultTemp.ToString(), out ifValueTemp))
                            {
                                throw new Exception("ElseIF函数条件格式错误");
                            }
                            if (ifValueTemp)//如果条件为true，则继续解析里边的每行数据格式和内容
                            {
                                var newrowTemp = ParsingData(sheet, sheet.Rows[i + 1], celltemp, fields, detailData, userinfo);
                                if (newrowTemp != null && newrowTemp.Count > 0)
                                    newRows.AddRange(newrowTemp);

                            }
                        }
                        //判断同层的else逻辑
                        else if (KeywordHelper.IsKey_Else(celltemp.CellValue))
                        {
                            checkRegion = 2;
                            sheet.Rows[i].RowStatus = RowStatus.Deleted;
                            var newrowTemp = ParsingData(sheet, sheet.Rows[i + 1], celltemp, fields, detailData, userinfo);
                            if (newrowTemp != null && newrowTemp.Count > 0)
                                newRows.AddRange(newrowTemp);
                        }
                        //结束if模块
                        else if (KeywordHelper.IsKey_EndIF(celltemp.CellValue))
                        {
                            checkRegion = 3;
                            sheet.Rows[i].RowStatus = RowStatus.Deleted;
                            break;//完成if块的逻辑处理，跳出循环
                        }
                        else //if范围内的数据
                        {
                            var newrowTemp = ParsingData(sheet, sheet.Rows[i], celltemp, fields, detailData, userinfo);
                            if (newrowTemp != null && newrowTemp.Count > 0)
                                newRows.AddRange(newrowTemp);
                        }
                    }
                }
                if (checkRegion != 3)
                {
                    throw new Exception("IF函数必须由ENDIF结束，请检查模板定义");
                }

            }

            return newRows;
        }
        #endregion

        #region --处理Loop代码块的逻辑--
        private List<ExcelRowInfo> CheckLoopCtlCellValue(ExcelSheetInfo sheet, ExcelRowInfo row, ExcelCellInfo cell, List<IDictionary<string, object>> fields, IDictionary<string, object> detailData, AccountUserInfo userinfo)
        {
            List<ExcelRowInfo> newRows = new List<ExcelRowInfo>();
            string formula = null;
            int checkRegion = -1;//标识当前检查区域，-1=未进入Loop, 0=Loop, 1=EndLoop

            if (KeywordHelper.IsKey_Loop(cell.CellValue, out formula))
            {
                checkRegion = 0;
                row.RowStatus = RowStatus.Deleted;
                var rowIndex = sheet.Rows.IndexOf(row);
                List<ExcelRowInfo> templateRows = new List<ExcelRowInfo>();
                for (int i = rowIndex + 1; i < sheet.Rows.Count; i++)
                {
                    if (KeywordHelper.IsKey_EndLoop(cell.CellValue))
                    {
                        checkRegion = 1;
                        row.RowStatus = RowStatus.Deleted;
                        break;
                    }
                    else templateRows.Add(sheet.Rows[i]);
                }


                var fieldFormat = ParsingVariable(formula, fields, detailData, userinfo);
                var fieldnames = KeywordHelper.GetFieldNames(fieldFormat);
                if (fieldnames.Length != 1)
                {
                    throw new Exception("Loop的字段只能是一个表格控件的字段名称");
                }
                else //表格控件
                {
                    var tempfield = fieldnames[0].Trim();
                    //获取实体字段名称
                    var entityfield = fields.Find(m => tempfield.Equals(m["fieldname"]) || tempfield.Equals(m["displayname"]));
                    if ((EntityFieldControlType)entityfield["controltype"] != EntityFieldControlType.LinkeTable)
                    {
                        throw new Exception("Loop的字段必须为表格控件");
                    }
                    var fieldconfig = entityfield["fieldconfig"].ToString();
                    var linkTabelEntityId = JObject.Parse(fieldconfig)["entityId"].ToString();
                    List<IDictionary<string, object>> linkTableFields = _entityProRepository.EntityFieldProQuery(linkTabelEntityId, userinfo.UserId).FirstOrDefault().Value;

                    var entityfieldname = entityfield["fieldname"].ToString();
                    //获取表格控件的数据
                    var entityfieldvalue = detailData.ContainsKey(entityfieldname) && detailData[entityfieldname] != null ? detailData[entityfieldname] as List<IDictionary<string, object>> : null;
                    if (entityfieldvalue != null)
                    {
                        foreach (var itemDic in entityfieldvalue)
                        {
                            var rows = new ExcelRowInfo[templateRows.Count];
                            templateRows.CopyTo(rows);
                            foreach (var rowItem in rows)
                            {
                                foreach (var celltemp in rowItem.Cells)
                                {
                                    var newrowTemp = ParsingData(sheet, rowItem, celltemp, fields, detailData, userinfo, linkTableFields, itemDic);
                                    if (newrowTemp != null && newrowTemp.Count > 0)
                                        newRows.AddRange(newrowTemp);
                                }
                            }
                            newRows.AddRange(rows);
                        }
                    }
                }

                if (checkRegion != 1)
                {
                    throw new Exception("Loop函数必须由ENDLoop结束，请检查模板定义");
                }
            }

            return newRows;
        }
        #endregion

        #region --处理值函数逻辑--
        private string ParsingValueFunc(string formulaArg, List<IDictionary<string, object>> fields, IDictionary<string, object> detailData, AccountUserInfo userinfo)
        {
            List<string> fieldList = null;
            var formula = KeywordHelper.ParsingFormula(formulaArg, out fieldList);
            if (fieldList != null && fieldList.Count > 0)
            {
                foreach (var fieldFormat in fieldList)
                {
                    if (KeywordHelper.IsCurUserName(fieldFormat))
                    {
                        formula = formula.Replace(fieldFormat, userinfo.UserName.ToString());
                    }
                    else if (KeywordHelper.IsCurUserId(fieldFormat))
                    {
                        formula = formula.Replace(fieldFormat, userinfo.UserId.ToString());
                    }
                    else if (KeywordHelper.IsCurDate(fieldFormat))
                    {
                        formula = formula.Replace(fieldFormat, DateTime.Now.ToString("yyyy-MM-dd"));
                    }
                    else if (KeywordHelper.IsCurTime(fieldFormat))
                    {
                        formula = formula.Replace(fieldFormat, DateTime.Now.ToString("yyyy-MM-dd HH:mm:SS"));
                    }
                    else if (KeywordHelper.IsCurDeptName(fieldFormat))
                    {
                        formula = formula.Replace(fieldFormat, userinfo.DepartmentName.ToString());
                    }
                    else if (KeywordHelper.IsCurDeptId(fieldFormat))
                    {
                        formula = formula.Replace(fieldFormat, userinfo.DepartmentId.ToString());
                    }
                    else if (KeywordHelper.IsEnterpriseName(fieldFormat))
                    {
                        var enterpriseInfo = _accountRepository.GetEnterpriseInfo();
                        formula = formula.Replace(fieldFormat, enterpriseInfo.EnterpriseName);
                    }
                    else
                    {
                        var fieldnames = KeywordHelper.GetFieldNames(fieldFormat);
                        if (fieldnames.Length == 1)//实体的普通字段
                        {
                            var tempfield = fieldnames[0].Trim();
                            //获取实体字段名称
                            var entityfieldname = fields.Find(m => tempfield.Equals(m["fieldname"]) || tempfield.Equals(m["displayname"]))["fieldname"].ToString();
                            var entityfieldvalue = detailData.ContainsKey(entityfieldname) && detailData[entityfieldname] != null ? detailData[entityfieldname].ToString() : string.Empty;
                            formula = formula.Replace(fieldFormat, entityfieldvalue);
                        }

                    }
                }

            }
            return formula;

        }
        #endregion


        public void TetSaveDoc(string myname)
        {
            WordprocessingDocument doc = WordprocessingDocument.Open("c:/tmp/template.docx", true);
            //doc.Close();
            //doc.Dispose();
            //doc = WordprocessingDocument.Open("c:/tmp/a.docx", true);
            IEnumerable<Paragraph> ps = doc.MainDocumentPart.Document.Body.Elements<Paragraph>();
            int totalCount = ps.Count();
            for (int i = 0; i < totalCount; i++)
            {
                Paragraph p = ps.ElementAt(i);
                int topPos = p.InnerText.IndexOf("【#我的名字#】");
                if (topPos >= 0)
                {
                    ReplaceInParagraph(p, "【#我的名字#】", myname);
                }


            }


            doc.SaveAs("c:/tmp/a.pdf");
            doc.Close();
            doc.Dispose();

        }
        private class TmpClass
        {
            public Text text { get; set; }
            public Run run { get; set; }
        }
        private void ReplaceInParagraph(Paragraph p, string scheme, string replace)
        {
            List<TmpClass> removeList = new List<TmpClass>();
            bool isStart = false;
            bool isFound = false;
            Text firstText = null;
            string firstMath = "";
            foreach (Run r in p.Elements<Run>())
            {
                foreach (Text t in r.Elements<Text>())
                {
                    if (t.Text == null) continue;
                    string mat = matchText(t.Text, scheme);
                    if (mat != null)
                    {
                        if (isStart == false)
                        {
                            isStart = true;
                            firstText = t;
                            firstMath = mat;
                        }
                        else
                        {
                            TmpClass tempClass = new TmpClass()
                            {
                                run = r,
                                text = t
                            };
                            removeList.Add(tempClass);
                        }
                        scheme = scheme.Substring(mat.Length);
                        if (scheme.Length == 0)
                        {
                            isFound = true;
                            break;
                        }
                    }


                }
                if (isFound) break;
            }
            if (isFound)
            {
                if (firstText != null)
                {
                    firstText.Text = firstText.Text.Replace(firstMath, replace);
                }
                foreach (TmpClass item in removeList)
                {
                    item.run.RemoveChild(item.text);
                }
            }
        }
        private string matchText(string t1, string t2)
        {
            if (t1 == null || t2 == null) return null;
            int curIndex = 0;
            bool isStart = false;
            string matchText = "";
            int t1len = t1.Length;
            for (int i = 0; i < t1len; i++)
            {
                if (t1[i] == t2[curIndex])
                {
                    matchText = matchText + t1[i];
                    if (isStart == false)
                    {
                        isStart = true;
                    }
                    curIndex++;
                    if (curIndex == t2.Length) break;
                }
                else
                {
                    if (isStart) return null;
                }
            }
            if (isStart) return matchText;
            else return null;

        }
        private void ReplaceInRun(Run r, string scheme, string replace)
        {
            List<Text> removeList = new List<Text>();
            bool isStart = false;
            Text firstText = null;
            foreach (Text t in r.Elements<Text>())
            {
                if (t.Text == null) continue;
                if (scheme.StartsWith(t.Text))
                {
                    if (isStart == false)
                    {
                        isStart = true;
                        firstText = t;
                    }
                    else
                    {
                        removeList.Add(t);
                    }
                    scheme = scheme.Substring(t.Text.Length);
                    if (scheme.Length == 0) break;
                }

            }
            if (removeList.Count > 0)
            {
                foreach (Text t in removeList)
                {
                    r.RemoveChild<Text>(t);
                }
            }
            if (firstText != null) firstText.Text = replace;
        }



        public byte[] GetOutputDocument(OutputDocumentParameter formData, out string fileName)
        {
            fileName = null;
            if (formData.Data != null)
            {
                if (formData.Data.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    throw new Exception("please upload a valid excel file of version 2007 and above");
                }
                var excelData = ExcelHelper.ReadExcel(formData.Data.OpenReadStream());
                if (excelData == null || excelData.Sheets == null || excelData.Sheets.Count == 0)
                    throw new Exception("error");
                //excelData.Sheets.FirstOrDefault().Rows.Add()

                var sheet = excelData.Sheets.FirstOrDefault();
                var rows = new List<ExcelRowInfo>();

                foreach (var row in sheet.Rows)
                {
                    rows.Add(row);
                    switch (row.RowIndex)
                    {
                        case 4:
                            var cell = row.Cells.Find(m => m.ColumnName == "C");
                            cell.CellValue = "test";
                            cell.IsUpdated = true;
                            break;
                        case 8:
                        case 10:
                        case 11:
                        case 12:
                        case 15:
                        case 17:
                            row.RowStatus = RowStatus.Deleted;
                            break;
                        case 14:
                            {
                                var row1 = sheet.Rows.Find(m => m.RowIndex == 13);
                                row1.RowStatus = RowStatus.Add;
                                var row2 = row;
                                row2.RowStatus = RowStatus.Add;
                                rows.Add(row1);
                                rows.Add(row2);
                                rows.Add(row1);
                                rows.Add(row2);
                            }
                            break;
                    }

                }

                sheet.Rows = rows;
                var bytes = ExcelHelper.WrightExcel(excelData);
                FileStream fs = new FileStream(string.Format(@"C:\Users\Administrator\Desktop\{0}.xlsx", Guid.NewGuid().ToString()), FileMode.Create);
                fs.Write(bytes, 0, bytes.Length);
                fs.Dispose();
                //var fileID = Guid.NewGuid().ToString();
                //上传文档到文件服务器
                //new FileServices().UploadFile(null, fileID, string.Format("test.xlsx"), bytes);

            }

            //ExcelHelper
            return null;
        }

    }
}

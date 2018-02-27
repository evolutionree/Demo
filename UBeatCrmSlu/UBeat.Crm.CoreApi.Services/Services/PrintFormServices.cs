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

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class PrintFormServices : BaseServices
    {
        private readonly IPrintFormRepository _repository;
        private readonly IAccountRepository _accountRepository ;

        FileServices _fileServices;
        public PrintFormServices(IPrintFormRepository repository, IAccountRepository accountRepository)
        {
            _repository = repository;
            _fileServices = new FileServices();
            _accountRepository = accountRepository;
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
            if(templateInfo==null)
                throw new Exception("输出模板不正确，请重新选择模板");
            if (!templateInfo.FileId.HasValue|| templateInfo.FileId.Value==Guid.Empty)
                throw new Exception("未上传输出模板文件，请先上传输出模板文件");
            
            var fileData= _fileServices.GetFileData(null, templateInfo.FileId.ToString());
            Stream fileStream = new MemoryStream(fileData);
            var excelData = ExcelHelper.ReadExcel(fileStream);
            if (excelData == null || excelData.Sheets == null || excelData.Sheets.Count == 0)
                throw new Exception("输出模板文件解析错误，请检查模板文件格式并上传Office 2007以上版本模板文件");

            Dictionary<string, object> detailData = new Dictionary<string, object>();

            foreach (var sheet in excelData.Sheets)
            {
                var newRows = new List<ExcelRowInfo>();
                foreach (var row in sheet.Rows)
                {
                    if (row.RowStatus == RowStatus.Deleted) continue;
                    foreach (var cell in row.Cells)
                    {
                        //是否固定变量
                        CheckFixVariableCellValue(cell, usernumber);
                        //是否IF控制函数
                        newRows.AddRange(CheckIFCtlCellValue(sheet, row, cell, detailData, usernumber));
                        //是否Loop控制函数
                        newRows.AddRange(CheckLoopCtlCellValue(sheet, row, cell, detailData, usernumber));
                        
                        //是否值函数
                    }
                }
                foreach(var row in newRows)
                {
                    var rowIndex = sheet.Rows.FindLastIndex(m => m.RowIndex == row.RowIndex);
                    sheet.Rows.Insert(rowIndex+1, row);
                }
            }
            var bytes = ExcelHelper.WrightExcel(excelData);
            //documentBytes.Add(bytes);
            return new OutputResult<object> ();
        }


        #region --解析是否固定变量，并用真实值替换变量名--
        private void CheckFixVariableCellValue(ExcelCellInfo cell, int usernumber)
        {
            var userinfo = _accountRepository.GetAccountUserInfo(usernumber);
            if (KeywordHelper.IsCurUserName(cell.CellValue))
            {
                cell.CellValue = userinfo.UserName.ToString();
                cell.IsUpdated = true;
            }
            else if (KeywordHelper.IsCurUserId(cell.CellValue))
            {
                cell.CellValue = userinfo.UserId.ToString();
                cell.IsUpdated = true;
            }
            else if (KeywordHelper.IsCurDate(cell.CellValue))
            {
                cell.CellValue = DateTime.Now.ToString("yyyy-MM-dd");
                cell.IsUpdated = true;
            }
            else if (KeywordHelper.IsCurTime(cell.CellValue))
            {
                cell.CellValue = DateTime.Now.ToString("yyyy-MM-dd HH:mm:SS");
                cell.IsUpdated = true;
            }
            else if (KeywordHelper.IsCurDeptName(cell.CellValue))
            {
                
                cell.CellValue = userinfo.DepartmentName.ToString();
                cell.IsUpdated = true;
            }
            else if (KeywordHelper.IsCurDeptId(cell.CellValue))
            {
                cell.CellValue = userinfo.DepartmentId.ToString();
                cell.IsUpdated = true;
            }
            else if (KeywordHelper.IsEnterpriseName(cell.CellValue))
            {
                var enterpriseInfo = _accountRepository.GetEnterpriseInfo();
                cell.CellValue = enterpriseInfo.EnterpriseName;
                cell.IsUpdated = true;
            }
        }
        #endregion

        private List<ExcelRowInfo> CheckIFCtlCellValue(ExcelSheetInfo sheet,ExcelRowInfo row,ExcelCellInfo cell, Dictionary<string, object> detailData, int usernumber)
        {
            List<ExcelRowInfo> newRows = new List<ExcelRowInfo>();
            string formula = null;
            bool is_in_if = false;
            if (KeywordHelper.IsKey_IF(cell.CellValue,out formula))
            {
                is_in_if = true;
                row.RowStatus = RowStatus.Deleted;
            }
            else if(KeywordHelper.IsKey_ElseIF(cell.CellValue, out formula))
            {
                if (!is_in_if)
                    throw new Exception("IF函数格式错误");
                row.RowStatus = RowStatus.Deleted;
            }
            else if (KeywordHelper.IsKey_EndIF(cell.CellValue))
            {
                if (!is_in_if)
                    throw new Exception("IF函数格式错误");
                is_in_if = false;
                row.RowStatus = RowStatus.Deleted;
            }

            return newRows;
        }
        private List<ExcelRowInfo> CheckLoopCtlCellValue(ExcelSheetInfo sheet, ExcelRowInfo row, ExcelCellInfo cell, Dictionary<string, object> detailData, int usernumber)
        {
            List<ExcelRowInfo> newRows = new List<ExcelRowInfo>();
            string formula = null;
            bool is_in_loop = false;
            
            if (KeywordHelper.IsKey_Loop(cell.CellValue, out formula))
            {
                is_in_loop = true;
                row.RowStatus = RowStatus.Deleted;
            }
            else if (KeywordHelper.IsKey_EndLoop(cell.CellValue))
            {
                if (!is_in_loop)
                    throw new Exception("Loop函数格式错误");
                is_in_loop = false;
                row.RowStatus = RowStatus.Deleted;
            }
            return newRows;
        }


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

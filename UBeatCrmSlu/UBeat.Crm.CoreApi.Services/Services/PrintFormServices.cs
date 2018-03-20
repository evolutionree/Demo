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
using Irony.Parsing;
using UBeat.Crm.CoreApi.Services.Utility.OpenXMLUtility.Irony.Evaluations;
using UBeat.Crm.CoreApi.Services.Utility.OpenXMLUtility.Irony;
using System.Reflection;
using System.Data.Common;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public interface IPrintServices
    {
        /// <summary>
        /// 获取打印数据源
        /// </summary>
        /// <param name="tran">数据库事务，打开和关闭由调用方处理，实现方不需进行这两个操作，只需处理好业务逻辑即可</param>
        /// <param name="entityId">实体id</param>
        /// <param name="recId">记录id</param>
        /// <param name="usernumber">当前操作人</param>
        /// <returns>返回数据已字典形式，如果不是实体中的字段，字典中的key必须和模板定义的字段匹配上</returns>
        IDictionary<string, object> GetPrintDetailData(DbTransaction tran, Guid entityId, Guid recId, int usernumber);
    }

    public class PrintFormServices : BaseServices, IPrintServices
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

        public PrintFormServices()
        {
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
                AssemblyName = data.AssemblyName,
                ClassTypeName = data.ClassTypeName,
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
        public OutputResult<object> DeleteTemplates(DeleteTemplatesModel data, int usernumber)
        {
            if (data == null)
                throw new Exception("参数不可为空");
            if (data.RecIds == null || data.RecIds.Count == 0)
                throw new Exception("参数recids不可为空");

            _repository.DeleteTemplates(data.RecIds, usernumber);
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
                AssemblyName = data.AssemblyName,
                ClassTypeName = data.ClassTypeName,
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

        #region --获取某条数据拥有的模板列表--
        public OutputResult<object> GetRecDataTemplateList(EntityRecTempModel data, int userNumber)
        {
            if (data == null)
                throw new Exception("参数不可为空");
            return new OutputResult<object>(_repository.GetRecDataTemplateList(data.EntityId, data.RecId, userNumber));
        }
        #endregion

        #region --生成打印文档--
        public OutputResult<object> PrintEntity(PrintEntityModel data, int usernumber)
        {
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
            var fileID = Guid.NewGuid().ToString();
            string curDir = Directory.GetCurrentDirectory();
            string tmppath = Path.Combine(curDir, "reportexports");

            if (Directory.Exists(curDir))
            {
                Directory.CreateDirectory(tmppath);
            }
            string fileFullPath = Path.Combine(tmppath, fileID + ".xlsx");
            FileStream fs = new FileStream(fileFullPath, FileMode.Create);
            fs.Write(bytes, 0, bytes.Length);
            fs.Dispose();
            return new OutputResult<object>(new { FileId = fileID, FileName = string.Format("{0}.xlsx", templateInfo.TemplateName) });
        }
        #endregion

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
            else if (templateInfo.DataSourceType == DataSourceType.DbFunction) //数据库函数方式实现
            {
                if (string.IsNullOrEmpty(templateInfo.DataSourceFunc))
                    throw new Exception("数据库函数方式获取数据时，函数名称不可为空");
                detailData = _repository.GetPrintDetailDataByProc(entityId, recId, templateInfo.DataSourceFunc, usernumber);
            }
            else if (templateInfo.DataSourceType == DataSourceType.InternalMethor)
            {
                if (string.IsNullOrEmpty(templateInfo.AssemblyName))
                    throw new Exception("代码函数方式获取数据时，程序集名称不可为空");
                if (string.IsNullOrEmpty(templateInfo.ClassTypeName))
                    throw new Exception("代码函数方式获取数据时，类名称不可为空");
                var assemblyName = templateInfo.AssemblyName; //如：UBeat.Crm.CoreApi.Services
                var classTypeName = templateInfo.ClassTypeName; //如：UBeat.Crm.CoreApi.Services.Services.PrintFormServices

                Assembly assembly;
                if (templateInfo.AssemblyName.EndsWith(".dll"))
                {
                    string currentDirectory = Path.GetDirectoryName(typeof(PrintFormServices).Assembly.Location);
                    var assemblyFile = Path.Combine(currentDirectory, templateInfo.AssemblyName);
                    assembly = Assembly.LoadFrom(assemblyFile);
                }
                else
                {
                    assembly = Assembly.Load(new AssemblyName(assemblyName));
                }

                Type type = assembly.GetType(classTypeName);//用类型的命名空间和名称获得类型
                object obj = null;
                try
                {
                    obj = Activator.CreateInstance(type);//利用无参数实例初始化类型
                }
                catch
                {
                    throw new Exception(string.Format("创建类{0}出错，请检查是否包含默认无参构造函数", classTypeName));
                }
                if (obj is IPrintServices)
                {
                    using (var conn = GetDbConnect())
                    {
                        conn.Open();
                        var tran = conn.BeginTransaction();
                        try
                        {
                            detailData = (obj as IPrintServices).GetPrintDetailData(tran, entityId, recId, usernumber);
                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            throw new Exception("数据库执行失败");
                        }
                        finally
                        {
                            conn.Close();
                            conn.Dispose();
                        }
                    }
                }
                else
                {
                    throw new Exception("数据库调用方法必须实现IPrintServices接口");
                }

            }

            return detailData;
        }

        public IDictionary<string, object> GetPrintDetailData(DbTransaction tran, Guid entityId, Guid recId, int usernumber)
        {
            return null;
        }

        #endregion

        #region --解析excel每个单元格的数据，并得到结果--
        private List<ExcelRowInfo> ParsingData(ExcelSheetInfo sheet, ExcelRowInfo row, ExcelCellInfo cell, List<IDictionary<string, object>> fields, IDictionary<string, object> detailData, AccountUserInfo userinfo, List<IDictionary<string, object>> linkTableFields = null, IDictionary<string, object> linkTableDetailData = null)
        {
            var newRows = new List<ExcelRowInfo>();
            bool isKey_IF = false;
            //是否IF控制函数
            var newRowsTemp = CheckIFCtlCellValue(sheet, row, cell, fields, detailData, userinfo, out isKey_IF, linkTableFields, linkTableDetailData);
            if (newRowsTemp != null && newRowsTemp.Count > 0)
                newRows.AddRange(newRowsTemp);
            if (isKey_IF)
                return newRows;
            bool isLoop = false;
            //是否Loop控制函数
            newRowsTemp = CheckLoopCtlCellValue(sheet, row, cell, fields, detailData, userinfo, out isLoop);
            if (newRowsTemp != null && newRowsTemp.Count > 0)
                newRows.AddRange(newRowsTemp);
            if (isLoop)
                return newRows;
            //解析表达式的值，得到最后的表达式字符串
            var formula = GetExpressionValue(cell.CellValue, fields, detailData, userinfo, linkTableFields, linkTableDetailData);
            if (cell.CellValue != formula)
            {
                cell.IsUpdated = true;
                cell.CellValue = formula;
            }
            return newRows;
        }
        #endregion

        #region --获取表达式的值，使用词法分析，并计算该表达式的最终数据--
        public string GetExpressionValue(string input, List<IDictionary<string, object>> fields, IDictionary<string, object> detailData, AccountUserInfo userinfo, List<IDictionary<string, object>> linkTableFields = null, IDictionary<string, object> linkTableDetailData = null)
        {

            var grammar = new ExpressionGrammar();
            var language = new LanguageData(grammar);
            var parser = new Parser(language);
            var syntaxTree = parser.Parse(input);
            if (syntaxTree.Root == null)
            {
                return input;
            }
            var res = PerformEvaluate(syntaxTree.Root, fields, detailData, userinfo, linkTableFields, linkTableDetailData);
            if (res == null)
                return input;
            var valueResult = res.Value == null ? null : res.Value.ToString();

            return valueResult;
        }

        public Evaluation PerformEvaluate(ParseTreeNode node, List<IDictionary<string, object>> fields, IDictionary<string, object> detailData, AccountUserInfo userinfo, List<IDictionary<string, object>> linkTableFields = null, IDictionary<string, object> linkTableDetailData = null)
        {
            switch (node.Term.Name)
            {
                case "BinaryExpression":
                    return ParsingBinaryExpression(node, fields, detailData, userinfo, linkTableFields, linkTableDetailData);
                case "Number":
                    return ParsingNumberExpression(node);
                case "String":
                    return new ConstantEvaluation(node.Token.Text.Trim().Trim('"'));
                case "Field":
                    return ParsingFieldExpression(node, fields, detailData, userinfo, linkTableFields, linkTableDetailData);
                case "FuncDefExpression":
                    return ParsingFuncNameExpression(node, fields, detailData, userinfo);
                case "BoolenExpression":
                    return ParsingBoolExpression(node, fields, detailData, userinfo, linkTableFields, linkTableDetailData);
                case "Arg":
                    return ParsingArgExpression(node, fields, detailData, userinfo, linkTableFields, linkTableDetailData);
                case "Term":
                case "Expression":
                    return ParsingTermExpression(node, fields, detailData, userinfo, linkTableFields, linkTableDetailData);
                default: break;
            }

            throw new InvalidOperationException($"Unrecognizable term {node.Term.Name}.");
        }
        #endregion

        #region --解析数字节点表达式--
        private Evaluation ParsingNumberExpression(ParseTreeNode node)
        {
            var value = Convert.ToDouble(node.Token.Text);
            return new ConstantEvaluation(value);
        }

        #endregion

        #region --解析实体字段节点的表达式--
        private Evaluation ParsingFieldExpression(ParseTreeNode node, List<IDictionary<string, object>> fields, IDictionary<string, object> detailData, AccountUserInfo userinfo, List<IDictionary<string, object>> linkTableFields = null, IDictionary<string, object> linkTableDetailData = null)
        {
            if (node.Token == null || string.IsNullOrEmpty(node.Token.Text))
            {
                return new ConstantEvaluation(null);
            }
            var formula = node.Token.Text;
            var isLinkTabelField = formula.Split('.').Length > 1;//判断是否是嵌套表格控件中的字段
            string formulaValue = null;
            if (isLinkTabelField)
            {
                formulaValue = ParsingLinkTableVariable(formula, linkTableFields, linkTableDetailData, userinfo);
            }
            else formulaValue = ParsingVariable(formula, fields, detailData, userinfo);
            return new ConstantEvaluation(formulaValue);
        }
        #endregion

        #region --解析函数节点表达式--
        private Evaluation ParsingFuncNameExpression(ParseTreeNode node, List<IDictionary<string, object>> fields, IDictionary<string, object> detailData, AccountUserInfo userinfo, List<IDictionary<string, object>> linkTableFields = null, IDictionary<string, object> linkTableDetailData = null)
        {
            object formulaResult = null;

            var funcNameNode = node.ChildNodes.FirstOrDefault(m => m.Term.Name == "FuncName");

            var argsExpressionNode = node.ChildNodes.FirstOrDefault(m => m.Term.Name == "FuncArgsExpression");
            if (funcNameNode == null || argsExpressionNode == null)
            {
                throw new InvalidOperationException($"函数定义错误{node.Term.Name}.");
            }
            if (funcNameNode.Token == null || string.IsNullOrEmpty(funcNameNode.Token.Text))
            {
                return new ConstantEvaluation(null);
            }
            var funcName = funcNameNode.Token.Text.ToLower().Trim();
            switch (funcName)
            {
                case "count":
                    {
                        formulaResult = ExcuteCount(argsExpressionNode, fields, detailData, userinfo);
                    }
                    break;
                case "columnsum":
                    {
                        formulaResult = ExcuteColumnSum(argsExpressionNode, fields, detailData, userinfo);
                    }
                    break;
                case "concat":
                    {
                        StringBuilder argvalues = new StringBuilder();
                        foreach (var argExpr in argsExpressionNode.ChildNodes)
                        {
                            var argEvaluate = PerformEvaluate(argExpr, fields, detailData, userinfo, linkTableFields, linkTableDetailData);
                            argvalues.Append(argEvaluate.Value == null ? string.Empty : argEvaluate.Value.ToString());
                        }
                        formulaResult = argvalues.ToString();
                    }
                    break;

            }

            return new ConstantEvaluation(formulaResult);
        }

        #region --执行columnsum函数--
        private double ExcuteColumnSum(ParseTreeNode argsExpressionNode, List<IDictionary<string, object>> fields, IDictionary<string, object> detailData, AccountUserInfo userinfo)
        {
            if (argsExpressionNode.ChildNodes.Count != 1)
            {
                throw new Exception("columnsum函数定义错误");
            }
            var argNode = argsExpressionNode.ChildNodes.FirstOrDefault();
            if (argNode.ChildNodes.Count != 1)
            {
                throw new Exception("参数格式错误");
            }
            var fieldDef = argNode.ChildNodes.FirstOrDefault();

            var formula = fieldDef.Token == null ? string.Empty : fieldDef.Token.Text.ToString();
            var fieldnames = KeywordHelper.GetFieldNames(formula);
            if (fieldnames.Length != 2)
            {
                throw new Exception("columnsum函数定义错误,参数必须是嵌套实体的字段，如产品明细.数量");
            }
            var tempEntityfield = fieldnames[0].Trim();//实体字段
            var entityfield = fields.Find(m => tempEntityfield.Equals(m["fieldname"]) || tempEntityfield.Equals(m["displayname"]));
            if ((EntityFieldControlType)entityfield["controltype"] != EntityFieldControlType.LinkeTable)
            {
                throw new Exception("columnsum函数定义错误，函数的字段的父级字段必须为表格控件");
            }
            var fieldconfig = entityfield["fieldconfig"].ToString();
            var linkTabelEntityId = JObject.Parse(fieldconfig)["entityId"].ToString();
            var linkTableFields = _entityProRepository.EntityFieldProQuery(linkTabelEntityId, userinfo.UserId).FirstOrDefault().Value;

            var entityfieldname = entityfield["fieldname"].ToString();
            //获取表格控件的数据
            var entityfieldvalue = detailData.ContainsKey(entityfieldname) && detailData[entityfieldname] != null ? detailData[entityfieldname] as List<IDictionary<string, object>> : null;
            if (entityfieldvalue != null)
            {
                var tempfield = fieldnames[1].Trim();//嵌套实体的字段
                var fieldname = linkTableFields.Find(m => tempfield.Equals(m["fieldname"]) || tempfield.Equals(m["displayname"]))["fieldname"].ToString(); ;
                return entityfieldvalue.Sum(m =>
                {
                    double tempvalue = 0;
                    double.TryParse(m[fieldname] == null ? "0" : m[fieldname].ToString(), out tempvalue);
                    return tempvalue;
                }
                );
            }

            return 0;
        }
        #endregion

        #region --执行count函数--
        private int ExcuteCount(ParseTreeNode argsExpressionNode, List<IDictionary<string, object>> fields, IDictionary<string, object> detailData, AccountUserInfo userinfo)
        {
            if (argsExpressionNode.ChildNodes.Count != 1)
            {
                throw new Exception("count函数定义错误");
            }

            var argNode = argsExpressionNode.ChildNodes.FirstOrDefault();
            if (argNode.ChildNodes.Count != 1)
            {
                throw new Exception("参数格式错误");
            }
            var fieldDef = argNode.ChildNodes.FirstOrDefault();

            var formula = fieldDef.Token == null ? string.Empty : fieldDef.Token.Text.ToString();

            var fieldnames = KeywordHelper.GetFieldNames(formula);
            if (fieldnames.Length != 1)
            {
                throw new Exception("count函数定义错误,参数必须是嵌套字段");
            }
            var tempEntityfield = fieldnames[0].Trim();//实体字段
            var entityfield = fields.Find(m => tempEntityfield.Equals(m["fieldname"]) || tempEntityfield.Equals(m["displayname"]));
            if ((EntityFieldControlType)entityfield["controltype"] != EntityFieldControlType.LinkeTable)
            {
                throw new Exception("count函数定义错误，函数的字段必须为表格控件");
            }
            var entityfieldname = entityfield["fieldname"].ToString();
            //获取表格控件的数据
            var entityfieldvalue = detailData.ContainsKey(entityfieldname) && detailData[entityfieldname] != null ? detailData[entityfieldname] as List<IDictionary<string, object>> : null;
            if (entityfieldvalue != null)
            {
                return entityfieldvalue.Count;//计算嵌套实体的行数。
            }

            return 0;
        }
        #endregion

        #endregion

        #region --解析函数参数节点表达式--
        private Evaluation ParsingArgExpression(ParseTreeNode node, List<IDictionary<string, object>> fields, IDictionary<string, object> detailData, AccountUserInfo userinfo, List<IDictionary<string, object>> linkTableFields = null, IDictionary<string, object> linkTableDetailData = null)
        {
            if (node.ChildNodes.Count == 0)
            {
                return new ConstantEvaluation(null);
            }
            return PerformEvaluate(node.ChildNodes[0], fields, detailData, userinfo, linkTableFields, linkTableDetailData);

        }
        #endregion

        #region --解析Term节点表达式--
        private Evaluation ParsingTermExpression(ParseTreeNode node, List<IDictionary<string, object>> fields, IDictionary<string, object> detailData, AccountUserInfo userinfo, List<IDictionary<string, object>> linkTableFields = null, IDictionary<string, object> linkTableDetailData = null)
        {
            if (node.ChildNodes.Count == 0)
            {
                return new ConstantEvaluation(null);
            }
            return PerformEvaluate(node.ChildNodes[0], fields, detailData, userinfo, linkTableFields, linkTableDetailData);

        }
        #endregion

        #region --解析数学公式节点，并计算数学公式--
        private Evaluation ParsingBinaryExpression(ParseTreeNode node, List<IDictionary<string, object>> fields, IDictionary<string, object> detailData, AccountUserInfo userinfo, List<IDictionary<string, object>> linkTableFields = null, IDictionary<string, object> linkTableDetailData = null)
        {
            var leftNode = node.ChildNodes[0];
            var opNode = node.ChildNodes[1];
            var rightNode = node.ChildNodes[2];
            Evaluation left = PerformEvaluate(leftNode, fields, detailData, userinfo, linkTableFields, linkTableDetailData);
            Evaluation right = PerformEvaluate(rightNode, fields, detailData, userinfo, linkTableFields, linkTableDetailData);
            BinaryOperation op = BinaryOperation.Add;
            switch (opNode.Term.Name)
            {
                case "+":
                    op = BinaryOperation.Add;
                    break;
                case "-":
                    op = BinaryOperation.Sub;
                    break;
                case "*":
                    op = BinaryOperation.Mul;
                    break;
                case "/":
                    op = BinaryOperation.Div;
                    break;
            }
            return new BinaryEvaluation(left, right, op);
        }
        #endregion

        #region --解析关系运算节点，并计算表达式结果--
        private Evaluation ParsingBoolExpression(ParseTreeNode node, List<IDictionary<string, object>> fields, IDictionary<string, object> detailData, AccountUserInfo userinfo, List<IDictionary<string, object>> linkTableFields = null, IDictionary<string, object> linkTableDetailData = null)
        {
            var leftNode = node.ChildNodes[0];
            var opNode = node.ChildNodes[1];
            var rightNode = node.ChildNodes[2];
            Evaluation left = PerformEvaluate(leftNode, fields, detailData, userinfo, linkTableFields, linkTableDetailData);
            Evaluation right = PerformEvaluate(rightNode, fields, detailData, userinfo, linkTableFields, linkTableDetailData);
            BoolOperation op = BoolOperation.Equal;
            switch (opNode.Term.Name)
            {
                case "==":
                    op = BoolOperation.Equal;
                    break;
                case ">":
                    op = BoolOperation.GreaterThan;
                    break;
                case ">=":
                    op = BoolOperation.GreaterThanEqual;
                    break;
                case "<":
                    op = BoolOperation.LessThan;
                    break;
                case "<=":
                    op = BoolOperation.LessThanEqual;
                    break;
                case "!=":
                    op = BoolOperation.NotEqual;
                    break;
                case "&&":
                    op = BoolOperation.And; break;
                case "||":
                    op = BoolOperation.OR;
                    break;
            }
            return new BoolEvaluation(left, right, op);
        }
        #endregion

        #region --计算变量的真实值--

        #region --计算实体变量的值--
        /// <summary>
        /// 解析实体变量
        /// </summary>
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
                            bool isId = tempfield.ToLower().EndsWith("_id");//判断是否是取id的值，如果不是，则取对应的 _name 字段
                            var entityfieldvalue = string.Empty;
                            if (detailData.ContainsKey(tempfield)) //先匹配key，如果不存在，再解析实体字段定义，拿到字段名称再查询字典数据
                            {
                                entityfieldvalue = detailData[tempfield] != null ? detailData[tempfield].ToString() : string.Empty;
                                formula = formula.Replace(fieldFormat, entityfieldvalue);
                            }
                            else
                            {
                                if (isId)
                                {
                                    tempfield = tempfield.Remove(tempfield.ToLower().LastIndexOf("_id"));
                                }
                                //获取实体字段名称
                                var fieldobj = fields.Find(m => tempfield.Equals(m["fieldname"]) || tempfield.Equals(m["displayname"]));
                                if (fieldobj != null && fieldobj.ContainsKey("fieldname") && fieldobj["fieldname"] != null)
                                {
                                    switch ((EntityFieldControlType)fieldobj["controltype"])
                                    {
                                        case EntityFieldControlType.AreaGroup:
                                        case EntityFieldControlType.FileAttach:
                                        case EntityFieldControlType.HeadPhoto:
                                        case EntityFieldControlType.TreeSingle:
                                        case EntityFieldControlType.TakePhoto:
                                        case EntityFieldControlType.TreeMulti:
                                        case EntityFieldControlType.LinkeTable:
                                            break;
                                        default:
                                            { //如果是表格控件等嵌套实体字段，则跳过解析，由处理嵌套表格控件的逻辑处理
                                                var entityfieldname = fieldobj["fieldname"].ToString();

                                                if (!isId && detailData.ContainsKey(entityfieldname + "_name"))
                                                {
                                                    var entityfieldkey = entityfieldname + "_name";
                                                    entityfieldvalue = detailData.ContainsKey(entityfieldkey) && detailData[entityfieldkey] != null ? detailData[entityfieldkey].ToString() : string.Empty;
                                                }
                                                else entityfieldvalue = detailData.ContainsKey(entityfieldname) && detailData[entityfieldname] != null ? detailData[entityfieldname].ToString() : string.Empty;
                                                formula = formula.Replace(fieldFormat, entityfieldvalue);
                                            }
                                            break;

                                    }


                                }
                            }

                        }

                    }
                }

            }
            return formula;

        }
        #endregion

        #region --计算嵌套表格控件变量的值--
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
                        bool isId = tempfield.ToLower().EndsWith("_id");//判断是否是取id的值，如果不是，则取对应的 _name 字段
                        var entityfieldvalue = string.Empty;
                        if (tableDetailData.ContainsKey(tempfield)) //先匹配key，如果不存在，再解析实体字段定义，拿到字段名称再查询字典数据
                        {
                            entityfieldvalue = tableDetailData[tempfield] != null ? tableDetailData[tempfield].ToString() : string.Empty;
                        }
                        else
                        {
                            if (isId)
                            {
                                tempfield = tempfield.Remove(tempfield.ToLower().LastIndexOf("_id"));
                            }
                            //获取实体字段名称
                            var fieldobj = tableFields.Find(m => tempfield.Equals(m["fieldname"]) || tempfield.Equals(m["displayname"]));
                            if (fieldobj != null && fieldobj.ContainsKey("fieldname") && fieldobj["fieldname"] != null)
                            {
                                switch ((EntityFieldControlType)fieldobj["controltype"])
                                {
                                    case EntityFieldControlType.AreaGroup:
                                    case EntityFieldControlType.FileAttach:
                                    case EntityFieldControlType.HeadPhoto:
                                    case EntityFieldControlType.TreeSingle:
                                    case EntityFieldControlType.TakePhoto:
                                    case EntityFieldControlType.TreeMulti:
                                    case EntityFieldControlType.LinkeTable:
                                        break;
                                    default:
                                        {
                                            //获取实体字段名称
                                            var entityfieldname = fieldobj["fieldname"].ToString();
                                            if (!isId && tableDetailData.ContainsKey(entityfieldname + "_name"))
                                            {
                                                var nameFeild = entityfieldname + "_name";
                                                entityfieldvalue = tableDetailData.ContainsKey(nameFeild) && tableDetailData[nameFeild] != null ? tableDetailData[nameFeild].ToString() : string.Empty;
                                            }
                                            else entityfieldvalue = tableDetailData.ContainsKey(entityfieldname) && tableDetailData[entityfieldname] != null ? tableDetailData[entityfieldname].ToString() : string.Empty;
                                        }
                                        break;
                                }
                            }
                        }
                        formula = formula.Replace(fieldFormat, entityfieldvalue);
                    }
                }

            }
            return formula;

        }
        #endregion

        #endregion

        #region --处理IF代码块的逻辑--
        private List<ExcelRowInfo> CheckIFCtlCellValue(ExcelSheetInfo sheet, ExcelRowInfo row, ExcelCellInfo cell, List<IDictionary<string, object>> fields, IDictionary<string, object> detailData, AccountUserInfo userinfo, out bool isKey_IF, List<IDictionary<string, object>> linkTableFields = null, IDictionary<string, object> linkTableDetailData = null)
        {
            isKey_IF = false;
            List<ExcelRowInfo> newRows = new List<ExcelRowInfo>();
            string formula = null;
            int checkRegion = -1;//标识当前检查区域，-1=未进入if 0=if,1=endif
            if (KeywordHelper.IsKey_IF(cell.CellValue, out formula))
            {
                isKey_IF = true;
                checkRegion = 0;
                row.RowStatus = RowStatus.Deleted;
                //解析表达式，得到最终的表达式字符串
                var formulaResult = GetExpressionValue(formula, fields, detailData, userinfo, linkTableFields, linkTableDetailData);
                //处理比较操作符
                //formulaResult = formulaResult.Replace("==", "=");

                //DataTable dt = new DataTable();
                //var computeResult = dt.Compute(formulaResult, null);//计算表达式的值
                bool ifValue = false;
                if (formulaResult == null || !bool.TryParse(formulaResult.ToString(), out ifValue))
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
                        //结束if模块
                        if (KeywordHelper.IsKey_EndIF(celltemp.CellValue))
                        {
                            checkRegion = 1;
                            sheet.Rows[i].RowStatus = RowStatus.Deleted;
                            break;//完成if块的逻辑处理，跳出循环
                        }
                        else //if范围内的数据
                        {
                            if (!ifValue)
                            {
                                sheet.Rows[i].RowStatus = RowStatus.Deleted;

                            }
                            else
                            {
                                var newrowTemp = ParsingData(sheet, sheet.Rows[i], celltemp, fields, detailData, userinfo);
                                if (newrowTemp != null && newrowTemp.Count > 0)
                                    newRows.AddRange(newrowTemp);
                            }
                        }
                    }
                    if (checkRegion == 1)
                        break;
                }
                if (checkRegion != 1)
                {
                    throw new Exception("IF函数必须由ENDIF结束，请检查模板定义");
                }

            }

            return newRows;
        }
        #endregion

        #region --处理Loop代码块的逻辑--
        private List<ExcelRowInfo> CheckLoopCtlCellValue(ExcelSheetInfo sheet, ExcelRowInfo row, ExcelCellInfo cell, List<IDictionary<string, object>> fields, IDictionary<string, object> detailData, AccountUserInfo userinfo, out bool isLoop)
        {
            isLoop = false;
            List<ExcelRowInfo> newRows = new List<ExcelRowInfo>();
            string formula = null;
            int checkRegion = -1;//标识当前检查区域，-1=未进入Loop, 0=Loop, 1=EndLoop

            if (KeywordHelper.IsKey_Loop(cell.CellValue, out formula))
            {
                isLoop = true;
                checkRegion = 0;
                row.RowStatus = RowStatus.Deleted;
                var rowIndex = sheet.Rows.IndexOf(row);
                List<ExcelRowInfo> templateRows = new List<ExcelRowInfo>();
                for (int i = rowIndex + 1; i < sheet.Rows.Count; i++)
                {
                    foreach (var celltemp in sheet.Rows[i].Cells)
                    {
                        if (KeywordHelper.IsKey_EndLoop(celltemp.CellValue))
                        {
                            checkRegion = 1;
                            row.RowStatus = RowStatus.Deleted;
                            break;
                        }
                    }
                    if (checkRegion == 1)
                    {
                        sheet.Rows[i].RowStatus = RowStatus.Deleted;
                        break;
                    }
                    else templateRows.Add(sheet.Rows[i]);
                }
                if (checkRegion != 1)
                {
                    throw new Exception("Loop函数必须由ENDLoop结束，请检查模板定义");
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
                        //获取循环模板行中最大的RowIndex属性，用于精确定位新增的循环行数据在整个excel中的位置
                        var templateRowsMaxIndex = templateRows.Last().RowIndex;
                        //循环表格控件的数据集合，每行为一组，对templateRows进行解析
                        foreach (var itemDic in entityfieldvalue)
                        {
                            foreach (var rowItem in templateRows)
                            {
                                var rowItemTemp = rowItem.Clone();
                                foreach (var celltemp in rowItemTemp.Cells)
                                {
                                    var newrowTemp = ParsingData(sheet, rowItemTemp, celltemp, fields, detailData, userinfo, linkTableFields, itemDic);
                                    if (newrowTemp != null && newrowTemp.Count > 0)
                                        newRows.AddRange(newrowTemp);
                                }
                                rowItemTemp.RowStatus = RowStatus.Add;
                                rowItemTemp.RowIndex = templateRowsMaxIndex;
                                newRows.Add(rowItemTemp);
                            }
                        }
                        //遍历了所有表格控件的行数据后，把原有excel模板中的模板行标记为deleted状态
                        foreach (var rowItem in templateRows)
                        {
                            rowItem.RowStatus = RowStatus.Deleted;
                        }
                    }
                }


            }

            return newRows;
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

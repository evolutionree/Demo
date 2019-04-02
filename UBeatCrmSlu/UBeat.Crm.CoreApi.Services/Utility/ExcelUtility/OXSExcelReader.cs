using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using System.IO;
using Newtonsoft.Json;
using System.Xml;
using System.Text;
using DocumentFormat.OpenXml;
using UBeat.Crm.CoreApi.Services.Models.Excels;
using System.Threading;

namespace UBeat.Crm.CoreApi.Services.Utility.ExcelUtility
{
    public class OXSExcelReader
    {


        #region -- 读取Excel模板--
        /// <summary>
        /// 读取Excel模板定义数据：header+列的字段映射
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static ExcelTemplate ReadExcelTemplate(Stream file)
        {
            var excel = new ExcelTemplate();

            WorkbookPart workbookPart;
            try
            {
                var document = SpreadsheetDocument.Open(file, false);
                workbookPart = document.WorkbookPart;
                var shareStringPart = workbookPart.GetPartsOfType<SharedStringTablePart>().First();
                excel.SharedStringTableOuterXml = shareStringPart == null || shareStringPart.SharedStringTable == null ? null : shareStringPart.SharedStringTable.OuterXml;

                var sheets = workbookPart.Workbook.Descendants<Sheet>();
                var sqlSheet = sheets.Where(m => m.Name.Value.ToLower().Equals("sqldefine")).FirstOrDefault();
                var sqlMap = ReadSheetSqlTemplate(workbookPart, sqlSheet);
                foreach (var sheet in sheets)
                {
                    var workSheet = ((WorksheetPart)workbookPart.GetPartById(sheet.Id)).Worksheet;
                    if (sheet.Name.Value.Equals("注意事项"))
                    {
                        excel.AttentionOuterXml = workSheet.OuterXml;
                        continue;
                    }
                    if (sheet.Name.Value.ToLower().Equals("sqldefine"))
                    {
                        excel.SQLDefineOuterXml = workSheet.OuterXml;
                        continue;
                    }
                    if (sheet.Name.Value.Equals("模板定义说明"))
                    {
                        excel.TemplateDefineOuterXml = workSheet.OuterXml;
                        continue;
                    }
                    var data = ReadSheetTemplate(workbookPart, sheet);
                    if (data.ColumnMap.Count == 0)
                    {
                        throw new Exception(string.Format("{0}的字段映射列设置错误", sheet.Name));
                    }
                    var isStoredProcCursor = sqlMap.ContainsKey(sheet.Name) ? sqlMap[sheet.Name][0] : "0";
                    data.IsStoredProcCursor = int.Parse(isStoredProcCursor);
                    if (!sqlMap.ContainsKey(sheet.Name))
                    {
                        throw new Exception(string.Format("没有定义{0}的执行sql", sheet.Name));
                    }
                    data.ExecuteSQL = sqlMap[sheet.Name][1];
                    data.DefaultDataSql = sqlMap[sheet.Name][2];
                    excel.DataSheets.Add(data);
                }

            }
            catch (Exception ex)
            {
                throw new Exception("读取模板文件出错", ex);
            }
            return excel;
        }
        #endregion

        #region --读取Excel的数据行--



        public static ImportSheetData ReadExcelList(Stream file, SheetDefine sheetTemplate)
        {
            var list = new List<SheetDefine>();
            list.Add(sheetTemplate);
            return ReadExcelList(file, list).FirstOrDefault();
        }
        public static List<Dictionary<string, object>> ReadExcelFirstSheet(Stream file)
        {
            WorkbookPart workbookPart;
            try
            {
                var document = SpreadsheetDocument.Open(file, false);
                workbookPart = document.WorkbookPart;
                var sheet = workbookPart.Workbook.Descendants<Sheet>().FirstOrDefault();
                return ReadSheet(workbookPart, sheet);
            }
            catch (Exception ex)
            {
            }
            return null;
        }
        public static List<ImportSheetData> ReadExcelList(Stream file, List<SheetDefine> sheetDefines)
        {
            var dataList = new List<ImportSheetData>();

            WorkbookPart workbookPart;

            try
            {
                if (sheetDefines == null || sheetDefines.Count <= 0)
                {
                    throw new Exception("sheetDefines 不可为空");
                }
                var document = SpreadsheetDocument.Open(file, false);
                workbookPart = document.WorkbookPart;

                var sheets = workbookPart.Workbook.Descendants<Sheet>();
                if (sheets.Where(m => m.Name.Value.ToLower().Equals("sqldefine") || m.Name.Value.Equals("注意事项")).Count() > 0)
                {
                    throw new Exception("导入的Excel不能包含\"注意事项\" 和\"sqldefine\"的页签");
                }

                //if (sheets.Count() != sheetDefines.Count)
                //{
                //    throw new Exception("导入的Excel与模板不匹配");
                //}
                foreach (var sheet in sheets)
                {
                    SheetDefine template = sheetDefines.Find(m => m.SheetName == sheet.Name);

                    if (template == null)
                        throw new Exception(string.Format("导入的Excel与模板不匹配,Sheet表【{0}】不属于模板的有效数据表", sheet.Name));
                    dataList.Add(ReadSheet(workbookPart, sheet, template));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return dataList;
        }
        #endregion


        #region --private methor--
        //读取模板数据
        private static Dictionary<string, List<string>> ReadSheetSqlTemplate(WorkbookPart workbookPart, Sheet sheet)
        {
            var data = new Dictionary<string, List<string>>();
            if (sheet == null)
                return data;

            var workSheet = ((WorksheetPart)workbookPart.GetPartById(sheet.Id)).Worksheet;


            var sheetData = workSheet.Elements<SheetData>().First();
            List<Row> rows = sheetData.Elements<Row>().ToList();

            if (rows.Count < 2)
            {
                throw new Exception("定义sql的sheet格式不正确");
            }
            //获取最后一行非空行的下标
            //var lastNotEmptyRowIndex = rows.Last(m => !IsEmptyRow(m, workbookPart)).RowIndex.Value;
            //从表头下面一行开始读取数据，直到最后一行非空行
            for (var i = 1; i < rows.Count; i++)
            {
                var rowData = ReadOneRowData(rows[i], workbookPart);

                if (rowData != null && rowData.Count >= 3 && !string.IsNullOrEmpty(rowData[0]))
                {
                    var value = new List<string>();
                    value.Add(rowData[1]);
                    value.Add(rowData[2]);
                    if (rowData.Count >= 4)
                    {
                        value.Add(rowData[3]);
                    }
                    else
                    {
                        value.Add("");
                    }

                    if (data.ContainsKey(rowData[0]))
                        data[rowData[0]] = value;
                    else data.Add(rowData[0], value);
                }

            }
            return data;
        }

        //读取模板数据
        private static SheetTemplate ReadSheetTemplate(WorkbookPart workbookPart, Sheet sheet)
        {
            var data = new SheetTemplate();
            data.SheetName = sheet.Name;

            var workSheet = ((WorksheetPart)workbookPart.GetPartById(sheet.Id)).Worksheet;

            var columns = workSheet.Descendants<Columns>().FirstOrDefault();
            data.ColumnsOuterXml = columns.OuterXml;

            var sheetData = workSheet.Elements<SheetData>().First();
            List<Row> rows = sheetData.Elements<Row>().ToList();

            if (rows.Count < 4)
            {
                throw new Exception("Excel模板中最少包含4行，其中包括列头定义,字段映射,是否必填 和 字段类型的定义");
            }
            var lastNotEmptyRowIndex = rows.Last(m => !IsEmptyRow(m, workbookPart)).RowIndex.Value;
            //获取表头模板
            data.HeadersTemplate = ReadOXSHeaders(rows.Take((int)lastNotEmptyRowIndex - 3), workbookPart, sheet.Id);
            //获取字段映射关系以及是否必填的配置信息
            var columnMapRows = rows.Where(m => m.RowIndex <= lastNotEmptyRowIndex && m.RowIndex > lastNotEmptyRowIndex - 3);
            data.ColumnMap = ReadColumnMap(columnMapRows, workbookPart);
            //获取样式
            data.StylesheetXml = workbookPart.WorkbookStylesPart == null ? null : workbookPart.WorkbookStylesPart.Stylesheet.OuterXml;
            return data;
        }

        private static bool IsEmptyRow(Row row, WorkbookPart workbookPart)
        {
            var rowData = ReadOneRowData(row, workbookPart);
            return rowData.Count == 0 || rowData.All(m => m.Trim().Equals(string.Empty));
        }
        private static List<string> ReadOneRowData(Row row, WorkbookPart workbookPart)
        {
            var dataRow = new List<string>();
            var cellEnumerator = GetExcelCellEnumerator(row);

            while (cellEnumerator.MoveNext())
            {
                var cell = cellEnumerator.Current;
                var text = ReadExcelCell(cell, workbookPart);
                dataRow.Add(text);
            }
            return dataRow;
        }
        private static HeadersTemplate ReadOXSHeaders(IEnumerable<Row> rows, WorkbookPart workbookPart, string sheetId)
        {
            var workSheet = ((WorksheetPart)workbookPart.GetPartById(sheetId)).Worksheet;
            var mergeCells = workSheet.Elements<MergeCells>();
            List<string> outerXmls = new List<string>();
            foreach (var m in mergeCells)
            {
                outerXmls.Add(m.OuterXml);
            }

            var rowCellsTemplate = new List<CellModel>();
            foreach (var row in rows)
            {

                var cellEnumerator = GetExcelCellEnumerator(row);
                while (cellEnumerator.MoveNext())
                {
                    var cell = cellEnumerator.Current;
                    var text = ReadExcelCell(cell, workbookPart);
                    var cellModel = new CellModel();
                    if (cell.CellReference == null)
                    {
                        continue;
                    }
                    cellModel.CellReference = cell.CellReference;
                    cellModel.CellFormula = cell.CellFormula == null ? null : cell.CellFormula.Text;
                    cellModel.CellMetaIndex = cell.CellMetaIndex != null ? (uint?)cell.CellMetaIndex.Value : null;
                    cellModel.CellValue = text;
                    cellModel.ExtensionList = cell.ExtensionList == null ? null : cell.ExtensionList.OuterXml;
                    cellModel.InlineString = cell.InlineString == null ? null : cell.InlineString.OuterXml;
                    cellModel.StyleIndex = cell.StyleIndex != null ? (uint?)cell.StyleIndex.Value : null;
                    cellModel.ValueMetaIndex = cell.ValueMetaIndex != null ? (uint?)cell.ValueMetaIndex.Value : null;

                    rowCellsTemplate.Add(cellModel);
                }
            }
            var template = new HeadersTemplate()
            {
                MergeCellsOuterXmls = outerXmls,
                RowCellsTemplate = rowCellsTemplate,
                RowsCount = rows.Count()
            };
            return template;
        }

        private static List<ColumnMapModel> ReadColumnMap(IEnumerable<Row> rows, WorkbookPart workbookPart)
        {
            if (rows.Count() <= 0)
                throw new Exception("缺少模板定义必要的ColumnMap 和 IsNotEmpty 行");
            var dataRow = new List<ColumnMapModel>();
            var fieldCellEnumerator = GetExcelCellEnumerator(rows.FirstOrDefault());
            int columnIndex = 0;
            while (fieldCellEnumerator.MoveNext())
            {
                var cell = fieldCellEnumerator.Current;
                var fieldName = ReadExcelCell(cell, workbookPart);
                if (string.IsNullOrEmpty(cell.CellReference))
                {
                    columnIndex++;
                    continue;
                }
                bool isNotEmpty = false;
                FieldType fieldType = FieldType.Text;
                var currentCellName = OpenXMLExcelHelper.GetColumnName(cell.CellReference);
                if (rows.Count() >= 2)
                {
                    //获取是否必填
                    string cellReference = currentCellName + rows.ElementAtOrDefault(1).RowIndex;
                    var tempCell = rows.ElementAtOrDefault(1).Elements<Cell>().Where(c => c.CellReference.Value == cellReference).FirstOrDefault();
                    if (tempCell == null)
                    {
                        isNotEmpty = false;
                    }
                    else
                    {
                        var isNotEmptyCellValue = ReadExcelCell(tempCell, workbookPart);
                        isNotEmpty = isNotEmptyCellValue.Equals("TRUE") || isNotEmptyCellValue.Equals("1");
                    }
                }
                if (rows.Count() >= 3)
                {
                    //获取数据类型

                    string cellReference = currentCellName + rows.ElementAtOrDefault(2).RowIndex;
                    var tempCell = rows.ElementAtOrDefault(2).Elements<Cell>().Where(c => c.CellReference.Value == cellReference).FirstOrDefault();
                    if (tempCell == null)
                    {
                        fieldType = FieldType.Text;
                    }
                    else
                    {
                        var dataTypeCellValue = ReadExcelCell(tempCell, workbookPart);
                        Enum.TryParse(dataTypeCellValue, out fieldType);
                    }
                }
                var model = new ColumnMapModel()
                {
                    Index = columnIndex++,
                    FieldName = fieldName,
                    IsNotEmpty = isNotEmpty,
                    FieldType = fieldType
                };
                dataRow.Add(model);
            }
            return dataRow;
        }

        private static List<Dictionary<string, object>> ReadSheet(WorkbookPart workbookPart, Sheet sheet)
        {
            List<Dictionary<string, object>> DataRows = new List<Dictionary<string, object>>();

            var workSheet = ((WorksheetPart)workbookPart.GetPartById(sheet.Id)).Worksheet;
            int headerRowsCount = 0;
            var sheetData = workSheet.Elements<SheetData>().First();
            List<Row> rows = sheetData.Elements<Row>().ToList();
            if (rows.Count <= 0)
            {
                return DataRows;
            }
            foreach (Row row in rows)
            {
                Dictionary<string, object> rowData = new Dictionary<string, object>();
                int columnIndex = 0;
                foreach (Cell cell in row.Descendants<Cell>())
                {

                    rowData.Add(columnIndex.ToString(), GetCellValue(cell, workbookPart));
                    columnIndex++;
                }
                DataRows.Add(rowData);

            }
            return DataRows;
        }
        private static ImportSheetData ReadSheet(WorkbookPart workbookPart, Sheet sheet, SheetDefine templateDefine)
        {
            var data = new ImportSheetData();
            data.SheetName = sheet.Name;
            data.ExecuteSQL = templateDefine.ExecuteSQL;
            var workSheet = ((WorksheetPart)workbookPart.GetPartById(sheet.Id)).Worksheet;

            var sheetData = workSheet.Elements<SheetData>().First();
            List<Row> rows = sheetData.Elements<Row>().ToList();
            if (rows.Count <= 0)
            {
                return data;
                //throw new Exception(string.Format("导入Excel中不允许包含空表，但{0}为空", sheet.Name));
            }
            int headerRowsCount = 1;
            List<ColumnMapModel> columnMap = new List<ColumnMapModel>();
            if (templateDefine is SheetTemplate)
            {
                var template = templateDefine as SheetTemplate;
                headerRowsCount = template.HeadersTemplate.RowsCount;
                columnMap = template.ColumnMap;
            }
            else
            {
                bool HasTableField = false;

                //读取表头数据


                var template = templateDefine as MultiHeaderSheetTemplate;
                #region 检查是否有嵌套表格需要导入
                foreach (var header in template.Headers)
                {
                    if (header.SubHeaders != null && header.SubHeaders.Count > 0)
                    {
                        HasTableField = true;
                        headerRowsCount = 2;
                        break;
                    }
                }
                #endregion
                List<string> headrow0 = ReadOneRowData(rows[0], workbookPart);
                List<string> headrow1 = null;
                if (HasTableField) headrow1 = ReadOneRowData(rows[1], workbookPart);
                //判断是否都包含了必填列
                /*var existColumns = template.Headers.FindAll(m => m.IsNotEmpty).Select(m => m.HeaderText).Intersect(headrow1);
                var lackColumns = template.Headers.FindAll(m => m.IsNotEmpty).Select(m => m.HeaderText).Except(existColumns);
                if (lackColumns.Count() > 0)
                {
                    var lackColumnsName = string.Join(",", lackColumns.ToArray());
                    throw new Exception(string.Format("表{0}中缺少必填列：{1}", sheet.Name, lackColumnsName));
                }*/
                //读取表头，获得列映射关系，没有在模板定义的列剔除
                int indextemp = 0;
                string lastItem = "";
                int colIndex = 0;
                foreach (var item in headrow0)
                {
                    colIndex++;
                    MultiHeader header1 = null;
                    MultiHeader header2 = null;
                    if (item == null || item.Length == 0)
                    {
                        if (lastItem == null || lastItem.Length == 0)
                        {
                            continue;
                        }
                        else
                        {
                            header1 = template.Headers.FirstOrDefault(m => m.HeaderText.Trim().Equals(lastItem));
                        }
                        header2 = header1.SubHeaders.FirstOrDefault(m => m.HeaderText.Trim().Equals(headrow1[colIndex - 1]));
                    }
                    else
                    {
                        lastItem = item;
                        header1 = template.Headers.FirstOrDefault(m => m.HeaderText.Trim().Equals(lastItem));
                        if (header1 == null)
                        {
                            throw (new Exception("请使用正确的导入模板"));
                        }
                        else
                        {

                            if (header1.SubHeaders != null && header1.SubHeaders.Count > 0)
                            {
                                header2 = header1.SubHeaders.FirstOrDefault(m => m.HeaderText.Trim().Equals(headrow1[colIndex - 1]));
                                if (header2 == null) continue;
                            }
                        }
                    }
                    if (header1 == null) continue;
                    string fieldname = header1.FieldName;
                    if (header2 != null)
                    {
                        fieldname = fieldname + "." + header2.FieldName;
                    }


                    columnMap.Add(new ColumnMapModel()
                    {
                        Index = indextemp++,
                        FieldName = fieldname,
                        IsNotEmpty = (header2 == null ? header1.IsNotEmpty : header2.IsNotEmpty),
                        FieldType = header2 == null ? header1.FieldType : header2.FieldType
                    });
                }
            }
            // Read the sheet data
            if (rows.Count > headerRowsCount)
            {
                //获取最后一行非空行的下标
                var lastNotEmptyRowIndex = rows.Last(m => !IsEmptyRow(m, workbookPart)).RowIndex.Value;
                var datarows = rows.GetRange(headerRowsCount, (int)(lastNotEmptyRowIndex - headerRowsCount));
                foreach (var row in datarows)
                {
                    var rowdata = ReadRowDataToDictionary(row, workbookPart, sheet, columnMap);
                    data.DataRows.Add(rowdata);
                }

            }
            return data;
        }


        private static Dictionary<string, object> ReadRowDataToDictionary(Row row, WorkbookPart workbookPart, Sheet sheet, List<ColumnMapModel> columnMap)
        {
            var dataRow = new Dictionary<string, object>();
            var cellEnumerator = GetExcelCellEnumerator(row, columnMap.Count);
            int columnIndex = 0;

            while (cellEnumerator.MoveNext())
            {
                var cell = cellEnumerator.Current;
                var text = ReadExcelCell(cell, workbookPart).Trim();
                if (columnMap.Exists(m => m.Index == columnIndex))
                {
                    var map = columnMap.FirstOrDefault(m => m.Index == columnIndex);
                    switch (map.FieldType)
                    {
                        case FieldType.TimeDate:
                            {
                                double tmpValue = 0;
                                if (double.TryParse(text, out tmpValue))
                                {
                                    if (tmpValue > 59)
                                        tmpValue -= 1;
                                    var tmpDate = new DateTime(1899, 12, 31).AddDays(tmpValue);
                                    text = tmpDate.ToString("yyyy-MM-dd");
                                }
                                else
                                {
                                    DateTime dt;
                                    if (DateTime.TryParse(text, out dt))
                                    {
                                        text = string.Format("{0:yyyy-MM-dd}", dt);
                                    }
                                }
                            }
                            break;
                        case FieldType.TimeStamp:
                            {
                                double tmpValue = 0;
                                if (double.TryParse(text, out tmpValue))
                                {
                                    if (tmpValue > 59)
                                        tmpValue -= 1;
                                    var tmpDate = new DateTime(1899, 12, 31).AddDays(tmpValue);
                                    text = tmpDate.ToString("yyyy-MM-dd HH:mm:ss");
                                }
                                else
                                {
                                    DateTime dt;
                                    if (DateTime.TryParse(text, out dt))
                                    {
                                        text = string.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);
                                    }
                                }
                            }
                            break;
                        case FieldType.NumberDecimal:
                            {
                                double dbvalue = 0;
                                if (text != null && text.Length > 0)
                                {
                                    Double.TryParse(text, out dbvalue);
                                    text = dbvalue.ToString();
                                }
                            }
                            break;


                    }


                    //if (map.IsNotEmpty && string.IsNullOrEmpty(text))
                    //{
                    //    var columnText = OpenXMLExcelHelper.GetColumnName((uint)map.Index);
                    //    throw new Exception(string.Format("{0}的第{1}列为必填列，单元格{2}不可为空", sheet.Name, columnText, cell.CellReference.Value));
                    //}
                    dataRow.Add(map.FieldName, text);
                }
                columnIndex++;
            }
            //如果是空行，则返回null
            if (dataRow.Count == 0 || dataRow.Values.All(m => m == null || m.Equals(string.Empty)))
            {
                return null;
            }
            return dataRow;
        }


        private static IEnumerator<Cell> GetExcelCellEnumerator(Row row, int columnCnount = 0)
        {
            int currentCount = 0;

            //只有读取模板header时不需要检查，读取导入数据时，必须检查，保证解析后每行的字典数据完整
            if (columnCnount > 0)
            {
                uint rowindex = row.RowIndex.Value;
                //检查需要的每个列都有单元格数据，没有的话，则填空单元格，避免读取缺少Cell数量
                for (uint i = 0; i < columnCnount; i++)
                {
                    var columnLetter = OpenXMLExcelHelper.GetColumnName(i);
                    string cellReference = columnLetter + rowindex;
                    var cell = row.Elements<Cell>().Where(c => c.CellReference.Value == cellReference).FirstOrDefault();
                    if (cell == null)
                    {
                        cell = new Cell() { CellReference = cellReference };
                        row.Append(cell);
                    }
                }
            }
            foreach (Cell cell in row.Descendants<Cell>())
            {
                string columnName = OpenXMLExcelHelper.GetColumnName(cell.CellReference);

                var currentColumnIndex = OpenXMLExcelHelper.GetColumnIndex(columnName);

                for (; currentCount < currentColumnIndex; currentCount++)
                {
                    var emptycell = new Cell()
                    {
                        DataType = null,
                        CellValue = new CellValue(string.Empty)
                    };
                    yield return emptycell;
                }

                yield return cell;
                currentCount++;
            }
        }

        private static string ReadExcelCell(Cell cell, WorkbookPart workbookPart)
        {
            return GetCellValue(cell, workbookPart);
            //var cellValue = cell.CellValue;
            //var text = (cellValue == null) ? cell.InnerText : cellValue.Text;
            //if ((cell.DataType != null) && (cell.DataType == CellValues.SharedString))
            //{
            //    if (cell.DataType != null)
            //    {
            //        switch (cell.DataType.Value)
            //        {
            //            case CellValues.SharedString:
            //                text = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>()
            //                                    .ElementAt(Convert.ToInt32(cell.CellValue.Text)).InnerText;
            //                break;
            //            case CellValues.Boolean:
            //                switch (text)
            //                {
            //                    case "0":
            //                        text = "FALSE";
            //                        break;
            //                    default:
            //                        text = "TRUE";
            //                        break;
            //                }
            //                break;
            //        }
            //    }
            //}

            //return (text ?? string.Empty).Trim();
        }

        #endregion



        /// <summary>
        /// 根据Excel单元格和WorkbookPart对象获取单元格的值
        /// </summary>
        /// <param name="cell">Excel单元格对象</param>
        /// <param name="workBookPart">Excel WorkbookPart对象</param>
        /// <returns>单元格的值</returns>
        private static string GetCellValue(Cell cell, WorkbookPart workBookPart)
        {
            string cellValue = string.Empty;
            if (cell.ChildElements.Count == 0 || cell.CellReference == null)//Cell节点下没有子节点
            {
                return cellValue;
            }

            string cellRefId = cell.CellReference.InnerText;//获取引用相对位置
            string cellInnerText = cell.CellValue.InnerText;//获取Cell的InnerText
            cellValue = cellInnerText;//指定默认值(其实用来处理Excel中的数字)


            //获取WorkbookPart中共享String数据
            SharedStringTable sharedTable = workBookPart.SharedStringTablePart.SharedStringTable;

            try
            {
                EnumValue<CellValues> cellType = cell.DataType;//获取Cell数据类型
                if (cellType != null)//Excel对象数据
                {
                    switch (cellType.Value)
                    {
                        case CellValues.SharedString://字符串
                            //获取该Cell的所在的索引
                            int cellIndex = int.Parse(cellInnerText);
                            cellValue = sharedTable.ChildElements[cellIndex].InnerText;
                            break;
                        case CellValues.Boolean://布尔
                            cellValue = (cellInnerText == "1") ? "TRUE" : "FALSE";
                            break;
                        case CellValues.Date://日期
                            cellValue = string.Format("{0:d}", Convert.ToDateTime(cellInnerText));
                            break;
                        case CellValues.Number://数字
                            cellValue = Convert.ToDecimal(cellInnerText).ToString();
                            break;
                        default: cellValue = cellInnerText; break;
                    }
                }
                else//格式化数据
                {
                    // If there is no data type, this must be a string that has been formatted as a number
                    CellFormat cf;
                    if (cell.StyleIndex == null)
                    {
                        cf = workBookPart.WorkbookStylesPart.Stylesheet.CellFormats.Descendants<CellFormat>().ElementAt<CellFormat>(0);
                    }
                    else
                    {
                        cf = workBookPart.WorkbookStylesPart.Stylesheet.CellFormats.Descendants<CellFormat>().ElementAt<CellFormat>(Convert.ToInt32(cell.StyleIndex.Value));
                    }


                    //获取WorkbookPart中NumberingFormats样式集合
                    List<string> dicStyles = GetNumberFormatsStyle(workBookPart);
                    if (dicStyles.Count > 0 && cell.StyleIndex != null)//对于数字,cell.StyleIndex==null
                    {
                        int styleIndex = Convert.ToInt32(cell.StyleIndex.Value);
                        string cellStyle = dicStyles.Count >= styleIndex ? dicStyles[styleIndex - 1] : null;//获取该索引的样式
                        if (cellStyle != null)
                        {
                            if (cellStyle.Contains("yyyy") || cellStyle.Contains("h")
                                || cellStyle.Contains("dd") || cellStyle.Contains("ss"))
                            {
                                //如果为日期或时间进行格式处理,去掉“;@”
                                cellStyle = cellStyle.Replace(";@", "");
                                while (cellStyle.Contains("[") && cellStyle.Contains("]"))
                                {
                                    int otherStart = cellStyle.IndexOf('[');
                                    int otherEnd = cellStyle.IndexOf("]");

                                    cellStyle = cellStyle.Remove(otherStart, otherEnd - otherStart + 1);
                                }
                                //double doubleDateTime = double.Parse(cellInnerText);

                                double tmpValue = 0;
                                if (double.TryParse(cellInnerText, out tmpValue))
                                {
                                    if (tmpValue > 59)
                                        tmpValue -= 1;
                                    var tmpDate = new DateTime(1899, 12, 31).AddDays(tmpValue);
                                    cellValue = tmpDate.ToString("yyyy-MM-dd HH:mm:ss");

                                }

                                if (cellStyle.Contains("m")) { cellStyle = cellStyle.Replace("m", "M"); }
                                if (cellStyle.Contains("AM/PM")) { cellStyle = cellStyle.Replace("AM/PM", ""); }

                            }
                            else//其他的货币、数值
                            {
                                cellValue = cellInnerText;
                                //                        cellStyle = cellStyle.Substring(cellStyle.LastIndexOf('.') - 1).Replace("\\", "");
                                //decimal decimalNum = decimal.Parse(cellInnerText);
                                //cellValue = decimal.Parse(decimalNum.ToString(cellStyle)).ToString();
                            }
                        }
                    }
                    else if (
                        (cf.NumberFormatId >= 14 && cf.NumberFormatId <= 22) ||
                        (cf.NumberFormatId >= 165 && cf.NumberFormatId <= 180) ||
                        cf.NumberFormatId == 278 || cf.NumberFormatId == 185 ||
                        cf.NumberFormatId == 196 || cf.NumberFormatId == 217 || cf.NumberFormatId == 326) // Dates
                    {
                        double tmpValue = 0;
                        if (double.TryParse(cellInnerText, out tmpValue))
                        {
                            if (tmpValue > 59)
                                tmpValue -= 1;
                            var tmpDate = new DateTime(1899, 12, 31).AddDays(tmpValue);
                            cellValue = tmpDate.ToString("yyyy-MM-dd HH:mm:ss");

                        }

                        //DateTime dt = new DateTime(1899, 12, 30).AddDays(double.Parse(cellInnerText));
                        //cellValue = string.Format("{0:d}", dt);
                    }
                }
            }
            catch (Exception exp)
            {
                //string expMessage = string.Format("Excel中{0}位置数据有误,请确认填写正确！", cellRefId);
                //throw new Exception(expMessage);
                cellValue = "N/A";
            }
            return cellValue;
        }

        /// <summary>
        /// 根据WorkbookPart获取NumberingFormats样式集合
        /// </summary>
        /// <param name="workBookPart">WorkbookPart对象</param>
        /// <returns>NumberingFormats样式集合</returns>
        private static List<string> GetNumberFormatsStyle(WorkbookPart workBookPart)
        {
            List<string> dicStyle = new List<string>();
            Stylesheet styleSheet = workBookPart.WorkbookStylesPart.Stylesheet;

            if (styleSheet.NumberingFormats == null)
                return dicStyle;
            OpenXmlElementList list = styleSheet.NumberingFormats.ChildElements;//获取NumberingFormats样式集合

            foreach (var element in list)//格式化节点
            {
                if (element.HasAttributes)
                {
                    using (OpenXmlReader reader = OpenXmlReader.Create(element))
                    {
                        if (reader.Read())
                        {
                            if (reader.Attributes.Count > 0)
                            {
                                string numFmtId = reader.Attributes[0].Value;//格式化ID
                                string formatCode = reader.Attributes[1].Value;//格式化Code
                                dicStyle.Add(formatCode);//将格式化Code写入List集合
                            }
                        }
                    }
                }
            }
            return dicStyle;
        }
    }

}

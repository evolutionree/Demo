using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Utility.OpenXMLUtility
{
    public class ExcelHelper
    {
        #region --读取Excel文件--
        public static ExcelInfo ReadExcelList(Stream file)
        {
            var excel = new ExcelInfo();
            var sheetList = new List<ExcelSheetInfo>();

            WorkbookPart workbookPart;

            try
            {
                //SpreadsheetDocument.creat(file, SpreadsheetDocumentType.)
                var document = SpreadsheetDocument.Open(file, false);
                workbookPart = document.WorkbookPart;
               
                var sheets = workbookPart.Workbook.Descendants<Sheet>();
                
                foreach (var sheet in sheets)
                {
                    //sheetList.Add(ReadSheet(workbookPart, sheet));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return excel;
        }
        #endregion

        #region --读取一个Sheet表--
        //private static ExcelSheetInfo ReadSheet(WorkbookPart workbookPart, Sheet sheet)
        //{
        //    var sheetInfo = new ExcelSheetInfo();
        //    sheetInfo.SheetName = sheet.Name;
        //    sheetInfo.SheetId = sheet.SheetId.Value;
        //    var workSheet = ((WorksheetPart)workbookPart.GetPartById(sheet.Id)).Worksheet;

        //    var sheetData = workSheet.Elements<SheetData>().First();
        //    //sheetInfo.Rows = sheetData.Elements<Row>().ToList();
        //    sheetInfo.Rows.FirstOrDefault().Row.OuterXml
        //    //int headerRowsCount = 1;


        //    //return sheetInfo;
        //}
        #endregion


        public static bool IsEmptyRow(Row row, WorkbookPart workbookPart)
        {
            var rowData = ReadRowData(row, workbookPart);
            return rowData.Count == 0 || rowData.All(m => m.Trim().Equals(string.Empty));
        }
        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="row"></param>
        /// <param name="workbookPart"></param>
        /// <returns></returns>
        public static List<string> ReadRowData(Row row, WorkbookPart workbookPart)
        {
            var dataRow = new List<string>();
            var cellEnumerator = GetExcelCellEnumerator(row);

            while (cellEnumerator.MoveNext())
            {
                var cell = cellEnumerator.Current;
                //var text = ReadExcelCell(cell, workbookPart);
                //dataRow.Add(text);
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
                    //var columnLetter = OpenXMLExcelHelper.GetColumnName(i);
                    //string cellReference = columnLetter + rowindex;
                    //var cell = row.Elements<Cell>().Where(c => c.CellReference.Value == cellReference).FirstOrDefault();
                    //if (cell == null)
                    //{
                    //    cell = new Cell() { CellReference = cellReference };
                    //    row.Append(cell);
                    //}
                }
            }
            foreach (Cell cell in row.Descendants<Cell>())
            {
                //string columnName = OpenXMLExcelHelper.GetColumnName(cell.CellReference);

                //var currentColumnIndex = OpenXMLExcelHelper.GetColumnIndex(columnName);

                //for (; currentCount < currentColumnIndex; currentCount++)
                //{
                //    var emptycell = new Cell()
                //    {
                //        DataType = null,
                //        CellValue = new CellValue(string.Empty)
                //    };
                //    yield return emptycell;
                //}

                //yield return cell;
                //currentCount++;
            }
            return null;
        }
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
                                cellStyle = cellStyle.Substring(cellStyle.LastIndexOf('.') - 1).Replace("\\", "");
                                decimal decimalNum = decimal.Parse(cellInnerText);
                                cellValue = decimal.Parse(decimalNum.ToString(cellStyle)).ToString();
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

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UBeat.Crm.CoreApi.Services.Models.Excels;
using UBeat.Crm.CoreApi.Services.Utility.ExcelUtility;

namespace UBeat.Crm.CoreApi.Services.Utility.OpenXMLUtility
{
    public class ExcelHelper
    {
        #region --读取Excel文件--
        public static ExcelInfo ReadExcel(Stream file)
        {
            var excel = new ExcelInfo();

            try
            {
                excel.ExcelFileBytes = StreamHelper.StreamToBytes(file);
                excel.Sheets = new List<ExcelSheetInfo>();
                var document = SpreadsheetDocument.Open(file, false);
                var workbookPart = document.WorkbookPart;

                var sheets = workbookPart.Workbook.Descendants<Sheet>();

                foreach (var sheet in sheets)
                {
                    excel.Sheets.Add(ReadSheet(workbookPart, sheet));
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return excel;
        }
        #endregion

        #region --写入Excel文件--
        public static byte[] WrightExcel(ExcelInfo excel)
        {
            try
            {
                var stream = new MemoryStream();
                stream.Write(excel.ExcelFileBytes, 0, excel.ExcelFileBytes.Length);
                //创建文档对象
                //var document = SpreadsheetDocument.Open(file, true);
                // 设置当前流的位置为流的开始
                //stream.Seek(0, SeekOrigin.Begin);

                var document = SpreadsheetDocument.Open(stream, true);

                //创建Workbook（工作簿）

                var rootbookpart = document.WorkbookPart;
                //Add Sheets to the Workbook.

                var sheets = rootbookpart.Workbook.Descendants<Sheet>();

                foreach (var sheetData in excel.Sheets)
                {
                    var sheet = sheets.Where(m => m.Name.Value.ToLower().Equals(sheetData.SheetName.ToLower())).FirstOrDefault();
                    //如果找到匹配的sheet，则修改,反之，新增sheet
                    if (sheet != null)
                    {
                        var sheetbookpart = (WorksheetPart)rootbookpart.GetPartById(sheet.Id);

                        var workbookStylesPart = rootbookpart.GetPartsOfType<WorkbookStylesPart>();

                        UpdateSheet(sheetData, sheetbookpart, workbookStylesPart.FirstOrDefault());
                    }
                    else
                    {
                        sheet = InsertSheet(sheetData, rootbookpart);
                        rootbookpart.Workbook.Sheets.AppendChild(sheet);
                    }
                }


                rootbookpart.Workbook.Save();

                document.Close();
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private static Sheet InsertSheet(ExcelSheetInfo sheetData, WorkbookPart workbookPart)
        {
            return new Sheet();
        }
        private static void UpdateSheet(ExcelSheetInfo data, WorksheetPart sheetbookpart, WorkbookStylesPart workbookStylesPart)
        {
            var worksheet = sheetbookpart.Worksheet;
            var sheetData = worksheet.GetFirstChild<SheetData>();
            var rows = sheetData.Elements<Row>();
            List<Row> tempRows = new List<Row>();

            var mergeCells = worksheet.Elements<MergeCells>().FirstOrDefault();


            MergeCells newMergeCells = null;
            var firstRowdata = data.Rows.FirstOrDefault();
            foreach (var rowdata in data.Rows)
            {
                Row temRow = new Row(rowdata.OuterXml);
                if (rowdata.RowStatus == RowStatus.Deleted)//删除行
                {
                    continue;
                }
                uint rowIndex = firstRowdata.RowIndex + (uint)tempRows.Count;
                tempRows.Add(temRow);

                if (mergeCells != null)
                {
                    var mergeCellList = mergeCells.Elements<MergeCell>();
                    var tempMergeCells = GetMergeCells(temRow, mergeCellList);
                    if (tempMergeCells != null && tempMergeCells.Count > 0)
                    {
                        foreach (var mergeCell in tempMergeCells)
                        {
                            var cellNames = mergeCell.Reference.Value.Split(':');
                            var rowindex1 = OpenXMLExcelHelper.GetRowIndex(cellNames[0]);
                            var colindex1 = OpenXMLExcelHelper.GetColumnName(cellNames[0]);
                            var rowindex2 = OpenXMLExcelHelper.GetRowIndex(cellNames[1]);
                            var colindex2 = OpenXMLExcelHelper.GetColumnName(cellNames[1]);
                            var rowindex1_new = rowIndex;
                            var rowindex2_new = rowIndex + (rowindex2 - rowindex1);
                            if (newMergeCells == null)
                                newMergeCells = new MergeCells();
                            newMergeCells.Append(new MergeCell() { Reference = string.Format("{0}{1}:{2}{3}", colindex1, rowindex1_new, colindex2, rowindex2_new) });
                        }
                    }
                }

                temRow.RowIndex = rowIndex;
                RefreshRow(temRow, rowdata.Cells, workbookStylesPart, sheetbookpart);
            }
            sheetData.RemoveAllChildren<Row>();
            sheetData.Append(tempRows);
            if (mergeCells != null)
            {
                worksheet.RemoveAllChildren<MergeCells>();
                OpenXMLExcelHelper.InsertMergeCells(worksheet, newMergeCells);
            }

        }

        public static void RefreshRow(Row row, List<ExcelCellInfo> celldatas, WorkbookStylesPart workbookStylesPart, WorksheetPart sheetbookpart)
        {
            var worksheet = sheetbookpart.Worksheet;
            var stylesheet = workbookStylesPart.Stylesheet;
            var styleNumberingFormats = stylesheet.NumberingFormats;

           
            var cells = row.Descendants<Cell>();
            foreach (var cell in cells)
            {
                if (cell.CellReference.HasValue)
                {
                    var columnName = OpenXMLExcelHelper.GetColumnName(cell.CellReference);
                    cell.CellReference = string.Format("{0}{1}", columnName, row.RowIndex);
                    var celldata = celldatas.Find(m => m.ColumnName == columnName);
                    if (celldata != null && celldata.IsUpdated)
                    {
                        if (!celldata.IsImageCell)
                        {

                            cell.CellValue = new CellValue(celldata.CellValue.ToString());
                            var styleIndex = (int)cell.StyleIndex.Value;
                            var cellFormat = stylesheet.CellFormats.ChildElements[styleIndex] as CellFormat;
                            
                            CellValues cellValues = CellValues.String;
                            if (styleNumberingFormats != null && styleNumberingFormats.ChildElements != null)
                            {
                                var numberingFormatList = styleNumberingFormats.ChildElements.Cast<NumberingFormat>();
                                if (numberingFormatList != null && numberingFormatList.Count() > 0
                                    && numberingFormatList.Where(m => m.NumberFormatId.HasValue && cellFormat.NumberFormatId.HasValue && m.NumberFormatId.Value == cellFormat.NumberFormatId.Value).Count() > 0)
                                {
                                    double num = 0;
                                    if (double.TryParse(celldata.CellValue.ToString(), out num))
                                    {
                                        cellValues = CellValues.Number;
                                    }
                                }
                            }

                            cell.DataType = new EnumValue<CellValues>(cellValues);
                        }
                        else //图片资源
                        {
                            uint rowindex1 = row.RowIndex;
                            uint colindex1 = OpenXMLExcelHelper.GetColumnIndex(columnName);
                            uint rowindex2 = rowindex1;
                            uint colindex2 = colindex1;

                            var mergeCells = worksheet.Elements<MergeCells>().FirstOrDefault();
                            if (mergeCells != null)
                            {
                                var mergeCellList = mergeCells.Elements<MergeCell>();
                                foreach (var merge in mergeCellList)
                                {
                                    if (merge.Reference.Value.Contains(string.Format("{0}:", cell.CellReference.Value)))
                                    {
                                        var cellNames = merge.Reference.Value.Split(':');
                                         rowindex1 = OpenXMLExcelHelper.GetRowIndex(cellNames[0]);
                                         colindex1 = OpenXMLExcelHelper.GetColumnIndex(OpenXMLExcelHelper.GetColumnName(cellNames[0]));
                                         rowindex2 = OpenXMLExcelHelper.GetRowIndex(cellNames[1]);
                                         colindex2 = OpenXMLExcelHelper.GetColumnIndex(OpenXMLExcelHelper.GetColumnName(cellNames[1]));
                                        break;
                                    }
                                }
                            }
                          


                            ImagePartType dataType = ImagePartType.Jpeg;
                            if (celldata.ImageInfo == null || celldata.ImageInfo.Images == null)
                                return;
                            cell.CellValue = new CellValue("");

                            int offsetx = 1;
                            int offsety = 0;
                            int offsetavg = 0;
                            int? imagewidth = celldata.ImageInfo.Width <= 0 ? null : (int?)celldata.ImageInfo.Width;
                            int? imageheight = celldata.ImageInfo.Height <= 0 ? null : (int?)celldata.ImageInfo.Height;
                            imagewidth = 100;
                            imageheight = 100;
                            //offsetavg = (celldata.ImageInfo.Width - 2 * (celldata.ImageInfo.Images.Count + 1)) / celldata.ImageInfo.Images.Count;
                            offsetavg = imagewidth.Value;

                            var colums = worksheet.GetFirstChild<Columns>();
                            var sheetData = worksheet.GetFirstChild<SheetData>();
                            var rows = sheetData.Elements<Row>();
                            OffsetXY offsetXY=new OffsetXY();
                            //offsetXY.OffsetType = OffsetType.XY;
                            //offsetXY.XOffset = (long)Math.Ceiling( colums.Take((int)colindex1).Sum(m => ((Column)m).Width));
                            //var ss = rows.Take((int)rowindex1);
                            //var sss = ss.Sum(m => m.Height); 
                            //offsetXY.YOffset = (long)Math.Ceiling(rows.Take((int)rowindex1).Sum(m => m.Height));
                            uint colIndex = colindex1;
                            foreach (var img in celldata.ImageInfo.Images)
                            {
                                offsetXY = OpenXMLExcelHelper.InsertImage(sheetbookpart,  rowindex1 - 1, colIndex, rowindex2 - 1, colIndex, offsetx, offsety, imagewidth.Value, imageheight,  img, dataType, offsetXY);
                                //offsetXY.OffsetType = OffsetType.X;
                                offsetXY = null;
                                colIndex++;
                                if (colIndex > colindex2)
                                    colIndex = colindex2;
                            }
                           
                        }



                    }

                }
            }
        }
        private static List<MergeCell> GetMergeCells(Row row, IEnumerable<MergeCell> mergeCells)
        {
            List<MergeCell> mergeCellList = new List<MergeCell>();
            var cells = row.Descendants<Cell>();
            foreach (var cell in cells)
            {
                foreach (var merge in mergeCells)
                {
                    if (merge.Reference.Value.Contains(string.Format("{0}:", cell.CellReference.Value)))
                    {
                        mergeCellList.Add(merge);
                        break;
                    }
                }
            }
            return mergeCellList;
        }


        #endregion


        #region --插入一行
        //public static void MergeTwoCells(Worksheet worksheet, string cell1Name, string cell2Name)
        //{
        //    OpenXMLExcelHelper.MergeTwoCells(worksheet, cell1Name, cell2Name);
        //}
        #endregion
        #region --检查是否是合并单元格--
        public static bool IsMergeCell(string cellName, MergeCells mergeCells, out string mergeCellReference)
        {
            mergeCellReference = null;
            var mergeCellList = mergeCells.Descendants<MergeCell>();
            foreach (var merge in mergeCellList)
            {
                if (merge.Reference.Value.Contains(string.Format("{0}:", cellName)))
                {
                    mergeCellReference = merge.Reference;
                    return true;
                }
            }
            return false;
        }
        #endregion
        #region --合并单元格--
        public static void MergeTwoCells(MergeCells mergeCells, string cell1Name, string cell2Name)
        {
            // Create the merged cell and append it to the MergeCells collection. 
            MergeCell mergeCell = new MergeCell() { Reference = new StringValue(cell1Name + ":" + cell2Name) };
            mergeCells.Append(mergeCell);
        }

        #endregion


        #region --读取一个Sheet表--
        private static ExcelSheetInfo ReadSheet(WorkbookPart workbookPart, Sheet sheet)
        {
            var sheetInfo = new ExcelSheetInfo();
            sheetInfo.SheetName = sheet.Name;
            sheetInfo.Rows = new List<ExcelRowInfo>();
            var workSheet = ((WorksheetPart)workbookPart.GetPartById(sheet.Id)).Worksheet;

            var sheetData = workSheet.Elements<SheetData>().FirstOrDefault();
            var rows = sheetData.Elements<Row>();

            foreach (var row in rows)
            {
                var rowData = ReadRowData(row, workbookPart);
                rowData.OuterXml = row.OuterXml;
                sheetInfo.Rows.Add(rowData);
            }

            return sheetInfo;
        }
        #endregion

        #region --读取行数据--
        /// <summary>
        /// 读取行数据
        /// </summary>
        /// <param name="row"></param>
        /// <param name="workbookPart"></param>
        /// <returns></returns>
        private static ExcelRowInfo ReadRowData(Row row, WorkbookPart workbookPart)
        {
            var dataRow = new ExcelRowInfo();
            dataRow.RowIndex = row.RowIndex;
            dataRow.RowStatus = RowStatus.Normal;
            dataRow.Cells = new List<ExcelCellInfo>();
            var cells = row.Descendants<Cell>();
            foreach (var cell in cells)
            {
                var celldata = new ExcelCellInfo();
                celldata.IsUpdated = false;
                celldata.ColumnName = OpenXMLExcelHelper.GetColumnName(cell.CellReference);
                celldata.CellValue = GetCellValue(cell, workbookPart);
                dataRow.Cells.Add(celldata);
            }
            return dataRow;
        }
        #endregion

        #region --读取单元格数据--
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
                    }
                }
            }
            catch (Exception exp)
            {
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
        #endregion
    }
}

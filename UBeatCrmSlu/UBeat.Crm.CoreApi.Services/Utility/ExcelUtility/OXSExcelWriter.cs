using System;
using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Drawing;
using System.Linq;
using UBeat.Crm.CoreApi.Services.Models.Excels;
using Newtonsoft.Json;

namespace UBeat.Crm.CoreApi.Services.Utility.ExcelUtility
{
	public class OXSExcelWriter
	{
		public static byte[] GenerateExcelTemplate(ExcelTemplate templates)
		{
			if (templates == null || templates.DataSheets == null || templates.DataSheets.Count <= 0)
				throw new Exception("模板对象解析错误");
			var stream = new MemoryStream();
			//创建文档对象
			var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook);

			//创建Workbook（工作簿）
			var rootbookpart = document.AddWorkbookPart();
			rootbookpart.Workbook = new Workbook();
			//Add Sheets to the Workbook.
			var sheets = rootbookpart.Workbook.AppendChild(new Sheets());
			var stylesPart = rootbookpart.AddNewPart<WorkbookStylesPart>();
			stylesPart.Stylesheet = new Stylesheet();
			stylesPart.Stylesheet.Save();
			//判断是否加载模板中的样式设计
			var sheetTemplate = templates.DataSheets.FirstOrDefault();
			if (sheetTemplate != null && sheetTemplate.StylesheetXml != null)
			{
				stylesPart.Stylesheet = new Stylesheet(sheetTemplate.StylesheetXml);
				stylesPart.Stylesheet.Save();
			}
			//如果有共享数据，则写入共享数据字段字符串表
			if (!string.IsNullOrEmpty(templates.SharedStringTableOuterXml))
			{
				SharedStringTablePart shareStringPart = rootbookpart.AddNewPart<SharedStringTablePart>();
				shareStringPart.SharedStringTable = new SharedStringTable(templates.SharedStringTableOuterXml);
			}

			//添加具体数据的模板页
			foreach (var m in templates.DataSheets)
			{
				var datarows = new List<Dictionary<string, object>>();

				var fieldRrow = new Dictionary<string, object>();
				var isNotEmptyRrow = new Dictionary<string, object>();
				var fieldTypeRrow = new Dictionary<string, object>();
				foreach (var mapitem in m.ColumnMap)
				{
					fieldRrow.Add(mapitem.FieldName, mapitem.FieldName);
					isNotEmptyRrow.Add(mapitem.FieldName, mapitem.IsNotEmpty ? 1 : 0);
					fieldTypeRrow.Add(mapitem.FieldName, (int)mapitem.FieldType);
				}
				datarows.Add(fieldRrow);
				datarows.Add(isNotEmptyRrow);
				datarows.Add(fieldTypeRrow);
				var sheetData = new ExportSheetData()
				{
					SheetDefines = m,
					DataRows = datarows
				};
				CreateWorksheetPart(rootbookpart, sheetData);
			}
			//添加注意事项页签
			var worksheetPart1 = rootbookpart.AddNewPart<WorksheetPart>();
			worksheetPart1.Worksheet = new Worksheet(templates.AttentionOuterXml);//一张工作表的根元素
			sheets.AppendChild(new Sheet()
			{
				Id = rootbookpart.GetIdOfPart(worksheetPart1),
				SheetId = new UInt32Value((uint)sheets.Count() + 1),
				Name = "注意事项"
			});
			//添加sqldefine页签
			var worksheetPart2 = rootbookpart.AddNewPart<WorksheetPart>();
			worksheetPart2.Worksheet = new Worksheet(templates.SQLDefineOuterXml);//一张工作表的根元素 
			sheets.AppendChild(new Sheet()
			{
				Id = rootbookpart.GetIdOfPart(worksheetPart2),
				SheetId = new UInt32Value((uint)sheets.Count() + 1),
				Name = "SQLDefine"
			});
			//添加模板定义说明页签
			var worksheetPart3 = rootbookpart.AddNewPart<WorksheetPart>();
			worksheetPart3.Worksheet = new Worksheet(templates.TemplateDefineOuterXml);//一张工作表的根元素 
			sheets.AppendChild(new Sheet()
			{
				Id = rootbookpart.GetIdOfPart(worksheetPart3),
				SheetId = new UInt32Value((uint)sheets.Count() + 1),
				Name = "模板定义说明"
			});
			rootbookpart.Workbook.Save();
			document.Close();
			return stream.ToArray();
		}
		public static byte[] GenerateImportTemplate(ExcelTemplate templates, Dictionary<string, List<Dictionary<string, object>>> sheetRows)
		{
			if (templates == null || templates.DataSheets == null || templates.DataSheets.Count <= 0)
				throw new Exception("模板对象解析错误");
			var stream = new MemoryStream();
			//创建文档对象
			var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook);

			//创建Workbook（工作簿）
			var rootbookpart = document.AddWorkbookPart();
			rootbookpart.Workbook = new Workbook();
			//Add Sheets to the Workbook.
			var sheets = rootbookpart.Workbook.AppendChild(new Sheets());
			var stylesPart = rootbookpart.AddNewPart<WorkbookStylesPart>();
			stylesPart.Stylesheet = new Stylesheet();
			stylesPart.Stylesheet.Save();
			//判断是否加载模板中的样式设计
			var sheetTemplate = templates.DataSheets.FirstOrDefault();
			if (sheetTemplate != null && sheetTemplate.StylesheetXml != null)
			{
				stylesPart.Stylesheet = new Stylesheet(sheetTemplate.StylesheetXml);
				stylesPart.Stylesheet.Save();
			}
			//如果有共享数据，则写入共享数据字段字符串表
			if (!string.IsNullOrEmpty(templates.SharedStringTableOuterXml))
			{
				SharedStringTablePart shareStringPart = rootbookpart.AddNewPart<SharedStringTablePart>();
				shareStringPart.SharedStringTable = new SharedStringTable(templates.SharedStringTableOuterXml);
			}

			//添加具体数据的模板页
			foreach (var m in templates.DataSheets)
			{
				var sheetData = new ExportSheetData()
				{
					SheetDefines = m,
					DataRows = sheetRows.ContainsKey(m.SheetName) ? sheetRows[m.SheetName] : new List<Dictionary<string, object>>()
				};
				CreateWorksheetPart(rootbookpart, sheetData);
			}
			//添加注意事项页签
			var worksheetPart1 = rootbookpart.AddNewPart<WorksheetPart>();
			worksheetPart1.Worksheet = new Worksheet(templates.AttentionOuterXml);//一张工作表的根元素
			sheets.AppendChild(new Sheet()
			{
				Id = rootbookpart.GetIdOfPart(worksheetPart1),
				SheetId = new UInt32Value((uint)sheets.Count() + 1),
				Name = "注意事项"
			});
			rootbookpart.Workbook.Save();
			document.Close();
			return stream.ToArray();
		}

		//public static byte[] GenerateExcelTemplate(List<SheetDefine> sheetTemplates)
		//{
		//    var sqltemp = new SimpleSheetTemplate();
		//    sqltemp.SheetName = "SQLDefine";
		//    string firstHeader = "sheetname";
		//    string secondHeader = "是否游标返回数据（是=1，否=0）";
		//    string thirdHeader = "sqltemplate(sql查询语句，必须使用系统规范的标准返回值定义执行函数，其中参数名必须和Excel字段映射表对应，而且不能包含系统默认的参数名：@userno";
		//    sqltemp.Headers = new List<SimpleHeader>()
		//        {
		//            new SimpleHeader(){ HeaderText=firstHeader, Width=100 , FieldName=firstHeader, IsNotEmpty=false},
		//            new SimpleHeader(){ HeaderText=secondHeader, Width=230, FieldName=secondHeader, IsNotEmpty=false},
		//            new SimpleHeader(){ HeaderText=thirdHeader, Width=1080, FieldName=thirdHeader, IsNotEmpty=false},
		//        };


		//    var sqldatarows = new List<Dictionary<string, object>>();
		//    var sheetsData = new List<ExportSheetData>();

		//    foreach (var m in sheetTemplates)
		//    {
		//        var datarows = new List<Dictionary<string, object>>();

		//        var fieldRrow = new Dictionary<string, object>();
		//        var isNotEmptyRrow = new Dictionary<string, object>();
		//        var columnMap = new List<ColumnMapModel>();
		//        if (m is SheetTemplate)
		//        {
		//            columnMap = (m as SheetTemplate).ColumnMap;
		//        }
		//        else
		//        {
		//            var template = m as SimpleSheetTemplate;
		//            int indextemp = 0;
		//            foreach (var item in template.Headers)
		//            {
		//                columnMap.Add(new ColumnMapModel() { Index = indextemp++, FieldName = item.FieldName, IsNotEmpty = item.IsNotEmpty });
		//            }
		//        }
		//        foreach (var mapitem in columnMap)
		//        {
		//            fieldRrow.Add(mapitem.FieldName, mapitem.FieldName);
		//            isNotEmptyRrow.Add(mapitem.FieldName, mapitem.IsNotEmpty ? 1 : 0);
		//        }
		//        datarows.Add(fieldRrow);
		//        datarows.Add(isNotEmptyRrow);
		//        sheetsData.Add(new ExportSheetData()
		//        {
		//            SheetDefines = m,
		//            DataRows = datarows
		//        });
		//        //生成sql sheet表的行数据
		//        var sqlrowDic = new Dictionary<string, object>();
		//        sqlrowDic.Add(firstHeader, m.SheetName);
		//        sqlrowDic.Add(thirdHeader, m.IsStoredProcCursor);
		//        sqlrowDic.Add(secondHeader, m.ExecuteSQL);
		//        sqldatarows.Add(sqlrowDic);
		//    }
		//    sheetsData.Add(new ExportSheetData()
		//    {
		//        SheetDefines = sqltemp,
		//        DataRows = sqldatarows
		//    });
		//    return GenerateExcel(sheetsData);
		//}


		public static byte[] GenerateExcel(ExportSheetData sheetData)
		{
			var list = new List<ExportSheetData>();
			list.Add(sheetData);
			return GenerateExcel(list);
		}

		public static byte[] GenerateExcel(List<ExportSheetData> sheetsData)
		{
			if (sheetsData == null || sheetsData.Count <= 0)
				throw new Exception("SheetsData 不可为空");
			var stream = new MemoryStream();
			//创建文档对象
			var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook);

			//创建Workbook（工作簿）
			var rootbookpart = document.AddWorkbookPart();
			rootbookpart.Workbook = new Workbook();
			//Add Sheets to the Workbook.
			var sheets = rootbookpart.Workbook.AppendChild(new Sheets());
			var stylesPart = rootbookpart.AddNewPart<WorkbookStylesPart>();
			stylesPart.Stylesheet = new Stylesheet();
			stylesPart.Stylesheet.Save();

			//如果是从模板导出，则判断是否加载模板中的样式设计
			if (sheetsData.FirstOrDefault().SheetDefines is SheetTemplate)
			{
				var sheetTemplate = sheetsData.FirstOrDefault().SheetDefines as SheetTemplate;
				if (sheetTemplate != null && sheetTemplate.StylesheetXml != null)
				{
					stylesPart.Stylesheet = new Stylesheet(sheetTemplate.StylesheetXml);

					stylesPart.Stylesheet.Save();
				}
				else
				{
					stylesPart.Stylesheet = OpenXMLExcelHelper.GenerateStyleSheet();
					stylesPart.Stylesheet.Save();
				}
			}
			else
			{
				stylesPart.Stylesheet = OpenXMLExcelHelper.GenerateStyleSheet();
				stylesPart.Stylesheet.Save();
			}

			foreach (var data in sheetsData)
			{
				CreateWorksheetPart(rootbookpart, data);
			}
			rootbookpart.Workbook.Save();
			document.Close();
			return stream.ToArray();
		}

		private static uint CreateErrorTipStyleIndex(WorkbookPart rootbookpart)
		{
			var stylesPart = rootbookpart.GetPartsOfType<WorkbookStylesPart>().FirstOrDefault();
			if (stylesPart == null)
				return 1;

			var redfont = stylesPart.Stylesheet.Fonts.AppendChild(new DocumentFormat.OpenXml.Spreadsheet.Font());
			redfont.FontSize = new FontSize() { Val = 10 };
			redfont.Color = new DocumentFormat.OpenXml.Spreadsheet.Color() { Rgb = new HexBinaryValue() { Value = "FF3030" } };
			redfont.FontName = new FontName() { Val = "Calibri" };
			var redFontId = stylesPart.Stylesheet.Fonts.Count() - 1;
			CellFormat cf = stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat());
			cf.Alignment = new Alignment()
			{
				Horizontal = HorizontalAlignmentValues.Left,
				Vertical = VerticalAlignmentValues.Center
			};
			cf.ApplyAlignment = true;
			cf.NumberFormatId = 0;
			cf.FontId = (uint)redFontId;
			cf.BorderId = 0;
			cf.FillId = 0;

			return (uint)stylesPart.Stylesheet.CellFormats.Count() - 1;
		}

		private static void CreateWorksheetPart(WorkbookPart rootbookpart, ExportSheetData data)
		{

			var sheets = rootbookpart.Workbook.Sheets;
			if (sheets == null)
				sheets = rootbookpart.Workbook.AppendChild(new Sheets());

			//创建Worksheet（工作表）到Workbook，表示包含文本、数字、日期或公式的单元格网格的工作表类型
			var worksheetPart = rootbookpart.AddNewPart<WorksheetPart>();

			//创建SheetData（单元格表）到Worksheet
			var sheetData = new SheetData();//表数据对象
			worksheetPart.Worksheet = new Worksheet();//一张工作表的根元素

			//worksheetPart.Worksheet.AppendChild(AutoFitColumns(data));
			worksheetPart.Worksheet.AppendChild(sheetData);

			var sheet = new Sheet()
			{
				Id = rootbookpart.GetIdOfPart(worksheetPart),
				SheetId = new UInt32Value((uint)sheets.Count() + 1),
				Name = data.SheetDefines.SheetName ?? string.Format("Sheet {0}", sheets.Count() + 1)
			};
			sheets.AppendChild(sheet);
			//列映射关系
			var columnMap = new List<ColumnMapModel>();
			// Add header
			if (data.SheetDefines is SimpleSheetTemplate)
			{
				var sheetTemplate = data.SheetDefines as SimpleSheetTemplate;
				if (sheetTemplate.Headers != null && sheetTemplate.Headers.Count > 0)
				{
					columnMap = CreateHeaders(rootbookpart, worksheetPart, sheetTemplate.Headers);
					worksheetPart.Worksheet.InsertAfter(GetColumns(sheetTemplate.Headers), worksheetPart.Worksheet.SheetFormatProperties);
				}
			}
			else
			{
				var sheetTemplate = data.SheetDefines as SheetTemplate;
				columnMap = new List<ColumnMapModel>(sheetTemplate.ColumnMap);
				if (sheetTemplate.HeadersTemplate != null)
				{
					CreateHeaders(rootbookpart, worksheetPart, sheetTemplate.HeadersTemplate);
				}
				// Add the column configuration if available
				if (sheetTemplate.ColumnsOuterXml != null)
				{
					var columns = new Columns(sheetTemplate.ColumnsOuterXml);
					worksheetPart.Worksheet.InsertAfter(columns, worksheetPart.Worksheet.SheetFormatProperties);
				}
			}

			//如果有错误提示内容，则在最后创建一列错误提示列
			var errorColumnName = Guid.NewGuid().ToString();
			if (data.RowErrors != null && data.RowErrors.Count > 0)
			{
				uint errorTipStyleIndex = CreateErrorTipStyleIndex(rootbookpart);
				columnMap.Add(new ColumnMapModel()
				{
					FieldName = errorColumnName,
					Index = columnMap.Select(m => m.Index).Max() + 1,
					IsNotEmpty = false,
					StyleIndex = errorTipStyleIndex
				});
			}
			// Add sheet data
			if (columnMap != null && columnMap.Count > 0)
				InsertRowsData(rootbookpart, worksheetPart, columnMap, data.DataRows, errorColumnName, data.RowErrors);

			rootbookpart.Workbook.Save();
		}

		#region --创建表格头部--
		private static List<ColumnMapModel> CreateHeaders(WorkbookPart workbookPart, WorksheetPart worksheetPart, List<SimpleHeader> headers)
		{
			try
			{
				List<ColumnMapModel> columnMap = new List<ColumnMapModel>();
				Worksheet worksheet = worksheetPart.Worksheet;
				if (headers == null || headers.Count <= 0)
					throw new Exception("Excel 必须包含Header数据");
				uint columnIndex = 0;
				foreach (var m in headers)
				{

					var cell = OpenXMLExcelHelper.InsertText(workbookPart, worksheetPart, columnIndex, 1, m.HeaderText ?? "");
					if (m.IsNotEmpty)
					{
						cell.StyleIndex = 5;
					}
					else cell.StyleIndex = 4;

					columnMap.Add(new ColumnMapModel() { Index = (int)columnIndex, FieldName = m.FieldName, IsNotEmpty = m.IsNotEmpty });
					columnIndex++;
				}
				return columnMap;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		private static void CreateHeaders(WorkbookPart workbookPart, WorksheetPart worksheetPart, HeadersTemplate template)
		{
			Worksheet worksheet = worksheetPart.Worksheet;
			if (template == null || template.RowCellsTemplate == null)
				throw new Exception("Excel 必须包含Header数据");

			foreach (var m in template.RowCellsTemplate)
			{
				var rowIndex = OpenXMLExcelHelper.GetRowIndex(m.CellReference);
				string columnName = OpenXMLExcelHelper.GetColumnName(m.CellReference);
				var columnIndex = OpenXMLExcelHelper.GetColumnIndex(columnName);
				var cell = OpenXMLExcelHelper.InsertText(workbookPart, worksheetPart, columnIndex, rowIndex, m.CellValue.ToString());
				if (m.CellFormula != null)
					cell.CellFormula = new CellFormula(m.CellFormula);
				if (m.StyleIndex.HasValue)
					cell.StyleIndex = m.StyleIndex.Value;
				if (m.CellMetaIndex.HasValue)
					cell.CellMetaIndex = m.CellMetaIndex;
				if (m.ValueMetaIndex.HasValue)
					cell.ValueMetaIndex = m.ValueMetaIndex;
				if (m.InlineString != null)
					cell.InlineString = new InlineString(m.InlineString);
				if (m.ExtensionList != null)
					cell.ExtensionList = new DocumentFormat.OpenXml.Spreadsheet.ExtensionList(m.ExtensionList);
			}
			if (template.MergeCellsOuterXmls != null)
			{
				foreach (var m in template.MergeCellsOuterXmls)
				{
					OpenXMLExcelHelper.InsertMergeCells(worksheet, new MergeCells(m));
				}
			}
		}
		#endregion

		#region --插入每行数据--
		private static void InsertRowsData(WorkbookPart workbookPart, WorksheetPart worksheetPart, List<ColumnMapModel> columnMap, List<Dictionary<string, object>> rowsdata, string errorColumnName = null, List<string> rowErrors = null)
		{
			if (columnMap == null || columnMap.Count <= 0)
				throw new Exception("Excel 必须包含columnMap数据");

			Worksheet worksheet = worksheetPart.Worksheet;
			SheetData sheetData = worksheet.GetFirstChild<SheetData>();
			var columnMapOrderBy = columnMap.OrderBy(m => m.Index);
			uint rowIdex = (UInt32)sheetData.Descendants<Row>().Count();
			uint cellIdex = 0;

			if (rowsdata == null)
				return;
			for (int i = 0; i < rowsdata.Count; i++)
			{
				var row = rowsdata[i];
				cellIdex = 0;
				var newrow = new Row { RowIndex = ++rowIdex };
				List<OXSExcelDataCell> cellList = new List<OXSExcelDataCell>();
				foreach (var column in columnMapOrderBy)
				{
					object cellValue = null;
					uint styleIndex = 0;
					//判断是否是错误提示列
					if (column.FieldName.Equals(errorColumnName) && rowErrors != null)
					{
						cellValue = rowErrors.ElementAtOrDefault(i) ?? "";
						styleIndex = column.StyleIndex;
					}
					else cellValue = row.ContainsKey(column.FieldName) ? row[column.FieldName] : "";
					OXSExcelDataCell celldata = new OXSExcelDataCell("");
					if (cellValue is OXSExcelDataCell)
					{
						celldata = cellValue as OXSExcelDataCell;
					}
					else
					{
                        
                        if (cellValue is decimal)
                            cellValue = Decimal.ToDouble((decimal)cellValue);
                        celldata = new OXSExcelDataCell(cellValue == null ? "" : cellValue.ToString());
					}
					cellList.Add(celldata);
					InsertCellData(celldata, workbookPart, worksheetPart, cellIdex++, rowIdex, newrow, styleIndex);
				}

				var maxrowheight = GetMaxRowHeight(cellList);
				if (maxrowheight > 0)
				{
					newrow.CustomHeight = true;
					newrow.Height = maxrowheight;
				}
				sheetData.AppendChild(newrow);
			}
		}
		#endregion

		#region --插入单元格数据--
		private static void InsertCellData(OXSExcelDataCell callData, WorkbookPart workbookPart, WorksheetPart worksheetPart, uint columnIdex, uint rowIndex, Row row = null, uint styleIndex = 0)
		{
			if (callData.DataType == OXSDataItemType.String)
			{
				var newcell = OpenXMLExcelHelper.InsertText(workbookPart, worksheetPart, columnIdex, rowIndex, callData.Data == null ? string.Empty : callData.Data.ToString(), row);
				newcell.StyleIndex = styleIndex;
			}
			else if (callData.DataType == OXSDataItemType.Hyperlink)
			{
				HyperlinkData hyperlinkData = callData.Data as HyperlinkData;
				if (hyperlinkData == null || string.IsNullOrEmpty(hyperlinkData.Text))
				{
					return;
				}
				var cell = OpenXMLExcelHelper.CreateHyperlinkCell(columnIdex, rowIndex, hyperlinkData.Text, hyperlinkData.Hyperlink);

				row.AppendChild(cell);
			}
			else//其他的类型均为图片
			{
				ImagePartType dataType = (ImagePartType)callData.DataType;

				int offsetx = 1;
				int offsety = 0;
				int offsetavg = 0;
				int? imagewidth = callData.CellWidth <= 0 ? null : (int?)callData.CellWidth;
				int? imageheight = callData.CellHeight <= 0 ? null : (int?)callData.CellHeight;
				if (callData.Data is List<byte[]>)
				{
					List<byte[]> datas = callData.Data as List<byte[]>;
					offsetavg = (callData.CellWidth - 2 * (datas.Count + 1)) / datas.Count;

					foreach (var m in datas)
					{
						OpenXMLExcelHelper.InsertImage(worksheetPart, m, dataType, rowIndex - 1, columnIdex, offsetavg, imageheight, offsetx, offsety);
						offsetx += offsetavg + 1;
					}
				}
				else
				{
					OpenXMLExcelHelper.InsertImage(worksheetPart, (callData.Data as byte[]), dataType, rowIndex - 1, columnIdex, imagewidth, imageheight, offsetx, offsety);
				}
			}
		}
		#endregion

		#region --计算当前行的最大高度--
		private static double GetMaxRowHeight(List<OXSExcelDataCell> rowdata)
		{
			int maxRowHeight = 0;
			foreach (var col in rowdata)
			{
				if (col.CellHeight > maxRowHeight)
					maxRowHeight = col.CellHeight;
			}
			//点数，是point简称 1磅=0.03527厘米=1/72英寸
			//1英寸=2.54厘米=96像素（分辨率为96dpi)
			return maxRowHeight / 96.0 * 72;
		}
		#endregion


		private static Columns GetColumns(List<SimpleHeader> headers)
		{

			double colWidth = 0;
			Columns columns = new Columns();

			double maxWidth = 7;
			int index = 0;
			foreach (var header in headers)
			{
				double width = 0;
				if (header.Width <= 0)
				{
					colWidth = header.HeaderText.Length;
				}
				else
				{
					//double charWidth = Math.Truncate((pixels - 5) / maxWidth * 100 + 0.5) / 100;
					colWidth = Math.Truncate((header.Width - 5) / maxWidth * 100 + 0.5) / 100;
				}
				width = Math.Truncate((colWidth * maxWidth + 5) / maxWidth * 256) / 256;
				//单位转换公式地址 https://msdn.microsoft.com/en-us/library/documentformat.openxml.spreadsheet.column
				Column col = new Column() { BestFit = true, Min = (UInt32)(index + 1), Max = (UInt32)(index + 1), CustomWidth = true, Width = width };
				index++;
				columns.Append(col);
			}
			return columns;
		}
	}
}

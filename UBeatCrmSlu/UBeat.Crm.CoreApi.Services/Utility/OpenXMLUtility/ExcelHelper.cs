using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Utility.OpenXMLUtility
{
    public class ExcelHelper
    {
        #region --读取Excel文件--
        public static List<ImportSheetData> ReadExcelList(Stream file)
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


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using System.Text.RegularExpressions;

using UBeat.Crm.CoreApi.Services.Models.Excels;
using System.DrawingCore;

namespace UBeat.Crm.CoreApi.Services.Utility.ExcelUtility
{
    public class OpenXMLExcelHelper
    {

        #region --获取列名，如A，B，C列--
        public static string GetColumnName(uint columnIndex)
        {
            var intFirstLetter = ((columnIndex) / 676) + 64;
            var intSecondLetter = ((columnIndex % 676) / 26) + 64;
            var intThirdLetter = (columnIndex % 26) + 65;

            var firstLetter = (intFirstLetter > 64) ? (char)intFirstLetter : ' ';
            var secondLetter = (intSecondLetter > 64) ? (char)intSecondLetter : ' ';
            var thirdLetter = (char)intThirdLetter;

            return string.Concat(firstLetter, secondLetter, thirdLetter).Trim();
        }

        // Given a cell name, parses the specified cell to get the column name.
        public static string GetColumnName(string cellName)
        {
            // Create a regular expression to match the column name portion of the cell name.
            Regex regex = new Regex("[A-Za-z]+");
            Match match = regex.Match(cellName);
            return match.Value;
        }
        #endregion

        #region --获取列下标，如第1，2，3，4列--
        public static uint GetColumnIndex(string columnName)
        {
            var alpha = new Regex("^[A-Z]+$");
            if (!alpha.IsMatch(columnName)) throw new ArgumentException();

            char[] colLetters = columnName.ToCharArray();
            Array.Reverse(colLetters);

            uint convertedValue = 0;
            for (uint i = 0; i < colLetters.Length; i++)
            {
                char letter = colLetters[i];
                // ASCII 'A' = 65
                int current = i == 0 ? letter - 65 : letter - 64;
                convertedValue += (uint)current * (uint)Math.Pow(26, i);
            }

            return convertedValue;
        }
        #endregion

        #region --获取行下标，如第1，2，3，4行--
        public static uint GetRowIndex(string cellName)
        {
            // Create a regular expression to match the row index portion the cell name.
            Regex regex = new Regex(@"\d+");
            Match match = regex.Match(cellName);
            return uint.Parse(match.Value);
        }
        #endregion

        #region --创建文本单元格--

        public static Cell InsertText(WorkbookPart workbookPart, WorksheetPart worksheetPart, uint columnIdex, uint rowIndex, string text, Row row = null, CellTypeSelfDefined cellType = CellTypeSelfDefined.Normal)
        {
            Cell cell = InsertCellInWorksheet(columnIdex, rowIndex, worksheetPart, row);
            cell.CellValue = new CellValue(text);
            if (cellType == CellTypeSelfDefined.Number)
            {
                cell.DataType = new EnumValue<CellValues>(CellValues.Number);
            }
            else if (cellType == CellTypeSelfDefined.Date) {
                cell.DataType = new EnumValue<CellValues>(CellValues.Date);
            }else
            {
                cell.DataType = new EnumValue<CellValues>(CellValues.String);
            }
            return cell;
        }
        public static Cell InsertSharedText(WorkbookPart workbookPart, WorksheetPart worksheetPart, uint columnIdex, uint rowIndex, string text, Row row = null)
        {
            // Get the SharedStringTablePart. If it does not exist, create a new one.
            SharedStringTablePart shareStringPart;
            if (workbookPart.GetPartsOfType<SharedStringTablePart>().Count() > 0)
            {
                shareStringPart = workbookPart.GetPartsOfType<SharedStringTablePart>().First();
            }
            else
            {
                shareStringPart = workbookPart.AddNewPart<SharedStringTablePart>();
            }

            // Insert the text into the SharedStringTablePart.
            int index = InsertSharedStringItem(text, shareStringPart);

            Cell cell = InsertCellInWorksheet(columnIdex, rowIndex, worksheetPart, row);

            // Set the value of cell A1.
            cell.CellValue = new CellValue(index.ToString());
            cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
            //workbookPart.Workbook.Save();

            return cell;
        }

        // Given text and a SharedStringTablePart, creates a SharedStringItem with the specified text 
        // and inserts it into the SharedStringTablePart. If the item already exists, returns its index.
        private static int InsertSharedStringItem(string text, SharedStringTablePart shareStringPart)
        {
            // If the part does not contain a SharedStringTable, create one.
            if (shareStringPart.SharedStringTable == null)
            {
                shareStringPart.SharedStringTable = new SharedStringTable();
            }
            int i = 0;
       
            // Iterate through all the items in the SharedStringTable. If the text already exists, return its index.
            foreach (SharedStringItem item in shareStringPart.SharedStringTable.Elements<SharedStringItem>())
            {
                if (item.InnerText == text)
                {
                    return i;
                }
                i++;
            }
            // The text does not exist in the part. Create the SharedStringItem and return its index.
            shareStringPart.SharedStringTable.AppendChild(new SharedStringItem(new Text(text)));
            //shareStringPart.SharedStringTable.Save();
            return i;
        }

        public static void InserRowInSheetData(SheetData sheetData, uint rowIndex)
        {
            if (sheetData.Elements<Row>().Count() < rowIndex)
            {
                for (uint i = (uint)sheetData.Elements<Row>().Count() + 1; i <= rowIndex; i++)
                {
                    var row = new Row() { RowIndex = i };
                    sheetData.Append(row);
                }
            }
        }

        private static Cell InsertCellInWorksheet(uint columnIdex, uint rowIndex, WorksheetPart worksheetPart, Row row = null)
        {
            Worksheet worksheet = worksheetPart.Worksheet;
            SheetData sheetData = worksheet.GetFirstChild<SheetData>();
            var columnName = GetColumnName(columnIdex);
            string cellReference = columnName + rowIndex;
            // If the row of the worksheet is null,find by rowIndex.
            if (row == null)
            {
                // If the worksheet does not contain a row with the specified row index, insert one.
                if (sheetData.Elements<Row>().Where(r => r.RowIndex == rowIndex).Count() == 0)
                {
                    InserRowInSheetData(sheetData, rowIndex);
                }
                row = sheetData.Elements<Row>().Where(r => r.RowIndex == rowIndex).First();
            }
            // If there is not a cell with the specified column name, insert one.  
            if (row.Elements<Cell>().Where(c => c.CellReference.Value == columnName + rowIndex).Count() > 0)
            {
                return row.Elements<Cell>().Where(c => c.CellReference.Value == cellReference).First();
            }
            else
            {
                // Cells must be in sequential order according to CellReference. Determine where to insert the new cell.
                Cell refCell = null;
                foreach (Cell cell in row.Elements<Cell>())
                {
                    var columLetters= GetColumnName(cell.CellReference.Value);
                    //if (string.Compare(cell.CellReference.Value, cellReference, true) > 0)
                    if (GetColumnIndex(columLetters) > columnIdex)
                    {
                        refCell = cell;
                        break;
                    }
                }
                Cell newCell = new Cell() { CellReference = cellReference };
                row.InsertBefore(newCell, refCell);
                return newCell;
            }
        }


        #endregion

        #region --创建超链接单元格--
        public static Cell CreateHyperlinkCell(uint cellIdex, uint rowIdex, string text, string url)
        {
            CellFormula cellFormula1 = new CellFormula()
            {
                Space = SpaceProcessingModeValues.Preserve
            };
            cellFormula1.Text = string.Format(@"HYPERLINK(""{0}"", ""{1}"")", url, text);

            var cell = new Cell
            {
                DataType = CellValues.InlineString,
                CellReference = GetColumnName(cellIdex) + rowIdex,
                CellValue = new CellValue(text),
                CellFormula = cellFormula1,
            };
            return cell;
        }
        #endregion

        #region --插入图片--
        public static OffsetXY InsertImage(WorksheetPart wsp, byte[] bytes, ImagePartType imagePartType, uint rowId, uint cellId, long? width, long? height, long offsetx = 0, long offsety = 0, OffsetXY offsetXY = null)
        {
            return InsertImage(wsp, rowId, cellId, rowId, cellId, offsetx, offsety, width, height, bytes, imagePartType, offsetXY);
        }

        /// <summary>
        /// 插入图片
        /// </summary>
        /// <param name="wsp"></param>
        /// <param name="rowId1">起始行编号</param>
        /// <param name="columnId1">起始列编号</param>

        /// <param name="offsetx">X偏差像素</param>
        /// <param name="offsety">Y偏差像素</param>
        /// <param name="width">图片宽度</param>
        /// <param name="height">图片高度</param>
        /// <param name="bytes">图片数据</param>
        /// <param name="imagePartType">格式</param>
        /// <param name="offsetXY">图片插入位置相对单元格左上角的偏差值,用于同个单元格多张图片时计算位移</param>
        /// <returns></returns>
        public static OffsetXY InsertImage(WorksheetPart wsp, uint rowId1, uint columnId1, uint rowId2, uint columnId2, long offsetx, long offsety, long? width, long? height, byte[] bytes, ImagePartType imagePartType, OffsetXY offsetXY=null)
        {
            try
            {
                
                using (Stream imageStream = new MemoryStream(bytes))
                {
                    DrawingsPart dp;
                    ImagePart imgp;
                    WorksheetDrawing wsd;

                    if (wsp.DrawingsPart == null)
                    {
                        //----- no drawing part exists, add a new one
                        dp = wsp.AddNewPart<DrawingsPart>();
                        imgp = dp.AddImagePart(imagePartType, wsp.GetIdOfPart(dp));
                        wsd = new WorksheetDrawing();
                    }
                    else
                    {
                        //----- use existing drawing part
                        dp = wsp.DrawingsPart;
                        imgp = dp.AddImagePart(imagePartType);
                        dp.CreateRelationshipToPart(imgp);
                        wsd = dp.WorksheetDrawing;
                    }
                    
                    imgp.FeedData(new MemoryStream(bytes));
                    int imageNumber = dp.ImageParts.Count();
                    if (imageNumber == 1)
                    {
                        Drawing drawing = new Drawing();
                        drawing.Id = dp.GetIdOfPart(imgp);
                        wsp.Worksheet.Append(drawing);
                    }
                    var extents = new DocumentFormat.OpenXml.Drawing.Extents();
                    //Bitmap bmtemp = new Bitmap(imageStream);
                    //Bitmap bm = bmtemp;
                    //bm.SetResolution(96, 96);
                    //if (width != null && height != null)
                    //{
                    //    bm = new Bitmap(bmtemp, (int)width.Value, (int)height.Value);
                    //}
                    //float verticalResolution = bm.VerticalResolution;
                    //float horizontalResolution = bm.HorizontalResolution;

                    ////计算公式：EMU = pixel * 914400 / Resolution
                    //extents.Cx = ((long)bm.Width ) * (long)((float)914400 / horizontalResolution);
                    //extents.Cy = ((long)bm.Height ) * (long)((float)914400 / verticalResolution);
                    //bmtemp.Dispose();
                    //bm.Dispose();
                    //linux 不支持bitmap，故写死这段范围和图片分辨率作为计算条件
                    float verticalResolution = 96;
                    float horizontalResolution = 96;
                    //计算公式：EMU = pixel * 914400 / Resolution
                    extents.Cx = ((long)width.Value) * (long)((float)914400 / horizontalResolution);
                    extents.Cy = ((long)height.Value) * (long)((float)914400 / verticalResolution);
                    if (offsetXY == null)
                        offsetXY = new OffsetXY();
                    var XOffsetEMU = offsetx * (long)((float)914400 / horizontalResolution);
                    var YOffsetEMU = offsety * (long)((float)914400 / verticalResolution);
                 

                    var fromXOffset = XOffsetEMU;
                    var fromYOffset = YOffsetEMU;
                   

                    switch (offsetXY.OffsetType)
                    {
                        case OffsetType.X:
                            fromXOffset = offsetXY.XOffset+ XOffsetEMU;
                            break;
                        case OffsetType.Y:
                            fromYOffset = offsetXY.YOffset+ YOffsetEMU;
                            break;
                        case OffsetType.XY:
                            fromXOffset = offsetXY.XOffset + XOffsetEMU;
                            fromYOffset = offsetXY.YOffset + YOffsetEMU;
                            break;
                    }


                    TwoCellAnchor anchor = wsd.AppendChild(new TwoCellAnchor() { EditAs = EditAsValues.Absolute });
                    

                     var picture = new DocumentFormat.OpenXml.Drawing.Spreadsheet.Picture()
                    {
                        NonVisualPictureProperties = GetNonVisualPictureProperties(imageNumber),
                        BlipFill = GetBlipFill(dp, imgp),
                        ShapeProperties = GetShapeProperties(extents)
                    };

                    //anchor.Extent = new Extent();
                    //anchor.Extent.Cx = extents.Cx;
                    //anchor.Extent.Cy = extents.Cy;
                    //anchor.Position = new Position();
                    //anchor.Position.X = fromXOffset;
                    //anchor.Position.Y = fromYOffset;
                    anchor.FromMarker = new DocumentFormat.OpenXml.Drawing.Spreadsheet.FromMarker(
                        new ColumnId(columnId1.ToString()),
                        new ColumnOffset(fromXOffset.ToString()),
                        new RowId(rowId1.ToString()),
                        new RowOffset(fromYOffset.ToString()));
                    anchor.ToMarker = new DocumentFormat.OpenXml.Drawing.Spreadsheet.ToMarker(
                       new ColumnId(columnId2.ToString()),
                       new ColumnOffset((fromXOffset + extents.Cx).ToString()),
                       new RowId(rowId2.ToString()),
                       new RowOffset((fromYOffset + extents.Cy).ToString()));


                    anchor.Append(picture);
                    anchor.Append(new ClientData());
                    
                    wsd.Save(dp);
                    offsetXY.XOffset = fromXOffset + extents.Cx;
                    offsetXY.YOffset = fromYOffset + extents.Cy;
                    return offsetXY;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static NonVisualPictureProperties GetNonVisualPictureProperties(int imageNumber)
        {
            var nvdp = new NonVisualDrawingProperties();
            nvdp.Id = new UInt32Value((uint)(1024 + imageNumber));
            nvdp.Name = "Picture " + imageNumber.ToString();
            nvdp.Description = "";
            var picLocks = new DocumentFormat.OpenXml.Drawing.PictureLocks();
            picLocks.NoChangeAspect = true;
            picLocks.NoChangeArrowheads = true;
            var nvpdp = new NonVisualPictureDrawingProperties();
            nvpdp.PictureLocks = picLocks;
            return new NonVisualPictureProperties()
            {
                NonVisualDrawingProperties = nvdp,
                NonVisualPictureDrawingProperties = nvpdp
            };

        }
        private static BlipFill GetBlipFill(DrawingsPart dp, ImagePart imgp)
        {
            var stretch = new DocumentFormat.OpenXml.Drawing.Stretch();
            stretch.FillRectangle = new DocumentFormat.OpenXml.Drawing.FillRectangle();
            
            BlipFill blipFill = new BlipFill();
            
            var blip = new DocumentFormat.OpenXml.Drawing.Blip();
            blip.Embed = dp.GetIdOfPart(imgp);
            blip.CompressionState = DocumentFormat.OpenXml.Drawing.BlipCompressionValues.Print;
            
            blipFill.Blip = blip;
            blipFill.SourceRectangle = new DocumentFormat.OpenXml.Drawing.SourceRectangle();
            blipFill.Append(stretch);
            return blipFill;
        }
        private static ShapeProperties GetShapeProperties(DocumentFormat.OpenXml.Drawing.Extents extents)
        {
            var sp = new ShapeProperties();
            sp.BlackWhiteMode = DocumentFormat.OpenXml.Drawing.BlackWhiteModeValues.Auto;
            sp.Transform2D = new DocumentFormat.OpenXml.Drawing.Transform2D()
            {
                Offset = new DocumentFormat.OpenXml.Drawing.Offset() { X = 0, Y = 0 },
                Extents = extents
            };
            var prstGeom = new DocumentFormat.OpenXml.Drawing.PresetGeometry()
            {
                Preset = DocumentFormat.OpenXml.Drawing.ShapeTypeValues.Rectangle,
                AdjustValueList = new DocumentFormat.OpenXml.Drawing.AdjustValueList()
            };

            sp.Append(prstGeom);
            sp.Append(new DocumentFormat.OpenXml.Drawing.NoFill());
            return sp;
        }

        #endregion

        #region --合并单元格--

        public static void InsertMergeCells(Worksheet worksheet, MergeCells mergeCells)
        {

            // Insert a MergeCells object into the specified position. 
            if (worksheet.Elements<CustomSheetView>().Count() > 0)
            {
                worksheet.InsertAfter(mergeCells, worksheet.Elements<CustomSheetView>().First());
            }
            else if (worksheet.Elements<DataConsolidate>().Count() > 0)
            {
                worksheet.InsertAfter(mergeCells, worksheet.Elements<DataConsolidate>().First());
            }
            else if (worksheet.Elements<SortState>().Count() > 0)
            {
                worksheet.InsertAfter(mergeCells, worksheet.Elements<SortState>().First());
            }
            else if (worksheet.Elements<AutoFilter>().Count() > 0)
            {
                worksheet.InsertAfter(mergeCells, worksheet.Elements<AutoFilter>().First());
            }
            else if (worksheet.Elements<Scenarios>().Count() > 0)
            {
                worksheet.InsertAfter(mergeCells, worksheet.Elements<Scenarios>().First());
            }
            else if (worksheet.Elements<ProtectedRanges>().Count() > 0)
            {
                worksheet.InsertAfter(mergeCells, worksheet.Elements<ProtectedRanges>().First());
            }
            else if (worksheet.Elements<SheetProtection>().Count() > 0)
            {
                worksheet.InsertAfter(mergeCells, worksheet.Elements<SheetProtection>().First());
            }
            else if (worksheet.Elements<SheetCalculationProperties>().Count() > 0)
            {
                worksheet.InsertAfter(mergeCells, worksheet.Elements<SheetCalculationProperties>().First());
            }
            else
            {
                worksheet.InsertAfter(mergeCells, worksheet.Elements<SheetData>().First());
            }
            //worksheet.Save();
        }

        public static void MergeTwoCells(Worksheet worksheet, string cell1Name, string cell2Name)
        {

            if (worksheet == null || string.IsNullOrEmpty(cell1Name) || string.IsNullOrEmpty(cell2Name))
            {
                return;
            }
            // Verify if the specified cells exist, and if they do not exist, create them.
            CreateSpreadsheetCellIfNotExist(worksheet, cell1Name);
            CreateSpreadsheetCellIfNotExist(worksheet, cell2Name);

            MergeCells mergeCells;
            if (worksheet.Elements<MergeCells>().Count() > 0)
            {
                mergeCells = worksheet.Elements<MergeCells>().First();
            }
            else
            {
                mergeCells = new MergeCells();
                InsertMergeCells(worksheet, mergeCells);
            }
            // Create the merged cell and append it to the MergeCells collection. 
            MergeCell mergeCell = new MergeCell() { Reference = new StringValue(cell1Name + ":" + cell2Name) };
            mergeCells.Append(mergeCell);
            //worksheet.Save();
        }

        // Given a Worksheet and a cell name, verifies that the specified cell exists.
        // If it does not exist, creates a new cell. 
        private static Cell CreateSpreadsheetCellIfNotExist(Worksheet worksheet, string cellName)
        {

            //string columnName = GetColumnName(cellName);
            uint rowIndex = GetRowIndex(cellName);
            SheetData sheetData = worksheet.Descendants<SheetData>().First();
            IEnumerable<Row> rows = worksheet.Descendants<Row>().Where(r => r.RowIndex == rowIndex);

            // If the Worksheet does not contain the specified row, create the specified row.
            // Create the specified cell in that row, and insert the row into the Worksheet.
            if (rows.Count() == 0)
            {
                InserRowInSheetData(sheetData, rowIndex);
            }
            Row row = sheetData.Descendants<Row>().Where(r => r.RowIndex == rowIndex).First();
            var cell = row.Elements<Cell>().Where(c => c.CellReference.Value == cellName).FirstOrDefault();
            if (cell == null)
            {
                cell = new Cell() { CellReference = cellName };
                row.Append(cell);
                //worksheet.Save();
            }
            return cell;
        }

        #endregion

        #region --GenerateStyleSheet--
        public static Stylesheet GenerateStyleSheet()
        {
            Fonts fonts = new Fonts(
                new DocumentFormat.OpenXml.Spreadsheet.Font( // Index 0 - The default font.
                    new FontSize() { Val = 10 },
                    new DocumentFormat.OpenXml.Spreadsheet.Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                    new FontName() { Val = "Calibri" }),
                new DocumentFormat.OpenXml.Spreadsheet.Font( // Index 1 - The bold font
                    new Bold(),
                    new FontSize() { Val = 10 },
                    new DocumentFormat.OpenXml.Spreadsheet.Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                    new FontName() { Val = "Calibri" }),
                new DocumentFormat.OpenXml.Spreadsheet.Font( // Index 2 - The bold and red font
                    new Bold(),
                    new FontSize() { Val = 10 },
                    new DocumentFormat.OpenXml.Spreadsheet.Color() { Rgb = new HexBinaryValue() { Value = "FF3030" } },
                    new FontName() { Val = "Calibri" }),
                 new DocumentFormat.OpenXml.Spreadsheet.Font( // Index 3 - The red font
                    new FontSize() { Val = 10 },
                    new DocumentFormat.OpenXml.Spreadsheet.Color() { Rgb = new HexBinaryValue() { Value = "FF3030" } },
                    new FontName() { Val = "Calibri" })
            );
            Fills fills = new Fills(
                new Fill( // Index 0 - The default fill.
                    new PatternFill() { PatternType = PatternValues.None }),
                new Fill( // Index 1 - The default fill of gray 125 (required)
                    new PatternFill() { PatternType = PatternValues.Gray125 }),
                new Fill( // Index 2 - The yellow fill.
                    new PatternFill(
                    new ForegroundColor() { Rgb = new HexBinaryValue() { Value = "EEE9E9" } }
                    )
                    { PatternType = PatternValues.Solid })
            );
            Borders borders = new Borders(
                new Border( // Index 0 - The default border.
                    new LeftBorder(),
                    new RightBorder(),
                    new TopBorder(),
                    new BottomBorder(),
                    new DiagonalBorder()),
                new Border( // Index 1 - Applies a Left, Right, Top, Bottom border to a cell
                    new LeftBorder(new DocumentFormat.OpenXml.Spreadsheet.Color() { Auto = true })
                    {
                        Style = BorderStyleValues.Thin
                    },
                    new RightBorder(new DocumentFormat.OpenXml.Spreadsheet.Color() { Auto = true })
                    {
                        Style = BorderStyleValues.Thin
                    },
                    new TopBorder(new DocumentFormat.OpenXml.Spreadsheet.Color() { Auto = true })
                    {
                        Style = BorderStyleValues.Thin
                    },
                    new BottomBorder(new DocumentFormat.OpenXml.Spreadsheet.Color() { Auto = true })
                    {
                        Style = BorderStyleValues.Thin
                    },
                    new DiagonalBorder())
                
            );
            CellFormats cellFormats = new CellFormats(
                new CellFormat() { FontId = 0, FillId = 0, BorderId = 0 }, // Index 0 - The default cell style
                new CellFormat() { FontId = 3, FillId = 0, BorderId = 0, ApplyFont = true }, // Index 1 - red font ,default fill and default border.
                new CellFormat() { FontId = 1, FillId = 0, BorderId = 0, ApplyFont = true }, // Index 2 - bold font ,default fill and default border.
                new CellFormat() { FontId = 2, FillId = 0, BorderId = 0, ApplyFont = true }, // Index 3 - bold and red font ,default fill and default border.
                new CellFormat() { FontId = 1, FillId = 2, BorderId = 1, ApplyFont = true }, // Index 4 - bold font ,2 fill and 1 border.
                new CellFormat() { FontId = 2, FillId = 2, BorderId = 1, ApplyFill = true }, // Index 5 - bold and red font ,2 fill and 1 border.
                
                new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center, WrapText = true }) //Alignment Center
                { FontId = 1, FillId = 0, BorderId = 0, ApplyFont = true }, // Index 6 - bold font ,default fill and default border.
                new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center, WrapText = true }) //Alignment Center
                { FontId = 2, FillId = 0, BorderId = 0, ApplyFont = true }, // Index 7 - bold and red font ,default fill and default border.
                new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center, WrapText = true }) //Alignment Center
                { FontId = 1, FillId = 1, BorderId = 1, ApplyFont = true }, // Index 8 - bold font ,1 fill and 1 border.
                new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center, WrapText = true }) //Alignment Center
                { FontId = 2, FillId = 1, BorderId = 1, ApplyFill = true } // Index 9 - bold and red font ,1 fill and 1 border.

            );

            return new Stylesheet(fonts, fills, borders, cellFormats); // return
        }
        #endregion

        public enum CellTypeSelfDefined {
            Normal = 0 ,
            String  =1,
            Number =2,
            Date = 3
        }
    }
}

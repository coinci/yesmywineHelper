using NPOI.SS.UserModel;using NPOI.XSSF.UserModel;using System;using System.Collections.Generic;using System.Data;using System.IO;using System.Text;namespace CommonClass{    public class ExcelHelper    {        public IWorkbook ExcelWorkbook;        public ExcelHelper()        {            ExcelWorkbook = new XSSFWorkbook();            ExcelWorkbook.CreateSheet(DEFAULT_SHEET_NAME);            CurrentSheet = 0;        }        /// <summary>        ///        /// </summary>        /// <param name="path">file path of the xls file you want to operate on,if file not exist, this code will create and initialize.</param>        public ExcelHelper(string path)        {            ExcelPath = path;            //if file not exist, create and initialize.            if (!File.Exists(path))            {                ExcelWorkbook = new XSSFWorkbook();                ExcelWorkbook.CreateSheet(DEFAULT_SHEET_NAME);                //SaveAs(ExcelPath,true);            }            //if exist, open and start your operation            else            {                using (Stream fs = new FileStream(path, FileMode.Open, FileAccess.Read))                {                    ExcelWorkbook = new XSSFWorkbook(fs);                    //fs.Close();                }            }            CurrentSheet = 0;        }        public const string DEFAULT_SHEET_NAME = "Sheet1";        /// <summary>        /// indict which sheet you are operating. default is 0.you can switch to the sheet you need by change this number        /// </summary>        public int CurrentSheet { get; set; }        /// <summary>        /// the path of the excel path you are reading        /// </summary>        public string ExcelPath { get; set; }        /// <summary>        /// save current file.the path is ExcelPath        /// </summary>        /// <param name="overwrite"></param>        public void Save(bool overwrite = true)        {            SaveAs(ExcelPath, overwrite);        }        /// <summary>        /// Save the file to an another path.        /// </summary>        /// <param name="path"></param>        /// <param name="overwrite"></param>        public void SaveAs(string path, bool overwrite = true)        {            if (!Directory.Exists(Path.GetDirectoryName(path)))                Directory.CreateDirectory(Path.GetDirectoryName(path));            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))            {                ExcelWorkbook.Write(fs);                //fs.Close();            }        }        /// <summary>        /// Get a file the byte stream of the excel file. you can transfer this stream by TCP/HTTP.        /// </summary>        /// <returns></returns>        public byte[] GetMemoryStream()        {            byte[] buffer = new byte[1];            using (MemoryStream ms = new MemoryStream())            {                ExcelWorkbook.Write(ms);                buffer = ms.ToArray();            }            return buffer;        }        /// <summary>        /// Get the content from a cell.        /// </summary>        /// <param name="row">start row postion, start with 0</param>        /// <param name="col">start column postion</param>        /// <param name="returnFormula">choose to return Formula or number</param>        /// <returns></returns>        public object GetCell(int row, int col, bool returnFormula = true)        {            ISheet eSheet = ExcelWorkbook.GetSheetAt(CurrentSheet);            ICell eCell = eSheet.GetRow(row).GetCell(col);            if (eCell == null)                return null;            switch (eCell.CellType)            {                case CellType.Numeric:                    return eCell.NumericCellValue;                case CellType.Boolean:                    return eCell.BooleanCellValue;                case CellType.Blank:                    return string.Empty;                case CellType.Formula:                    if (returnFormula)                        return "=" + eCell.CellFormula;                    else                        return eCell.NumericCellValue;                case CellType.Error:                    return "Error:" + eCell.ErrorCellValue.ToString();                case CellType.Unknown:                default:                    return eCell.StringCellValue;            }        }        /// <summary>        /// write content to a cell        /// </summary>        /// <param name="row">start row postion，start with 0</param>        /// <param name="col">start column postion</param>        /// <param name="value">content to write</param>        public void WriteCell(int row, int col, object value)        {            ISheet eSheet = ExcelWorkbook.GetSheetAt(CurrentSheet);            ICell eCell = PrepareCell2Write(row, col, eSheet);            String valT = value.GetType().Name;            switch (valT)            {                case "Double":                    eCell.SetCellValue((double)value);                    break;                case "Int32":                    eCell.SetCellValue((double)(int)value);                    break;                case "Single":                    eCell.SetCellValue((double)(Single)value);                    break;                case "UInt32":                    eCell.SetCellValue((double)(UInt32)value);                    break;                case "Int64":                    eCell.SetCellValue((double)(Int64)value);                    break;                case "UInt64":                    eCell.SetCellValue((double)(UInt64)value);                    break;                case "Int16":                    eCell.SetCellValue((double)(Int16)value);                    break;                case "UInt16":                    eCell.SetCellValue((double)(UInt16)value);                    break;                case "DateTime":                    eCell.SetCellValue(((DateTime)value).ToString());                    break;                case "Boolean":                    eCell.SetCellValue((bool)value);                    break;                case "IRichTextString":                    eCell.SetCellValue((IRichTextString)value);                    break;                case "String":                    string strTargetValue = value as string;                    if (!string.IsNullOrEmpty(strTargetValue)                        && strTargetValue[0] == '=')//this "if" allows formula.                    {                        eCell.SetCellType(CellType.Formula);                        eCell.CellFormula = ((String)value).Remove(0, 1);                    }                    else                        eCell.SetCellValue((string)value);                    break;                default:                    eCell.SetCellValue(value.ToString());                    break;            }        }        /// <summary>        /// check weither the cell is ready to be writen. if the cell is not exist, create it.        /// </summary>        /// <param name="row"></param>        /// <param name="col"></param>        /// <param name="eSheet"></param>        /// <returns></returns>        private static ICell PrepareCell2Write(int row, int col, ISheet eSheet)        {            IRow eRow = eSheet.GetRow(row);            if (eRow == null)            { //add row                eRow = eSheet.CreateRow(row);            }            ICell eCell = eRow.GetCell(col);            if (eCell == null)            { // add cell                eCell = eRow.CreateCell(col);            }            return eCell;        }        /// <summary>        /// return cells you choosed. if the cell is empty, that cell get null        /// </summary>        /// <param name="rowStart">start row index,start with 0</param>        /// <param name="rowEnd">end row index,start with 0</param>        /// <param name="colStart">start column index, start with 0</param>        /// <param name="colEnd">end row index,start with 0</param>        /// <param name="returnFormula">choose to return Formula or number</param>        /// <returns></returns>        public DataTable GetCells(int rowStart, int rowEnd, int colStart, int colEnd, bool returnFormula = true)        {            ISheet eSheet = ExcelWorkbook.GetSheetAt(CurrentSheet);            DataTable dt = new DataTable(eSheet.SheetName);            for (int i = colStart; i <= colEnd; i++)            {                dt.Columns.Add("Column" + i.ToString());            }            DataRow dr;            int colCount = colEnd - colStart + 1;            for (int i = rowStart; i <= rowEnd; i++)            {                dr = dt.NewRow();                if (eSheet.GetRow(i) != null)//if the row is empty, the entire row will not be read                {                    for (int j = 0; j < colCount; j++)                    {                        dr[j] = GetCell(i, j + colStart, returnFormula);                    }                }                dt.Rows.Add(dr);            }            return dt;        }        public DataTable GetAllCells(bool hasRowHeader = false)        {            ISheet eSheet = ExcelWorkbook.GetSheetAt(CurrentSheet);            System.Collections.IEnumerator rows = eSheet.GetRowEnumerator();            DataTable dt = new DataTable();            while (rows.MoveNext())            {                IRow row = (IRow)rows.Current;                for (int i = dt.Columns.Count; i < row.LastCellNum; i++)                {                    dt.Columns.Add(Convert.ToChar(((int)'A') + i).ToString());                }                DataRow dr = dt.NewRow();                for (int i = 0; i < row.LastCellNum; i++)                {                    ICell cell = row.GetCell(i);                    if (cell == null)                    {                        dr[i] = null;                    }                    else                    {                        dr[i] = cell.ToString();                    }                }                dt.Rows.Add(dr);            }            if (hasRowHeader)                dt.Rows.RemoveAt(0);            return dt;        }        /// <summary>        /// write content to a area of cells        /// </summary>        /// <param name="dtValue">datatable you want to write to the xls file</param>        /// <param name="startRow">start index of cells, start with 0</param>        /// <param name="col">start index of cells, start with 0</param>        /// <param name="writeHeader">weither to add header row</param>        public void WriteCells(DataTable dtValue, int startRow = 0, int startColumn = 0, bool writeHeader = false)        {            ISheet eSheet = ExcelWorkbook.GetSheetAt(CurrentSheet);            if (writeHeader)            {                for (int j = 0; j < dtValue.Columns.Count; j++)                {                    WriteCell(startRow, j + startColumn, dtValue.Columns[j].ColumnName);                }                startRow++;            }            for (int i = 0; i < dtValue.Rows.Count; i++)            {                for (int j = 0; j < dtValue.Columns.Count; j++)                {                    WriteCell(i + startRow, j + startColumn, dtValue.Rows[i][j]);                }            }        }        public void WriteCellsByRows(DataTable dtValue, int startRow = 0, int startColumn = 0, bool writeHeader = false)        {            ISheet eSheet = ExcelWorkbook.GetSheetAt(CurrentSheet);            if (writeHeader)            {                for (int j = 0; j < dtValue.Columns.Count; j++)                {                    WriteCell(startRow+j, startColumn, dtValue.Columns[j].ColumnName);                }                startColumn++;            }            for (int i = 0; i < dtValue.Rows.Count; i++)            {                for (int j = 0; j < dtValue.Columns.Count; j++)                {                    WriteCell(j + startRow, i + startColumn, dtValue.Rows[i][j]);                }            }        }        /// <summary>        /// set color of header-row        /// </summary>        /// <param name="row">row start index of header, start with 0 </param>        /// <param name="col">column start index of header,start with 0 </param>        /// <param name="dt"></param>        public void SetHeaderRowStyle(int row, int col, DataTable dt)        {            SetHeaderRowStyle(row, col, dt.Columns.Count);        }        /// <summary>        /// set the color of header-row        /// </summary>        /// <param name="row">row postion of the header,start from 0</param>        /// <param name="col">start column postion of the header,start from 0</param>        /// <param name="columnCount">how many columns does the header has</param>        public void SetHeaderRowStyle(int row, int startColumn, int columnCount,bool IsRowsShow=false)        {            ISheet eSheet = ExcelWorkbook.GetSheetAt(CurrentSheet);            ICellStyle style1 = SetStyle(IndexedColors.Black, IndexedColors.LightYellow);            style1.Alignment = HorizontalAlignment.Center;            style1.VerticalAlignment = VerticalAlignment.Center;            IFont f = style1.GetFont(ExcelWorkbook);            f.Boldweight = 3;            style1.SetFont(f);            for (int i = 0; i < columnCount; i++)            {                ICell eCell;                if(IsRowsShow)                {                     eCell = PrepareCell2Write(row + i, startColumn, eSheet);                }                else                {                     eCell = PrepareCell2Write(row, startColumn + i, eSheet);                }                                eCell.CellStyle = style1;            }        }        /// <summary>        /// set the width of a column        /// </summary>        /// <param name="columnID">start column position</param>        /// <param name="width">column width</param>        public void SetColumnWidth(int columnID, int width)        {            ISheet eSheet = ExcelWorkbook.GetSheetAt(CurrentSheet);            eSheet.SetColumnWidth(columnID, width * 256);        }        public void SetAutoSizeColumn(int columnID, int width)        {            ISheet eSheet = ExcelWorkbook.GetSheetAt(CurrentSheet);            for (int i = columnID; i < width; i++)            {                eSheet.AutoSizeColumn(i);            }        }        /// <summary>        /// set width of some columns        /// </summary>        /// <param name="startColumn">start index of the area</param>        /// <param name="columnCount">length of the area</param>        /// <param name="width">columns width</param>        public void SetColumnsWidth(int startColumn, int columnCount, int width)        {            ISheet eSheet = ExcelWorkbook.GetSheetAt(CurrentSheet);            for (int i = startColumn; i < columnCount; i++)                eSheet.SetColumnWidth(i, width * 256);        }        public void SetColumnsHidden(int columnIndex)        {            ISheet eSheet = ExcelWorkbook.GetSheetAt(CurrentSheet);            eSheet.SetColumnHidden(columnIndex, true);        }        /// <summary>        /// set styles to a range of cells        /// </summary>        /// <param name="startRow"></param>        /// <param name="startColumn"></param>        /// <param name="dt"></param>        /// <param name="HasRowHeader"></param>        public void SetAltRowStyle(int startRow, int startColumn, DataTable dt, bool HasRowHeader = false)        {            SetAltRowStyle(startRow, dt.Rows.Count, startColumn, dt.Columns.Count, HasRowHeader);        }        /// <summary>        /// set styles to a range of cells, including altinative row color,border,font color        /// </summary>        /// <param name="startRow"></param>        /// <param name="rowCount"></param>        /// <param name="startColumn"></param>        /// <param name="colcount"></param>        /// <param name="HasRowHeader"></param>        public void SetAltRowStyle(int startRow, int rowCount, int startColumn, int colcount, bool HasRowHeader = false)        {            if (HasRowHeader)            {                startRow++;            }            ISheet eSheet = ExcelWorkbook.GetSheetAt(CurrentSheet);            ICellStyle style1 = SetStyle(IndexedColors.Automatic, IndexedColors.White);            ICellStyle style2 = SetStyle(IndexedColors.Automatic, IndexedColors.White);            for (int i = 0; i < rowCount; i++)            {                //IRow r=eSheet.GetRow(row + i);//this method is not working,because it only effects blank cells,which are out of the concern.                for (int j = 0; j < colcount; j++)                {                    ICell cr = PrepareCell2Write(i + startRow, j + startColumn, eSheet);                    cr.CellStyle = (i % 2 == 0) ? style1 : style2;                }            }        }        /// <summary>        /// set the style of a cell        /// the style includes font color, background color, border.        /// </summary>        /// <param name="foreColor"></param>        /// <param name="backgroundColor"></param>        /// <returns></returns>        protected ICellStyle SetStyle(IndexedColors foreColor, IndexedColors backgroundColor)        {            XSSFWorkbook tempworkbook = new XSSFWorkbook();            ICellStyle ics = ExcelWorkbook.CreateCellStyle();            ics.FillPattern = FillPattern.SolidForeground;            ics.FillForegroundColor = backgroundColor.Index;            //foreground color            IFont f = ExcelWorkbook.CreateFont();            f.Color = foreColor.Index;            ics.SetFont(f);            //set bolder            ics.BorderBottom = BorderStyle.Thin;            ics.BorderLeft = BorderStyle.Thin;            ics.BorderRight = BorderStyle.Thin;            ics.BorderTop = BorderStyle.Thin;            ics.BottomBorderColor = ics.TopBorderColor = IndexedColors.Green.Index;            //set alignment            ics.Alignment = HorizontalAlignment.Center;            return ics;        }    }}
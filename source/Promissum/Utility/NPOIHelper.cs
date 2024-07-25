using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections;
using System.Linq;
using System.Text;
using Form = System.Windows.Forms;

namespace Lekco.Promissum.Utility
{
    public static class NPOIHelper
    {
        /// <summary>
        /// Save data as *.xlsx file.
        /// </summary>
        /// <param name="title">The title of <see cref="SaveFileDialog"/>.</param>
        /// <param name="name">The file's default name.</param>
        /// <returns>A <see cref="bool"/> representing whether its path has been settled successfully.</returns>
        public static bool SaveAsXlsx(string title, ref string name)
        {
            var sfd = new Form.SaveFileDialog()
            {
                AddExtension = true,
                FileName = name,
                Filter = "Excel 文档|*.xlsx",
                FilterIndex = 1,
                Title = title
            };
            if (sfd.ShowDialog() == Form.DialogResult.OK)
            {
                name = sfd.FileName;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the workbook a font-styled cell style, and returns it.
        /// </summary>
        /// <param name="workbook">The given workbook.</param>
        /// <param name="font">The target font.</param>
        /// <returns>The set font-styled cell style.</returns>
        public static XSSFCellStyle SetCellFontStyle(this XSSFWorkbook workbook, IFont font,
            HorizontalAlignment hAlignment = HorizontalAlignment.Center, VerticalAlignment vAlignment = VerticalAlignment.Center)
        {
            XSSFCellStyle style = (XSSFCellStyle)workbook.CreateCellStyle();
            style.SetFont(font);
            style.Alignment = hAlignment;
            style.VerticalAlignment = vAlignment;
            return style;
        }

        /// <summary>
        /// Sets the workbook a big-bold cell style, and returns it.
        /// </summary>
        /// <param name="workbook">The given workbook.</param>
        /// <param name="needBorder">Whether the cell need border.</param>
        /// <returns>The set big-bold cell style.</returns>
        public static XSSFCellStyle SetCellStyle(this XSSFWorkbook workbook, bool boldFont = false,
            HorizontalAlignment hAlignment = HorizontalAlignment.Center, VerticalAlignment vAlignment = VerticalAlignment.Center)
        {
            var font = (XSSFFont)workbook.CreateFont();
            font.FontName = "等线";
            font.IsBold = boldFont;
            return SetCellFontStyle(workbook, font, hAlignment, vAlignment);
        }

        public static void AutoFitColumnWidth(this ISheet sheet, int col, int row_start, int row_end)
        {
            double columnWidth = sheet.GetColumnWidth(col) / 256;
            for (int row_ptr = row_start; row_ptr < row_end; row_ptr++)
            {
                var row = sheet.GetRow(row_ptr);
                var cell = row.GetCell(col);
                columnWidth = Math.Max(columnWidth, Encoding.UTF8.GetBytes(cell.ToString() ?? "").Length);
            }
            sheet.SetColumnWidth(col, columnWidth * 256);
        }

        public static XSSFWorkbook GenerateWorkbook(string sheetName, IEnumerable source, string[] headers, Func<object, dynamic>[] funcs,
            int[]? leftAlignmentColumn = null)
        {
            if (headers.Length != funcs.Length)
            {
                throw new ArgumentException("The length of headers doesn't equal to the length of funcs.");
            }

            var wb = new XSSFWorkbook();
            var boldStyle = wb.SetCellStyle(boldFont: true);
            var centerStyle = wb.SetCellStyle();
            var leftStyle = wb.SetCellStyle(hAlignment: HorizontalAlignment.Left);

            var ws = wb.CreateSheet(sheetName);
            var row = ws.CreateRow(0);
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = row.CreateCell(i);
                cell.SetCellValue(headers[i]);
                cell.CellStyle = boldStyle;
            }
            int row_id = 1;
            foreach (var obj in source)
            {
                row = ws.CreateRow(row_id++);
                int col_id = 0;
                foreach (var func in funcs)
                {
                    var cell = row.CreateCell(col_id);
                    cell.SetCellValue(func(obj));
                    cell.CellStyle = leftAlignmentColumn?.Contains(col_id) ?? false ? leftStyle : centerStyle;
                    col_id++;
                }
            }

            for (int i = 0; i < headers.Length; i++)
            {
                ws.AutoFitColumnWidth(i, 0, row_id);
            }

            return wb;
        }
    }
}

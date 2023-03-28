using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using UnityEngine;

namespace Nico.Editor
{
    public static class ExcelUtil
    {
        public static ExcelTable[] GetTables(string excelPath)
        {
            var fileInfo = new FileInfo(excelPath);
            ExcelPackage excelPackage = new ExcelPackage(fileInfo);
            if (!File.Exists(excelPath))
            {
                Debug.Log($"{fileInfo.FullName}文件不存在");
            }

            //读取Excel的所有工作表
            ExcelWorksheets sheets = excelPackage.Workbook.Worksheets;
            var tables = new ExcelTable[sheets.Count];
            Debug.Log($"读取到{sheets.Count}个工作表");
            int i = 0;
            foreach (var sheet in sheets)
            {
                var table = GetTable(sheet);
                tables[i] = table;
                ++i;
            }

            return tables;
        }

        public static ExcelTable GetTable(ExcelWorksheet sheet)
        {
            var table = new ExcelTable
            {
                tableName = sheet.Name,
                colCount = sheet.Dimension.Columns,
                rowCount = sheet.Dimension.Rows - 3//前三行为描述性文字 并非实际数据
            };
            table.memberNames = new string[table.colCount];
            table.memberTypes = new string[table.colCount];
            //要忽略前两行
            table.data = new string[table.rowCount, table.colCount];
            //第一行是描述性文字,第二行为成员变量名 第三行为列类型
            for (int c = 0; c < table.colCount; c++)
            {
                table.memberNames[c] = sheet.Cells[2, c + 1].Value.ToString();
                table.memberTypes[c] = sheet.Cells[3, c + 1].Value.ToString();
            }
            //Excel的行列都是从1开始的 帧几把难受
            //描述 名称 类型 占了三行
            //需要的数据从第四行开始
            for (int r = 4; r <= sheet.Dimension.End.Row; r++)
            {
                for (int c = 1; c <= sheet.Dimension.End.Column; c++)
                {
                    table.data[r - 4, c - 1] = sheet.Cells[r, c].Value.ToString();
                }
            }
            return table;
        }
    }

    /// <summary>
    /// 默认
    /// </summary>
    public class ExcelTable
    {
        public string tableName;
        public string[] memberNames;
        public string[] memberTypes;
        public int colCount;
        public int rowCount;
        public string[,] data;
    }
}
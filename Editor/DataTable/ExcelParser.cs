#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using UnityEngine;

namespace UnityToolkit.Editor
{
    public static class ExcelParser
    {
        public static Dictionary<string, string[][]> ReadExcel(string excelPath)
        {
            Dictionary<string, string[][]> stringsMap = new Dictionary<string, string[][]>();
            FileInfo fileInfo = new FileInfo(excelPath);
            //读取Excel文件
            ExcelPackage excelPackage = new ExcelPackage(fileInfo);
            //获取第一个sheet
            foreach (ExcelWorksheet sheet in excelPackage.Workbook.Worksheets)
            {
                if (!ReadSheet(sheet, out string[][] values)) continue;
                stringsMap.Add(sheet.Name, values);
            }

            return stringsMap;
        }

        private static bool ReadSheet(ExcelWorksheet worksheet, out string[][] values)
        {
            values = null;
            if (worksheet.Dimension is null)
            {
                Debug.LogWarning($"Excel解析失败: {worksheet.Name} 为空");
                return false;
            }


            int rowCount = worksheet.Dimension.Rows + 1;
            int colCount = worksheet.Dimension.Columns;
            // Debug.Log($"{worksheet.Name} 行数: {rowCount} 列数: {colCount}");
            values = new string[rowCount][];

            for (int row = 1; row <= rowCount; row++)
            {
                values[row - 1] = new string[colCount]; // 这里有可能是空列
                for (int col = 1; col <= colCount; col++)
                {
                    var value = worksheet.Cells[row, col].Value;
                    if (value is null)
                    {
                        value = "";
                    }

                    values[row - 1][col - 1] = value.ToString();
                }
            }

            return true;
        }


        public static Dictionary<string, string[][]> ReadCsv(string csvPath, string seq = ",")
        {
            Dictionary<string, string[][]> data = new Dictionary<string, string[][]>();

            using StreamReader reader = new StreamReader(csvPath);
            while (reader.ReadLine() is { } line)
            {
                string[] parts = line.Split(new[] { seq }, StringSplitOptions.None);
                if (parts.Length <= 0) continue;
                string key = parts[0];
                string[][] values = new string[1][];
                values[0] = new string[parts.Length - 1];
                Array.Copy(parts, 1, values[0], 0, parts.Length - 1);
                data[key] = values;
            }

            return data;
        }
    }
}

#endif
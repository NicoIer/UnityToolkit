using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Nico.Editor
{
    public static class TableImporter
    {
        private static readonly Dictionary<string, string> defineDict = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> tableCodeDict = new Dictionary<string, string>();

        private static AddressAblesUpdateConfig _config =
            Resources.Load<AddressAblesUpdateConfig>("AddressablesUpdateConfig");

        public static void ImportData(string excelPath)
        {
            var stringsMap = ExcelParser.ReadExcel(excelPath);
            List<string> tableNames = stringsMap.Keys.ToList();
            List<string[][]> tableValues = GetTableValues(stringsMap);
            List<IDataTable> tables = GetTables(tableNames, tableValues);
            //保存到Addressables下
            foreach (IDataTable table in tables)
            {
                // Debug.Log($"table:{table.GetType()}");
                if (table is not ScriptableObject scriptableObject)
                {
                    continue;
                }

                var folderPath = _config.dataTableAssetFolderPath;
                var savePath = Path.Combine(folderPath, $"{table.GetType().Name}.asset");
                //保存资源
                if (!File.Exists(savePath))
                {
                    Nico.FileUtil.Create(savePath);
                }

                AssetDatabase.CreateAsset(scriptableObject, savePath);
            }

            AssetDatabase.Refresh();
        }

        private static List<string[][]> GetTableValues(Dictionary<string, string[][]> stringsMap)
        {
            List<string[][]> tableValues = new List<string[][]>();

            foreach (var kvp in stringsMap)
            {
                // Debug.Log("kvp.Key:" + kvp.Key);
                var tableName = kvp.Key;
                var values = kvp.Value; //这是单个表格的全部数据 我们只需要 3列往后的数据
                if (values.Length < 3)
                {
                    Debug.LogError($"table data error:{tableName} at least 3 col");
                    continue;
                }

                // 1.获取定义列 
                var defineColSet = new HashSet<int>();
                for (int col = 0; col < values[0].Length; col++)
                {
                    var value = values[1][col];
                    if (!value.Contains(":")) continue;
                    // Debug.Log($"{value} at col: {col} is define col");
                    defineColSet.Add(col);
                }

                //逐行解析
                List<List<string>> result = new List<List<string>>();
                for (int row = 3; row < values.Length; row++)
                {
                    List<string> obj = new List<string>();
                    for (int col = 0; col < values[0].Length; col++)
                    {
                        //如果是定义列 则跳过
                        if (defineColSet.Contains(col))
                        {
                            // Debug.Log($"skip define col:{col}");
                            continue;
                        }

                        var value = values[row][col] ?? "";

                        obj.Add(value);
                    }

                    result.Add(obj);
                }

                tableValues.Add(result.Select(x => x.ToArray()).ToArray());
            }

            return tableValues;
        }

        private static List<IDataTable> GetTables(List<string> tableNames, List<string[][]> tableValues)
        {
            var tables = new List<IDataTable>();
            for (int i = 0; i < tableNames.Count; i++)
            {
                if (!CreateTable(out IDataTable table, tableNames[i], tableValues[i]))
                {
                    continue;
                }

                // Debug.Log("created: " + table.GetType());
                tables.Add(table);
            }

            return tables;
        }

        public static bool CreateTable(out IDataTable table, string tableName, string[][] dataValues)
        {
            Assembly assembly = null;
            try
            {
                assembly = Assembly.Load(GlobalConst.DATATABLE_FOLDER_NAME);
            }
            catch (FileNotFoundException e)
            {
                Debug.LogWarning($"DataTable.dll not found ,msg:{e}");
                table = null;
                return false;
            }

            Type tableType = assembly.GetType($"Nico.{tableName}DataTable");
            Type dataType = assembly.GetType($"Nico.{tableName}Data");

            if (!TableDataCreator.CreateTable(out table, tableType))
            {
                // Debug.LogError($"create table error:{tableName}");
                return false;
            }

            foreach (var dataValue in dataValues)
            {
                bool isEmpty = false;
                for (int i = 0; i < dataValue.Length; i++)
                {
                    if (string.IsNullOrEmpty(dataValue[i]))
                    {
                        isEmpty = true;
                        break;
                    }
                }

                if (isEmpty)
                {
                    continue;
                }

                if (!TableDataCreator.CreateData(out ITableData data, dataType, dataValue))
                {
                    // Debug.LogError($"create data error:{tableName}");
                    continue;
                }

                table.Add(data);
            }


            return true;
        }

        #region 生成代码

        public static void ImportExcel(string excelPath)
        {
            var stringsMap = ExcelParser.ReadExcel(excelPath);
            foreach (var kvp in stringsMap)
            {
                GenerateCode(kvp.Key, kvp.Value);
            }

            //生成代码
            foreach (var kvp in defineDict)
            {
                string defineName = kvp.Key;
                string defineCode = kvp.Value;
                Nico.FileUtil.Write($"{_config.dataTableScriptsPath}/{defineName}.cs", defineCode);
            }

            foreach (var kvp in tableCodeDict)
            {
                string tableName = kvp.Key;
                string tableCode = kvp.Value;
                Nico.FileUtil.Write($"{_config.dataTableScriptsPath}/{tableName}DataTable.cs", tableCode);
            }

            //生成程序集定义
            Nico.FileUtil.Write($"{_config.dataTableScriptsPath}/DataTable.asmdef",
                _config.tableDataConfig.TDataTableAssemblyDefineTemplate);
        }

        private static void GenerateCode(string tableName, string[][] values)
        {
            //第一行读表时 第一行为空 用于描述符
            //第二行为成员变量类型
            //第三行为成员变量名
            //有的列可能为空 跳过
            //有的列是用 XXX:class  XXX:enum   XXX:struct 标记的 代表这列是一个类型定义


            //row=2读类型
            FindFieldTypeAndDefines(values[1], out var fieldDict, out var defines);
            //row=3读名字

            //生成表格代码
            var tableCode = GenerateTableCode(tableName, values[2], fieldDict);
            tableCodeDict.Add(tableName, tableCode);

            //生成类型定义代码
            foreach (var kvp in defines)
            {
                if (!kvp.Key.Contains(":")) continue;
                string defineCode = GenerateDefineCode(kvp.Key, kvp.Value, values);
                if (defineCode == null) continue;
                string name = kvp.Key.Substring(0, kvp.Key.IndexOf(":", StringComparison.Ordinal));
                defineDict.Add(name, defineCode);
            }
        }

        //Define 从第3行就开始是其成员变量了
        private static string GenerateDefineCode(string defineName, int colIdx, string[][] values)
        {
            if (defineName.EndsWith(":class"))
            {
                var className = defineName.Substring(0, defineName.Length - ":class".Length);
                List<string> fieldNames = new List<string>();
                List<string> fieldTypes = new List<string>();
                for (int row = 2; row < values.Length; row++)
                {
                    var value = values[row][colIdx];
                    if (string.IsNullOrEmpty(value)) continue;
                    string[] pair = value.Split(" ");
                    if (pair.Length != 2)
                    {
                        Debug.LogError(
                            $"Define:{defineName} row:{row} col:{colIdx} value:{value} is not a valid field");
                        return null;
                    }

                    fieldTypes.Add(pair[0]);
                    fieldNames.Add(pair[1]);
                }

                return DefineCreator.CreateClass(className, fieldNames.ToArray(), fieldTypes.ToArray());
            }

            if (defineName.EndsWith(":struct"))
            {
                var structName = defineName.Substring(0, defineName.Length - ":struct".Length);
                List<string> fieldNames = new List<string>();
                List<string> fieldTypes = new List<string>();
                for (int row = 2; row < values.Length; row++)
                {
                    var value = values[row][colIdx];
                    if (string.IsNullOrEmpty(value)) continue;
                    string[] pair = value.Split(" ");
                    if (pair.Length != 2)
                    {
                        Debug.LogError(
                            $"Define:{defineName} row:{row} col:{colIdx} value:{value} is not a valid field");
                        return null;
                    }

                    fieldTypes.Add(pair[0]);
                    fieldNames.Add(pair[1]);
                }

                return DefineCreator.CreateStruct(structName, fieldNames.ToArray(), fieldTypes.ToArray());
            }

            if (defineName.EndsWith(":enum"))
            {
                var enumName = defineName.Substring(0, defineName.Length - ":enum".Length);
                List<string> enumValues = new List<string>();
                for (int row = 2; row < values.Length; row++)
                {
                    var value = values[row][colIdx];
                    if (string.IsNullOrEmpty(value)) continue;
                    enumValues.Add(value);
                }

                return DefineCreator.CreateEnum(enumName, enumValues.ToArray());
            }

            Debug.LogError($"Unknown define type :{defineName}");
            return null;
        }

        private static string GenerateTableCode(string tableName, string[] values, Dictionary<string, int> fieldDict)
        {
            var fieldNames = ReadFieldNames(values, fieldDict);
            var fieldTypes = fieldDict.Keys.ToArray();
            return DefineCreator.CreateDataTable(tableName, fieldNames, fieldTypes);
        }


        private static string[] ReadFieldNames(string[] values, Dictionary<string, int> fieldDictionary)
        {
            string[] fieldNames = new string[fieldDictionary.Count];
            int count = 0;
            foreach (var kvp in fieldDictionary)
            {
                fieldNames[count] = values[kvp.Value];
                ++count;
            }

            return fieldNames;
        }

        private static void FindFieldTypeAndDefines(string[] values, out Dictionary<string, int> fieldDict,
            out Dictionary<string, int> defineDict)
        {
            fieldDict = new Dictionary<string, int>();
            defineDict = new Dictionary<string, int>();
            for (int i = 0; i < values.Length; i++)
            {
                var value = values[i];
                if (!value.Contains(":"))
                {
                    fieldDict.Add(value, i);
                }
                else
                {
                    defineDict.Add(value, i);
                }
            }
        }

        #endregion
    }
}
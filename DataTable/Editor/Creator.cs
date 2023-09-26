using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UnityToolkit.Editor
{
    public static class TableDataCreator
    {
        internal static bool CreateData(out ITableData tableData, Type type, string[] values)
        {
            tableData = null;
            if (type == null)
            {
                Debug.LogError("CreateData: type is null");
                return false;
            }

            tableData = Activator.CreateInstance(type) as ITableData;
            if (tableData == null)
            {
                Debug.LogError("CreateData: tableData is null");
                return false;
            }

            return tableData.Parse(values);
        }

        internal static bool CreateTable(out IDataTable table, Type type)
        {
            table = null;
            if (type == null)
            {
                Debug.LogError("CreateTable: type is null");
                return false;
            }

            table = ScriptableObject.CreateInstance(type) as IDataTable;
            if (table == null)
            {
                Debug.LogError("CreateTable: table is null");
                return false;
            }

            return true;
        }
    }


    // CodeGenerator
    public static class DefineCreator
    {
        public static string CreateEnum(TableDataConfig _config, string enumName, IEnumerable<string> enumValues)
        {
            return CreateEnum(_config.TEnumTemplate, enumName, enumValues);
        }

        public static string CreateClass(TableDataConfig _config, string className, string[] fieldNames,
            string[] fieldTypes)
        {
            return CreateClass(_config.TClassTemplate, className, fieldNames, fieldTypes);
        }

        public static string CreateStruct(TableDataConfig _config, string structName, string[] fieldNames,
            string[] fieldTypes)
        {
            return CreateStruct(_config.TStructTemplate, structName, fieldNames, fieldTypes);
        }

        public static string CreateDataTable(TableDataConfig _config, string tableName, string[] fieldNames,
            string[] fieldTypes)
        {
            return CreateDataTable(_config.DataTableTemplate, tableName, fieldNames, fieldTypes);
        }

        public static string CreateEnum(string template, string enumName, IEnumerable<string> enumValues)
        {
            string code = template;

            //替换表名
            code = code.Replace("EnumNameTemplate", $"{enumName}");
            StringBuilder sb = new StringBuilder();
            //生成成员变量定义
            foreach (string enumValue in enumValues)
            {
                sb.AppendLine($"\t\t{enumValue},");
            }

            //替换成员变量
            code = code.Replace("//fieldsTemplate", sb.ToString());
            return code;
        }

        public static string CreateClass(string template, string className, string[] fieldNames, string[] fieldTypes)
        {
            string code = template;
            //替换表名
            code = code.Replace("ClassTemplateName", $"{className}");
            //生成成员变量定义
            string fields = CreateFields(fieldNames, fieldTypes);
            //替换成员变量
            code = code.Replace("//fieldsTemplate", fields);

            return code;
        }

        public static string CreateStruct(string template, string structName, string[] fieldNames, string[] fieldTypes)
        {
            string code = template;

            //替换表名
            code = code.Replace("StructTemplateName", $"{structName}");

            //生成成员变量定义
            string fields = CreateFields(fieldNames, fieldTypes);
            //替换成员变量
            code = code.Replace("//fieldsTemplate", fields);

            return code;
        }

        public static string CreateDataTable(string template, string tableName, string[] fieldNames,
            string[] fieldTypes)
        {
            string code = template;

            //替换表名
            code = code.Replace("DataTableNameTemplate", $"{tableName}DataTable");

            //替换数据名
            code = code.Replace("TableDataNameTemplate", $"{tableName}Data");

            //生成成员变量定义
            string fields = CreateFields(fieldNames, fieldTypes);

            //替换成员变量
            code = code.Replace("//fieldsTemplate", fields);

            //生成解析函数定义
            string parsers = CreateParsers(fieldNames, fieldTypes);
            //替换解析函数
            code = code.Replace("//parseFieldsTemplate", parsers);


            return code;
        }

        internal static string CreateParsers(string[] fieldNames, string[] fieldTypes)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < fieldNames.Length; i++)
            {
                var fieldName = fieldNames[i];
                var fieldType = fieldTypes[i];
                //这里 i+1 是因为 Field第一个是id 而 id 是不需要解析的
                sb.AppendLine(CreateParser(fieldName, fieldType, i));
                sb.AppendLine();
            }

            return sb.ToString();
        }

        internal static string CreateParser(string fieldName, string fieldType, int idx)
        {
            // if (fieldType.EndsWith(":enum"))
            // {
            //     fieldType = fieldType.Replace(":enum", "");
            // }
            // else if (fieldType.EndsWith(":struct"))
            // {
            //     fieldType = fieldType.Replace(":struct", "");
            // }
            // else if (fieldType.EndsWith(":class"))
            // {
            //     fieldType = fieldType.Replace(":class", "");
            // }

            return
                $"\t\t\tif(!Nico.Editor.ParserManager.Parse<string,{fieldType}>(values[{idx}], out {fieldName})) return false;";
        }

        internal static string CreateField(string fieldName, string fieldType)
        {
            return $"\t\tpublic {fieldType} {fieldName};";
        }

        internal static string CreateFields(string[] fieldNames, string[] fieldTypes)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < fieldNames.Length; i++)
            {
                var fieldName = fieldNames[i];
                var fieldType = fieldTypes[i];
                sb.AppendLine(CreateField(fieldName, fieldType));
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
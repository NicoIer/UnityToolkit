using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
namespace Nico.Editor
{
    /// <summary>
    /// 代码生成器
    /// </summary>
    public static class CodeGenerator
    {
        //TODO 后续使用配置文件替代 读取配置文件中的模板 
        public const string TempContainerPath = "Assets/Plugins/Nico/Template/Container.ct";
        public const string TempDataPath = "Assets/Plugins/Nico/Template/Data.ct";
        public const string TempEnumPath = "Assets/Plugins/Nico/Template/Enum.ct";

        public static void Create(ExcelTable excelTable, string savePath)
        {
            //首先检测文件夹是否存在
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            string dataName = excelTable.tableName;
            var dataCodeTmp = File.ReadAllText(TempDataPath);
            GenerateDataClass(excelTable, dataCodeTmp, savePath);


            var containerCodeTmp = File.ReadAllText(TempContainerPath);
            GenerateContainerClass(dataName, containerCodeTmp, savePath);
        }

        public static void GenerateContainerClass(string dataName, string tmpStr, string savePath)
        {
            string containerName = $"{dataName}Container";
            string codeText = tmpStr.Replace("{dataName}", dataName)
                .Replace("{containerName}", containerName);
            if (File.Exists($"{savePath}{containerName}.cs"))
            {
                Debug.Log($"{savePath}{containerName}.cs文件已存在,是否覆盖?");
                if (!EditorUtility.DisplayDialog("提示", $"{savePath}{containerName}.cs文件已存在,是否覆盖?", "是", "否"))
                {
                    return;
                }
            }

            File.WriteAllText($"{savePath}{containerName}.cs", codeText);
        }

        public static void GenerateDataClass(ExcelTable excelTable, string tmpStr, string savePath)
        {
            string dataName = excelTable.tableName;
            var memberName = excelTable.memberNames;
            var memberType = excelTable.memberTypes;
            System.Diagnostics.Debug.Assert(memberType.Length == memberName.Length, "成员变量名和类型数量不一致");
            string codeText = tmpStr;
            //替换类名字符区域
            codeText = codeText.Replace("{dataName}", dataName);
            //替换成员变量字符区域
            string memberContent = "";

            //枚举类型的特殊处理
            List<string> enumNames = new List<string>();
            List<int> enumColumns = new List<int>();

            for (int i = 0; i < memberName.Length; i++)
            {
                if (memberName[i] == "id") //id字段不生成 只读取
                    continue;
                if (memberType[i].EndsWith("Enum")) //如果是枚举类型 就判断程序集中是否存在该枚举类型 如果不存在就尝试生成对应枚举类型的代码
                {
                    enumColumns.Add(i); //记录枚举类型的列
                    enumNames.Add(memberType[i]); //记录枚举类型的名称
                    memberContent += $"\t  \tpublic {memberType[i]} {memberName[i]};\n";
                    continue;
                }

                memberContent += $"\t  \tpublic {memberType[i]} {memberName[i]};\n";
            }

            //TODO 这里的替换方式有待优化 
            codeText = codeText.Replace("{filedContent}", memberContent);

            //处理枚举类型 为每个枚举类型生成一个枚举类
            if (enumNames.Count != 0)
            {
                GenerateEnumClass(enumNames, enumColumns, excelTable,savePath);
            }


            if (File.Exists($"{savePath}{dataName}.cs"))
            {
                Debug.Log($"{savePath}{dataName}.cs文件已存在,是否覆盖?");
                if (!EditorUtility.DisplayDialog("提示", $"{savePath}{dataName}.cs文件已存在,是否覆盖?", "是", "否"))
                {
                    return;
                }
            }

            File.WriteAllText($"{savePath}{dataName}.cs", codeText);
        }

        public static void GenerateEnumClass(List<string> enumNames, List<int> enumColumns, ExcelTable excelTable,string savePath)
        {
            //首先检测文件夹是否存在
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            for (int i = 0; i < enumNames.Count; i++)
            {
                string enumName = enumNames[i];
                int enumColumn = enumColumns[i];
                
                //获取指定枚举列的所有枚举值 并且去重复
                HashSet<string> enumValueSet = new HashSet<string>();
                for (int r = 0; r < excelTable.rowCount; r++)
                {
                    var enumValue = excelTable.data[r, enumColumn];
                    enumValueSet.Add(enumValue);
                }


                string codeText = GetEnumCodeText(enumName, enumValueSet.ToArray());
                if (codeText == null)
                {
                    continue;
                }

                if (File.Exists($"{savePath}{enumName}.cs"))
                {
                    Debug.Log($"{savePath}{enumName}.cs文件已存在,是否覆盖?");
                    if (!EditorUtility.DisplayDialog("提示", $"{savePath}{enumName}.cs文件已存在,是否覆盖?", "是", "否"))
                    {
                        return;
                    }
                }

                File.WriteAllText($"{savePath}{enumName}.cs", codeText);
            }
        }

        public static string GetEnumCodeText(string enumName, string[] enumValues)
        {
            //判断是否存在该枚举类型
            Type type = Type.GetType($"{typeof(IMetaData).Namespace}.{enumName}");
            if (type != null)
            {
                //存在则直接返回
                return null;
            }

            string tmpStr = File.ReadAllText(TempEnumPath);
            string codeText = tmpStr;
            string enumContent = "";
            foreach (var value in enumValues)
            {
                enumContent += $"\t \t{value},\n";
            }

            codeText = codeText.Replace("{enumName}", enumName)
                .Replace("{enumContent}", enumContent);

            return codeText;
        }
    }
}
#endif
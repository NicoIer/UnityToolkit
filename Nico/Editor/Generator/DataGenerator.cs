using System;
using System.Linq;
using System.Reflection;
using Nico.Data;
using UnityEngine;

namespace Nico.Editor
{
    public static class DataGenerator
    {
        public static IMetaDataContainer Create(ExcelTable table, Type metaDataType,
            Type metaContainerType, Assembly assembly)
        {
            //获取表格的成员名和成员类型
            var memberNames = table.memberNames;
            var memberTypes = table.memberTypes;
            //获取metaData的信息
            var metaDataFields = metaDataType.GetFields();
            var metaDataProperties = metaDataType.GetProperties();
            //验证表格的成员名和成员类型是否和metaData的成员变量和属性一致

            #region 安全检测

            for (int i = 0; i < memberNames.Length; i++)
            {
                var memberName = memberNames[i];
                var memberType = memberTypes[i];
                //断言 memberName必须是metaData的成员变量或者属性
                if (metaDataFields.All(f => f.Name != memberName) &&
                    metaDataProperties.All(p => p.Name != memberName))
                {
                    Debug.LogError($"memberName:{memberName}不是metaData的成员变量或者属性");
                    return null;
                }

                //断言 memberType必须是metaData的成员变量或者属性的类型
                if (metaDataFields.Any(f => f.Name == memberName && f.FieldType.Name != memberType) &&
                    metaDataProperties.Any(p => p.Name == memberName && p.PropertyType.Name != memberType))
                {
                    Debug.LogError($"memberType:{memberType}不是metaData的成员变量或者属性的类型");
                    return null;
                }
            }

            //断言 metaDataType必须有id属性
            var idProperty = metaDataType.GetProperty("id");
            if (idProperty == null)
            {
                Debug.LogError($"metaDataType:{metaDataType}必须有id属性");
                return null;
            }

            //断言 memberNames的长度必须和memberTypes的长度一致
            if (memberNames.Length != memberTypes.Length)
            {
                Debug.LogError($"memberNames的长度必须和memberTypes的长度一致");
                return null;
            }

            #endregion

            //创建一个MetaDataContainer TODO 这里暂时只支持ScriptableObject
            IMetaDataContainer container = ScriptableObject.CreateInstance(metaContainerType) as IMetaDataContainer;
            //获取metaData的成员变量信息

            for (int r = 0; r < table.rowCount; r++)
            {
                //创建一个MetaData对象
                IMetaData metaData = Activator.CreateInstance(metaDataType) as IMetaData;
                for (int c = 0; c < table.colCount; c++)
                {
                    //当前行列的数据
                    var value = table.data[r, c];
                    //当前列的成员名 和 成员类型
                    var memberName = memberNames[c];
                    var memberType = memberTypes[c];
                    //将对应行的数据转换成对应的类型
                    var nameSpace = typeof(IMetaData).Namespace;
                    var convertedValue = ConvertTo(memberType, value, nameSpace, assembly);
                    //设置metaData的成员变量的值
                    if (metaDataFields.Any(f => f.Name == memberName))
                    {
                        var field = metaDataFields.First(f => f.Name == memberName);
                        field.SetValue(metaData, convertedValue);
                    }
                    else if (metaDataProperties.Any(p => p.Name == memberName))
                    {
                        var property = metaDataProperties.First(p => p.Name == memberName);
                        property.SetValue(metaData, convertedValue);
                    }
                }

                container.AddData(metaData);
            }

            return container;
        }

        public static object ConvertTo(string typeStr, string value, string nameSapce, Assembly assembly)
        {
            var v = BaseConvertTo(typeStr, value);
            if (v is not null)
            {
                return v;
            }

            if (typeStr.EndsWith("Enum"))
            {
                string targetValue = value;
                if (value.Contains("="))
                {
                    string[] kv = value.Split("=");
                    targetValue = kv[0];
                }

                return Enum.Parse(assembly.GetType($"{nameSapce}.{typeStr}"), targetValue);
            }

            return null;
        }

        /// <summary>
        /// 内置基础类型转换 
        /// </summary>
        /// <param name="typeStr"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object BaseConvertTo(string typeStr, string value)
        {
            switch (typeStr)
            {
                case "int": return int.Parse(value);
                case "string": return value;
                case "float": return float.Parse(value);
                case "bool": return bool.Parse(value);
                case "int[]": return StringToObjArray<int>(value, "int");
            }

            return null;
        }

        /// <summary>
        /// 通过分隔符转换
        /// </summary>
        /// <param name="value"></param>
        /// <param name="typeStr"></param>
        /// <param name="sep"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] StringToObjArray<T>(string value, string typeStr, string sep = ",")
        {
            string[] strings = value.Split(sep);
            T[] objs = new T[strings.Length];
            for (int i = 0; i != strings.Length; ++i)
            {
                var str = strings[i];
                objs[i] = (T)BaseConvertTo(typeStr, str);
            }

            return objs;
        }
    }
}
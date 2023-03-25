using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
namespace Nico.Util
{
    /// <summary>
    /// 代码生成器
    /// </summary>
    [CreateAssetMenu(fileName = "CodeGenerate", menuName = "Test/CodeGenerate", order = 0)]
    public class CodeGenerate : ScriptableObject
    {
        public string directorPath = "Excels/";
        public string csSaveDirePath = "Assets/Test/";
        public string soSaveDirPath = "Assets/Test/SO/";
        public string tempContainerPath = "Assets/Plugins/Nico/Template/Container.nico";
        public string tempDataPath = "Assets/Plugins/Nico/Template/Data.nico";
        public string dataPath = "Assets/Test/";

        public void Generate()
        {
            Debug.Log("生成代码~~~");
            string dataName = "TestMetaData";
            string containerName = "TestMetaDataContainer";


            #region 创建数据类

            //读取模板字符串
            var tempDataStr = File.ReadAllText(tempDataPath);
            List<Tuple<string, string>> testData = new List<Tuple<string, string>>();
            var testField1 = new Tuple<string, string>("int", "testField1");
            testData.Add(testField1);
            var testField2 = new Tuple<string, string>("string", "name");
            testData.Add(testField2);

            //替换其中的变量字符区域
            string filedContent = "";
            foreach (var tuple in testData)
            {
                filedContent += $"\t \tpublic {tuple.Item1} {tuple.Item2};\n";
            }

            var dataStr = tempDataStr.Replace("{dataName}", dataName).Replace("{filedContent}", filedContent);
            //写入文件
            File.WriteAllText(csSaveDirePath + dataName + ".cs", dataStr);

            #endregion

            #region 创建数据容器类

            //读取模板字符串
            var tempContainerStr = File.ReadAllText(tempContainerPath);
            //替换其中的变量
            var containerStr = tempContainerStr.Replace("{dataName}", dataName)
                .Replace("{containerName}", containerName);
            //写入文件
            File.WriteAllText(csSaveDirePath + containerName + ".cs", containerStr);

            #endregion

            AssetDatabase.Refresh(); //刷新资源
        }

        public void Test()
        {
            //当前测试 数据读取 功能
            AssetDatabase.Refresh();
            //获取当前程序集中所有IMetaDataContainer的实现类
            var types = ReflectUtil.GetTypesByInterface<IMetaDataContainer>(AppDomain.CurrentDomain);
            //当前默认 type 都继承自 ScriptableObject
            Debug.Log($"当前程序集中所有IMetaDataContainer的实现类:{string.Join(",", types)}");
            //查询指定文件夹下的所有表格
            var results = AssetUtil.GetScriptableObject<IMetaDataContainer>(dataPath);
            Debug.Log($"目录:{dataPath}下所有Container:{string.Join(",", results)}");
        }
    }

    [CustomEditor(typeof(CodeGenerate))]
    public class CodeGenerateEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var deliveryManager = target as CodeGenerate;
            if (GUILayout.Button("生成代码"))
            {
                if (deliveryManager != null) deliveryManager.Generate();
            }

            if (GUILayout.Button("测试"))
            {
                if (deliveryManager != null) deliveryManager.Test();
            }
        }
    }
}
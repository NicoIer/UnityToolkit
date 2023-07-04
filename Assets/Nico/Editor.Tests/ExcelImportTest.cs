#if UNITY_EDITOR


using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nico.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Nico.Tests
{
    [TestFixture]
    public class ExcelImportTest
    {
        [Test]
        public void Import()
        {
            //在工程目录下选择excel文件
            //拿到工程目录
            // string projectPath = Application.dataPath; //从Assets 回退到工程目录
            // projectPath = projectPath.Substring(0, projectPath.Length - 6);
            // string excelPath = EditorUtility.OpenFilePanelWithFilters("select excel", projectPath, new string[] { "Excel Files", "csv,xlsx,xls" });
            
            string excelPath = "Excel/Test.xlsx";
            var stringsMap = ExcelParser.ReadExcel(excelPath);
            Assert.AreEqual(true, stringsMap.Count == 1);
            Assert.AreEqual(true, stringsMap.ContainsKey("Sheet1"));
            // foreach (var kvp in stringsMap)
            // {
            //     string sheetName = kvp.Key;
            //     string[][] values = kvp.Value;
            //     Debug.Log($"sheetName:{sheetName}");
            // }
            //
            // Debug.Log($"excelPath:{excelPath}");
        }

        [Test]
        public void ImportTable()
        {
            string excelPath = "Excel/Test.xlsx";
            TableImporter.ImportExcel(excelPath);
        }

        [Test]
        public void ImportData()
        {
            string excelPath = "Excel/Test.xlsx";
            TableImporter.ImportData(excelPath);
        }

        // [Test]
        // public void GetTypeTest()
        // {
        //     Assembly assembly = Assembly.Load("DataTable");
        //     Type type = assembly.GetType("Nico.Sheet1DataTable");
        //     Assert.AreEqual(true, type != null);
        //     Assert.AreEqual(true, type == typeof(Sheet1DataTable));
        // }

        // [Test]
        // public void ParseExist()
        // {
        //     Assert.AreEqual(true,ParserManager.Contains(typeof(string),typeof(TestClass1)));
        //     Assert.AreEqual(true,ParserManager.Contains(typeof(string),typeof(TestEnum1)));
        //     Assert.AreEqual(true,ParserManager.Contains(typeof(string),typeof(TestStruct1)));
        // }

        // [Test]
        // public void ParseOk()
        // {
        //     ParserManager.Parse<string, TestClass1>("id = 0;name = 123;pos = 1,233",out TestClass1 class1);
        //     Assert.AreEqual(0,class1.id);
        //     Assert.AreEqual("123",class1.name);
        //     Assert.AreEqual(new Vector2Int(1,233),class1.pos);
        // }
    }
}
#endif
using System.IO;
using System.Threading.Tasks;
using Nico.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Nico.Tests
{
    [TestFixture]
    public class DefineCreateTest
    {
        private const string _folderPath = "Assets/Plugins/Nico/Editor/Addressables/";

        private readonly AddressAblesUpdateConfig config =
            AssetDatabase.LoadAssetAtPath<AddressAblesUpdateConfig>(_folderPath + "AddressablesUpdateConfig.asset");

        [Test]
        public void CreateClass()
        {
            string className = "TestClass";
            string code = DefineCreator.CreateClass(
                config.tableDataConfig.TClassTemplate,
                className,
                new string[] { "id", "pos" },
                new string[] { "int", nameof(Vector2Int) }
            );
            var scriptPath = $"{config.dataTableScriptsPath}/{className}.cs";
            Nico.FileUtil.Write(scriptPath, code);
            AssetDatabase.Refresh();
        }

        [Test]
        public void CreateEnum()
        {
            string enumName = "TestEnum";
            string code = DefineCreator.CreateEnum(
                config.tableDataConfig.TEnumTemplate, enumName,
                new string[]
                {
                    "X1", "X2", "X3", "X4"
                });
            var scriptPath = $"{config.dataTableScriptsPath}/{enumName}.cs";
            Nico.FileUtil.Write(scriptPath, code);
            AssetDatabase.Refresh();
        }


        [Test]
        public void CreateTable()
        {
            string tableName = "Test";
            //加载 AddressablesUpdateConfig.asset
            string tableCode = DefineCreator.CreateDataTable(
                config.tableDataConfig.DataTableTemplate,
                tableName,
                new string[] { "name", "pos" },
                new string[] { "string", "Vector2Int" }
            );
            var scriptPath = $"{config.dataTableScriptsPath}/{tableName}.cs";
            Nico.FileUtil.Write(scriptPath, tableCode);
            AssetDatabase.Refresh();
        }

        [Test]
        public void AssemblyDefineCreateTest()
        {
            var definePath = $"{config.dataTableScriptsPath}/DataTable.asmdef";
            Nico.FileUtil.ReplaceContent(config.tableDataConfig.TDataTableAssemblyDefineTemplate, definePath);
            AssetDatabase.Refresh();
        }
    }
}
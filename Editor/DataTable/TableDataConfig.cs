#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityToolkit.Editor
{
    [CreateAssetMenu(fileName = "TableDataConfig", menuName = "Toolkit/TableDataConfig", order = 0)]
    public class TableDataConfig : ScriptableObject
    {
        private TextAsset tDataTableTemplate;
        private TextAsset tEnumTemplate;
        private TextAsset tClassTemplate;
        private TextAsset tStructTemplate;
        [Multiline]
        public string assetSavePath = "Assets/DataTable/";
        [Multiline]
        public string codeSavePath = "Assets/DataTable/";
        private string _selfPath = "";

        private string selfPath
        {
            get
            {
                if (_selfPath == "")
                {
                    _selfPath = Path.GetDirectoryName(
                        AssetDatabase.GetAssetPath(
                            MonoScript.FromScriptableObject(this)));
                }

                return _selfPath;
            }
        }

        public string DataTableTemplate
        {
            get
            {
                if (tDataTableTemplate == null)
                {
                    tDataTableTemplate = AssetDatabase.LoadAssetAtPath<TextAsset>(
                        $"{selfPath}/DataTableTemplate.txt");
                }

                return tDataTableTemplate.text;
            }
        }

        public string TEnumTemplate
        {
            get
            {
                if (tEnumTemplate == null)
                {
                    tEnumTemplate = AssetDatabase.LoadAssetAtPath<TextAsset>(
                        $"{selfPath}/EnumTemplate.txt");
                }

                return tEnumTemplate.text;
            }
        }

        public string TClassTemplate
        {
            get
            {
                if (tClassTemplate == null)
                {
                    tClassTemplate = AssetDatabase.LoadAssetAtPath<TextAsset>(
                        $"{selfPath}/ClassTemplate.txt");
                }

                return tClassTemplate.text;
            }
        }

        public string TStructTemplate
        {
            get
            {
                if (tStructTemplate == null)
                {
                    tStructTemplate = AssetDatabase.LoadAssetAtPath<TextAsset>(
                        $"{selfPath}/StructTemplate.txt");
                }

                return tStructTemplate.text;
            }
        }
    }
}
#endif
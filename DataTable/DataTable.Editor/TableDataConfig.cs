#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Nico.Editor
{
    [CreateAssetMenu(fileName = "TableDataConfig", menuName = "Config/TableDataConfig", order = 0)]
    public class TableDataConfig : ScriptableObject
    {
        [SerializeField] private TextAsset tDataTableTemplate;
        [SerializeField] private TextAsset tEnumTemplate;
        [SerializeField] private TextAsset tClassTemplate;
        [SerializeField] private TextAsset tStructTemplate;
        [SerializeField] private TextAsset tDataTableAssemblyDefineTemplate;
        [SerializeField] public string assetSavePath = "Assets/DataTable/";
        [SerializeField] public string codeSavePath = "Assets/DataTable/";
        
        public string DataTableTemplate
        {
            get
            {
                if (tDataTableTemplate == null)
                {
                    tDataTableTemplate = AssetDatabase.LoadAssetAtPath<TextAsset>(
                        $"{NicoEditorUtil.pluginPath}/DataTable/DataTable.Editor/DataTableTemplate.txt");
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
                        $"{NicoEditorUtil.pluginPath}/DataTable/DataTable.Editor/EnumTemplate.txt");
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
                        $"{NicoEditorUtil.pluginPath}/DataTable/DataTable.Editor/ClassTemplate.txt");
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
                        $"{NicoEditorUtil.pluginPath}/DataTable/DataTable.Editor/StructTemplate.txt");
                }
                return tStructTemplate.text;
            }
        }

        public string TDataTableAssemblyDefineTemplate
        {
            get
            {
                if (tDataTableAssemblyDefineTemplate == null)
                {
                    tDataTableAssemblyDefineTemplate = AssetDatabase.LoadAssetAtPath<TextAsset>(
                        $"{NicoEditorUtil.pluginPath}/DataTable/DataTable.Editor/DataTable.asmdef.txt");
                }
                return tDataTableAssemblyDefineTemplate.text;
            }
        }
    }
}
#endif

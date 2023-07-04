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

        public string DataTableTemplate => tDataTableTemplate.text;
        public string TEnumTemplate => tEnumTemplate.text;
        public string TClassTemplate => tClassTemplate.text;
        public string TStructTemplate => tStructTemplate.text;

        public string TDataTableAssemblyDefineTemplate => tDataTableAssemblyDefineTemplate.text;
    }
}
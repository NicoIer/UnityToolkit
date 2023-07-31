#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Nico.Editor
{
    [CreateAssetMenu(fileName = "AddressablesUpdateConfig", menuName = "Config/AddressablesUpdateConfig", order = 0)]
    public class AddressAblesUpdateConfig : ScriptableObject
    {
        private static readonly Dictionary<string, string> DefaultFolderToLabel = new Dictionary<string, string>()
        {
            // { GlobalConst.DATATABLE_FOLDER_NAME, GlobalConst.DATATABLE_LABEL },
            // { GlobalConst.POOL_OBJECT_PREFAB_FOLDER_NAME, GlobalConst.POOL_OBJECT_PREFAB_LABEL },
            { AddressablesConst.HOTUPDATE_DLL_FOLDER_NAME, AddressablesConst.HOTUPDATE_DLL_LABEL },
            { AddressablesConst.AOT_DLL_FOLDER_NAME, AddressablesConst.AOT_DLL_LABEL }
        };

        [HideInInspector] public string resourcesPath =
            "Assets/AddressablesResources";
        

        public AddressableAssetSettings settings => AddressableAssetSettingsDefaultObject.GetSettings(true);
        public SerializableDictionary<string, string> folderToLabel = new();
        [HideInInspector] public SerializableDictionary<string, AddressableAssetGroup> groups = new();
        [HideInInspector, SerializeField] public List<string> labels = new();

        [HideInInspector] public AssetLabelReference defaultHotUpdateLabel;
        public void OnValidate()
        {
            groups.Clear();
            foreach (var assetGroup in settings.groups)
            {
                groups[assetGroup.Name] = assetGroup;
            }

            //默认值必须弄上去
            foreach (var kvp in DefaultFolderToLabel)
            {
                folderToLabel[kvp.Key] = kvp.Value;
            }

            foreach (var label in folderToLabel.Values)
            {
                settings.AddLabel(label);
            }

            labels.Clear();
            foreach (var label in settings.GetLabels())
            {
                labels.Add(label);
            }
        }
    }
}
#endif
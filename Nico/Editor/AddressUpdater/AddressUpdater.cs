using System.IO;
using Nico.Data;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

#if UNITY_EDITOR
namespace Nico.Editor
{
    public class AddressUpdater : EditorWindow
    {
        [SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;

        [MenuItem("Tools/AddressUpdater")]
        public static void ShowExample()
        {
            AddressUpdater wnd = GetWindow<AddressUpdater>();
            wnd.titleContent = new GUIContent("AddressUpdater");
        }

        private TextField addressableFolderText;
        private TextField dataFolderText;

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;
            VisualElement labelFromUxml = m_VisualTreeAsset.Instantiate();
            root.Add(labelFromUxml);
            VisualElement topVisual = root.Query<VisualElement>("Top").First();

            #region Button

            Button dataUpdateBtn = topVisual.Query<Button>("dataUpdateBtn").First();
            dataUpdateBtn.clickable.clicked += OnUpdateBtnClick;

            #endregion

            #region TextField

            addressableFolderText = topVisual.Query<TextField>("addressableFolderText").First();
            dataFolderText = topVisual.Query<TextField>("dataFolderText").First();

            #endregion
        }

        private void OnUpdateBtnClick()
        {
            //向AddressableAssetSettings中添加一个Group
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            string addressableFolder = addressableFolderText.value; //资源根文件夹 
            string nicoDataGroupName = dataFolderText.value; //数据文件件
            var nicoDataPath = $"{addressableFolder}/{nicoDataGroupName}";
            //检测文件夹是否存在
            if (!Directory.Exists(nicoDataPath))
            {
                Debug.LogError($"{nicoDataPath}文件夹不存在");
                return;
            }

            AddressableAssetGroup group;
            if (!settings.FindGroup(nicoDataGroupName))
            {
                group = settings.CreateGroup(nicoDataGroupName, false, false, false, null,
                    typeof(BundledAssetGroupSchema));
            }
            else
            {
                group = settings.FindGroup("Nico-Data");
            }

            //清空Group中的所有资源
            Addressables.ClearDependencyCacheAsync(nicoDataGroupName);
            //获取nicoDataPath文件夹下的所有资源 的 ScriptableObject
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { nicoDataPath });
            //拿到其中继承自Container或者IMetaDataContainer的资源
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ScriptableObject obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (obj is Container || obj is IMetaDataContainer)
                {
                    //将资源添加到Group中
                    AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);
                    //设置资源的Path
                    entry.SetAddress("Nico-Data/" + obj.GetType().Name);
                }
            }
        }
    }
}
#endif
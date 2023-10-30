using UnityEngine;

namespace UnityToolkit
{
    public interface IUIPanel
    {
        internal GameObject GetGameObject();
        internal RectTransform GetRectTransform();
        internal void SetState(UIPanelState state);
        internal int GetSortingOrder();
        public void OnLoaded(); //加载面板时调用
        public void OnOpened(); //打开面板时调用
        public void OnClosed(); //关闭面板时调用
        public void OnDispose(); //销毁面板时调用
    }

    /// <summary>
    /// UIBind 使用这个特性标记的字段在编辑器模式下,点击AutoBind按钮会自动绑定到对应的子物体
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class UIBindAttribute : System.Attribute
    {
        //不填参数则默认使用变量名
        public string path;
    }

    /// <summary>
    /// UI面板状态
    /// </summary>
    public enum UIPanelState
    {
        None,
        Loaded, // 已加载
        Opening, // 打开中
        Opened, // 已打开
        Closing, // 关闭中
        Closed, //  已关闭
        Disposing // 销毁中
    }


    /// <summary>
    /// UI面板 任意一个UI面板同时只能存在一个实例
    /// Panel是唯一的
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [DisallowMultipleComponent]
    public abstract class UIPanel : MonoBehaviour, IUIPanel
    {
        public Canvas canvas
        {
            get
            {
                if (_panelCanvas == null)
                {
                    _panelCanvas = GetComponent<Canvas>();
                }

                return _panelCanvas;
            }
        }

        private Canvas _panelCanvas; //用于显示面板的Canvas
#if ODIN_INSPECTOR
        [field: SerializeField, Sirenix.OdinInspector.ReadOnly]
#else
        [field: SerializeField]
#endif

        public UIPanelState state { get; internal set; } = UIPanelState.None;

        public int sortingOrder;

        GameObject IUIPanel.GetGameObject()
        {
            return gameObject;
        }

        RectTransform IUIPanel.GetRectTransform()
        {
            return transform as RectTransform;
        }

        void IUIPanel.SetState(UIPanelState state)
        {
            this.state = state;
        }

        int IUIPanel.GetSortingOrder()
        {
            return sortingOrder;
        }

        public virtual void OnLoaded()
        {
            canvas.sortingOrder = sortingOrder;
        }

        public virtual void OnOpened()
        {
            gameObject.SetActive(true);
        }

        public virtual void OnClosed()
        {
            gameObject.SetActive(false);
        }

        public virtual void OnDispose()
        {
            Destroy(gameObject);
        }

        protected void CloseSelf()
        {
            UIRoot.Singleton.ClosePanel(GetType());
        }


        public bool IsOpened()
        {
            return state == UIPanelState.Opened;
        }

        public bool IsClosed()
        {
            return state == UIPanelState.Closed;
        }


#if UNITY_EDITOR

        private void Reset()
        {
            sortingOrder = canvas.sortingOrder;
            canvas.overrideSorting = true;
        }

        private void OnValidate()
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = sortingOrder;
        }

// TODO
        // [UnityEditor.MenuItem("GameObject/UI/UIPanel", false, 0)]
        // private static void CreateUIPanel()
        // {
        //     //从场景中找UIRoot的UIDatabase的路径
        //     string uiDatabasePath = null;
        //     var uiRoot = FindObjectOfType<UIRoot>();
        //     if (uiRoot == null)
        //     {
        //         //弹出提示 先创建UIRoot
        //         UnityEditor.EditorUtility.DisplayDialog("提示", "请先创建UIRoot", "确定");
        //         return;
        //     }
        //
        //     if (uiRoot.UIDatabase == null)
        //     {
        //         //弹出提示 先给UIRoot创建UIDatabase
        //         UnityEditor.EditorUtility.DisplayDialog("提示", "请先给UIRoot创建UIDatabase", "确定");
        //         return;
        //     }
        //     uiDatabasePath = UnityEditor.AssetDatabase.GetAssetPath(uiRoot.UIDatabase);
        //     //一个模板C#文件  XXXPanel.cs
        //     string templateFilePath = UnityEditor.AssetDatabase.GUIDToAssetPath("b0b4b0b0b0b0b0b0b0b0b0b0b0b0b0b");
        //     //一个模板预制体文件 XXXPanel.prefab
        //     string templatePrefabPath = UnityEditor.AssetDatabase.GUIDToAssetPath("b0b4b0b0b0b0b0b0b0b0b0b0b0b0b0b");
        //     
        //     
        //
        // }
        
        

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
#else
        [UnityEngine.ContextMenu("ReNamePanel")]
#endif
        public void ReNamePanel()
        {
            if (Application.isPlaying)
            {
                return;
            }

            //找到预制体的位置
            string prefabPath = UnityEditor.AssetDatabase.GetAssetPath(gameObject);
            //修改预制体的名字为类名
            gameObject.name = GetType().Name;
            UnityEditor.AssetDatabase.RenameAsset(prefabPath, GetType().Name);
        }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
#else
        [UnityEngine.ContextMenu("AutoBind")]
#endif
        public void AutoBind()
        {
            //找自己所有被UIBind特性标记的字段
            var fields = GetType().GetFields(System.Reflection.BindingFlags.Instance |
                                             System.Reflection.BindingFlags.NonPublic |
                                             System.Reflection.BindingFlags.Public);
            foreach (var field in fields)
            {
                var bindAttr = field.GetCustomAttributes(typeof(UIBindAttribute), true);
                if (bindAttr.Length <= 0)
                {
                    //TODO 看不是不UGUI下的UI组件
                    continue;
                }


                var uiBindAttr = bindAttr[0] as UIBindAttribute;
                if (string.IsNullOrEmpty(uiBindAttr.path)) //没填path 则递归搜第一个同名
                {
                    //field.Name的首字母大写
                    string fieldName = field.Name;
                    fieldName = fieldName.Substring(0, 1).ToUpper() + fieldName.Substring(1);
                    var child = transform.Find(fieldName);
                    if (child != null)
                    {
                        field.SetValue(this, child.GetComponent(field.FieldType));
                    }
                }
                else //填path则直接按path搜
                {
                    var child = transform.Find(uiBindAttr.path);
                    if (child != null)
                    {
                        field.SetValue(this, child.GetComponent(field.FieldType));
                    }
                }
            }
        }

#endif
    }
}
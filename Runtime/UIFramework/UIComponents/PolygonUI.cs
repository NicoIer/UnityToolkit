#if UNITY_5_6_OR_NEWER

using System;
using UnityEngine;
using UnityEngine.UI;

namespace UnityToolkit
{
    [ExecuteAlways]
    public class PolygonUI : Graphic, IUIComponent
    {
        /// <summary>
        /// 根据 transform.child 位置 绘制 Mesh
        /// </summary>
        /// <param name="vh"></param>
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            if (transform.childCount <= 2)
            {
                return;
            }

            Color32 color32 = color;
            vh.Clear();


            // 中心点
            vh.AddVert(Vector3.zero, color32, new Vector2(0.5f, 0.5f));

            foreach (Transform child in transform)
            {
                vh.AddVert(child.localPosition, color32, new Vector2(0f, 0f));
            }


            for (int i = 0; i < (transform.childCount - 1); i++)
            {
                // 几何图形中的三角形
                vh.AddTriangle(0, i + 1, i + 2);
            }

            vh.AddTriangle(vh.currentVertCount - 1, 1, 0); // 闭合三角形
        }
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            // TODO 自己必须是正方形的

            float width = rectTransform.sizeDelta.x;
            float height = rectTransform.sizeDelta.y;

            float max = Mathf.Max(width, height);
            if (!Mathf.Approximately(width, height))
            {
                rectTransform.sizeDelta = new Vector2(max, max);
            }

            if (transform.childCount != vertexCount)
            {
                Initialize();
            }
        }
#endif
        public float radius
        {
            get
            {
                float a = Mathf.Abs(rectTransform.sizeDelta.x);
                float b = Mathf.Abs(rectTransform.sizeDelta.y);

                float max = Mathf.Max(a, b);

                return max * 0.5f;
            }
        }

        public int vertexCount = 6;


        public void Initialize()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject go = transform.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    Destroy(go);
                }
                else
                {
                    DestroyImmediate(go);
                }
            }

            if (vertexCount <= 2)
            {
                return;
            }

            float anglePer = 360f / vertexCount;

            for (int i = 1; i <= vertexCount; i++)
            {
                GameObject newGo;
                if (i < transform.childCount)
                {
                    newGo = transform.GetChild(i).gameObject;
                }
                else
                {
                    newGo = new GameObject(i.ToString());
                    newGo.AddComponent<RectTransform>();
                    newGo.transform.SetParent(transform);
                    newGo.transform.localPosition = Vector3.zero;
                    newGo.transform.localScale = Vector3.one;
                }

                newGo.gameObject.SetActive(true);
                var angle = anglePer * i;
                Vector2 pos = Quaternion.Euler(0, 0, angle) * new Vector2(0, 1) * radius;
                newGo.transform.localPosition = new Vector3(pos.x, pos.y, 0);
            }
        }

        public void SetValue(int index, float v)
        {
            index += 1; // 第一个点是中心点，从2开始表示6维图的第一个点
            float anglePer = 360f / (transform.childCount - 1); // 360度分成6份
            if (index < transform.childCount && index >= 0)
            {
                var go = transform.GetChild(index);
                var angle = anglePer * (index - 1);
                Vector2 pos = Quaternion.Euler(0, 0, angle) * new Vector2(0, 1) * radius * v;
                go.transform.localPosition = new Vector3(pos.x, pos.y, 0);
            }
        }
#if UNITY_EDITOR
        [UnityEditor.MenuItem("GameObject/UI/UnityToolkit/PolygonUI")]
        private static void CreatePolygonUI()
        {
            GameObject prefab = Resources.Load<GameObject>("UnityToolkit/PolygonUI");
            GameObject go = Instantiate(prefab, UnityEditor.Selection.activeTransform, false);
            UnityEditor.Selection.activeGameObject = go;
        }
#endif
    }
}
#endif
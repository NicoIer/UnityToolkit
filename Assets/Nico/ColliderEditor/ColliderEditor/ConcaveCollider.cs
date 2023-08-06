using System;
using System.Collections.Generic;
using System.Linq;
using ConcaveHull;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace ColliderEditor
{
    /// <summary>
    /// 目标平面
    /// </summary>
    public enum TargetPlane
    {
        XY,
        XZ,
        YZ
    }

    [RequireComponent(typeof(MeshFilter))]
    public class ConcaveCollider : MonoBehaviour
    {
        public TargetPlane targetPlane;
        public float height = 1;
        public float width = 0.3f;
        public MeshFilter meshFilter;
        [Range(-1, 1)] public float concavity = 0.1f;
        public int scaleFactor = 1;

        private void OnValidate()
        {
            meshFilter = GetComponent<MeshFilter>();
        }
    }


    [CustomEditor(typeof(ConcaveCollider))]
    public class ConcaveColliderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            //绘制一个按钮 "Simpliy"
            if (GUILayout.Button("Simplify"))
            {
                if (target is not ConcaveCollider collider)
                {
                    return;
                }

                if (collider.meshFilter.sharedMesh == null)
                {
                    return;
                }

                //获取当前编辑对象的Mesh的顶点
                Vector3[] vertices = collider.meshFilter.sharedMesh.vertices;
                //映射到对应平面上 
                HashSet<Node> nodes = new HashSet<Node>();
                for (int i = 0; i < vertices.Length; i++)
                {
                    switch (collider.targetPlane)
                    {
                        // case TargetPlane.XY:
                        //     nodes.Add(new Node
                        //     {
                        //         x = vertices[i].x,
                        //         y = vertices[i].y,
                        //     });
                        //     break;
                        case TargetPlane.XZ:
                            nodes.Add(new Node
                            {
                                x = vertices[i].x,
                                y = vertices[i].z,
                            });
                            break;
                        // case TargetPlane.YZ:
                        //     nodes.Add(new Node
                        //     {
                        //         x = vertices[i].y,
                        //         y = vertices[i].z,
                        //     });
                        //     break;
                        // default:
                        //     throw new ArgumentOutOfRangeException();
                    }
                }

                Hull hull = new Hull(nodes.ToList(), collider.concavity, collider.scaleFactor);
                List<Line> lines = hull.concaveLines;

                //用BoxCollider填充对应的线的位置
                foreach (var line in lines)
                {
                    GameObject go = new GameObject("____BoxCollider");
                    go.transform.SetParent(collider.transform);

                    BoxCollider boxCollider = go.AddComponent<BoxCollider>();
                    float centerX = (float)(line.start.x + line.end.x) / 2;
                    float centerY = (float)(line.start.y + line.end.y) / 2;
                    float len = (float)line.length;

                    Vector3 center = Vector3.zero;
                    Vector3 size = Vector3.zero;
                    Quaternion rotation = Quaternion.identity;
                    switch (collider.targetPlane)
                    {
                        // case TargetPlane.XY:
                        //     center = new Vector3(centerX, centerY, collider.height / 2);
                        //     size = new Vector3(len, collider.width, collider.height);
                        //     rotation = Quaternion.Euler(0, 0,
                        //         Vector2.Angle(line.end.ToVector2() - line.start.ToVector2(), Vector2.right));
                        //     break;
                        case TargetPlane.XZ:
                            center = new Vector3(centerX, collider.height / 2, centerY);
                            size = new Vector3(len, collider.height, collider.width);
                            rotation = Quaternion.Euler(0,
                                Vector2.Angle(line.end.ToVector2() - line.start.ToVector2(), Vector2.right), 0);
                            break;
                        // case TargetPlane.YZ:
                        //     center = new Vector3(collider.height / 2, centerX, centerY);
                        //     size = new Vector3(collider.height, len, collider.width);
                        //     rotation = Quaternion.Euler(0, Vector2.Angle(line.end.ToVector2() - line.start.ToVector2(), Vector2.right), 0);
                        //     break;
                        // default:
                        //     throw new ArgumentOutOfRangeException();
                    }

                    boxCollider.center = center;
                    boxCollider.size = size;
                    boxCollider.transform.rotation = rotation;
                }
            }
        }
    }
}
#endif
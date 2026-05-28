using UnityEditor;
using UnityEngine;

namespace UnityToolkit.Editor
{
    public static class MeshNormalizeUtility
    {
        [MenuItem("Assets/Normalize Mesh Vertices to [0,1]", validate = true)]
        static bool Validate()
        {
            return Selection.activeObject is Mesh;
        }

        [MenuItem("Assets/Normalize Mesh Vertices to [0,1]")]
        static void Execute()
        {
            var mesh = Selection.activeObject as Mesh;
            if (mesh == null) return;

            var path = AssetDatabase.GetAssetPath(mesh);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("[MeshNormalize] Mesh is not a saved asset.");
                return;
            }

            var vertices = mesh.vertices;
            if (vertices.Length == 0) return;

            // Find bounds
            var min = vertices[0];
            var max = vertices[0];
            for (int i = 1; i < vertices.Length; i++)
            {
                min = Vector3.Min(min, vertices[i]);
                max = Vector3.Max(max, vertices[i]);
            }

            var size = max - min;
            // Avoid division by zero on flat axes
            float sx = size.x > 0.0001f ? size.x : 1f;
            float sy = size.y > 0.0001f ? size.y : 1f;

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new Vector3(
                    (vertices[i].x - min.x) / sx,
                    (vertices[i].y - min.y) / sy,
                    0f
                );
            }

            Undo.RecordObject(mesh, "Normalize Mesh Vertices");
            mesh.vertices = vertices;
            mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);
            AssetDatabase.SaveAssets();

            Debug.Log($"[MeshNormalize] Normalized {vertices.Length} vertices from [{min.x:F2},{min.y:F2}]~[{max.x:F2},{max.y:F2}] to [0,1]. Asset: {path}");
        }
    }
}

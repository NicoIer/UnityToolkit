// using System.Collections.Generic;
// using System.IO;
// using UnityEditor;
// using UnityEngine;
// using UnityEngine.UI;
//
// namespace UnityToolkit.Editor
// {
//     [CustomEditor(typeof(UIDepthOccluder))]
//     public class UIDepthOccluderEditor : UnityEditor.Editor
//     {
//         const string k_DefaultSavePath = "Assets/Framework/UnityToolkit/Runtime/Renderer/GeneratedMeshes";
//         const int k_DefaultResolution = 64;
//
//         SerializedProperty _occluderMeshProp;
//
//         int _resolution = k_DefaultResolution;
//         float _alphaThreshold = 0.5f;
//         string _savePath = k_DefaultSavePath;
//
//         void OnEnable()
//         {
//             _occluderMeshProp = serializedObject.FindProperty("occluderMesh");
//         }
//
//         public override void OnInspectorGUI()
//         {
//             serializedObject.Update();
//             DrawDefaultInspector();
//
//             EditorGUILayout.Space(10);
//             EditorGUILayout.LabelField("Mesh Generator", EditorStyles.boldLabel);
//
//             _alphaThreshold = EditorGUILayout.Slider("Alpha Threshold", _alphaThreshold, 0f, 1f);
//             _resolution = EditorGUILayout.IntSlider("Grid Resolution", _resolution, 8, 256);
//             _savePath = EditorGUILayout.TextField("Save Directory", _savePath);
//
//             var occluder = (UIDepthOccluder)target;
//             var image = occluder.GetComponent<Image>();
//
//             EditorGUI.BeginDisabledGroup(image == null || image.sprite == null);
//             if (GUILayout.Button("Generate Mesh from Alpha", GUILayout.Height(30)))
//             {
//                 var mesh = GenerateMeshFromAlpha(image.sprite, _alphaThreshold, _resolution);
//                 if (mesh != null)
//                 {
//                     var savedMesh = SaveMesh(mesh, image.sprite.name, _savePath);
//                     Object.DestroyImmediate(mesh);
//
//                     if (savedMesh != null)
//                     {
//                         Undo.RecordObject(occluder, "Assign Generated Occluder Mesh");
//                         _occluderMeshProp.objectReferenceValue = savedMesh;
//                         serializedObject.ApplyModifiedProperties();
//                         Debug.Log($"[UIDepthOccluder] Mesh saved: {AssetDatabase.GetAssetPath(savedMesh)}");
//                     }
//                 }
//             }
//             EditorGUI.EndDisabledGroup();
//
//             if (image == null || image.sprite == null)
//             {
//                 EditorGUILayout.HelpBox("Needs an Image component with a Sprite assigned.", MessageType.Warning);
//             }
//
//             serializedObject.ApplyModifiedProperties();
//         }
//
//         static Mesh GenerateMeshFromAlpha(Sprite sprite, float threshold, int resolution)
//         {
//             var texture = sprite.texture;
//
//             // Read pixels via RenderTexture to handle non-readable textures
//             var rt = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32);
//             Graphics.Blit(texture, rt);
//             var prev = RenderTexture.active;
//             RenderTexture.active = rt;
//
//             var readTex = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
//             readTex.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
//             readTex.Apply();
//
//             RenderTexture.active = prev;
//             RenderTexture.ReleaseTemporary(rt);
//
//             // Get the sprite rect within the texture (atlas support)
//             var spriteRect = sprite.textureRect;
//             int srcX = Mathf.RoundToInt(spriteRect.x);
//             int srcY = Mathf.RoundToInt(spriteRect.y);
//             int srcW = Mathf.RoundToInt(spriteRect.width);
//             int srcH = Mathf.RoundToInt(spriteRect.height);
//
//             // Build alpha grid at target resolution
//             int gridW = Mathf.Min(resolution, srcW);
//             int gridH = Mathf.Min(resolution, srcH);
//             var alphaGrid = new bool[gridW, gridH];
//             int solidCount = 0;
//
//             for (int gy = 0; gy < gridH; gy++)
//             {
//                 for (int gx = 0; gx < gridW; gx++)
//                 {
//                     // Sample center of each grid cell
//                     float u = (gx + 0.5f) / gridW;
//                     float v = (gy + 0.5f) / gridH;
//                     int px = srcX + Mathf.Clamp(Mathf.RoundToInt(u * srcW), 0, srcW - 1);
//                     int py = srcY + Mathf.Clamp(Mathf.RoundToInt(v * srcH), 0, srcH - 1);
//
//                     float alpha = readTex.GetPixel(px, py).a;
//                     if (alpha >= threshold)
//                     {
//                         alphaGrid[gx, gy] = true;
//                         solidCount++;
//                     }
//                 }
//             }
//
//             Object.DestroyImmediate(readTex);
//
//             if (solidCount == 0)
//             {
//                 Debug.LogWarning("[UIDepthOccluder] No pixels above alpha threshold. No mesh generated.");
//                 return null;
//             }
//
//             // Greedy rect merging: row by row, merge horizontal runs, then merge vertically
//             var rects = GreedyMerge(alphaGrid, gridW, gridH);
//
//             // Build mesh in [0,1] normalized space
//             var vertices = new List<Vector3>();
//             var triangles = new List<int>();
//
//             foreach (var rect in rects)
//             {
//                 float x0 = (float)rect.x / gridW;
//                 float y0 = (float)rect.y / gridH;
//                 float x1 = (float)(rect.x + rect.width) / gridW;
//                 float y1 = (float)(rect.y + rect.height) / gridH;
//
//                 int baseIdx = vertices.Count;
//                 vertices.Add(new Vector3(x0, y0, 0)); // BL
//                 vertices.Add(new Vector3(x0, y1, 0)); // TL
//                 vertices.Add(new Vector3(x1, y1, 0)); // TR
//                 vertices.Add(new Vector3(x1, y0, 0)); // BR
//
//                 triangles.Add(baseIdx);
//                 triangles.Add(baseIdx + 1);
//                 triangles.Add(baseIdx + 2);
//                 triangles.Add(baseIdx);
//                 triangles.Add(baseIdx + 2);
//                 triangles.Add(baseIdx + 3);
//             }
//
//             var mesh = new Mesh
//             {
//                 name = $"{sprite.name}_OccluderMesh",
//                 vertices = vertices.ToArray(),
//                 triangles = triangles.ToArray()
//             };
//             mesh.RecalculateBounds();
//
//             Debug.Log($"[UIDepthOccluder] Generated mesh: {rects.Count} rects, {vertices.Count} verts from {solidCount}/{gridW * gridH} solid cells");
//             return mesh;
//         }
//
//         /// <summary>
//         /// Greedy rectangle merging on a boolean grid.
//         /// Merges horizontally first, then vertically for identical spans.
//         /// </summary>
//         static List<RectInt> GreedyMerge(bool[,] grid, int width, int height)
//         {
//             var used = new bool[width, height];
//             var rects = new List<RectInt>();
//
//             for (int y = 0; y < height; y++)
//             {
//                 for (int x = 0; x < width; x++)
//                 {
//                     if (!grid[x, y] || used[x, y]) continue;
//
//                     // Expand right
//                     int w = 1;
//                     while (x + w < width && grid[x + w, y] && !used[x + w, y])
//                         w++;
//
//                     // Expand down
//                     int h = 1;
//                     bool canExpand = true;
//                     while (y + h < height && canExpand)
//                     {
//                         for (int dx = 0; dx < w; dx++)
//                         {
//                             if (!grid[x + dx, y + h] || used[x + dx, y + h])
//                             {
//                                 canExpand = false;
//                                 break;
//                             }
//                         }
//                         if (canExpand) h++;
//                     }
//
//                     // Mark used
//                     for (int dy = 0; dy < h; dy++)
//                         for (int dx = 0; dx < w; dx++)
//                             used[x + dx, y + dy] = true;
//
//                     rects.Add(new RectInt(x, y, w, h));
//                 }
//             }
//
//             return rects;
//         }
//
//         static Mesh SaveMesh(Mesh mesh, string spriteName, string directory)
//         {
//             if (!Directory.Exists(directory))
//                 Directory.CreateDirectory(directory);
//
//             string fileName = $"{spriteName}_OccluderMesh.asset";
//             string fullPath = System.IO.Path.Combine(directory, fileName);
//
//             // Check for existing asset
//             var existing = AssetDatabase.LoadAssetAtPath<Mesh>(fullPath);
//             if (existing != null)
//             {
//                 existing.Clear();
//                 existing.vertices = mesh.vertices;
//                 existing.triangles = mesh.triangles;
//                 existing.RecalculateBounds();
//                 EditorUtility.SetDirty(existing);
//                 AssetDatabase.SaveAssets();
//                 return existing;
//             }
//
//             var savedMesh = Object.Instantiate(mesh);
//             savedMesh.name = mesh.name;
//             AssetDatabase.CreateAsset(savedMesh, fullPath);
//             AssetDatabase.SaveAssets();
//             return AssetDatabase.LoadAssetAtPath<Mesh>(fullPath);
//         }
//     }
// }

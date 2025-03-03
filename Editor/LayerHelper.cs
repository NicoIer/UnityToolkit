#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityToolkit.Editor
{
    public static class LayersHelper
    {
        public static bool IsLayerExist(string name)
        {
            return LayerMask.NameToLayer(name) != -1;
        }

        public static void CreateLayer(string name)
        {
            if (IsLayerExist(name))
            {
                Debug.Log("Layer " + name + " already exists");
                return;
            }

            SerializedObject tagManager =
                new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");

            for (int j = 8; j < layers.arraySize; j++)
            {
                SerializedProperty newLayer = layers.GetArrayElementAtIndex(j);

                if (string.IsNullOrEmpty(newLayer.stringValue))
                {
                    newLayer.stringValue = name;
                    tagManager.ApplyModifiedProperties();
                    Debug.Log("Layer created: " + name);
                    return;
                }
            }

            Debug.LogError("Failed to create layer: " + name + ", maximum number of layers reached.");
        }

        public static void DisableCollisions(int layer)
        {
            SerializedObject tagManager =
                new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

            string[] layers = Enumerable.Range(0, 31)
                .Select(index => LayerMask.LayerToName(index))
                .Where(l => !string.IsNullOrEmpty(l))
                .ToArray();

            for (int i = 0; i < layers.Length; i++)
            {
                int current = LayerMask.NameToLayer(layers[i]);

                Physics.IgnoreLayerCollision(layer, current);
                Physics2D.IgnoreLayerCollision(layer, current);
            }

            tagManager.ApplyModifiedProperties();
        }
    }
}


#endif
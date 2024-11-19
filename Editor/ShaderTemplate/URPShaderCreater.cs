#if UNITY_EDITOR

using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace UnityToolkit.Editor
{
    using Path = System.IO.Path;
    internal static class URPShaderCreater
    {
        // URP Shader Template for .shader file


        [MenuItem("Assets/Create/Shader/URP/Unlit Shader", false, 0)]
        public static void CreateUnlitShaderEditor()
        {
            // 拿到当前在Project视图中选中的文件夹 或者 文件 或者当前所在文件夹
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
                ScriptableObject.CreateInstance<MyDoCreateScriptAsset>(),
                GetSelectedPathOrFallback() + "/New Unlit.shader",
                null,
                "Assets/Plugins/UnityToolkit/Editor/ShaderTemplate/Unlit.shader");
        }

        [MenuItem("Assets/Create/Shader/URP/FullScreen Shader", false, 0)]
        public static void CreateFullScreenShaderEditor()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
                ScriptableObject.CreateInstance<MyDoCreateScriptAsset>(),
                GetSelectedPathOrFallback() + "/New FullScreen.shader",
                null,
                "Assets/Plugins/UnityToolkit/Editor/ShaderTemplate/FullScreen.shader");
        }

        // public static void CreateSimple


        private static string GetSelectedPathOrFallback()
        {
            string path = "Assets";
            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = System.IO.Path.GetDirectoryName(path);
                    break;
                }
            }

            return path;
        }


        class MyDoCreateScriptAsset : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                UnityEngine.Object o = CreateScriptAssetFromTemplate(pathName, resourceFile);
                ProjectWindowUtil.ShowCreatedAsset(o);
            }

            private static Object CreateScriptAssetFromTemplate(string pathName, string resourceFile)
            {
                string fullPath = Path.GetFullPath(pathName);
                StreamReader streamReader = new StreamReader(resourceFile);
                string text = streamReader.ReadToEnd();
                streamReader.Close();
                string fileNameWithoutExtension = "Universal/" + Path.GetFileNameWithoutExtension(pathName);
                text = Regex.Replace(text, "#NAME#", fileNameWithoutExtension);


                bool encoderShouldEmitUTF8Identifier = true;
                bool throwOnInvalidBytes = false;
                UTF8Encoding encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier, throwOnInvalidBytes);
                bool append = false;
                StreamWriter streamWriter = new StreamWriter(fullPath, append, encoding);
                streamWriter.Write(text);
                streamWriter.Close();
                AssetDatabase.ImportAsset(pathName);
                return AssetDatabase.LoadAssetAtPath(pathName, typeof(UnityEngine.Object));
            }
        }
    }
}
#endif

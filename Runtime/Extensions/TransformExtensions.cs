#if UNITY_5_6_OR_NEWER
using UnityEngine;

namespace UnityToolkit
{
    public static class TransformExtensions
    {

        public static Transform FindRecursive(this Transform trans, string name, bool includeSelf = false)
        {
            if (includeSelf && trans.name == name)
            {
                return trans;
            }

            for (int i = 0; i < trans.childCount; ++i)
            {
                var child = trans.GetChild(i);
                if (trans.name == name)
                {
                    return trans;
                }
            }

            for (int i = 0; i < trans.childCount; ++i)
            {
                var child = trans.GetChild(i);
                var result = child.FindRecursive(name, true);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public static void CopyLocalPosAndRotAndScale(this Transform trans, Transform target)
        {
            trans.SetLocalPositionAndRotation(target.localPosition, target.localRotation);
            trans.localScale = target.localScale;
        }


        public static void DestroyAllChildImmediate(this Transform trans)
        {
            for (int i = trans.childCount - 1; i >= 0; i--)
            {
                var child = trans.GetChild(i);
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
        }
    }
}
#endif
using UnityEngine;

namespace Nico.Util
{
    public static class RectHelper
    {
        // 获取UI元素的左上角位置
        public static Vector2 LeftTop(RectTransform rtf)
        {

            var parent = rtf.parent;
            Vector2 uiTopLeftPos = new Vector2(rtf.anchorMin.x * parent.GetComponent<RectTransform>().rect.width,
                (1 - rtf.anchorMax.y) * parent.GetComponent<RectTransform>().rect.height);
            return uiTopLeftPos;
        }
    }
}
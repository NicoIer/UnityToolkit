#if UNITY_5_6_OR_NEWER
using UnityEngine;
using System;

namespace UnityToolkit
{
    public abstract class LoopScrollRectMulti : LoopScrollRectBase
    {
        [NonSerialized]
        public ItemRendererDelegate itemRenderer = null;
        
        protected override void ProvideData(Transform transform, int index)
        {
            itemRenderer(transform, index);
        }
        
        // Multi Data Source cannot support TempPool
        protected override RectTransform GetFromTempPool(int itemIdx)
        {
            RectTransform nextItem = ItemProvider(itemIdx).transform as RectTransform;
            nextItem.transform.SetParent(m_Content, false);
            nextItem.gameObject.SetActive(true);

            ProvideData(nextItem, itemIdx);
            return nextItem;
        }

        protected override void ReturnToTempPool(bool fromStart, int count)
        {
            Debug.Assert(m_Content.childCount >= count);
            if (fromStart)
            {
                for (int i = count - 1; i >= 0; i--)
                {
                    ItemReturn(m_Content.GetChild(i));
                }
            }
            else
            {
                int t = m_Content.childCount - count;
                for (int i = m_Content.childCount - 1; i >= t; i--)
                {
                    ItemReturn(m_Content.GetChild(i));
                }
            }
        }

        protected override void ClearTempPool()
        {
        }
    }
}
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Nico.Collections;
using UnityEngine;

namespace Nico.UI
{
    internal sealed class UGUILayerManager
    {
        private RectTransform transform;
        private PriorityQueue<IUIWindow> _queue;
        private Transform hide;
        internal bool HasWindow => _queue.Count > 0;
        private Stack<IUIWindow> tmp;

        internal UGUILayerManager(RectTransform transform, RectTransform hide)
        {
            this.hide = hide;
            this.transform = transform;
            _queue = new PriorityQueue<IUIWindow>((_1, _2) => _1.Priority() - _2.Priority());
            tmp = new Stack<IUIWindow>();
        }

        internal void Remove(IUIWindow window)
        {
            //这里之所以使用Stack来回放,是因为要保证恢复后的顺序不变,优先级队列在优先级相等的情况下,会保证先进先出
            tmp.Clear();
            while (_queue.Count > 0)
            {
                IUIWindow current = _queue.Dequeue();
                if (window == current)
                {
                    HideWindow(current);
                    break;
                }

                tmp.Push(current);
            }

            //将栈中的元素重新压入队列中
            while (tmp.Count > 0)
            {
                _queue.Enqueue(tmp.Pop());
            }
        }

        //将某个UI压入栈中
        internal void Push(IUIWindow window)
        {
            window.gameObject.transform.SetParent(transform, false);
            ShowWindow(window);
            _queue.Enqueue(window);
            //调整当前层级的UI显示顺序
            //若新进入的UI优先级非常高,则直接放到最下面即可
            if (window.Priority() > _queue.Peek().Priority())
            {
                window.transform.SetAsLastSibling();
                return;
            }
            //重新遍历一遍队列,将优先级最高的放到最下面
            foreach (IUIWindow iui in _queue.EnumerateMinToMax())
            {
                iui.transform.SetAsLastSibling();
            }
        }

        //弹出栈顶的UI
        internal bool Pop(out IUIWindow window)
        {
            window = default;
            if (_queue.Count == 0)
            {
                return false;
            }

            window = _queue.Dequeue();
            HideWindow(window);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ShowWindow(IUIWindow window)
        {
            window.gameObject.SetActive(true);
            //TODO 处理模态窗口 不允许穿透点击其他UIWindow(但是游戏内的射线检测还是可以穿透的)
            window.OnOpen();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HideWindow(IUIWindow window)
        {
            window.transform.SetParent(hide);
            window.gameObject.SetActive(false);
            window.OnClose();
        }
    }
}
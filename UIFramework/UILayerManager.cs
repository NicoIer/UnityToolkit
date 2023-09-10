using System.Collections.Generic;
using Nico;
using UnityEngine;

namespace Nico
{
    public class UILayerManager
    {
        // private readonly int _uiLayerMask;
        // private readonly int _hiddenLayerMask;
        private PriorityQueue<IUIPanel> _queue;
        public bool HasItem => _queue.Count > 0;
        private Stack<IUIPanel> _tmp;
        private Transform rectTransform;
        public UILayerManager(RectTransform transform)//,int uiLayerMask, int hiddenLayerMask)
        {
            this.rectTransform = transform;
            // this._uiLayerMask = uiLayerMask;
            // this._hiddenLayerMask = hiddenLayerMask;
            _queue = new PriorityQueue<IUIPanel>((_1, _2) => _1.Priority() - _2.Priority());
            _tmp = new Stack<IUIPanel>();
        }

        public void Remove(IUIPanel panel)
        {
            _tmp.Clear();
            while (_queue.Count > 0)
            {
                IUIPanel current = _queue.Dequeue();
                if (panel == current)
                {
                    HidePanel(current);
                    break;
                }

                _tmp.Push(current);
            }

            while (_tmp.Count > 0)
            {
                _queue.Enqueue(_tmp.Pop());
            }
        }

        public void RemoveAll()
        {
            _tmp.Clear();
            while (_queue.Count > 0)
            {
                IUIPanel current = _queue.Dequeue();
                HidePanel(current);
            }
        }

        public void Push(IUIPanel panel)
        {
            ShowPanel(panel);
            _queue.Enqueue(panel);
            if (panel.Priority() > _queue.Peek().Priority())
            {
                panel.GetTransform().SetAsLastSibling();
                return;
            }

            foreach (IUIPanel iuiPanel in _queue.EnumerateMinToMax())
            {
                iuiPanel.GetTransform().SetAsLastSibling();
            }
        }

        public bool Pop(out IUIPanel panel)
        {
            panel = default;
            if (_queue.Count == 0)
            {
                return false;
            }

            panel = _queue.Dequeue();
            HidePanel(panel);
            return true;
        }

        public void ShowPanel(IUIPanel panel)
        {
            panel.GetGameObject().SetActive(true);
            // panel.gameObject.layer = _hiddenLayerMask;
            panel.OnShow();
        }

        public void HidePanel(IUIPanel panel)
        {
            // panel.gameObject.layer = _uiLayerMask;
            panel.GetGameObject().SetActive(false);
            panel.OnHide();
        }
    }
}
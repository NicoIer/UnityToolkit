#if UNITY_5_3_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityToolkit
{
    public enum VisibilityMode
    {
        /// <summary>
        /// 通过 GameObject.SetActive 控制显示/隐藏
        /// </summary>
        GameObject,

        /// <summary>
        /// 通过 CanvasGroup 的 alpha、interactable、blocksRaycasts 控制显示/隐藏
        /// </summary>
        CanvasGroup,
    }

    [Flags]
    public enum VisibilityModeEnum
    {
        Alpha = 1 << 0,
        Interactable = 1 << 1,
        BlockRaycasts = 1 << 2,

        All = Alpha | Interactable | BlockRaycasts
    }

    /// <summary>
    /// UI 可见性阻止器组件
    /// 用于管理 UI 元素的显示/隐藏，支持多个来源同时控制
    /// 只有当没有任何来源请求隐藏时，UI 元素才会显示
    /// </summary>
    public class UIVisibilityBlocker : MonoBehaviour
    {
        [SerializeField] [Tooltip("可见性控制模式")]
        private VisibilityMode visibilityMode = VisibilityMode.GameObject;

        [SerializeField] [Tooltip("要控制显示/隐藏的目标 GameObject，如果为空则控制自身（GameObject 模式下使用）")]
        private GameObject target;

        [SerializeField] [Tooltip("要控制的 CanvasGroup，如果为空则尝试从自身获取（CanvasGroup 模式下使用）")]
        private CanvasGroup canvasGroup;

        [SerializeField] [Tooltip("是否在 Inspector 中显示当前的阻止来源（仅用于调试）")]
        private bool showDebugInfo = false;

        [SerializeField] [Tooltip("当前所有请求隐藏的来源（只读，用于调试）")]
        private List<string> debugBlockSources = new List<string>();

        /// <summary>
        /// 存储所有请求隐藏的来源标识
        /// </summary>
        private readonly Dictionary<string, VisibilityModeEnum> _blockSources = new Dictionary<string, VisibilityModeEnum>();
        private VisibilityModeEnum _appliedBlockMode;

        /// <summary>
        /// 获取要控制的目标 GameObject
        /// </summary>
        private GameObject Target => target != null ? target : gameObject;

        /// <summary>
        /// 获取 CanvasGroup（优先使用序列化字段，否则从自身获取）
        /// </summary>
        private CanvasGroup CachedCanvasGroup
        {
            get
            {
                if (canvasGroup == null)
                    canvasGroup = GetComponent<CanvasGroup>();
                return canvasGroup;
            }
        }

        /// <summary>
        /// 是否被阻止显示（有任何来源请求隐藏）
        /// </summary>
        public bool IsBlocked => _blockSources.Count > 0;

        /// <summary>
        /// 当前阻止来源的数量
        /// </summary>
        public int BlockCount => _blockSources.Count;

        /// <summary>
        /// 请求隐藏（添加阻止）
        /// </summary>
        /// <param name="source">请求隐藏的来源标识</param>
        public void Block(string source, VisibilityModeEnum mode = VisibilityModeEnum.All)
        {
            if (string.IsNullOrEmpty(source))
            {
                Debug.LogWarning($"[UIVisibilityBlocker] Block: source 不能为空 (target: {Target.name})");
                return;
            }

            _blockSources[source] = mode;
            UpdateVisibility();
            UpdateDebugInfo();
        }

        /// <summary>
        /// 取消隐藏请求（移除阻止）
        /// </summary>
        /// <param name="source">之前请求隐藏的来源标识</param>
        public void Unblock(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                Debug.LogWarning($"[UIVisibilityBlocker] Unblock: source 不能为空 (target: {Target.name})");
                return;
            }

            _blockSources.Remove(source);
            UpdateVisibility();
            UpdateDebugInfo();
        }
        //
        // /// <summary>
        // /// 检查指定来源是否已请求隐藏
        // /// </summary>
        // public bool HasBlock(string source)
        // {
        //     return _blockSources.Contains(source);
        // }

        /// <summary>
        /// 清除所有隐藏请求
        /// </summary>
        public void ClearAll()
        {
            if (_blockSources.Count > 0)
            {
                _blockSources.Clear();
                UpdateVisibility();
                UpdateDebugInfo();
            }
        }
        //
        // /// <summary>
        // /// 强制设置可见性（忽略阻止状态，谨慎使用）
        // /// </summary>
        // public void ForceSetActive(bool active)
        // {
        //     Target.SetActive(active);
        // }

        /// <summary>
        /// 根据当前阻止状态更新可见性
        /// </summary>
        private void UpdateVisibility()
        {
            var blockedMode = GetBlockedMode();

            switch (visibilityMode)
            {
                case VisibilityMode.CanvasGroup:
                    var cg = CachedCanvasGroup;
                    if (cg != null)
                    {
                        ApplyCanvasGroupMode(cg, blockedMode);
                    }
                    else
                    {
                        Debug.LogWarning($"[UIVisibilityBlocker] CanvasGroup 模式但未找到 CanvasGroup 组件 (target: {Target.name})，回退到 GameObject 模式");
                        ApplyGameObjectMode(blockedMode);
                    }
                    break;

                case VisibilityMode.GameObject:
                default:
                    ApplyGameObjectMode(blockedMode);
                    break;
            }

            _appliedBlockMode = blockedMode;
        }

        private VisibilityModeEnum GetBlockedMode()
        {
            var mode = 0;
            foreach (var blockSource in _blockSources)
            {
                mode |= (int)blockSource.Value;
            }

            return (VisibilityModeEnum)mode;
        }

        private void ApplyCanvasGroupMode(CanvasGroup cg, VisibilityModeEnum blockedMode)
        {
            if (ShouldApplyMode(blockedMode, VisibilityModeEnum.Alpha))
                cg.alpha = blockedMode.HasFlag(VisibilityModeEnum.Alpha) ? 0f : 1f;

            if (ShouldApplyMode(blockedMode, VisibilityModeEnum.Interactable))
                cg.interactable = !blockedMode.HasFlag(VisibilityModeEnum.Interactable);

            if (ShouldApplyMode(blockedMode, VisibilityModeEnum.BlockRaycasts))
                cg.blocksRaycasts = !blockedMode.HasFlag(VisibilityModeEnum.BlockRaycasts);
        }

        private void ApplyGameObjectMode(VisibilityModeEnum blockedMode)
        {
            if (ShouldApplyMode(blockedMode, VisibilityModeEnum.Alpha))
                Target.SetActive(!blockedMode.HasFlag(VisibilityModeEnum.Alpha));

            var cg = CachedCanvasGroup;
            if (cg == null)
                return;

            if (ShouldApplyMode(blockedMode, VisibilityModeEnum.Interactable))
                cg.interactable = !blockedMode.HasFlag(VisibilityModeEnum.Interactable);

            if (ShouldApplyMode(blockedMode, VisibilityModeEnum.BlockRaycasts))
                cg.blocksRaycasts = !blockedMode.HasFlag(VisibilityModeEnum.BlockRaycasts);
        }

        private bool ShouldApplyMode(VisibilityModeEnum currentMode, VisibilityModeEnum mode)
        {
            return currentMode.HasFlag(mode) || _appliedBlockMode.HasFlag(mode);
        }

        /// <summary>
        /// 更新调试信息
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void UpdateDebugInfo()
        {
            if (showDebugInfo)
            {
                debugBlockSources.Clear();
                foreach (var blockSource in _blockSources)
                {
                    debugBlockSources.Add($"{blockSource.Key}: {blockSource.Value}");
                }
            }
        }

        private void OnValidate()
        {
            // 在编辑器中修改 showDebugInfo 时更新显示
            if (!showDebugInfo)
            {
                debugBlockSources.Clear();
            }
        }

        private void OnDestroy()
        {
            // 组件禁用时清除所有阻止，避免状态残留
            _blockSources.Clear();
            debugBlockSources.Clear();
            _appliedBlockMode = 0;
        }
    }
}
#endif

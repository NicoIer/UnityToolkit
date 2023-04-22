using System;
using System.Collections.Generic;
using Nico.Design;
using UnityEngine;

namespace Game
{
    public partial class TimerSystem : GlobalSingleton<TimerSystem>
    {
#if UNITY_EDITOR
        [field: SerializeReference]
#endif
        private LinkedList<TimerStruct> timerList { get; } = new();

        private readonly HashSet<Action> actionSet = new();

        // private NativeArray<TimerStruct> timerStructs; //TODO 后续用于JobSystem优化
        private void LateUpdate()
        {
            LinkedListNode<TimerStruct> currentNode = timerList.First;
            while (currentNode != null)
            {
                TimerStruct t = currentNode.Value;
                t.timer -= Time.deltaTime;
                if (t.timer <= 0)
                {
                    t.action?.Invoke();
                    LinkedListNode<TimerStruct> next = currentNode.Next;

                    timerList.Remove(currentNode);
                    actionSet.Remove(currentNode.Value.action);

                    currentNode = next;
                }
                else
                {
                    currentNode = currentNode.Next;
                }
            }
        }

        public void RegisterTimer(float timer, Action action)
        {
            if (actionSet.Contains(action))
            {
                return;
            }

            actionSet.Add(action);
            timerList.AddLast(new TimerStruct(timer, action));
        }

        public void UnregisterTimer(Action action)
        {
            if (!actionSet.Contains(action))
            {
                return;
            }

            actionSet.Remove(action);
            //取消上一次注册的定时器
            var cur = timerList.First;
            while (cur != null)
            {
                if (cur.Value.action == action)
                {
                    timerList.Remove(cur);
                    break;
                }

                cur = cur.Next;
            }
        }
    }

    public partial class TimerSystem
    {
        [Serializable]
        private class TimerStruct
        {
            public float timer;
            public Action action { get; }

            public TimerStruct(float timer, Action action)
            {
                this.timer = timer;
                this.action = action;
            }
        }
    }
}
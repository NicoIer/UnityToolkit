using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace Nico.Job
{




    [BurstCompile]
    internal struct SlerpForwardJob : IJobParallelForTransform
    {
        public float deltaTime;
        public float setSpeed;
        //ReadOnly 允许多个线程同时读取  NativeArray
        [ReadOnly] public NativeArray<Vector3> moveDirs;

        public void Execute(int index, TransformAccess transform)
        {
            // 使用 Slerp 计算新的 forward 向量
            Vector3 forward = transform.rotation * Vector3.forward;
            forward = Vector3.Slerp(forward, moveDirs[index], deltaTime * setSpeed);

            // 更新 Transform 的 forward 向量
            Quaternion rotation = Quaternion.LookRotation(forward);
            transform.rotation = rotation;
        }
    }
}
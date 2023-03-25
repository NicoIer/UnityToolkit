using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace Nico.Job
{
    [BurstCompile]
    internal struct SetForwardTransformsJob : IJobParallelForTransform
    {
        [ReadOnly] public NativeArray<Vector3> moveDirs;

        public void Execute(int index, TransformAccess transform)
        {
            // 更新 Transform 的 forward 向量
            Quaternion rotation = Quaternion.LookRotation(moveDirs[index]);
            transform.rotation = rotation;
        }
    }
}
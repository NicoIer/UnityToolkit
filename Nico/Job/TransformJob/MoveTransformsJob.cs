using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace Nico.Job
{
    [BurstCompile]
    internal struct MoveTransformsJob : IJobParallelForTransform
    {
        public float deltaTime;
        public float speed;
        [ReadOnly] public NativeArray<Vector3> moveDirs;

        public void Execute(int index, TransformAccess transform)
        {
            // 计算新位置
            Vector3 position = transform.position;
            position += moveDirs[index] * speed * deltaTime;

            // 更新 Transform 的位置
            transform.position = position;
        }
    }
}
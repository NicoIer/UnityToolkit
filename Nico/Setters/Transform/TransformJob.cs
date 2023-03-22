using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace Nico
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

    [BurstCompile]
    internal struct SlerpForwardTransformsJob : IJobParallelForTransform
    {
        private float _deltaTime;
        private float _setSpeed;
        //ReadOnly 允许多个线程同时读取  NativeArray
        [ReadOnly] public NativeArray<Vector3> moveDirs;

        public void Execute(int index, TransformAccess transform)
        {
            // 使用 Slerp 计算新的 forward 向量
            Vector3 forward = transform.rotation * Vector3.forward;
            forward = Vector3.Slerp(forward, moveDirs[index], _deltaTime * _setSpeed);

            // 更新 Transform 的 forward 向量
            Quaternion rotation = Quaternion.LookRotation(forward);
            transform.rotation = rotation;
        }
    }
}
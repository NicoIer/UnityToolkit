using Nico.Job;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace Nico.Job
{
    public static class TransformWorker
    {
        public static void SetForward(Transform[] transform, Vector3[] moveDir, float setSpeed = 0f)
        {
            SlerpForwardJob slerpForwardJob = new SlerpForwardJob
            {
                setSpeed = setSpeed,
                deltaTime = Time.deltaTime,
                moveDirs = new NativeArray<Vector3>(moveDir, Allocator.TempJob)
            };
            var transformAccessArray = new TransformAccessArray(transform);
            var jobHandle = slerpForwardJob.Schedule(transformAccessArray);
            jobHandle.Complete();
            slerpForwardJob.moveDirs.Dispose();
            transformAccessArray.Dispose();
        }


        public static void Move(Transform[] transforms, Vector3[] moveDirs, float speed)
        {
            var transformAccessArray = new TransformAccessArray(transforms);
            var moveTransformsJob = new MoveTransformsJob
            {
                deltaTime = Time.deltaTime,
                speed = speed,
                moveDirs = new NativeArray<Vector3>(moveDirs, Allocator.TempJob)
            };
            var moveTransformsJobHandle = moveTransformsJob.Schedule(transformAccessArray);
            moveTransformsJobHandle.Complete();
            transformAccessArray.Dispose();
            moveTransformsJob.moveDirs.Dispose();
        }
    }
}
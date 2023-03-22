using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace Nico
{
    public static class TransformSetter
    {
        public static void Move(Transform transform, Vector3 moveDir, float speed)
        {
            transform.position += speed * moveDir * Time.deltaTime;
            // MoveJob(transform, moveDir, speed);
        }

        public static void SetForward(Transform transform, Vector3 moveDir)
        {
            transform.forward = moveDir;
        }

        public static void SetForward(Transform transform, Vector3 moveDir, float setSpeed)
        {
            transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * setSpeed);
        }

        #region Job System

        #endregion

        public static void MoveJob(Transform transform, Vector3 moveDir, float speed)
        {
            MoveJob(new[] { transform }, new[] { moveDir }, speed);
        }

        public static void MoveJob(Transform[] transforms, Vector3[] moveDirs, float speed)
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
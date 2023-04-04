using UnityEngine;
using UnityEngine.Jobs;

namespace Nico.Job
{
    public struct LookAtCameraJob : IJobParallelForTransform
    {
        public Vector3 cameraPosition;
        public Quaternion cameraRotation;

        public void Execute(int index, TransformAccess transform)
        {
            transform.rotation = cameraRotation;
        }
    }
}
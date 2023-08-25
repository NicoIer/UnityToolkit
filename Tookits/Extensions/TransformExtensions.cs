using UnityEngine;

namespace Nico
{
    public static class TransformExtensions
    {
        public static void MoveTo(this Transform transform, Transform tar,float speed,float deltaTime)
        {
            transform.position = Vector3.MoveTowards(transform.position, tar.position, speed * deltaTime);
        }
    }
}
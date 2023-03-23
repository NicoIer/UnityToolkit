using UnityEngine;

namespace Nico.Components
{
    /// <summary>
    /// 像子物体一样跟随另一个物体 会 旋转和位移
    /// 当跟随的物体被销毁/引用为null 时 会自动销毁自己
    /// </summary>
    public class TransformFollower : MonoBehaviour
    {
        [field: SerializeField] public Transform Target { get; private set; }

        
        public void SetFollowTarget(Transform target)
        {
            Target = target;
        }

        private void LateUpdate()
        {
            if (Target == null)
            {
                return;
            }

            //类型以target为父物体的方式修改自己的位置 和 旋转
            //TODO 做到不依赖父物体 却可以达到同样效果的程度 也就是说 旋转和位移是相对于target的
            transform.position = Target.position;
            transform.rotation = Target.rotation;
        }
    }
}
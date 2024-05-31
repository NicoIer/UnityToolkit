using UnityEngine;

namespace UnityToolkit
{
    public static class CameraExtensions
    {
        /// <summary>
        /// 获取透视相机的矩形区域
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static Rect GetPerspectiveCameraRect(this Camera camera, float distance)
        {
            Vector3 cameraPos = camera.transform.position;
            if (distance <= 0 || distance > camera.farClipPlane)
            {
                distance = camera.farClipPlane;
            }

            // fov表示相机的垂直视野角度
            // 2 * tan(fov/2) = height / distance
            float height = 2 * distance * Mathf.Tan(camera.fieldOfView / 2 * Mathf.Deg2Rad);
            float width = height * camera.aspect;

            Rect rect = new Rect
            {
                xMin = cameraPos.x - width / 2,
                xMax = cameraPos.x + width / 2,
                yMin = cameraPos.y - height / 2,
                yMax = cameraPos.y + height / 2
            };
            return rect;
        }

        /// <summary>
        /// 获取正交相机的矩形区域
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static Rect GetOrthographicCameraRect(this Camera camera)
        {
            Vector3 cameraPos = camera.transform.position;
            float height = camera.orthographicSize * 2; //高度 = 正交相机的size*2
            float width = height * camera.aspect; //宽度 = 高度*宽高比

            Rect rect = new Rect
            {
                xMin = cameraPos.x - width / 2,
                xMax = cameraPos.x + width / 2,
                yMin = cameraPos.y - height / 2,
                yMax = cameraPos.y + height / 2
            };
            return rect;
        }
    }
}
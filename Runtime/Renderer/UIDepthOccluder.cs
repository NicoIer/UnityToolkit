using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityToolkit
{
    [RequireComponent(typeof(Image))]
    public class UIDepthOccluder : MonoBehaviour
    {
        public static readonly List<UIDepthOccluder> s_ActiveOccluders = new List<UIDepthOccluder>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            s_ActiveOccluders.Clear();
        }

        [Tooltip("Mesh defining the occluder shape. Vertices should be in normalized [0,1] space.")]
        public Mesh occluderMesh;

        Image _image;
        RectTransform _rectTransform;
        bool _registered;
        readonly Vector3[] _worldCorners = new Vector3[4];

        void Awake()
        {
            _image = GetComponent<Image>();
            _rectTransform = GetComponent<RectTransform>();
        }

        void OnEnable()
        {
            if (_image.canvasRenderer.GetInheritedAlpha() >= 1f)
            {
                s_ActiveOccluders.Add(this);
                _registered = true;
            }
        }

        void OnDisable()
        {
            if (_registered)
            {
                s_ActiveOccluders.Remove(this);
                _registered = false;
            }
        }

        void LateUpdate()
        {
            bool visible = _image.canvasRenderer.GetInheritedAlpha() >= 1f;
            if (visible && !_registered)
            {
                s_ActiveOccluders.Add(this);
                _registered = true;
            }
            else if (!visible && _registered)
            {
                s_ActiveOccluders.Remove(this);
                _registered = false;
            }
        }

        /// <summary>
        /// Returns a matrix that transforms mesh vertices from normalized [0,1] local space
        /// to viewport [0,1] space matching the RectTransform screen coverage.
        /// </summary>
        public Matrix4x4 GetLocalToViewportMatrix(Camera cam)
        {
            _rectTransform.GetWorldCorners(_worldCorners);

            Vector2 viewMin, viewMax;
            var canvas = _image.canvas;

            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                viewMin = new Vector2(_worldCorners[0].x / Screen.width, _worldCorners[0].y / Screen.height);
                viewMax = new Vector2(_worldCorners[2].x / Screen.width, _worldCorners[2].y / Screen.height);
            }
            else
            {
                var canvasCam = (canvas != null && canvas.worldCamera != null) ? canvas.worldCamera : cam;
                var screenMin = canvasCam.WorldToScreenPoint(_worldCorners[0]);
                var screenMax = canvasCam.WorldToScreenPoint(_worldCorners[2]);
                viewMin = new Vector2(screenMin.x / Screen.width, screenMin.y / Screen.height);
                viewMax = new Vector2(screenMax.x / Screen.width, screenMax.y / Screen.height);
            }

            // Mesh [0,1] -> viewport [viewMin, viewMax]
            // Scale then translate
            var sx = viewMax.x - viewMin.x;
            var sy = viewMax.y - viewMin.y;

            return new Matrix4x4(
                new Vector4(sx, 0, 0, 0),
                new Vector4(0, sy, 0, 0),
                new Vector4(0, 0, 1, 0),
                new Vector4(viewMin.x, viewMin.y, 0, 1)
            );
        }
    }
}

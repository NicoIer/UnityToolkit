#if UNITY_5_6_OR_NEWER
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Pool;

namespace UnityToolkit
{
    public static class RayCaster2D
    {
        public const int MaxCount = 100;
        private static Collider2D[] collider2Ds = new Collider2D[MaxCount];

        public static int OverlapCircle(Vector2 position, float radius, out Collider2D[] cols,
            LayerMask layerMask)
        {
            int count = Physics2D.OverlapCircleNonAlloc(position, radius, collider2Ds, layerMask);
            cols = collider2Ds;
            return count;
        }

        public static int OverlapBox(out Collider2D[] results, Vector3 point, Vector2 size, float facingAngle,
            LayerMask layer)
        {
            int ans = Physics2D.OverlapBoxNonAlloc(point, size, facingAngle, collider2Ds, layer);
            results = collider2Ds;
            return ans;
        }
    }

    /// <summary>
    /// 游戏中的射线检测管理器
    /// </summary>
    public static class RayCaster
    {
        const int MaxCount = 100;
        private static readonly Collider[] _colliders = new Collider[MaxCount];
        private static readonly RaycastHit[] _raycastHit = new RaycastHit[MaxCount];
        private static Camera _mainCamera;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Init()
        {
            _mainCamera = Camera.main;
            Application.quitting -= ClearStatic;
            Application.quitting += ClearStatic;
        }

        private static void ClearStatic()
        {
            _mainCamera = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetMainCamera(Camera maCamera)
        {
            _mainCamera = maCamera;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Ray GetMainCameraRay(Vector3 mousePosition)
        {
            return _mainCamera.ScreenPointToRay(mousePosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int OverlapSphereAll(Vector3 origin, float radius,out Collider[] cols,
            LayerMask layerMask)
        {
            //将原来的colliders全部变成null
            cols = RayCaster._colliders;
            return Physics.OverlapSphereNonAlloc(origin, radius, cols, layerMask);
        }

        public static bool OverLabSphereTarget<T>(out List<T> objs, Vector3 origin, float radius, LayerMask layerMask)
        {
            int count = Physics.OverlapSphereNonAlloc(origin, radius, RayCaster._colliders, layerMask);
            objs = new List<T>();
            for (int i = 0; i < count; i++)
            {
                var col = _colliders[i];
                if (col.gameObject.TryGetComponent<T>(out T obj))
                {
                    objs.Add(obj);
                }
            }

            return objs.Count != 0;
        }


        /// <summary>
        /// 拿到第一个具有 T 类型组件的 碰撞体
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="collider"></param>
        /// <param name="origin"></param>
        /// <param name="radius"></param>
        /// <param name="direction"></param>
        /// <param name="maxDistance"></param>
        /// <param name="layerMask"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SphereCastOne<T>(out T obj, out Collider collider, Vector3 origin, float radius,
            Vector3 direction,
            float maxDistance, LayerMask layerMask)
        {
            int hitCount = Physics.SphereCastNonAlloc(origin, radius, direction, _raycastHit, maxDistance, layerMask);
            //从检测到的碰撞体中找到第一个符合条件的碰撞体
            for (int i = 0; i < hitCount; i++)
            {
                if (!_raycastHit[i].collider.gameObject.TryGetComponent(out T t)) continue;
                obj = t;
                collider = _raycastHit[i].collider;
                return true;
            }

            obj = default;
            collider = null;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SphereCastOne(out Collider collider, Vector3 origin, float radius, Vector3 direction,
            float maxDistance, LayerMask layerMask)
        {
            int hitCount = Physics.SphereCastNonAlloc(origin, radius, direction, _raycastHit, maxDistance, layerMask);
            if (hitCount > 0)
            {
                collider = _raycastHit[0].collider;
                return true;
            }

            collider = null;
            return false;
        }

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static void SphereCastAll(Vector3 origin, float radius, Vector3 direction, float maxDistance,
        //     LayerMask layerMask)
        // {
        //     int hitCount = Physics.SphereCastNonAlloc(origin, radius, direction, raycastHit, maxDistance, layerMask);
        // }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RayCast(Vector3 origin, Vector3 direction, float maxDistance, LayerMask layerMask,
            out RaycastHit hit)
        {
            return Physics.Raycast(origin, direction, out hit, maxDistance, layerMask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RayCast(Ray ray, out RaycastHit hit, float maxDistance, LayerMask layerMask)
        {
            return Physics.Raycast(ray, out hit, maxDistance, layerMask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetMousePointedRayHit(out RaycastHit hit, LayerMask layerMask)
        {
            var ray = GetMainCameraRay(Input.mousePosition);
            if (RayCast(ray, out hit, float.MaxValue, layerMask))
            {
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckBox(BoxCollider box, LayerMask collisionLayer)
        {
            bool colliderEnabled = box.enabled;
            box.enabled = false;
            var bounds = box.bounds;
            int count = Physics.OverlapBoxNonAlloc(bounds.center, box.size / 2, _colliders, Quaternion.identity,
                collisionLayer);
            box.enabled = colliderEnabled;
            return count != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool BoxFind<T>(out T tar, out Collider col, Vector3 position, Vector3 halfSize,
            LayerMask layerMask)
        {
            // print("BoxFind");
            //以position为中心 进行 halfSize大小的盒子检测
            int count = Physics.OverlapBoxNonAlloc(position, halfSize, _colliders, Quaternion.identity,
                layerMask);
            //从检测到的碰撞体中找到第一个符合条件的碰撞体
            for (int i = 0; i < count; i++)
            {
                if (!_colliders[i].gameObject.TryGetComponent(out T t)) continue;
                tar = t;
                col = _colliders[i];
                return true;
            }

            tar = default;
            col = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool BoxFindAll<T>(out List<T> tar, out List<Collider> col, out int count,
            Vector3 position,
            Vector3 halfSize, LayerMask layerMask)
        {
            // print("BoxFind");
            //以position为中心 进行 halfSize大小的盒子检测
            count = Physics.OverlapBoxNonAlloc(position, halfSize, _colliders, Quaternion.identity,
                layerMask);
            // Debug.Log("box find count:" + count);
            //从检测到的碰撞体中找到第一个符合条件的碰撞体
            tar = ListPool<T>.Get();
            col = ListPool<Collider>.Get();
            for (int i = 0; i < count; i++)
            {
                if (!_colliders[i].gameObject.TryGetComponent(out T t)) continue;
                tar.Add(t);
                col.Add(_colliders[i]);
            }

            count = tar.Count;
            return count != 0;
        }

        public static int BoxCheck(Vector3 position, Vector3 halfSize, LayerMask layerMask, out Collider[] colliders,
            Quaternion quaternion)
        {
            int count = Physics.OverlapBoxNonAlloc(position, halfSize, _colliders, quaternion, layerMask);
            colliders = _colliders;
            return count;
        }

        public static int OverlapBox(out Collider[] colliders, Vector3 handPosition, Vector3 halfExtents,
            Quaternion identity, LayerMask entityLayer)
        {
            int count = Physics.OverlapBoxNonAlloc(handPosition, halfExtents, _colliders, identity, entityLayer);
            colliders = _colliders;
            return count;
        }

        public static int OverlapBoxAll(Vector3 center, Vector3 halfExtents,out Collider[] colliders, LayerMask layer)
        {
            int count = Physics.OverlapBoxNonAlloc(center, halfExtents, _colliders, Quaternion.identity, layer);
            colliders = _colliders;
            return count;
        }
    }
}
#endif
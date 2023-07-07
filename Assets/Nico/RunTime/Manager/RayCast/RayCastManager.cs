
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Nico
{
    /// <summary>
    /// 游戏中的射线检测管理器
    /// </summary>
    public static class RayCastManager
    {
        const int MAX_COUNT = 25;
        private static readonly Collider[] _colliders = new Collider[MAX_COUNT];
        private static readonly RaycastHit[] _raycastHit = new RaycastHit[MAX_COUNT];
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
        public static bool OverLabSphereAll(out Collider[] cols, out int count, Vector3 origin, float radius,
            LayerMask layerMask)
        {
            //将原来的colliders全部变成null
            var hitCount = Physics.OverlapSphereNonAlloc(origin, radius, RayCastManager._colliders, layerMask);
            if (hitCount > 0)
            {
                cols = RayCastManager._colliders;
                count = hitCount;
                return true;
            }

            cols = null;
            count = 0;
            return false;
        }

        public static bool OverLabSphereTarget<T>(out List<T> objs, Vector3 origin, float radius, LayerMask layerMask)
        {
            int count = Physics.OverlapSphereNonAlloc(origin,radius,RayCastManager._colliders,layerMask);
            objs = new List<T>();
            for (int i = 0; i < count; i++)
            {
                var col = _colliders[i];
                if(col.gameObject.TryGetComponent<T>(out T obj))
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
            tar = new List<T>();
            col = new List<Collider>();
            for (int i = 0; i < count; i++)
            {
                if (!_colliders[i].gameObject.TryGetComponent(out T t)) continue;
                tar.Add(t);
                col.Add(_colliders[i]);
            }

            count = tar.Count;
            return count != 0;
        }
    }
}
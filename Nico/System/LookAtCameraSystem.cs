using System.Collections.Generic;
using Nico.Components;
using Nico.Job;
using UnityEngine;
using UnityEngine.Jobs;

namespace Nico.Design
{
    public class LookAtCameraSystem : SceneSingleton<LookAtCameraSystem>
    {
        readonly List<Transform> lookAtCameras = new();
        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
            foreach (var component in FindObjectsOfType<LookAtCameraComponent>())
            {
                lookAtCameras.Add(component.transform);
            }
        }

        #region 注册 取消 注册

        public void Register(Transform lookAtCamera)
        {
            lookAtCameras.Add(lookAtCamera);
        }

        public void UnRegister(Transform lookAtCamera)
        {
            lookAtCameras.Remove(lookAtCamera);
        }

        #endregion

        private void Update()
        {
            // 1. 获取摄像机的位置
            // 2. 获取所有挂载了 LookAtCamera 的物体
            // 3. 让所有物体的朝向都是摄像机的位置
            if (mainCamera is null) return;

            var cameraTransform = mainCamera.transform;
            //开启Job 执行任务
            var job = new LookAtCameraJob
            {
                cameraPosition = cameraTransform.position,
                cameraRotation = cameraTransform.rotation
            };
            var handle = job.Schedule(new TransformAccessArray(lookAtCameras.ToArray()));
            handle.Complete();
        }
    }
}
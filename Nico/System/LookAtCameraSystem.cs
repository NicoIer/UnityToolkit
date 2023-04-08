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
        public Camera mainCamera;

        private void Start()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            foreach (var component in FindObjectsOfType<LookAtCameraComponent>())
            {
                lookAtCameras.Add(component.transform);
            }
        }

        public void SetCamera(Camera maincamera)
        {
            this.mainCamera = maincamera;
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
            if (mainCamera == null) return;

            var cameraTransform = mainCamera.transform;
            //开启Job 执行任务
            var job = new LookAtCameraJob
            {
                cameraPosition = cameraTransform.position,
                cameraRotation = cameraTransform.rotation
            };
            var array = new TransformAccessArray(lookAtCameras.ToArray());
            var handle = job.Schedule(array);
            array.Dispose();
            handle.Complete();
            
        }
    }
}
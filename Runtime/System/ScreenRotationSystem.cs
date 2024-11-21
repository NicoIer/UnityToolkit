#if UNITY_2021_3_OR_NEWER

using UnityEngine;
using UnityToolkit;
using Screen = UnityEngine.Device.Screen;

namespace UnityToolkit
{
    [System.Serializable]
    public struct ScreenRotationSystem : ISystem, IOnUpdate
    {
        public void OnUpdate(float deltaTime)
        {
            if (Screen.orientation == ScreenOrientation.Portrait ||
                Screen.orientation == ScreenOrientation.PortraitUpsideDown)
            {
                Screen.orientation = ScreenOrientation.AutoRotation;
                Screen.autorotateToLandscapeRight = true;
                Screen.autorotateToLandscapeLeft = true;
                Screen.autorotateToPortraitUpsideDown = false;
                Screen.autorotateToPortrait = false;
            }
        }

        public void Dispose()
        {
        }
    }
}
#endif

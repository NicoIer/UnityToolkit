#if UNITY_2017_1_OR_NEWER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityToolkit
{
    public class ExperimentalAttribute : Attribute
    {
    }

    public static class VolumeExtensions
    {
        // /// <summary>
        // /// Reload VolumeManager
        // /// Base on UniversalRP 14.0.9
        // /// Using Reflection to access private fields and methods
        // /// Expensive operation, use it only when necessary
        // /// </summary>
        // /// <param name="volumeManager"></param>
        // [Experimental]
        // public static void Reload(this VolumeManager volumeManager)
        // {
        //     // 相当于重新走一遍构造函数 加载热更代码后 重新加载VolumeManager 这样就可以扫到热更代码中的VolumeComponent
        //     MethodInfo reloadBaseTypesMethodInfo = typeof(VolumeManager).GetMethod("ReloadBaseTypes",
        //         BindingFlags.Instance | BindingFlags.NonPublic);
        //     if (reloadBaseTypesMethodInfo == null)
        //     {
        //         Debug.LogError($"{nameof(VolumeManager)}.ReloadBaseTypes not found. check your UniversalRP version");
        //         return;
        //     }
        //     reloadBaseTypesMethodInfo.Invoke(volumeManager, null); 
        //     // volumeManager.ReloadBaseTypes();
        //
        //
        //     VolumeStack stack = volumeManager.CreateStack();
        //
        //
        //     FieldInfo stackFieldInfo = typeof(VolumeManager).GetField("m_DefaultStack",
        //         BindingFlags.Instance | BindingFlags.NonPublic);
        //     if (stackFieldInfo == null)
        //     {
        //         Debug.LogError($"{nameof(VolumeManager)}.m_DefaultStack not found. check your UniversalRP version");
        //         return;
        //     }
        //
        //     stackFieldInfo.SetValue(volumeManager, stack); 
        //     // volumeManager.m_DefaultStack = stack; 
        //     
        //     volumeManager.stack = stack;
        // }


        /// <summary>
        /// Add a VolumeComponent to VolumeManager
        /// Base on UniversalRP 14.0.9
        /// Only need to call when using HybridCLR or ILRuntime
        /// </summary>
        /// <param name="volumeManager"></param>
        /// <typeparam name="TVolumeComponent"></typeparam>
        [Experimental]
        public static void AddDefault<TVolumeComponent>(this VolumeManager volumeManager)
            where TVolumeComponent : VolumeComponent
        {
            if (volumeManager.stack.GetComponent<TVolumeComponent>() != null)
            {
                Debug.LogWarning($"VolumeManager already contains {typeof(TVolumeComponent)}");
                return;
            }

            if (volumeManager.baseComponentTypeArray.Contains(typeof(TVolumeComponent)))
            {
                Debug.LogWarning($"VolumeManager already contains {typeof(TVolumeComponent)}");
                return;
            }


            PropertyInfo baseComponentTypeArrayInfo =
                typeof(VolumeManager).GetProperty("baseComponentTypeArray",
                    BindingFlags.Instance | BindingFlags.Public);
            if (baseComponentTypeArrayInfo == null)
            {
                Debug.LogError(
                    $"{nameof(VolumeManager)}.baseComponentTypeArray not found. check your UniversalRP version");
                return;
            }

            // do something like this
            // volumeManager.baseComponentTypeArray = volumeManager.baseComponentTypeArray.Append(typeof(TVolumeComponent)).ToArray();
            Type[] baseComponentTypeArray = (Type[])baseComponentTypeArrayInfo.GetValue(volumeManager);
            Type[] newBaseComponentTypeArray = new Type[baseComponentTypeArray.Length + 1];
            Array.Copy(baseComponentTypeArray, newBaseComponentTypeArray, baseComponentTypeArray.Length);
            newBaseComponentTypeArray[baseComponentTypeArray.Length] = typeof(TVolumeComponent);
            baseComponentTypeArrayInfo.SetValue(volumeManager, newBaseComponentTypeArray);

            if (!volumeManager.baseComponentTypeArray.Contains(typeof(TVolumeComponent)))
            {
                Debug.LogError($"{nameof(VolumeManager)}.baseComponentTypeArray no change. I don't know why");
                return;
            }


            // refresh stack
            VolumeStack stack = volumeManager.stack;
            stack.AddComponent(ScriptableObject.CreateInstance<TVolumeComponent>());
        }

        /// <summary>
        /// Add a VolumeComponent to VolumeStack
        /// Base on UniversalRP 14.0.9
        /// Only need to call when using HybridCLR or ILRuntime
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="component"></param>
        /// <typeparam name="TVolumeComponent"></typeparam>
        [Experimental]
        private static void AddComponent<TVolumeComponent>(this VolumeStack stack, TVolumeComponent component)
            where TVolumeComponent : VolumeComponent
        {
            if (stack.GetComponent<TVolumeComponent>() != null)
            {
                Debug.LogWarning($"VolumeStack already contains {typeof(TVolumeComponent)}");
                return;
            }

            // stack.requiresReset = true;
            FieldInfo stackRequiresReset =
                typeof(VolumeStack).GetField("requiresReset", BindingFlags.Instance | BindingFlags.NonPublic);
            if (stackRequiresReset == null)
            {
                Debug.LogError($"{nameof(VolumeStack)}.requiresReset not found. check your UniversalRP version");
                return;
            }

            stackRequiresReset.SetValue(stack, true);

            // var defaultParameters = stack.defaultParameters;
            FieldInfo stackDefaultParameters =
                typeof(VolumeStack).GetField("defaultParameters", BindingFlags.Instance | BindingFlags.NonPublic);

            if (stackDefaultParameters == null)
            {
                Debug.LogError($"{nameof(VolumeStack)}.defaultParameters not found. check your UniversalRP version");
                return;
            }

            FieldInfo stackComponents =
                typeof(VolumeStack).GetField("components", BindingFlags.Instance | BindingFlags.NonPublic);
            if (stackComponents == null)
            {
                Debug.LogError($"{nameof(VolumeStack)}.components not found. check your UniversalRP version");
                return;
            }

            (VolumeParameter parameter, VolumeParameter defaultValue)[] currentDefaultParameters =
                ((VolumeParameter parameter, VolumeParameter defaultValue)[])stackDefaultParameters.GetValue(stack);
            // create new
            (VolumeParameter parameter, VolumeParameter defaultValue)[] newDefaultParameters =
                new (VolumeParameter parameter, VolumeParameter defaultValue)[currentDefaultParameters.Length + 1];
            // Copy
            Array.Copy(currentDefaultParameters, newDefaultParameters, currentDefaultParameters.Length);
            // Add new to stack dict
            // stack.components.Add(typeof(TVolumeComponent), component);
            Dictionary<Type, VolumeComponent> components =
                (Dictionary<Type, VolumeComponent>)stackComponents.GetValue(stack);
            components.Add(component.GetType(), component);

            // Add Parameter
            for (int i = 0; i < component.parameters.Count; i++)
            {
                newDefaultParameters[currentDefaultParameters.Length] = new()
                {
                    parameter = component.parameters[i],
                    defaultValue = component.parameters[i].Clone() as VolumeParameter,
                };
            }

            stackDefaultParameters.SetValue(stack, newDefaultParameters);
        }
    }
}
#endif
using System;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace Nico
{
    public static class NetworkLoop
    {
        internal enum AddMode
        {
            Beginning,
            End
        }

        public static Action onEarlyUpdate;
        public static Action onLateUpdate;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ResetStatics()
        {
            onEarlyUpdate = null;
            onLateUpdate = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void RuntimeInitializeOnLoad()
        {
            PlayerLoopSystem playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            AddToPlayerLoop(NetworkEarlyUpdate, typeof(NetworkLoop), ref playerLoop, typeof(EarlyUpdate), AddMode.End);
            AddToPlayerLoop(NetworkLateUpdate, typeof(NetworkLoop), ref playerLoop, typeof(PreLateUpdate), AddMode.End);
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        internal static bool AddToPlayerLoop(PlayerLoopSystem.UpdateFunction function, Type ownerType,
            ref PlayerLoopSystem playerLoop, Type playerLoopSystemType, AddMode addMode)
        {
            // did we find the type? e.g. EarlyUpdate/PreLateUpdate/etc.
            if (playerLoop.type == playerLoopSystemType)
            {
                // debugging
                //Debug.Log($"Found playerLoop of type {playerLoop.type} with {playerLoop.subSystemList.Length} Functions:");
                //foreach (PlayerLoopSystem sys in playerLoop.subSystemList)
                //    Debug.Log($"  ->{sys.type}");

                // make sure the function wasn't added yet.
                // with domain reload disabled, it would otherwise be added twice:
                // fixes: https://github.com/MirrorNetworking/Mirror/issues/3392
                if (Array.FindIndex(playerLoop.subSystemList, (s => s.updateDelegate == function)) != -1)
                {
                    // loop contains the function, so return true.
                    return true;
                }

                // resize & expand subSystemList to fit one more entry
                int oldListLength = (playerLoop.subSystemList != null) ? playerLoop.subSystemList.Length : 0;
                Array.Resize(ref playerLoop.subSystemList, oldListLength + 1);

                // IMPORTANT: always insert a FRESH PlayerLoopSystem!
                // We CAN NOT resize and then OVERWRITE an entry's type/loop.
                // => PlayerLoopSystem has native IntPtr loop members
                // => forgetting to clear those would cause undefined behaviour!
                // see also: https://github.com/vis2k/Mirror/pull/2652
                PlayerLoopSystem system = new PlayerLoopSystem
                {
                    type = ownerType,
                    updateDelegate = function
                };

                // prepend our custom loop to the beginning
                if (addMode == AddMode.Beginning)
                {
                    // shift to the right, write into first array element
                    Array.Copy(playerLoop.subSystemList, 0, playerLoop.subSystemList, 1,
                        playerLoop.subSystemList.Length - 1);
                    playerLoop.subSystemList[0] = system;
                }
                // append our custom loop to the end
                else if (addMode == AddMode.End)
                {
                    // simply write into last array element
                    playerLoop.subSystemList[oldListLength] = system;
                }

                // debugging
                //Debug.Log($"New playerLoop of type {playerLoop.type} with {playerLoop.subSystemList.Length} Functions:");
                //foreach (PlayerLoopSystem sys in playerLoop.subSystemList)
                //    Debug.Log($"  ->{sys.type}");

                return true;
            }

            // recursively keep looking
            if (playerLoop.subSystemList != null)
            {
                for (int i = 0; i < playerLoop.subSystemList.Length; ++i)
                {
                    if (AddToPlayerLoop(function, ownerType, ref playerLoop.subSystemList[i], playerLoopSystemType,
                            addMode))
                        return true;
                }
            }

            return false;
        }

        static void NetworkEarlyUpdate()
        {
            if (!Application.isPlaying) return;
            onEarlyUpdate?.Invoke();
        }

        static void NetworkLateUpdate()
        {
            if (!Application.isPlaying) return;
            onLateUpdate?.Invoke();
        }
    }
}
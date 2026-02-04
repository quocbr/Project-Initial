using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
#if UNITY_EDITOR
using Unity.Collections.LowLevel.Unsafe;
#endif

namespace quocbr.Common
{
    
    public static class NativeLeakDetectionBoot
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnableLeakDetection()
        {
#if UNITY_EDITOR
            NativeLeakDetection.Mode = NativeLeakDetectionMode.EnabledWithStackTrace;
#endif
        }
    }
}
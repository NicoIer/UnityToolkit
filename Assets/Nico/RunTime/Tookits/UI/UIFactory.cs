using System;
using UnityEngine;

namespace Nico.UI
{
    internal static class UIFactory<T> where T : UIInterface
    {
        internal static Func<T> createCs;
        internal static Func<GameObject> createObj;
    }
}
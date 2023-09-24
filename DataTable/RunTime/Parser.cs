#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Nico.Editor
{
    public delegate bool ParseDelegate<in TData, TResult>(TData value, out TResult result);

    public static class Parser<TData, TResult>
    {
        public static ParseDelegate<TData, TResult> parser;
    }
    
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Struct, Inherited = false)]
    public class ParseAttribute : Attribute
    {
    }
}
#endif
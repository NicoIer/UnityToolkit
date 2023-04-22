using System;
using UnityEngine;

namespace Nico.Data
{
    public abstract class Container : ScriptableObject , IInitializable
    {
        public abstract T Get<T>(int id) where T : ContainerData;

        public abstract Type GetDataType();
        public abstract void Init();
    }
}
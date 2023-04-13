using System;
using UnityEngine;

namespace Nico.Data
{
    public abstract class Container : ScriptableObject
    {
        public abstract T Get<T>(int id) where T : ContainerData;

        public abstract Type GetDataType();
    }
}
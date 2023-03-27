using System.Collections.Generic;
using UnityEngine;

namespace Nico.Design
{
    public class PoolSetting : ScriptableObject
    {
        [field: SerializeField] public List<GameObject> prefabs { get; private set; }
    }
}
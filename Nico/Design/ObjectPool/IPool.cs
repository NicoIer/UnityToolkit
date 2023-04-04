

using System;
using UnityEngine;

namespace Nico.Design
{
    public interface IPool
    {
         void Return<T>(T poolObj) where T : IPoolObj;

         T Get<T>() where T : IPoolObj;
         IPoolObj Get(Type objType);
    }
}
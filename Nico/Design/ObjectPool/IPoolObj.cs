using UnityEngine;

namespace Nico.Design
{
    public interface IPoolObj
    {
        GameObject GetGameObject();
        void OnReturn();
        void OnGet();
    }
}
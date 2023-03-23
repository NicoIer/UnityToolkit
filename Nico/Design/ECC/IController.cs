using UnityEngine;

namespace Nico.ECC
{
    /// <summary>
    /// 游戏对象的控制器接口 用于控制游戏对象的行为 将游戏物体的Update逻辑更新拆分到多个控制器中
    /// </summary>
    public interface IController<out T> where T : MonoBehaviour
    {
        T Owner { get; }
        void Update();
    }
}
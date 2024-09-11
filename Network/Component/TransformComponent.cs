using System;
using System.Text;
using MemoryPack;
using UnityToolkit.MathTypes;

namespace Network
{
    /// <summary>
    /// 坐标系
    /// </summary>
    public enum CoordinateSpace : byte
    {
        World = 0, // 世界坐标
        Local = 1, // 本地坐标
    }


    [Serializable]
    [MemoryPackable]
    public partial class TransformComponent : INetworkComponent
    {
        /// <summary>
        /// 掩码 对应位置 旋转 缩放 有时候我们不希望上传所有的数据
        /// </summary>
        [Flags]
        public enum Mask : byte
        {
            Pos = 1 << 0, //1
            Rotation = 1 << 1, //2
            Scale = 1 << 2, //4
            All = Pos | Rotation | Scale
        }

        public bool interpolation;
        public CoordinateSpace coordinateSpace;
        public Mask mask;
        public Vector3 pos;
        public Quaternion rotation;
        public Vector3 scale;

        public event Action OnUpdated;


        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("[transform] ");
            builder.Append($"CoordinateSpace: {coordinateSpace} ");
            builder.Append($"Mask: {mask} ");
            if (mask.HasFlag(Mask.Pos))
            {
                builder.Append($"{pos} ");
            }

            if (mask.HasFlag(Mask.Rotation))
            {
                builder.Append($"{rotation} ");
            }

            if (mask.HasFlag(Mask.Scale))
            {
                builder.Append($"{scale} ");
            }

            return builder.ToString();
        }

        private TransformComponent _temp;

        public void UpdateFromPacket(in NetworkComponentPacket packet)
        {
            if (packet.mask.HasFlag(NetworkComponentPacket.Mask.Type) &&
                packet.type != NetworkId<TransformComponent>.Value)
            {
                NetworkLogger.Warning(
                    $"{nameof(TransformComponent)} 从 {packet.type} 反序列化为 {NetworkId<TransformComponent>.Value} 异常");
                return;
            }

            if (_temp == null)
            {
                _temp = new TransformComponent();
            }

            // TransformComponent component = MemoryPackSerializer.Deserialize<TransformComponent>(packet.data);
            TransformComponent component = _temp;
            ArraySegment<byte> temp = packet.data;
            component.interpolation = NetworkBuffer.ReadBlittable<bool>(ref temp);
            component.coordinateSpace = NetworkBuffer.ReadBlittable<CoordinateSpace>(ref temp);
            component.mask = NetworkBuffer.ReadBlittable<Mask>(ref temp);

            if (component.mask.HasFlag(Mask.Pos))
            {
                component.pos = new Vector3
                {
                    x = NetworkBuffer.ReadBlittable<float>(ref temp),
                    y = NetworkBuffer.ReadBlittable<float>(ref temp),
                    z = NetworkBuffer.ReadBlittable<float>(ref temp)
                };
            }

            if (component.mask.HasFlag(Mask.Rotation))
            {
                component.rotation = new Quaternion
                {
                    x = NetworkBuffer.ReadBlittable<float>(ref temp),
                    y = NetworkBuffer.ReadBlittable<float>(ref temp),
                    z = NetworkBuffer.ReadBlittable<float>(ref temp),
                    w = NetworkBuffer.ReadBlittable<float>(ref temp)
                };
            }

            if (component.mask.HasFlag(Mask.Scale))
            {
                component.scale = new Vector3
                {
                    x = NetworkBuffer.ReadBlittable<float>(ref temp),
                    y = NetworkBuffer.ReadBlittable<float>(ref temp),
                    z = NetworkBuffer.ReadBlittable<float>(ref temp)
                };
            }


            bool changed = false;

            if (mask != component.mask)
            {
                mask = component.mask;
                changed |= true;
            }

            if (coordinateSpace != component.coordinateSpace)
            {
                coordinateSpace = component.coordinateSpace;
                changed |= true;
            }

            if (component.mask.HasFlag(Mask.Pos))
            {
                pos = component.pos;
                changed |= true;
            }

            if (component.mask.HasFlag(Mask.Rotation))
            {
                rotation = component.rotation;
                changed |= true;
            }

            if (component.mask.HasFlag(Mask.Scale))
            {
                scale = component.scale;
                changed |= true;
            }

            if (changed)
            {
                // NetworkLogger.Debug($"OnUpdated {GetHashCode()}");
                OnUpdated?.Invoke();
            }
        }

        public NetworkComponentPacket ToDummyPacket(NetworkBuffer buffer)
        {
            buffer.WriteBlittable(interpolation);
            buffer.WriteBlittable(coordinateSpace); 
            buffer.WriteBlittable(mask);
            if (mask.HasFlag(Mask.Pos))
            {
                buffer.WriteBlittable(pos.x);
                buffer.WriteBlittable(pos.y);
                buffer.WriteBlittable(pos.z);
            }

            if (mask.HasFlag(Mask.Rotation))
            {
                buffer.WriteBlittable(rotation.x);
                buffer.WriteBlittable(rotation.y);
                buffer.WriteBlittable(rotation.z);
                buffer.WriteBlittable(rotation.w);
            }

            if (mask.HasFlag(Mask.Scale))
            {
                buffer.WriteBlittable(scale.x);
                buffer.WriteBlittable(scale.y);
                buffer.WriteBlittable(scale.z);
            }

            // MemoryPackSerializer.Serialize(buffer, this);
            var packet = new NetworkComponentPacket
            {
                type = NetworkId<TransformComponent>.Value,
                data = buffer
            };
            packet.mask = NetworkComponentPacket.Mask.Type;
            return packet;
        }
    }
}
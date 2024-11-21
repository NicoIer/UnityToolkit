using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityToolkit;

namespace Network.Server
{
    public class NetworkServer : IServerHandler, IDisposable
    {
        public IServerSocket socket { get; private set; }
        private readonly NetworkBufferPool _bufferPool;
        private readonly NetworkServerMessageHandler _messageHandler;

        public int ConnectionCount { get; private set; }
        public ushort TargetFrameRate { get; private set; }
        public CancellationTokenSource Cts { get; private set; }
        public long DeltaTimeTick { get; private set; }

        private readonly SystemLocator _system;
        private readonly bool _compress;

        #region Status Montior

        /// <summary>
        /// 是否达到了目标帧率 注意 TargetFrameRate = 0 时会GG
        /// </summary>
        public bool ReachFrameRate => DeltaTimeTick > TimeSpan.FromSeconds(1d / TargetFrameRate).Ticks;

        #endregion

        public NetworkServer(IServerSocket socket, ushort targetFrameRate = 0, bool compress = true)
        {
            _compress = compress;
            ConnectionCount = 0;
            this.socket = socket;
            _bufferPool = new NetworkBufferPool(16);
            _messageHandler = new NetworkServerMessageHandler();
            TargetFrameRate = targetFrameRate;
            // Socket event handlers
            this.socket.OnConnected += OnConnected;
            this.socket.OnDataReceived += OnDataReceived;
            this.socket.OnDisconnected += OnDisconnected;
            this.socket.OnDataSent += OnDataSent;

            _system = new SystemLocator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ICommand AddMsgHandler<T>(Action<int, T> handler) where T : INetworkMessage
        {
            return _messageHandler.Add(handler);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSystem GetSystem<TSystem>() where TSystem : ISystem
        {
            return _system.Get<TSystem>();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveSystem<TSystem>() where TSystem : ISystem
        {
            _system.UnRegister<TSystem>();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddSystem<TSystem>(TSystem system) where TSystem : ISystem
        {
            _system.Register(system);
            if (system is IOnInit<NetworkServer> init)
            {
                init.OnInit(this);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddSystem<TSystem>() where TSystem : ISystem, IOnInit<NetworkServer>, new()
        {
            AddSystem(new TSystem());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send<TMessage>(int connectionId, TMessage message, bool noDelay = false)
            where TMessage : INetworkMessage
        {
            NetworkBuffer payloadBuffer = _bufferPool.Get();
            NetworkBuffer packetBuffer = _bufferPool.Get();

            if (_compress)
            {
                NetworkPacker.PackCompressed(message, payloadBuffer, packetBuffer);
            }
            else
            {
                NetworkPacker.Pack(message, payloadBuffer, packetBuffer);
            }


            socket.Send(connectionId, packetBuffer);

            _bufferPool.Return(payloadBuffer);
            _bufferPool.Return(packetBuffer);

            if (noDelay)
            {
                socket.TickOutgoing();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendToAll<T>(T msg, bool noDelay = false) where T : INetworkMessage
        {
            NetworkBuffer payloadBuffer = _bufferPool.Get();
            NetworkBuffer packetBuffer = _bufferPool.Get();

            if (_compress)
            {
                NetworkPacker.PackCompressed(msg, payloadBuffer, packetBuffer);
            }
            else
            {
                NetworkPacker.Pack(msg, payloadBuffer, packetBuffer);
            }

            socket.SendToAll(packetBuffer);

            _bufferPool.Return(payloadBuffer);
            _bufferPool.Return(packetBuffer);
            if (noDelay)
            {
                socket.TickOutgoing();
            }
        }


        public Task Run()
        {
            if (Cts != null)
            {
                NetworkLogger.Error($"NetworkManager:[{this}] is already running");
                return Task.CompletedTask;
            }

            Cts = new CancellationTokenSource();
            socket.Start();
            long maxFrameTime = TimeSpan.FromSeconds(1d / TargetFrameRate).Ticks; // 一帧最大时间
            var run = Task.Run(async () =>
            {
                Stopwatch stopwatch = new Stopwatch();
                float deltaTimeSeconds = 0;
                while (!Cts.Token.IsCancellationRequested)
                {
                    if (TargetFrameRate == 0)
                    {
                        OnUpdate(deltaTimeSeconds); // 执行一帧
                        DeltaTimeTick = stopwatch.ElapsedTicks; // 这帧执行的时间
                        continue;
                    }

                    stopwatch.Restart(); // 重置计时器
                    OnUpdate(DeltaTimeTick); // 执行一帧
                    DeltaTimeTick = stopwatch.ElapsedTicks; // 这帧执行的时间
                    deltaTimeSeconds =  TimeSpan.FromTicks(DeltaTimeTick).Seconds;
                    if (DeltaTimeTick < maxFrameTime) // 达到了帧率 休息一下
                    {
                        await Task.Delay(TimeSpan.FromTicks(maxFrameTime - DeltaTimeTick), Cts.Token);
                    }
                }
            }, Cts.Token);
            return run;
        }

        public void OnConnected(int connectionId)
        {
            ConnectionCount++;
            Send(connectionId, new AssignConnectionIdMessage(connectionId));
        }

        public void OnDataReceived(int connectionId, ArraySegment<byte> data)
        {
            // NetworkLogger.Debug($"Data received from connection {connectionId} cnt:{data.Count}");
            NetworkPacket packet = default;
            if (_compress)
            {
                if (!NetworkPacker.UnpackCompressed(data, out packet))
                {
                    return;
                }

                _messageHandler.Handle(packet.id, connectionId, packet.payload);
                return;
            }

            if (!_compress)
            {
                if (!NetworkPacker.Unpack(data, out packet))
                {
                    NetworkLogger.Error($"[{this}]Unpack error from connection {connectionId}");
                    return;
                }

                _messageHandler.Handle(packet.id, connectionId, packet.payload);
                return;
            }
        }

        public void OnDisconnected(int connectionId)
        {
            NetworkLogger.Info($"Client {connectionId} disconnected");
            // _connections.Remove(connectionId);
            ConnectionCount--;
        }

        public void OnDataSent(int connectionId, ArraySegment<byte> data)
        {
        }

        public void OnUpdate(float deltaTime)
        {
            socket.TickIncoming();

            foreach (var system in _system.systems)
            {
                if (system is IOnUpdate onUpdate)
                {
                    onUpdate.OnUpdate(deltaTime);
                }
            }

            socket.TickOutgoing();
        }

        public void Dispose()
        {
            _system?.Dispose();
            Cts?.Dispose();
            _messageHandler?.Dispose();
        }
    }
}
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityToolkit;

namespace Network.Client
{
    public class NetworkClient : IClientHandler, IDisposable
    {
        public int connectionId { get; private set; }
        public IClientSocket socket { get; private set; }
        private readonly NetworkBufferPool _bufferPool;
        private readonly NetworkClientMessageHandler _messageHandler;
        public ushort TargetFrameRate { get; private set; }
        public CancellationTokenSource Cts { get; private set; }

        public long DeltaTimeTick { get; private set; }

        private readonly SystemLocator _systems;
        private ICommand _disposer;
        private bool _compress;

        public NetworkClient(IClientSocket socket, ushort targetFrameRate = 60, bool compress = true)
        {
            _compress = compress;
            this.socket = socket;
            _bufferPool = new NetworkBufferPool(16);
            _messageHandler = new NetworkClientMessageHandler();
            TargetFrameRate = targetFrameRate;
            _systems = new SystemLocator();
            // Add socket event handlers
            this.socket.OnConnected += OnConnected;
            this.socket.OnDataReceived += OnDataReceived;
            this.socket.OnDisconnected += OnDisconnected;
            this.socket.OnDataSent += OnDataSent;

            _disposer = AddMsgHandler<AssignConnectionIdMessage>(OnAssignConnectionId);
        }

        private void OnAssignConnectionId(AssignConnectionIdMessage server)
        {
            if (connectionId != 0)
            {
                NetworkLogger.Warning($"[{this}]Connection id is already assigned");
            }

            connectionId = server.id;
        }

        public ICommand AddMsgHandler<T>(Action<T> handler) where T : INetworkMessage
        {
            return _messageHandler.Add(handler);
        }

        public TSystem GetSystem<TSystem>() where TSystem : ISystem
        {
            return _systems.Get<TSystem>();
        }

        public void AddSystem<TSystem>(TSystem system) where TSystem : ISystem
        {
            _systems.Register(system);
            if (system is IOnInit<NetworkClient> init)
            {
                init.OnInit(this);
            }
        }

        public void AddSystem<TSystem>() where TSystem : ISystem, IOnInit<NetworkClient>, new()
        {
            AddSystem(new TSystem());
        }

        /// <summary>
        /// 连接到服务器
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="autoTick">是否开启一个任务在后台处理消息，Unity中我们希望在一帧开始前处理所有网络消息，一帧结束后发送所有消息</param>
        /// <returns></returns>
        public Task Run(Uri uri, bool autoTick = true)
        {
            if (Cts is { IsCancellationRequested: false })
            {
                NetworkLogger.Error($"NetworkManager:[{this}] is already running");
                return Task.CompletedTask;
            }

            Cts = new CancellationTokenSource();
            socket.Connect(uri);

            if (!autoTick)
            {
                return Task.CompletedTask;
            }

            var run = Task.Run(async () =>
            {
                Stopwatch stopwatch = new Stopwatch();
                while (!Cts.Token.IsCancellationRequested)
                {
                    if (TargetFrameRate == 0)
                    {
                        stopwatch.Restart(); // 重置计时器
                        OnUpdate(); // 执行一帧
                        DeltaTimeTick = stopwatch.ElapsedTicks; // 这帧执行的时间
                        continue;
                    }

                    long frameTimeTicks = TimeSpan.FromSeconds(1d / TargetFrameRate).Ticks;
                    stopwatch.Restart();
                    OnUpdate();
                    DeltaTimeTick = stopwatch.ElapsedTicks;
                    // 当前帧用于睡眠的时间
                    var sleepTimeTicks = frameTimeTicks - DeltaTimeTick;

                    if (sleepTimeTicks > 0)
                    {
                        await Task.Delay(TimeSpan.FromTicks(sleepTimeTicks), Cts.Token);
                    }
                    // else
                    // {
                    //     NetworkLogger.Warning(
                    //         $"[{this}]Frame rate is low:{TimeSpan.FromTicks(DeltaTimeTick).Milliseconds}ms>={TimeSpan.FromTicks(frameTimeTicks).Milliseconds}ms");
                    // }
                }
            }, cancellationToken: Cts.Token);
            return run;
        }

        public void Stop()
        {
            socket.Disconnect();
            Cts.Cancel();
            Cts.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send<TMessage>(TMessage msg, bool noDelay = false) where TMessage : INetworkMessage
        {
            NetworkBuffer payloadBuffer = _bufferPool.Get();
            NetworkBuffer packetBuffer = _bufferPool.Get();

            if (_compress)
            {
                NetworkPacker.PackCompressed(msg, payloadBuffer, packetBuffer);
            }

            {
                NetworkPacker.Pack(msg, payloadBuffer, packetBuffer);
            }


            socket.Send(packetBuffer);

            _bufferPool.Return(payloadBuffer);
            _bufferPool.Return(packetBuffer);

            if (noDelay)
            {
                socket.TickOutgoing();
            }
        }

        public void OnConnected()
        {
            NetworkLogger.Info($"[{this}]Connected to server");
        }

        public void OnDataReceived(ArraySegment<byte> data)
        {
            NetworkPacket packet = default;
            if (_compress)
            {
                if (!NetworkPacker.UnpackCompressed(data, out packet))
                {
                    return;
                }

                _messageHandler.Handle(packet.id, packet.payload);
                return;
            }

            if (!_compress)
            {
                if (!NetworkPacker.Unpack(data, out packet))
                {
                    return;
                }

                _messageHandler.Handle(packet.id, packet.payload);
                return;
            }
        }

        public void OnDisconnected()
        {
            Cts.Cancel();
            NetworkLogger.Info($"[{this}]Disconnected from server");
        }

        public void OnDataSent(ArraySegment<byte> data)
        {
        }

        public void OnUpdate()
        {
            socket.TickIncoming();
            UpdateSystems();
            socket.TickOutgoing();
        }

        public void UpdateSystems()
        {
            float deltaTime = TimeSpan.FromTicks(DeltaTimeTick).Seconds;
            foreach (var system in _systems.systems)
            {
                if (system is IOnUpdate onUpdate)
                {
                    
                    onUpdate.OnUpdate(deltaTime);
                }
            }
        }

        public void Dispose()
        {
            _disposer.Execute();
            if (socket.connected)
            {
                socket.Disconnect();
            }

            _messageHandler?.Dispose();
            _systems?.Dispose();
            Cts?.Cancel();
            Cts?.Dispose();
        }
    }
}
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
        private readonly IServerSocket _socket;
        private readonly NetworkBufferPool _bufferPool;
        private readonly NetworkServerMessageHandler _messageHandler;

        public int ConnectionCount { get; private set; }
        public ushort TargetFrameRate { get; private set; }
        public CancellationTokenSource Cts { get; private set; }
        public long DeltaTimeTick { get; private set; }

        private readonly SystemLocator _system;

        public NetworkServer(IServerSocket socket, ushort targetFrameRate = 0)
        {
            ConnectionCount = 0;
            _socket = socket;
            _bufferPool = new NetworkBufferPool(16);
            _messageHandler = new NetworkServerMessageHandler();
            TargetFrameRate = targetFrameRate;
            // Socket event handlers
            _socket.OnConnected += OnConnected;
            _socket.OnDataReceived += OnDataReceived;
            _socket.OnDisconnected += OnDisconnected;
            _socket.OnDataSent += OnDataSent;

            _system = new SystemLocator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Register<T>(Action<int, T> handler) where T : INetworkMessage
        {
            _messageHandler.Add(handler);
        }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSystem Get<TSystem>() where TSystem : ISystem
        {
            return _system.Get<TSystem>();
        }

        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove<TSystem>() where TSystem : ISystem
        {
            _system.UnRegister<TSystem>();
        }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Register<TSystem>(TSystem system) where TSystem : ISystem
        {
            _system.Register(system);
            if (system is IOnInit<NetworkServer> init)
            {
                init.OnInit(this);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send<TMessage>(int connectionId, TMessage message) where TMessage : INetworkMessage
        {
            _socket.Send(connectionId, message, _bufferPool);
        }

        public Task Run()
        {
            if (Cts != null)
            {
                NetworkLogger.Error($"NetworkManager:[{this}] is already running");
                return Task.CompletedTask;
            }

            Cts = new CancellationTokenSource();
            _socket.Start();
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
                    stopwatch.Restart(); // 重置计时器
                    OnUpdate(); // 执行一帧
                    DeltaTimeTick = stopwatch.ElapsedTicks; // 这帧执行的时间
                    var sleepTimeTicks = frameTimeTicks - DeltaTimeTick; // 当前帧用于睡眠的时间
                    if (sleepTimeTicks > 0) //真实帧率比目标帧率高
                    {
                        await Task.Delay(TimeSpan.FromTicks(sleepTimeTicks), Cts.Token);
                    }
                    else // 目标帧率没有达到
                    {
                        NetworkLogger.Warning(
                            $"[{this}]Frame rate is low:{TimeSpan.FromTicks(DeltaTimeTick).Milliseconds}ms>={TimeSpan.FromTicks(frameTimeTicks).Milliseconds}ms");
                    }
                }
            }, Cts.Token);
            return run;
        }

        public void OnConnected(int connectionId)
        {
            ConnectionCount++;
        }

        public void OnDataReceived(int connectionId, ArraySegment<byte> data)
        {
            NetworkPacket packet = NetworkPacket.Unpack(data);
            _messageHandler.Handle(packet.id, connectionId, packet.payload);
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

        public void OnUpdate()
        {
            _socket.TickIncoming();

            foreach (var system in _system.systems)
            {
                if (system is IOnUpdate onUpdate)
                {
                    onUpdate.OnUpdate();
                }
            }

            _socket.TickOutgoing();
        }

        public void Dispose()
        {
            _system?.Dispose();
            Cts?.Dispose();
            _messageHandler?.Dispose();
        }
    }
}
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
        private IClientSocket _socket;
        private readonly NetworkBufferPool _bufferPool;
        private readonly NetworkClientMessageHandler _messageHandler;
        public ushort TargetFrameRate { get; private set; }
        public CancellationTokenSource Cts { get; private set; }

        public long DeltaTimeTick { get; private set; }

        private readonly SystemLocator _systems;

        public NetworkClient(IClientSocket socket, ushort targetFrameRate = 60)
        {
            _socket = socket;
            _bufferPool = new NetworkBufferPool(16);
            _messageHandler = new NetworkClientMessageHandler();
            TargetFrameRate = targetFrameRate;
            _systems = new SystemLocator();
            // Add socket event handlers
            _socket.OnConnected += OnConnected;
            _socket.OnDataReceived += OnDataReceived;
            _socket.OnDisconnected += OnDisconnected;
            _socket.OnDataSent += OnDataSent;
        }

        public void Register<T>(Action<T> handler) where T : INetworkMessage
        {
            _messageHandler.Add(handler);
        }

        public TSystem Get<TSystem>() where TSystem : ISystem
        {
            return _systems.Get<TSystem>();
        }

        public void Register<TSystem>(TSystem system) where TSystem : ISystem
        {
            _systems.Register(system);
            if (system is IOnInit<NetworkClient> init)
            {
                init.OnInit(this);
            }
        }

        public Task Run(Uri uri)
        {
            if (Cts != null)
            {
                NetworkLogger.Error($"NetworkManager:[{this}] is already running");
                return Task.CompletedTask;
            }

            Cts = new CancellationTokenSource();
            _socket.Connect(uri);
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
                    else
                    {
                        NetworkLogger.Warning(
                            $"[{this}]Frame rate is low:{TimeSpan.FromTicks(DeltaTimeTick).Milliseconds}ms>={TimeSpan.FromTicks(frameTimeTicks).Milliseconds}ms");
                    }
                }
            }, cancellationToken: Cts.Token);
            return run;
        }

        public void Stop()
        {
            _socket.Disconnect();
            Cts.Cancel();
            Cts.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send<TMessage>(TMessage msg) where TMessage : INetworkMessage
        {
            _socket.Send(msg, _bufferPool);
        }

        public void OnConnected()
        {
            NetworkLogger.Info($"[{this}]Connected to server");
        }

        public void OnDataReceived(ArraySegment<byte> data)
        {
            NetworkPacket packet = NetworkPacket.Unpack(data);
            _messageHandler.Handle(packet.id, packet.payload);
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
            _socket.TickIncoming();
            foreach (var system in _systems.systems)
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
            _messageHandler?.Dispose();
            _systems?.Dispose();
            Cts?.Dispose();
        }
    }
}
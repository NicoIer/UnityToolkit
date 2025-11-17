// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
﻿using System;
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
        public readonly NetworkClientMessageHandler messageHandler;
        public ushort TargetFrameRate { get; private set; }
        public CancellationTokenSource Cts { get; private set; }

        public long DeltaTimeTicks { get; private set; }
        public int Fps => (int)(TimeSpan.TicksPerSecond / DeltaTimeTicks);

        private readonly SystemLocator _systems;
        private readonly ICommand _disposer;
        private readonly bool _compress;

        public NetworkClient(IClientSocket socket, ushort targetFrameRate = 60, bool compress = true)
        {
            _compress = compress;
            this.socket = socket;
            _bufferPool = new NetworkBufferPool(16);
            messageHandler = new NetworkClientMessageHandler();
            TargetFrameRate = targetFrameRate;
            _systems = new SystemLocator();
            // Add socket event handlers
            this.socket.OnConnected += OnConnected;
            this.socket.OnDataReceived += OnDataReceived;
            this.socket.OnDisconnected += OnDisconnected;
            this.socket.OnDataSent += OnDataSent;

            // _disposer = AddMsgHandler<AssignConnectionIdMessage>(OnAssignConnectionId);
        }

        // private void OnAssignConnectionId(AssignConnectionIdMessage server)
        // {
        //     if (connectionId != 0)
        //     {
        //         NetworkLogger.Warning($"[{this}]Connection id is already assigned");
        //     }
        //
        //     connectionId = server.id;
        // }

        // public ICommand AddMsgHandler<T>(MessageHandler<T> handler) where T : INetworkMessage
        // {
        //     return messageHandler.Add<T>(handler);
        // }

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
                long frameMaxTime = TimeSpan.FromSeconds(1d / TargetFrameRate).Ticks;
                DeltaTimeTicks = frameMaxTime;
                Stopwatch stopwatch = new Stopwatch();
                while (!Cts.Token.IsCancellationRequested)
                {
                    stopwatch.Restart();
                    frameMaxTime = TimeSpan.FromSeconds(1d / TargetFrameRate).Ticks;
                    float deltaTime = (float)DeltaTimeTicks / TimeSpan.TicksPerSecond;
                    OnUpdate(deltaTime); // 执行一帧 时间是上一帧的时间
                    // ToolkitLog.Info($"deltaTime:{deltaTime} FPS:{FPS}");
                    DeltaTimeTicks = stopwatch.ElapsedTicks;
                    if (DeltaTimeTicks < frameMaxTime)
                    {
                        var sleepTimeTicks = frameMaxTime - DeltaTimeTicks;
                        DeltaTimeTicks = frameMaxTime;
                        await Task.Delay(TimeSpan.FromTicks(sleepTimeTicks), Cts.Token);
                    }
                }
            }, cancellationToken: Cts.Token);
            return run;
        }

        public void Stop()
        {
            socket.Disconnect();
            Cts.Cancel();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send<TMessage>(in TMessage msg, bool noDelay = false) where TMessage : INetworkMessage
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

                messageHandler.Handle(packet.id, packet.payload);
                return;
            }

            if (!_compress)
            {
                if (!NetworkPacker.Unpack(data, out packet))
                {
                    return;
                }

                messageHandler.Handle(packet.id, packet.payload);
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

        public void OnUpdate(in float deltaTime)
        {
            socket.TickIncoming();
            UpdateSystems();
            socket.TickOutgoing();
        }

        public void UpdateSystems()
        {
            float deltaTime = TimeSpan.FromTicks(DeltaTimeTicks).Seconds;
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
            _disposer?.Execute();
            if (socket.connected)
            {
                socket.Disconnect();
            }

            messageHandler?.Dispose();
            _systems?.Dispose();
            Cts?.Cancel();
            Cts?.Dispose();
        }
    }
}
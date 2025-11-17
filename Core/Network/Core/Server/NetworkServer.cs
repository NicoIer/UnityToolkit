// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
﻿using System;
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
        public readonly NetworkServerMessageHandler messageHandler;

        public int ConnectionCount { get; private set; }
        public ushort TargetFrameRate { get; private set; }
        public CancellationTokenSource Cts { get; private set; }
        public long DeltaTimeTick { get; private set; }
        public int Fps => (int)(TimeSpan.TicksPerSecond / DeltaTimeTick);

        private readonly SystemLocator _system;
        private readonly bool _compress;

        public delegate void UpdateDelegate(in float deltaTime);

        public event UpdateDelegate OnUpdateEvent;

        private IOCContainer _container;

        public NetworkServer(IServerSocket socket, ushort targetFrameRate = 60, bool compress = true)
        {
            _container = new IOCContainer();
            _compress = compress;
            ConnectionCount = 0;
            this.socket = socket;
            _bufferPool = new NetworkBufferPool(16);
            messageHandler = new NetworkServerMessageHandler();
            TargetFrameRate = targetFrameRate;
            messageHandler.Add<HeartBeat>(OnHeartBeat);
            // Socket event handlers
            this.socket.OnConnected += OnConnected;
            this.socket.OnDataReceived += OnDataReceived;
            this.socket.OnDisconnected += OnDisconnected;
            this.socket.OnDataSent += OnDataSent;

            _system = new SystemLocator();
        }

        private void OnHeartBeat(in int connectionid, in HeartBeat message)
        {
#if DEBUG || UNITY_EDITOR
            ToolkitLog.Debug($"Heartbeat received from connection {connectionid}");
#endif
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ICommand AddMsgHandler<T>(MessageHandler<T> handler) where T : INetworkMessage
        {
            return messageHandler.Add(handler);
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


        public Task Run(bool autoTick)
        {
            if (Cts != null)
            {
                NetworkLogger.Error($"NetworkManager:[{this}] is already running");
                return Task.CompletedTask;
            }

            Cts = new CancellationTokenSource();
            socket.Start();
            if (!autoTick) return Task.CompletedTask;
            long frameMaxTime = TimeSpan.FromSeconds(1d / TargetFrameRate).Ticks; // 一帧最大时间
            DeltaTimeTick = frameMaxTime;
            var run = Task.Run(async () =>
            {
                ToolkitLog.Debug($"NetworkServer:[{this}] is running");
                Stopwatch stopwatch = new Stopwatch();
                while (!Cts.Token.IsCancellationRequested)
                {
                    stopwatch.Restart(); // 重置计时器
                    frameMaxTime = TimeSpan.FromSeconds(1d / TargetFrameRate).Ticks; // 一帧最大时间
                    // 换成秒
                    float deltaTime = (float)DeltaTimeTick / TimeSpan.TicksPerSecond;
                    OnUpdate(deltaTime); // 执行一帧
                    DeltaTimeTick = stopwatch.ElapsedTicks; // 这帧执行的时间
                    if (DeltaTimeTick < frameMaxTime) // 达到了帧率 休息一下
                    {
                        long sleepMs = (frameMaxTime - DeltaTimeTick) / TimeSpan.TicksPerMillisecond;
                        DeltaTimeTick = frameMaxTime;
                        await Task.Delay(TimeSpan.FromMilliseconds(sleepMs), Cts.Token);
                    }
                }
            }, Cts.Token);
            return run;
        }

        public void Stop()
        {
            if (Cts == null)
            {
                NetworkLogger.Error($"NetworkManager:[{this}] is not running");
                return;
            }

            Cts.Cancel();
            socket.Stop();
            Cts = null;
            ToolkitLog.Debug($"NetworkServer:[{this}] is stopped");
        }

        public void OnConnected(int connectionId)
        {
            NetworkLogger.Info($"Client {connectionId} connected");
            ConnectionCount++;
            // Send(connectionId, new AssignConnectionIdMessage(connectionId));
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

                messageHandler.Handle(packet.id, connectionId, packet.payload);
                return;
            }

            if (!_compress)
            {
                if (!NetworkPacker.Unpack(data, out packet))
                {
                    NetworkLogger.Error($"[{this}]Unpack error from connection {connectionId}");
                    return;
                }

                messageHandler.Handle(packet.id, connectionId, packet.payload);
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


            OnUpdateEvent?.Invoke(deltaTime);


            socket.TickOutgoing();
        }

        public void Dispose()
        {
            _system?.Dispose();
            Cts?.Dispose();
            messageHandler?.Dispose();
        }
    }
}
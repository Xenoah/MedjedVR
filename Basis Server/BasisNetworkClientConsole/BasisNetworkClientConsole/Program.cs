using System.Diagnostics;
using Basis.Logging;
using Basis.Network;
using Basis.Config;
using Basis.Utils;
using Basis.Network.Core;

namespace Basis
{
    /// <summary>
    /// Programの責務をまとめるクラスです。
    /// ClientConsole領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    partial class Program
    {
        private const double DriverTickMs = 15.0;
        private const double MovementIntervalMs = 90.0;
        private static volatile bool _running = true;

        /// <summary>
        /// Mainを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static async Task Main(string[] args)
        {
            ErrorHandlers.AttachGlobalHandlers();
            ConfigManager.LoadOrCreateConfigXml("Config.xml");
            NetDebug.Logger = new BasisClientLogger();

            var clientManager = new ClientManager();
            clientManager.Prepare();

            AppDomain.CurrentDomain.ProcessExit += (_, __) =>
            {
                Console.WriteLine("Shutting down...");
                _running = false;
                clientManager.StopClientsAsync().GetAwaiter().GetResult();
            };

            MovementSender.Initialize(clientManager.ClientCount);

            // CPU core ごとに 1 worker で全 client を駆動する
            StartClientDriverLoops(clientManager.FinalClients, clientManager.FinalPeers);

            await clientManager.StartClientsAsync();

            // random reconnect を開始する
            _ = StartRandomReconnectLoop(clientManager);

            await Task.Delay(-1); // main を生かし続ける
        }

        /// <summary>
        /// Stopクライアントを停止します。保持している状態を片付け、次回起動に影響が残らないようにします。
        /// </summary>
        public static void StopClient(ClientManager manager, int index)
        {
            var peer = Volatile.Read(ref manager.FinalPeers[index]);
            if (peer != null)
            {
                peer.Disconnect();
            }
        }

        /// <summary>
        /// StartクライアントDriverLoopsを開始します。依存する状態を準備して実行ループや待ち受けを有効化します。
        /// </summary>
        private static void StartClientDriverLoops(NetworkClient[] clients, NetPeer[] peers)
        {
            int count = peers.Length;
            int workerCount = Math.Min(Environment.ProcessorCount, count);
            if (workerCount <= 0) return;

            int chunkSize = (count + workerCount - 1) / workerCount;

            for (int w = 0; w < workerCount; w++)
            {
                int start = w * chunkSize;
                int end = Math.Min(start + chunkSize, count);
                if (start >= end) break;

                double phaseOffsetMs = MovementIntervalMs * w / workerCount;

                var thread = new Thread(() => DriveSlice(clients, peers, start, end, phaseOffsetMs))
                {
                    Name = $"ClientDriver({start}-{end})",
                    IsBackground = true
                };
                thread.Start();
            }
        }

        /// <summary>
        /// DriveSliceを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        private static void DriveSlice(NetworkClient[] clients, NetPeer[] peers, int start, int end, double phaseOffsetMs)
        {
            var sw = Stopwatch.StartNew();
            double lastTickMs = 0;
            double lastMovementMs = phaseOffsetMs - MovementIntervalMs;

            while (_running)
            {
                double nowMs = sw.Elapsed.TotalMilliseconds;
                float dt = (float)(nowMs - lastTickMs);
                lastTickMs = nowMs;

                for (int i = start; i < end; i++)
                {
                    var client = Volatile.Read(ref clients[i]);
                    if (client != null)
                    {
                        client.Poll();
                        client.Update(dt);
                    }
                }

                if (nowMs - lastMovementMs >= MovementIntervalMs)
                {
                    lastMovementMs = nowMs;
                    for (int i = start; i < end; i++)
                    {
                        var peer = Volatile.Read(ref peers[i]);
                        if (peer != null && (peer.Tag as ConsoleClientIdentity)?.Authenticated == true)
                            MovementSender.ProcessSingle(peer, i);
                    }
                }

                int sleepMs = (int)(DriverTickMs - (sw.Elapsed.TotalMilliseconds - nowMs));
                if (sleepMs > 0)
                    Thread.Sleep(sleepMs);
            }
        }

        /// <summary>
        /// StartRandomReconnectLoopを開始します。依存する状態を準備して実行ループや待ち受けを有効化します。
        /// </summary>
        private static async Task StartRandomReconnectLoop(ClientManager clientManager)
        {
            int totalClients = clientManager.ClientCount;

            while (true)
            {
                int waitMinutes = Random.Shared.Next(1, 21); // 1–20 minutes
                await Task.Delay(TimeSpan.FromMinutes(waitMinutes));

                int indexToRestart = Random.Shared.Next(0, totalClients);
                BNL.Log($"Randomly restarting client at index {indexToRestart}");

                await clientManager.ReconnectClientAsync(indexToRestart);
            }
        }
    }
}

using Basis.Network.Core;
using System;
using System.Threading;

namespace Basis.Network.Server
{
    /// <summary>
    /// Basis統計の責務をまとめるクラスです。
    /// Server領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public static class BasisStatistics
    {
        /// <summary>
        /// 管理を保持します。型は NetManager で、関連処理から共有される値です。
        /// </summary>
        public static NetManager Manager;
        private static Thread workerThread;
        private static volatile bool keepPolling = true; // thread lifecycle の制御に使う

        /// <summary>
        /// StartWorkerThreadを開始します。依存する状態を準備して実行ループや待ち受けを有効化します。
        /// </summary>
        public static void StartWorkerThread(NetManager manager)
        {
          //  Manager = manager;

            // worker thread を開始する
           // workerThread = new Thread(PollStatistics);
          //  workerThread.IsBackground = true; // background thread は main application 終了時に自動終了する
          //  workerThread.Start();
        }

        /// <summary>
        /// StopWorkerThreadを停止します。保持している状態を片付け、次回起動に影響が残らないようにします。
        /// </summary>
        public static void StopWorkerThread()
        {
          //  keepPolling = false;

          // worker thread が正常に終了するのを待つ
           // if (workerThread != null && workerThread.IsAlive)
          //  {
            //    workerThread.Join();
           // }
        }

        /// <summary>
        /// Poll統計を実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        private static void PollStatistics()
        {
           /// while (keepPolling)
           // {
           //     // manager から statistics を poll する
             //   PollLatestStatistics();

            //    // 次の poll 前に少し待つ (例: 毎秒)
             //   Thread.Sleep(15000); // 必要に応じて delay を調整できる
        //    }
        }

        /// <summary>
        /// PollLatest統計を実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static void PollLatestStatistics()
        {
        //  BNL.Log("Packet Loss: " + Manager.Statistics.PacketLoss + "Packet Loss Percent: " + Manager.Statistics.PacketLossPercent + "Bytes Received: " + Manager.Statistics.BytesReceived + "Bytes Sent: " + Manager.Statistics.BytesSent + "Packets Sent: " + Manager.Statistics.PacketsSent);
        }
    }
}

using Basis.Network.Core;
using System.Threading;
using static BasisNetworkCore.Serializable.SerializableBasis;

namespace BasisNetworkServer.Security
{
    /// <summary>
    /// client error/exception reporting 用の server toggle。
    /// boot 時に Configuration.CrashReportingEnabled から seed され、client へ push されるため、
    /// client はこれが on の間だけ report を送る。変更時は admin path 経由で persist される。
    /// </summary>
    public static class BasisCrashReportStateManager
    {
        private static int _enabled = 1;

        /// <summary>
        /// Enabledを保持します。型は bool で、関連処理から共有される値です。
        /// </summary>
        public static bool Enabled => Interlocked.CompareExchange(ref _enabled, 0, 0) == 1;

        /// <summary>
        /// InitializeFrom設定を初期化します。設定、永続化ファイル、実行時キャッシュを起動時の状態へ整えます。
        /// </summary>
        public static void InitializeFromConfig(Configuration config)
        {
            Interlocked.Exchange(ref _enabled, config.CrashReportingEnabled ? 1 : 0);
        }

        /// <summary>
        /// SetEnabledを設定します。以後のネットワーク処理で参照される状態を更新します。
        /// </summary>
        public static bool SetEnabled(bool enabled)
        {
            int requestedValue = enabled ? 1 : 0;
            int previousValue = Interlocked.Exchange(ref _enabled, requestedValue);
            return previousValue != requestedValue;
        }

        /// <summary>
        /// Send状態Toピアを送信します。対象ピア、チャンネル、配送方式に合わせてパケット化します。
        /// </summary>
        public static void SendStateToPeer(NetPeer peer)
        {
            NetDataWriter writer = NetworkServer.RentWriter();
            try
            {
                new AdminRequest().Serialize(writer, AdminRequestMode.GlobalGetCrashReportState);
                writer.Put(Enabled);
                NetworkServer.TrySend(peer, writer, BasisNetworkCommons.AdminChannel, DeliveryMethod.ReliableOrdered);
            }
            finally
            {
                NetworkServer.ReturnWriter(writer);
            }
        }

        /// <summary>
        /// Broadcast状態を実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static void BroadcastState()
        {
            NetDataWriter writer = NetworkServer.RentWriter();
            try
            {
                new AdminRequest().Serialize(writer, AdminRequestMode.GlobalGetCrashReportState);
                writer.Put(Enabled);
                NetworkServer.BroadcastMessageToClients(
                    writer,
                    BasisNetworkCommons.AdminChannel,
                    NetworkServer.PeerSnapshot,
                    DeliveryMethod.ReliableOrdered);
            }
            finally
            {
                NetworkServer.ReturnWriter(writer);
            }
        }
    }
}

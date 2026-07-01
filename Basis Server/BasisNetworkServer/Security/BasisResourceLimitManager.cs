using Basis.Network.Core;
using System.Threading;
using static BasisNetworkCore.Serializable.SerializableBasis;

namespace BasisNetworkServer.Security
{
    /// <summary>
    /// client ごとの resource use (persistent database growth と content-share sphere) を制限する
    /// server-defined cap。boot 時に Configuration から seed され、admin panel から live edit できる
    /// (basis.moderation.globallock で gate)。config.xml に persist され、admin panel の同期用に broadcast される。
    /// </summary>
    public static class BasisResourceLimitManager
    {
        private const int DefaultMaxDatabaseEntries = 10000;
        private const int DefaultMaxDatabaseNameLength = 256;
        private const int DefaultMaxDatabasePayloadEntries = 1000;
        private const int DefaultMaxContentSpheresPerPlayer = 32;

        private const int AbsoluteMaxDatabaseEntries = 1000000;
        private const int AbsoluteMaxDatabaseNameLength = 8192;
        private const int AbsoluteMaxDatabasePayloadEntries = 100000;
        private const int AbsoluteMaxContentSpheresPerPlayer = 4096;

        private static int _maxDatabaseEntries = DefaultMaxDatabaseEntries;
        private static int _maxDatabaseNameLength = DefaultMaxDatabaseNameLength;
        private static int _maxDatabasePayloadEntries = DefaultMaxDatabasePayloadEntries;
        private static int _maxContentSpheresPerPlayer = DefaultMaxContentSpheresPerPlayer;

        /// <summary>
        /// MaxデータベースEntriesを保持します。型は int で、関連処理から共有される値です。
        /// </summary>
        public static int MaxDatabaseEntries => Interlocked.CompareExchange(ref _maxDatabaseEntries, 0, 0);
        /// <summary>
        /// MaxデータベースNameLengthを保持します。型は int で、関連処理から共有される値です。
        /// </summary>
        public static int MaxDatabaseNameLength => Interlocked.CompareExchange(ref _maxDatabaseNameLength, 0, 0);
        /// <summary>
        /// MaxデータベースPayloadEntriesを保持します。型は int で、関連処理から共有される値です。
        /// </summary>
        public static int MaxDatabasePayloadEntries => Interlocked.CompareExchange(ref _maxDatabasePayloadEntries, 0, 0);
        /// <summary>
        /// MaxContentSpheresPerプレイヤーを保持します。型は int で、関連処理から共有される値です。
        /// </summary>
        public static int MaxContentSpheresPerPlayer => Interlocked.CompareExchange(ref _maxContentSpheresPerPlayer, 0, 0);

        /// <summary>
        /// InitializeFrom設定を初期化します。設定、永続化ファイル、実行時キャッシュを起動時の状態へ整えます。
        /// </summary>
        public static void InitializeFromConfig(Configuration config)
        {
            SetLimits(config.MaxDatabaseEntries, config.MaxDatabaseNameLength, config.MaxDatabasePayloadEntries, config.MaxContentSpheresPerPlayer);
        }

        /// <summary>
        /// SetLimitsを設定します。以後のネットワーク処理で参照される状態を更新します。
        /// </summary>
        public static bool SetLimits(int maxDatabaseEntries, int maxDatabaseNameLength, int maxDatabasePayloadEntries, int maxContentSpheresPerPlayer)
        {
            Sanitize(ref maxDatabaseEntries, ref maxDatabaseNameLength, ref maxDatabasePayloadEntries, ref maxContentSpheresPerPlayer);
            int prevEntries = Interlocked.Exchange(ref _maxDatabaseEntries, maxDatabaseEntries);
            int prevNameLength = Interlocked.Exchange(ref _maxDatabaseNameLength, maxDatabaseNameLength);
            int prevPayloadEntries = Interlocked.Exchange(ref _maxDatabasePayloadEntries, maxDatabasePayloadEntries);
            int prevSpheres = Interlocked.Exchange(ref _maxContentSpheresPerPlayer, maxContentSpheresPerPlayer);
            return prevEntries != maxDatabaseEntries
                || prevNameLength != maxDatabaseNameLength
                || prevPayloadEntries != maxDatabasePayloadEntries
                || prevSpheres != maxContentSpheresPerPlayer;
        }

        /// <summary>
        /// Send状態Toピアを送信します。対象ピア、チャンネル、配送方式に合わせてパケット化します。
        /// </summary>
        public static void SendStateToPeer(NetPeer peer)
        {
            NetDataWriter writer = NetworkServer.RentWriter();
            try
            {
                Write(writer);
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
                Write(writer);
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

        /// <summary>
        /// Writeを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        private static void Write(NetDataWriter writer)
        {
            new AdminRequest().Serialize(writer, AdminRequestMode.GlobalGetResourceLimits);
            writer.Put(MaxDatabaseEntries);
            writer.Put(MaxDatabaseNameLength);
            writer.Put(MaxDatabasePayloadEntries);
            writer.Put(MaxContentSpheresPerPlayer);
        }

        /// <summary>
        /// Sanitizeを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        private static void Sanitize(ref int entries, ref int nameLength, ref int payloadEntries, ref int spheres)
        {
            if (entries < 1) entries = DefaultMaxDatabaseEntries;
            if (entries > AbsoluteMaxDatabaseEntries) entries = AbsoluteMaxDatabaseEntries;
            if (nameLength < 1) nameLength = DefaultMaxDatabaseNameLength;
            if (nameLength > AbsoluteMaxDatabaseNameLength) nameLength = AbsoluteMaxDatabaseNameLength;
            if (payloadEntries < 1) payloadEntries = DefaultMaxDatabasePayloadEntries;
            if (payloadEntries > AbsoluteMaxDatabasePayloadEntries) payloadEntries = AbsoluteMaxDatabasePayloadEntries;
            if (spheres < 1) spheres = DefaultMaxContentSpheresPerPlayer;
            if (spheres > AbsoluteMaxContentSpheresPerPlayer) spheres = AbsoluteMaxContentSpheresPerPlayer;
        }
    }
}

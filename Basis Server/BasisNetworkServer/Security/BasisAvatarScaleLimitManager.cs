using Basis.Network.Core;
using System.Threading;
using static BasisNetworkCore.Serializable.SerializableBasis;

namespace BasisNetworkServer.Security
{
    /// <summary>
    /// non-admin player が scale できる avatar eye height の server-defined minimum/maximum (metres)。
    /// boot 時に Configuration から seed され、GlobalGetAvatarScaleLimits 経由で client へ push されるため、
    /// client は avatar scale をこの範囲へ clamp する。admin (basis.moderation.globallock) は client-side clamp を bypass する。
    /// admin は range を live 変更でき、新しい値は config.xml へ persist されて broadcast される。
    /// </summary>
    public static class BasisAvatarScaleLimitManager
    {
        private const float DefaultMinMeters = 0.1f;
        private const float DefaultMaxMeters = 100f;
        private const float AbsoluteFloor = 0.01f;
        private const float AbsoluteCeiling = 1000f;

        private static float _minMeters = DefaultMinMeters;
        private static float _maxMeters = DefaultMaxMeters;

        /// <summary>
        /// MinMetersを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public static float MinMeters => Interlocked.CompareExchange(ref _minMeters, 0f, 0f);
        /// <summary>
        /// MaxMetersを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public static float MaxMeters => Interlocked.CompareExchange(ref _maxMeters, 0f, 0f);

        /// <summary>
        /// InitializeFrom設定を初期化します。設定、永続化ファイル、実行時キャッシュを起動時の状態へ整えます。
        /// </summary>
        public static void InitializeFromConfig(Configuration config)
        {
            SetLimits(config.MinAvatarEyeHeightMeters, config.MaxAvatarEyeHeightMeters);
        }

        /// <summary>sanitize し、min &lt;= max になるよう order して set し、どちらかの bound が実際に変わったかを返す。</summary>
        public static bool SetLimits(float minMeters, float maxMeters)
        {
            Sanitize(ref minMeters, ref maxMeters);
            float prevMin = Interlocked.Exchange(ref _minMeters, minMeters);
            float prevMax = Interlocked.Exchange(ref _maxMeters, maxMeters);
            return prevMin != minMeters || prevMax != maxMeters;
        }

        /// <summary>
        /// Send状態Toピアを送信します。対象ピア、チャンネル、配送方式に合わせてパケット化します。
        /// </summary>
        public static void SendStateToPeer(NetPeer peer)
        {
            NetDataWriter writer = NetworkServer.RentWriter();
            try
            {
                new AdminRequest().Serialize(writer, AdminRequestMode.GlobalGetAvatarScaleLimits);
                writer.Put(MinMeters);
                writer.Put(MaxMeters);
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
                new AdminRequest().Serialize(writer, AdminRequestMode.GlobalGetAvatarScaleLimits);
                writer.Put(MinMeters);
                writer.Put(MaxMeters);
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
        /// Sanitizeを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        private static void Sanitize(ref float minMeters, ref float maxMeters)
        {
            if (float.IsNaN(minMeters) || float.IsInfinity(minMeters) || minMeters <= 0f) minMeters = DefaultMinMeters;
            if (float.IsNaN(maxMeters) || float.IsInfinity(maxMeters) || maxMeters <= 0f) maxMeters = DefaultMaxMeters;
            if (minMeters < AbsoluteFloor) minMeters = AbsoluteFloor;
            if (maxMeters > AbsoluteCeiling) maxMeters = AbsoluteCeiling;
            if (maxMeters < minMeters) maxMeters = minMeters;
        }
    }
}

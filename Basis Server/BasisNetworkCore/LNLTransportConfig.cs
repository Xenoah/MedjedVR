using System;

namespace Basis.Network.Core
{
    [Serializable]
    /// <summary>
    /// LNLTransport設定の責務をまとめるクラスです。
    /// Core領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public sealed class LNLTransportConfig
    {
        /// <summary>Bump to force existing files to be rewritten; newly-added fields are healed automatically on load.</summary>
        public const int CurrentConfigVersion = 1;
        /// <summary>Schema version stamped into the file; 0 = pre-versioning, upgraded on load.</summary>
        public int ConfigVersion = 0;

        /// <summary>
        /// UseNativeSocketsを保持します。型は bool で、関連処理から共有される値です。
        /// </summary>
        public bool UseNativeSockets = true;
        /// <summary>
        /// NatPunchEnabledを保持します。型は bool で、関連処理から共有される値です。
        /// </summary>
        public bool NatPunchEnabled = true;
        /// <summary>
        /// NatPortPredictionRangeを保持します。型は int で、関連処理から共有される値です。
        /// </summary>
        public int NatPortPredictionRange = 32;
        /// <summary>
        /// PingIntervalを保持します。型は int で、関連処理から共有される値です。
        /// </summary>
        public int PingInterval = 1500;
        /// <summary>
        /// DisconnectTimeoutを保持します。型は int で、関連処理から共有される値です。
        /// </summary>
        public int DisconnectTimeout = 30000;
        /// <summary>
        /// SimulateパケットLossを保持します。型は bool で、関連処理から共有される値です。
        /// </summary>
        public bool SimulatePacketLoss = false;
        /// <summary>
        /// SimulateLatencyを保持します。型は bool で、関連処理から共有される値です。
        /// </summary>
        public bool SimulateLatency = false;
        /// <summary>
        /// SimulationパケットLossChanceを保持します。型は int で、関連処理から共有される値です。
        /// </summary>
        public int SimulationPacketLossChance = 10;
        /// <summary>
        /// SimulationMinLatencyを保持します。型は int で、関連処理から共有される値です。
        /// </summary>
        public int SimulationMinLatency = 50;
        /// <summary>
        /// SimulationMaxLatencyを保持します。型は int で、関連処理から共有される値です。
        /// </summary>
        public int SimulationMaxLatency = 150;
        /// <summary>
        /// ReconnectDelayを保持します。型は int で、関連処理から共有される値です。
        /// </summary>
        public int ReconnectDelay = 500;
        /// <summary>
        /// MaxConnectAttemptsを保持します。型は int で、関連処理から共有される値です。
        /// </summary>
        public int MaxConnectAttempts = 10;
        /// <summary>
        /// ReuseAddresssを保持します。型は bool で、関連処理から共有される値です。
        /// </summary>
        public bool ReuseAddresss = false;
        /// <summary>
        /// DontRouteを保持します。型は bool で、関連処理から共有される値です。
        /// </summary>
        public bool DontRoute = false;
        /// <summary>
        /// IPv6Enabledを保持します。型は bool で、関連処理から共有される値です。
        /// </summary>
        public bool IPv6Enabled = true;
        /// <summary>
        /// MtuOverrideを保持します。型は int で、関連処理から共有される値です。
        /// </summary>
        public int MtuOverride = 0;
        /// <summary>
        /// MtuDiscoveryを保持します。型は bool で、関連処理から共有される値です。
        /// </summary>
        public bool MtuDiscovery = true;
        /// <summary>
        /// DisconnectOnUnreachableを保持します。型は bool で、関連処理から共有される値です。
        /// </summary>
        public bool DisconnectOnUnreachable = false;
        /// <summary>
        /// AllowピアAddressChangeを保持します。型は bool で、関連処理から共有される値です。
        /// </summary>
        public bool AllowPeerAddressChange = true;
        /// <summary>
        /// MultiSocketCountを保持します。型は int で、関連処理から共有される値です。
        /// </summary>
        public int MultiSocketCount = 1;
    }
}

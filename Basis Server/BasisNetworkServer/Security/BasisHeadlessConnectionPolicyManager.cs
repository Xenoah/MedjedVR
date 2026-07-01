using Basis.Network.Core;
using Basis.Network.Server.Generic;
using System;
using System.Threading;
using static BasisNetworkCore.Serializable.SerializableBasis;
namespace BasisNetworkServer.Security
{
    /// <summary>
    /// headless client が接続状態を維持できるかを制御する runtime-only server policy。
    /// </summary>
    public static class BasisHeadlessConnectionPolicyManager
    {
        /// <summary>
        /// DisallowedReasonを保持します。型は string で、関連処理から共有される値です。
        /// </summary>
        public const string DisallowedReason = "Headless client disallowed by server.";

        private static int _headlessDisallowed;

        /// <summary>
        /// HeadlessDisallowedを保持します。型は bool で、関連処理から共有される値です。
        /// </summary>
        public static bool HeadlessDisallowed => Interlocked.CompareExchange(ref _headlessDisallowed, 0, 0) == 1;

        /// <summary>
        /// InitializeFrom設定を初期化します。設定、永続化ファイル、実行時キャッシュを起動時の状態へ整えます。
        /// </summary>
        public static void InitializeFromConfig(bool disallowHeadless)
        {
            Interlocked.Exchange(ref _headlessDisallowed, disallowHeadless ? 1 : 0);
        }

        /// <summary>
        /// SetDisallowHeadlessを設定します。以後のネットワーク処理で参照される状態を更新します。
        /// </summary>
        public static bool SetDisallowHeadless(bool disallowHeadless)
        {
            int requestedValue = disallowHeadless ? 1 : 0;
            int previousValue = Interlocked.Exchange(ref _headlessDisallowed, requestedValue);
            return previousValue != requestedValue;
        }

        /// <summary>
        /// IsHeadlessクライアントを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static bool IsHeadlessClient(SerializableBasis.ClientMetaDataMessage metaData)
        {
            return IsHeadlessPlatform(metaData.playerPlatform);
        }

        /// <summary>
        /// IsHeadlessPlatformを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static bool IsHeadlessPlatform(string playerPlatform)
        {
            if (string.IsNullOrWhiteSpace(playerPlatform))
            {
                return false;
            }

            return string.Equals(playerPlatform, "Headless", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(playerPlatform, "WindowsServer", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(playerPlatform, "LinuxServer", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(playerPlatform, "OSXServer", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// DisconnectConnectedHeadlessPeersを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static void DisconnectConnectedHeadlessPeers()
        {
            NetPeer[] peers = NetworkServer.PeerSnapshot;
            foreach (NetPeer peer in peers)
            {
                if (!BasisSavedState.GetLastPlayerMetaData(peer, out SerializableBasis.ClientMetaDataMessage metaData) ||
                    !IsHeadlessClient(metaData))
                {
                    continue;
                }

                BasisServerHandle.BasisServerHandleEvents.RejectWithReason(peer, DisallowedReason);
            }
        }

        /// <summary>
        /// Send状態Toピアを送信します。対象ピア、チャンネル、配送方式に合わせてパケット化します。
        /// </summary>
        public static void SendStateToPeer(NetPeer peer)
        {
            NetDataWriter writer = NetworkServer.RentWriter();
            try
            {
                new AdminRequest().Serialize(writer, AdminRequestMode.GlobalGetHeadlessDisallowState);
                writer.Put(HeadlessDisallowed);
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
                new AdminRequest().Serialize(writer, AdminRequestMode.GlobalGetHeadlessDisallowState);
                writer.Put(HeadlessDisallowed);
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
